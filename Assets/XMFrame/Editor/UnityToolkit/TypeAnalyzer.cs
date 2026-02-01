using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using XM.Utils.Attribute;

namespace UnityToolkit
{
    /// <summary>字段信息，用于代码生成。</summary>
    public class FieldInfo
    {
        public string Name { get; set; }
        public string UnmanagedType { get; set; }
        public string ManagedType { get; set; }
        public bool IsIndexField { get; set; }
        public string IndexName { get; set; }
        public int IndexPosition { get; set; }
        public bool NeedsRefField { get; set; }
        public string RefFieldName { get; set; }
        public bool NeedsConverter { get; set; }
        public string ConverterTypeName { get; set; }
        public string ConverterDomain { get; set; }
        public string SourceType { get; set; }
        public string TargetType { get; set; }
        public bool IsNotNull { get; set; }
        public string DefaultValueString { get; set; }
        /// <summary>[XMLLink] 组合链接：CfgS&lt;T&gt; 生成 FieldName_Dst、FieldName_Ref、FieldName 三字段。</summary>
        public bool IsXmlLink { get; set; }
        /// <summary>IsXmlLink 时，链接目标的 Unmanaged 类型名（如 TestConfigUnManaged）。</summary>
        public string XmlLinkDstUnmanagedType { get; set; }
    }

    /// <summary>索引组信息。</summary>
    public class IndexGroupInfo
    {
        public string IndexName { get; set; }
        public bool IsMultiValue { get; set; }
        public List<FieldInfo> Fields { get; set; } = new List<FieldInfo>();
    }

    /// <summary>配置类型信息。</summary>
    public class ConfigTypeInfo
    {
        public Type ManagedType { get; set; }
        public Type UnmanagedType { get; set; }
        public string Namespace { get; set; }
        public string ManagedTypeName { get; set; }
        public string UnmanagedTypeName { get; set; }
        /// <summary>表名：优先来自 [XmlDefined(xmlName)]，否则为类型名。用于生成 CfgS&lt;T&gt;.TableName 与 GetTblS()。</summary>
        public string TableName { get; set; }
        public List<FieldInfo> Fields { get; set; } = new List<FieldInfo>();
        public List<IndexGroupInfo> IndexGroups { get; set; } = new List<IndexGroupInfo>();
        public HashSet<string> RequiredUsings { get; set; } = new HashSet<string>();
        /// <summary>[XMLLink] 时，链接目标的 ClassHelper 类名（如 TestConfigClassHelper），用于生成 LinkHelperType = typeof(...)。取第一个 [XMLLink] 字段的 CfgS&lt;T&gt; 的 T.Name + "ClassHelper"。</summary>
        public string LinkHelperClassName { get; set; }
    }

    /// <summary>类型分析器，负责分析托管类型并映射到非托管类型。</summary>
    public static class TypeAnalyzer
    {
        private static readonly Dictionary<Type, ConfigTypeInfo> _configTypeInfoCache = new Dictionary<Type, ConfigTypeInfo>();

        public static void ClearConfigTypeInfoCache()
        {
            lock (_configTypeInfoCache) { _configTypeInfoCache.Clear(); }
        }

