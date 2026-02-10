using System;
using System.Collections.Generic;
using XM.Contracts;

namespace XM
{
    public class TypeConvert : TypeConverterBase<string, Type, TypeConvert>
    {
        public override bool Convert(string source, string modName, out Type target)
        {
            target = null;
            var sourceArr = source.Trim().Split("::");
            if (sourceArr.Length != 2)
            {
                return false;
            }

            var mod = IModManager.I.GetModRuntimeByName(sourceArr[0]);
            if (mod == null || mod.Assemblies is not { Length: > 0 })
            {
                return false;
            }

            foreach (var assembly in mod.Assemblies)
            {
                var type = assembly.GetType(sourceArr[1]);
                if (type != null)
                {
                    target = type;
                    break;
                }
            }

            return true;
        }
    }

    public class TypeConvertI : TypeConverterBase<Type, TypeI, TypeConvertI>
    {
        public override bool Convert(Type source, string modName, out TypeI target)
        {
            if (!TypeManager.TryGetTypeI(source, out target))
            {
                var modI = IModManager.I.GetModId(modName);
                target = TypeManager.RegisterTypeI(modI, source);
            }

            return true;
        }
    }

    public struct TypeI : IEquatable<TypeI>
    {
        public ModI ModI;
        public int Id;

        public bool Equals(TypeI other)
        {
            return ModI.Equals(other.ModI) && Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is TypeI other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ModI, Id);
        }
    }

    public static class TypeManager
    {
        private readonly static Dictionary<string, Type> StrToType = new Dictionary<string, Type>();
        private readonly static Dictionary<Type, string> TypeToStr = new Dictionary<Type, string>();

        private readonly static BidirectionalDictionary<Type, TypeI> AssemblyToStr =
            new BidirectionalDictionary<Type, TypeI>();

        private static readonly Dictionary<ModI, int> _nextTypeIdByMod = new();

        public static bool TryGetType(string typeName, out Type type)
        {
            lock (StrToType)
            {
                if (StrToType.TryGetValue(typeName, out type))
                    return true;
                type = null;
                return false;
            }
        }

        public static bool RegisterType(string typeName, Type type)
        {
            if (string.IsNullOrEmpty(typeName) || type == null)
                return false;

            lock (StrToType)
            {
                StrToType[typeName] = type;
                TypeToStr[type] = typeName;
                return true;
            }
        }

        public static bool TryGetTypeI(Type type, out TypeI typeI)
        {
            typeI = default;
            if (type == null)
                return false;

            return AssemblyToStr.TryGetValueByKey(type, out typeI);
        }

        public static bool TryGetTypeI(TypeI typeI, out Type type)
        {
            type = null;
            return AssemblyToStr.TryGetKeyByValue(typeI, out type);
        }

        public static TypeI RegisterTypeI(ModI mod, Type type)
        {
            if (type == null || !mod.Valid)
                return default;

            if (AssemblyToStr.TryGetValueByKey(type, out var existing))
                return existing;

            var nextId = _nextTypeIdByMod.GetValueOrDefault(mod, 1);
            _nextTypeIdByMod[mod] = nextId + 1;

            var typeI = new TypeI { ModI = mod, Id = nextId };
            AssemblyToStr.AddOrUpdate(type, typeI);
            return typeI;
        }
    }
}