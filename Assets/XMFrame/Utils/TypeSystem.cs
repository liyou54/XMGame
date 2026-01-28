using System;
using System.Collections.Generic;

namespace XM.Utils
{
    /// <summary>
    /// 类型ID结构体，用于类型的唯一标识
    /// </summary>
    public struct TypeI : IEquatable<TypeI>
    {
        public int Id;

        public TypeI(int id)
        {
            Id = id;
        }

        public bool Equals(TypeI other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is TypeI other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public static bool operator ==(TypeI left, TypeI right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TypeI left, TypeI right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"TypeI({Id})";
        }
    }

    /// <summary>
    /// 类型系统，提供 TypeI 与 Type 之间的双向映射
    /// </summary>
    public static class TypeSystem
    {
        private static BidirectionalDictionary<TypeI, Type> _typeLookUp;
        private static Dictionary<Type, TypeI> _typeToId;
        private static int _nextTypeId = 1;

        public static BidirectionalDictionary<TypeI, Type> TypeLookUp
        {
            get
            {
                if (_typeLookUp == null)
                {
                    Initialize();
                }
                return _typeLookUp;
            }
        }

        /// <summary>
        /// 初始化类型系统
        /// </summary>
        private static void Initialize()
        {
            _typeLookUp = new BidirectionalDictionary<TypeI, Type>();
            _typeToId = new Dictionary<Type, TypeI>();
        }

        /// <summary>
        /// 注册一个类型，返回对应的 TypeI
        /// </summary>
        /// <param name="type">要注册的类型</param>
        /// <returns>类型对应的 TypeI</returns>
        public static TypeI RegisterType(Type type)
        {
            if (_typeLookUp == null)
            {
                Initialize();
            }

            if (_typeToId.TryGetValue(type, out var existingId))
            {
                return existingId;
            }

            var newId = new TypeI(_nextTypeId++);
            _typeLookUp.Add(newId, type);
            _typeToId.Add(type, newId);
            return newId;
        }

        /// <summary>
        /// 注册一个类型（泛型版本）
        /// </summary>
        /// <typeparam name="T">要注册的类型</typeparam>
        /// <returns>类型对应的 TypeI</returns>
        public static TypeI RegisterType<T>()
        {
            return RegisterType(typeof(T));
        }

        /// <summary>
        /// 通过 TypeI 获取对应的 Type
        /// </summary>
        /// <param name="typeI">类型ID</param>
        /// <returns>对应的类型，如果不存在则返回 null</returns>
        public static Type GetType(TypeI typeI)
        {
            if (_typeLookUp == null)
            {
                Initialize();
            }
            return _typeLookUp.GetByKey(typeI);
        }

        /// <summary>
        /// 通过 Type 获取对应的 TypeI
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>对应的 TypeI，如果类型未注册则返回一个 Id 为 0 的 TypeI</returns>
        public static TypeI GetTypeId(Type type)
        {
            if (_typeLookUp == null)
            {
                Initialize();
            }

            if (_typeToId.TryGetValue(type, out var typeId))
            {
                return typeId;
            }

            return new TypeI(0); // 返回无效的 TypeI
        }

        /// <summary>
        /// 通过 Type 获取对应的 TypeI（泛型版本）
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <returns>对应的 TypeI，如果类型未注册则返回一个 Id 为 0 的 TypeI</returns>
        public static TypeI GetTypeId<T>()
        {
            return GetTypeId(typeof(T));
        }

        /// <summary>
        /// 检查类型是否已注册
        /// </summary>
        /// <param name="type">要检查的类型</param>
        /// <returns>如果类型已注册返回 true，否则返回 false</returns>
        public static bool IsTypeRegistered(Type type)
        {
            if (_typeLookUp == null)
            {
                Initialize();
            }
            return _typeToId.ContainsKey(type);
        }

        /// <summary>
        /// 检查类型是否已注册（泛型版本）
        /// </summary>
        /// <typeparam name="T">要检查的类型</typeparam>
        /// <returns>如果类型已注册返回 true，否则返回 false</returns>
        public static bool IsTypeRegistered<T>()
        {
            return IsTypeRegistered(typeof(T));
        }

        /// <summary>
        /// 清空所有已注册的类型
        /// </summary>
        public static void Clear()
        {
            if (_typeLookUp != null)
            {
                _typeLookUp.Clear();
            }
            if (_typeToId != null)
            {
                _typeToId.Clear();
            }
            _nextTypeId = 1;
        }

        /// <summary>
        /// 获取已注册类型的数量
        /// </summary>
        public static int RegisteredTypeCount
        {
            get
            {
                if (_typeLookUp == null)
                {
                    return 0;
                }
                return _typeLookUp.Count;
            }
        }
    }
}