        public static ConfigTypeInfo AnalyzeConfigType(Type configType)
        {
            if (configType == null)
                throw new ArgumentNullException(nameof(configType));
            if (!IsXConfigType(configType))
                throw new ArgumentException($"类型 {configType.Name} 不是 XConfig 类型");

            lock (_configTypeInfoCache)
            {
                if (_configTypeInfoCache.TryGetValue(configType, out var cached))
                    return cached;
            }

            var xmlDefined = configType.GetCustomAttribute<XmlDefinedAttribute>();
            var info = new ConfigTypeInfo
            {
                ManagedType = configType,
                Namespace = configType.Namespace ?? string.Empty,
                ManagedTypeName = configType.Name,
                TableName = !string.IsNullOrEmpty(xmlDefined?.XmlName) ? xmlDefined.XmlName : configType.Name
            };

            var baseType = configType.BaseType;
            if (baseType != null && baseType.IsGenericType)
            {
                var genericArgs = baseType.GetGenericArguments();
                if (genericArgs.Length >= 2)
                {
                    info.UnmanagedType = genericArgs[1];
                    info.UnmanagedTypeName = info.UnmanagedType.Name;
                    ValidateUnmanagedTypeName(configType, info);
                }
            }
            if (info.UnmanagedType == null)
            {
                var ixConfig = GetIXConfigInterfaceForType(configType);
                if (ixConfig != null && ixConfig.IsGenericType && ixConfig.GetGenericArguments().Length >= 2)
                {
                    var genericArgs = ixConfig.GetGenericArguments();
                    info.UnmanagedType = genericArgs[1];
                    info.UnmanagedTypeName = info.UnmanagedType.Name;
                    ValidateUnmanagedTypeName(configType, info);
                }
            }

            AnalyzeFields(configType, info);
            AnalyzeIndexGroups(info);
            CollectRequiredUsings(info);

            lock (_configTypeInfoCache) { _configTypeInfoCache[configType] = info; }
            return info;
        }

        public static bool IsXConfigType(Type type)
        {
            var baseType = type.BaseType;
            while (baseType != null)
            {
                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition().Name == "XConfig`2")
                    return true;
                baseType = baseType.BaseType;
            }
            return GetIXConfigInterface(type) != null;
        }

        private static Type GetIXConfigInterface(Type type)
        {
            foreach (var iface in type.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition().Name == "IXConfig`2" && iface.GetGenericArguments().Length >= 2)
                    return iface;
            }
            return null;
        }

        private static Type GetIXConfigInterfaceForType(Type type)
        {
            foreach (var iface in type.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition().Name == "IXConfig`2" && iface.GetGenericArguments().Length >= 2)
                {
                    var args = iface.GetGenericArguments();
                    if (args[0] == type)
                        return iface;
                }
            }
            return GetIXConfigInterface(type);
        }

        private static void ValidateUnmanagedTypeName(Type configType, ConfigTypeInfo info)
        {
            var expectedUnmanagedName = info.ManagedTypeName + "UnManaged";
            var expectedUnmanagedName2 = info.ManagedTypeName + "Unmanaged";
            if (info.UnmanagedTypeName != expectedUnmanagedName && info.UnmanagedTypeName != expectedUnmanagedName2)
                throw new ArgumentException($"类型 {configType.Name} 的 Unmanaged 类型名称不符合约定。期望: {expectedUnmanagedName} 或 {expectedUnmanagedName2}，实际: {info.UnmanagedTypeName}");
        }

        private static void AnalyzeFields(Type configType, ConfigTypeInfo info)
        {
            var fields = configType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (var field in fields)
            {
                var fieldInfo = new FieldInfo { Name = field.Name, ManagedType = GetTypeName(field.FieldType) };

                var indexAttr = field.GetCustomAttribute<XmlIndexAttribute>();
                if (indexAttr != null)
                {
                    fieldInfo.IsIndexField = true;
                    fieldInfo.IndexName = indexAttr.IndexName;
                    fieldInfo.IndexPosition = indexAttr.Position;
                }

                var converterAttr = field.GetCustomAttribute<XmlTypeConverterAttribute>();
                if (converterAttr != null && converterAttr.ConverterType != null)
                {
                    var converterBaseType = converterAttr.ConverterType.BaseType;
                    if (converterBaseType != null && converterBaseType.IsGenericType)
                    {
                        var genericArgs = converterBaseType.GetGenericArguments();
                        if (genericArgs.Length >= 2)
                        {
                            fieldInfo.NeedsConverter = true;
                            fieldInfo.ConverterTypeName = converterAttr.ConverterType.FullName ?? converterAttr.ConverterType.Name;
                            fieldInfo.ConverterDomain = converterAttr.Domain ?? "";
                            fieldInfo.SourceType = GetTypeName(genericArgs[0]);
                            fieldInfo.TargetType = GetTypeName(genericArgs[1]);
                            fieldInfo.UnmanagedType = fieldInfo.SourceType;
                            if (!string.IsNullOrEmpty(converterAttr.ConverterType.Namespace))
                                info.RequiredUsings.Add(converterAttr.ConverterType.Namespace);
                        }
                    }
                }

                if (!fieldInfo.NeedsConverter)
                {
                    var globalAttr = field.GetCustomAttribute<XmlGlobalConvertAttribute>();
                    if (globalAttr != null && globalAttr.ConverterType != null)
                    {
                        fieldInfo.NeedsConverter = true;
                        fieldInfo.ConverterTypeName = globalAttr.ConverterType.FullName ?? globalAttr.ConverterType.Name;
                        fieldInfo.ConverterDomain = globalAttr.Domain ?? "";
                        fieldInfo.SourceType = GetTypeName(typeof(string));
                        fieldInfo.TargetType = GetTypeName(field.FieldType);
                        fieldInfo.UnmanagedType = fieldInfo.TargetType;
                        if (!string.IsNullOrEmpty(globalAttr.ConverterType.Namespace))
                            info.RequiredUsings.Add(globalAttr.ConverterType.Namespace);
                    }
                }

                if (!fieldInfo.NeedsConverter)
                {
                    var assemblyDomain = GetConverterDomainForType(info.ManagedType.Assembly, field.FieldType);
                    if (!string.IsNullOrEmpty(assemblyDomain))
                    {
                        fieldInfo.NeedsConverter = true;
                        fieldInfo.ConverterDomain = assemblyDomain;
                        fieldInfo.SourceType = GetTypeName(typeof(string));
                        fieldInfo.TargetType = GetTypeName(field.FieldType);
                        fieldInfo.UnmanagedType = fieldInfo.TargetType;
                    }
                }

                if (!fieldInfo.NeedsConverter)
                    fieldInfo.UnmanagedType = MapToUnmanagedType(field.FieldType, field, info);

                if (IsConfigKeyType(field.FieldType))
                {
                    var xmlLinkAttr = field.GetCustomAttribute<XMLLinkAttribute>();
                    if (xmlLinkAttr != null)
                    {
                        fieldInfo.IsXmlLink = true;
                        var linkedType = field.FieldType.GetGenericArguments()[0];
                        fieldInfo.XmlLinkDstUnmanagedType = GetUnmanagedTypeNameForConfig(linkedType);
                        fieldInfo.NeedsRefField = false;
                        if (string.IsNullOrEmpty(info.LinkHelperClassName))
                            info.LinkHelperClassName = linkedType.Name + "ClassHelper";
                    }
                    else
                    {
                        fieldInfo.NeedsRefField = true;
                        fieldInfo.RefFieldName = field.Name + "_Ref";
                    }
                }

                var notNullAttr = field.GetCustomAttribute<XmlNotNullAttribute>();
                if (notNullAttr != null) fieldInfo.IsNotNull = true;
                var defaultAttr = field.GetCustomAttribute<XmlDefaultAttribute>();
                if (defaultAttr != null)
                {
                    if (!IsContainerType(field.FieldType))
                        fieldInfo.DefaultValueString = defaultAttr.Value ?? "";
                    else
                        UnityEngine.Debug.LogWarning($"[XmlDefault] 容器类型暂不支持默认值，已忽略: {configType.Name}.{field.Name}");
                }

                info.Fields.Add(fieldInfo);
            }
        }

        private static string MapToUnmanagedType(Type managedType, System.Reflection.FieldInfo fieldInfo, ConfigTypeInfo configInfo)
        {
            if (managedType.IsPrimitive || managedType == typeof(decimal))
                return GetTypeName(managedType);

            if (managedType == typeof(string))
            {
                string targetTypeName = null;
                if (fieldInfo != null)
                {
                    var strModeAttr = fieldInfo.GetCustomAttribute<XmlStringModeAttribute>();
                    if (strModeAttr != null)
                    {
                        var strModeField = typeof(XmlStringModeAttribute).GetField("StrMode", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (strModeField != null)
                        {
                            var strMode = (EXmlStrMode)strModeField.GetValue(strModeAttr);
                            switch (strMode)
                            {
                                case EXmlStrMode.EFix32: targetTypeName = "FixedString32Bytes"; break;
                                case EXmlStrMode.EFix64: targetTypeName = "FixedString64Bytes"; break;
                                case EXmlStrMode.EStrI: targetTypeName = "StrI"; break;
                                case EXmlStrMode.ELabelI: targetTypeName = "LabelI"; break;
                            }
                        }
                    }
                }
                if (string.IsNullOrEmpty(targetTypeName)) targetTypeName = "StrI";
                var targetType = FindTypeByName(targetTypeName);
                if (targetType != null && !string.IsNullOrEmpty(targetType.Namespace))
                    configInfo.RequiredUsings.Add(targetType.Namespace);
                return targetTypeName;
            }

            if (managedType.IsGenericType && managedType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elementType = managedType.GetGenericArguments()[0];
                return $"XBlobArray<{MapToUnmanagedType(elementType, null, configInfo)}>";
            }
            if (managedType.IsGenericType && managedType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var keyType = managedType.GetGenericArguments()[0];
                var valueType = managedType.GetGenericArguments()[1];
                return $"XBlobMap<{MapToUnmanagedType(keyType, null, configInfo)}, {MapToUnmanagedType(valueType, null, configInfo)}>";
            }
            if (managedType.IsGenericType && managedType.GetGenericTypeDefinition() == typeof(HashSet<>))
            {
                var elementType = managedType.GetGenericArguments()[0];
                return $"XBlobSet<{MapToUnmanagedType(elementType, null, configInfo)}>";
            }
            if (IsConfigKeyType(managedType))
            {
                var elementType = managedType.GetGenericArguments()[0];
                return $"CfgI<{GetTypeName(elementType)}>";
            }
            if (managedType.Name == "LabelS")
            {
                var targetType = FindTypeByName("LabelI");
                if (targetType != null && !string.IsNullOrEmpty(targetType.Namespace))
                    configInfo.RequiredUsings.Add(targetType.Namespace);
                return "LabelI";
            }
            if (managedType == typeof(Type))
            {
                var targetType = FindTypeByName("TypeI");
                if (targetType != null && !string.IsNullOrEmpty(targetType.Namespace))
                    configInfo.RequiredUsings.Add(targetType.Namespace);
                return "TypeI";
            }
            if (IsXConfigType(managedType))
            {
                Type unmanagedTypeFromGeneric = null;
                var baseType = managedType.BaseType;
                if (baseType != null && baseType.IsGenericType && baseType.GetGenericArguments().Length >= 2)
                    unmanagedTypeFromGeneric = baseType.GetGenericArguments()[1];
                if (unmanagedTypeFromGeneric == null)
                {
                    var ixConfig = GetIXConfigInterface(managedType);
                    if (ixConfig != null && ixConfig.GetGenericArguments().Length >= 2)
                        unmanagedTypeFromGeneric = ixConfig.GetGenericArguments()[1];
                }
                if (unmanagedTypeFromGeneric != null)
                {
                    var expectedUnmanagedName = managedType.Name + "UnManaged";
                    var assembly = managedType.Assembly;
                    try
                    {
                        string fullName = string.IsNullOrEmpty(managedType.Namespace) ? expectedUnmanagedName : managedType.Namespace + "." + expectedUnmanagedName;
                        var expectedUnmanagedType = assembly.GetType(fullName, false, false);
                        if (expectedUnmanagedType != null)
                            return GetTypeName(expectedUnmanagedType);
                    }
                    catch { }
                    return GetTypeName(unmanagedTypeFromGeneric);
                }
            }
            if (!string.IsNullOrEmpty(managedType.Namespace) && managedType.IsValueType && !managedType.IsEnum)
            {
                configInfo.RequiredUsings.Add(managedType.Namespace);
                return GetTypeName(managedType);
            }
            return GetTypeName(managedType);
        }

        private static bool IsConfigKeyType(Type type)
            => type.IsGenericType && type.GetGenericTypeDefinition().Name == "CfgS`1";

        /// <summary>获取 XConfig 类型对应的 Unmanaged 类型名（如 TestConfig -> TestConfigUnManaged）。CfgS 泛参可为托管或非托管类型。</summary>
        private static string GetUnmanagedTypeNameForConfig(Type linkedType)
        {
            if (linkedType == null) return "UnknownUnManaged";
            if (linkedType.Name.EndsWith("UnManaged", StringComparison.Ordinal) || linkedType.Name.EndsWith("Unmanaged", StringComparison.Ordinal))
                return linkedType.Name;
            if (!IsXConfigType(linkedType)) return linkedType.Name + "UnManaged";
            var ixConfig = GetIXConfigInterface(linkedType);
            if (ixConfig != null && ixConfig.GetGenericArguments().Length >= 2)
                return ixConfig.GetGenericArguments()[1].Name;
            var baseType = linkedType.BaseType;
            if (baseType != null && baseType.IsGenericType && baseType.GetGenericArguments().Length >= 2)
                return baseType.GetGenericArguments()[1].Name;
            return linkedType.Name + "UnManaged";
        }

        private static bool IsContainerType(Type type)
        {
            if (type == null || !type.IsGenericType) return false;
            var def = type.GetGenericTypeDefinition();
            return def == typeof(List<>) || def == typeof(Dictionary<,>) || def == typeof(HashSet<>);
        }

        private static readonly Dictionary<Assembly, Dictionary<Type, string>> ConverterTargetCache = new Dictionary<Assembly, Dictionary<Type, string>>();
        
        /// <summary>缓存：程序集 -> (源类型 -> (目标类型, 域))</summary>
        private static readonly Dictionary<Assembly, Dictionary<Type, (Type targetType, string domain)>> ConverterSourceTargetCache = 
            new Dictionary<Assembly, Dictionary<Type, (Type, string)>>();

        public static Type GetTargetTypeFromConverterType(Type converterType)
        {
            if (converterType == null) return null;
            foreach (var i in converterType.GetInterfaces())
            {
                if (!i.IsGenericType || i.GetGenericArguments().Length < 2) continue;
                var def = i.GetGenericTypeDefinition();
                if (def.Name != "ITypeConverter`2") continue;
                var args = i.GetGenericArguments();
                if (args[0] == typeof(string)) return args[1];
            }
            return null;
        }

        /// <summary>从转换器类型获取源类型和目标类型对</summary>
        public static (Type sourceType, Type targetType) GetConverterTypePair(Type converterType)
        {
            if (converterType == null) return (null, null);
            foreach (var i in converterType.GetInterfaces())
            {
                if (!i.IsGenericType || i.GetGenericArguments().Length < 2) continue;
                var def = i.GetGenericTypeDefinition();
                if (def.Name != "ITypeConverter`2") continue;
                var args = i.GetGenericArguments();
                return (args[0], args[1]);
            }
            return (null, null);
        }

        public static string GetConverterDomainForType(Assembly assembly, Type targetType)
        {
            if (assembly == null || targetType == null) return null;
            if (!ConverterTargetCache.TryGetValue(assembly, out var dict))
            {
                dict = BuildConverterTargetMap(assembly);
                ConverterTargetCache[assembly] = dict;
            }
            return dict.TryGetValue(targetType, out var domain) ? domain : null;
        }

        /// <summary>根据源类型获取目标类型和域</summary>
        public static bool TryGetConverterTargetBySourceType(Assembly assembly, Type sourceType, out Type targetType, out string domain)
        {
            targetType = null;
            domain = null;
            
            if (assembly == null || sourceType == null) return false;
            
            // 确保缓存已构建
            if (!ConverterTargetCache.ContainsKey(assembly))
            {
                BuildConverterTargetMap(assembly);
            }
            
            // 从源类型->目标类型缓存中查询
            if (ConverterSourceTargetCache.TryGetValue(assembly, out var sourceTargetDict))
            {
                if (sourceTargetDict.TryGetValue(sourceType, out var result))
                {
                    targetType = result.targetType;
                    domain = result.domain;
                    return true;
                }
            }
            
            return false;
        }

        private static Dictionary<Type, string> BuildConverterTargetMap(Assembly asm)
        {
            var dict = new Dictionary<Type, string>();
            var sourceTargetDict = new Dictionary<Type, (Type, string)>();
            
            try
            {
                foreach (var attr in asm.GetCustomAttributes(typeof(XmlGlobalConvertAttribute), false))
                {
                    if (!(attr is XmlGlobalConvertAttribute ga) || ga.ConverterType == null) continue;
                    
                    // 获取转换器的源类型和目标类型对
                    var (sourceType, targetType) = GetConverterTypePair(ga.ConverterType);
                    UnityEngine.Debug.Log($"[UnityToolkit.TypeAnalyzer] 程序集 {asm.GetName().Name}: 转换器 {ga.ConverterType.Name}, 源={sourceType?.Name}, 目标={targetType?.Name}");
                    
                    if (targetType != null)
                    {
                        var domain = ga.Domain ?? "";
                        
                        // 如果源类型是 string，添加到原有缓存（向后兼容）
                        if (sourceType == typeof(string) && !dict.ContainsKey(targetType))
                        {
                            dict[targetType] = domain;
                        }
                        
                        // 同时添加到源类型->目标类型缓存（支持所有类型对）
                        if (sourceType != null && !sourceTargetDict.ContainsKey(sourceType))
                        {
                            sourceTargetDict[sourceType] = (targetType, domain);
                            UnityEngine.Debug.Log($"[UnityToolkit.TypeAnalyzer] 注册转换器: {sourceType.Name} -> {targetType.Name}, 域={domain}");
                        }
                    }
                }
            }
            catch { }
            
            try
            {
                foreach (var type in asm.GetTypes())
                {
                    try
                    {
                        if (!IsXConfigType(type)) continue;
                        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                        {
                            var ga = field.GetCustomAttribute<XmlGlobalConvertAttribute>();
                            var ta = field.GetCustomAttribute<XmlTypeConverterAttribute>();
                            Type targetType = null;
                            Type sourceType = null;
                            string domain = null;
                            
                            if (ga != null && ga.ConverterType != null) 
                            { 
                                targetType = field.FieldType; 
                                sourceType = typeof(string);
                                domain = ga.Domain ?? ""; 
                            }
                            else if (ta != null && ta.ConverterType != null) 
                            { 
                                targetType = GetTargetTypeFromConverterType(ta.ConverterType) ?? field.FieldType; 
                                sourceType = typeof(string);
                                domain = ta.Domain ?? ""; 
                            }
                            
                            if (targetType != null && !dict.ContainsKey(targetType))
                                dict[targetType] = domain ?? "";
                            
                            if (sourceType != null && targetType != null && !sourceTargetDict.ContainsKey(sourceType))
                                sourceTargetDict[sourceType] = (targetType, domain ?? "");
                        }
                    }
                    catch { }
                }
            }
            catch (ReflectionTypeLoadException) { }
            
            // 保存源类型->目标类型缓存
            lock (ConverterSourceTargetCache)
            {
                ConverterSourceTargetCache[asm] = sourceTargetDict;
            }
            
            return dict;
        }

        private static string GetTypeName(Type type)
        {
            if (type.IsGenericType)
            {
                var name = type.Name.Split('`')[0];
                var args = type.GetGenericArguments().Select(GetTypeName);
                return $"{name}<{string.Join(", ", args)}>";
            }
            return type.Name;
        }

        private static void AnalyzeIndexGroups(ConfigTypeInfo info)
        {
            var indexGroups = new Dictionary<string, IndexGroupInfo>();
            foreach (var field in info.Fields)
            {
                if (field.IsIndexField)
                {
                    if (!indexGroups.ContainsKey(field.IndexName))
                        indexGroups[field.IndexName] = new IndexGroupInfo { IndexName = field.IndexName, IsMultiValue = false };
                    indexGroups[field.IndexName].Fields.Add(field);
                }
            }
            foreach (var kvp in indexGroups)
            {
                var group = kvp.Value;
                group.Fields = group.Fields.OrderBy(f => f.IndexPosition).ToList();
                var firstField = info.ManagedType.GetField(group.Fields[0].Name);
                if (firstField != null)
                {
                    var indexAttr = firstField.GetCustomAttribute<XmlIndexAttribute>();
                    if (indexAttr != null) group.IsMultiValue = indexAttr.IsMultiValue;
                }
                info.IndexGroups.Add(group);
            }
        }

        private static void CollectRequiredUsings(ConfigTypeInfo info)
        {
            info.RequiredUsings.Add("System");
            info.RequiredUsings.Add("System.Collections.Generic");
            info.RequiredUsings.Add("System.Xml");
            info.RequiredUsings.Add("XM");
            info.RequiredUsings.Add("XM.Contracts");
            info.RequiredUsings.Add("XM.Contracts.Config");
            foreach (var f in info.Fields) { if (f.NeedsConverter) { info.RequiredUsings.Add("XM.Utils"); break; } }
            if (info.ManagedType != null)
            {
                foreach (var field in info.ManagedType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (field.Name == "Data") continue;
                    CollectUsingsFromType(field.FieldType, info);
                }
            }
            foreach (var field in info.Fields)
            {
                CollectUsingsFromTypeString(field.UnmanagedType, info.ManagedType.Assembly, info);
                if (field.NeedsRefField) CollectUsingsFromTypeString("XBlobPtr", info.ManagedType.Assembly, info);
            }
        }

        private static void CollectUsingsFromType(Type type, ConfigTypeInfo configInfo)
        {
            if (type == null) return;
            if (!string.IsNullOrEmpty(type.Namespace)) configInfo.RequiredUsings.Add(type.Namespace);
            if (type.IsGenericType)
            {
                foreach (var arg in type.GetGenericArguments())
                    CollectUsingsFromType(arg, configInfo);
            }
        }

        private static void CollectUsingsFromTypeString(string typeString, Assembly assembly, ConfigTypeInfo configInfo)
        {
            if (string.IsNullOrEmpty(typeString)) return;
            string typeName = typeString;
            int genericIndex = typeName.IndexOf('<');
            if (genericIndex > 0) typeName = typeName.Substring(0, genericIndex);
            try
            {
                var type = assembly.GetType(typeName, false, false);
                if (type == null)
                {
                    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        try
                        {
                            type = asm.GetType(typeName, false, false);
                            if (type != null) break;
                        }
                        catch { }
                    }
                }
                if (type != null && !string.IsNullOrEmpty(type.Namespace))
                    configInfo.RequiredUsings.Add(type.Namespace);
            }
            catch { }
            if (typeString.Contains("<"))
            {
                int startIndex = typeString.IndexOf('<') + 1;
                int endIndex = typeString.LastIndexOf('>');
                if (endIndex > startIndex)
                {
                    string genericArgs = typeString.Substring(startIndex, endIndex - startIndex);
                    var args = SplitGenericArguments(genericArgs);
                    foreach (var arg in args)
                        CollectUsingsFromTypeString(arg.Trim(), assembly, configInfo);
                }
            }
        }

        private static List<string> SplitGenericArguments(string genericArgs)
        {
            var result = new List<string>();
            int depth = 0, start = 0;
            for (int i = 0; i < genericArgs.Length; i++)
            {
                char c = genericArgs[i];
                if (c == '<') depth++;
                else if (c == '>') depth--;
                else if (c == ',' && depth == 0) { result.Add(genericArgs.Substring(start, i - start)); start = i + 1; }
            }
            if (start < genericArgs.Length) result.Add(genericArgs.Substring(start));
            return result;
        }

        private static Type FindTypeByName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var type = assembly.GetType(typeName, false, false);
                    if (type != null) return type;
                    foreach (var t in assembly.GetTypes())
                    {
                        if (t.Name == typeName || t.FullName == typeName) return t;
                    }
                }
                catch { }
            }
            return null;
        }
    }
}
