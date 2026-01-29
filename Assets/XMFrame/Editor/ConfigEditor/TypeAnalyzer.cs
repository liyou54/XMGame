using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using XM;
using XM.Utils.Attribute;

namespace XM.Editor
{
    /// <summary>
    /// 字段信息，用于代码生成
    /// </summary>
    public class FieldInfo
    {
        public string Name { get; set; }
        public string UnmanagedType { get; set; }
        public string ManagedType { get; set; }
        public bool IsIndexField { get; set; }
        public string IndexName { get; set; }
        public int IndexPosition { get; set; }
        public bool NeedsRefField { get; set; } // 是否需要生成 _Ref 字段
        public string RefFieldName { get; set; }
        public bool NeedsConverter { get; set; } // 是否需要类型转换
        public string ConverterTypeName { get; set; } // 转换器类型完整名称（包括命名空间）
        public string ConverterDomain { get; set; } // 转换器域（空字符串表示全局）
        public string SourceType { get; set; } // 源类型（托管类型）
        public string TargetType { get; set; } // 目标类型（非托管类型）
        /// <summary>是否必要字段（[XmlNotNull]）：缺失时打告警。</summary>
        public bool IsNotNull { get; set; }
        /// <summary>默认值字符串（[XmlDefault(str)]）：标量字段在 XML 缺失或空时使用；容器暂不支持。</summary>
        public string DefaultValueString { get; set; }
    }

    /// <summary>
    /// 索引组信息
    /// </summary>
    public class IndexGroupInfo
    {
        public string IndexName { get; set; }
        public bool IsMultiValue { get; set; }
        public List<FieldInfo> Fields { get; set; } = new List<FieldInfo>();
    }

    /// <summary>
    /// 配置类型信息
    /// </summary>
    public class ConfigTypeInfo
    {
        public Type ManagedType { get; set; }
        public Type UnmanagedType { get; set; }
        public string Namespace { get; set; }
        public string ManagedTypeName { get; set; }
        public string UnmanagedTypeName { get; set; }
        public List<FieldInfo> Fields { get; set; } = new List<FieldInfo>();
        public List<IndexGroupInfo> IndexGroups { get; set; } = new List<IndexGroupInfo>();
        public HashSet<string> RequiredUsings { get; set; } = new HashSet<string>();
        /// <summary>是否有 XConfig 基类（继承）</summary>
        public bool HasBase { get; set; }
        /// <summary>基类托管类型名（如 TestConfig）</summary>
        public string BaseManagedTypeName { get; set; }
        /// <summary>基类非托管类型名（如 TestConfigUnManaged）</summary>
        public string BaseUnmanagedTypeName { get; set; }
    }

    /// <summary>
    /// 类型分析器，负责分析托管类型并映射到非托管类型。分析结果按类型缓存以加快批量生成。
    /// </summary>
    public static class TypeAnalyzer
    {
        private static readonly Dictionary<Type, ConfigTypeInfo> _configTypeInfoCache = new Dictionary<Type, ConfigTypeInfo>();

        /// <summary>
        /// 清除类型分析缓存（程序集重载或类型变更后可选调用）。
        /// </summary>
        public static void ClearConfigTypeInfoCache()
        {
            lock (_configTypeInfoCache) { _configTypeInfoCache.Clear(); }
        }

        /// <summary>
        /// 分析配置类型（带缓存，同一类型重复分析直接返回缓存）。
        /// </summary>
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

            var info = new ConfigTypeInfo
            {
                ManagedType = configType,
                Namespace = configType.Namespace ?? string.Empty,
                ManagedTypeName = configType.Name
            };

            // 获取泛型参数 TUnmanaged：优先从基类泛型，否则从实现的 IXConfig<T,TUnmanaged> 接口
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
                // 优先取“当前类型自己的” IXConfig<T, TUnmanaged>（第一个泛参为当前类型），避免派生类拿到基类的 TUnmanaged
                var ixConfig = GetIXConfigInterfaceForType(configType);
                if (ixConfig != null && ixConfig.IsGenericType && ixConfig.GetGenericArguments().Length >= 2)
                {
                    var genericArgs = ixConfig.GetGenericArguments();
                    info.UnmanagedType = genericArgs[1];
                    info.UnmanagedTypeName = info.UnmanagedType.Name;
                    ValidateUnmanagedTypeName(configType, info);
                }
            }

            // 继承：若直接基类是 XConfig 类型，则记录基类信息
            if (baseType != null && baseType != typeof(object) && IsXConfigType(baseType))
            {
                info.HasBase = true;
                info.BaseManagedTypeName = baseType.Name;
                var baseIx = GetIXConfigInterface(baseType);
                if (baseIx != null && baseIx.GetGenericArguments().Length >= 2)
                {
                    info.BaseUnmanagedTypeName = baseIx.GetGenericArguments()[1].Name;
                }
            }

            // 分析字段
            AnalyzeFields(configType, info);

            // 分析索引组
            AnalyzeIndexGroups(info);

            // 收集所需的 using 语句
            CollectRequiredUsings(info);

            lock (_configTypeInfoCache) { _configTypeInfoCache[configType] = info; }
            return info;
        }

        /// <summary>
        /// 检查是否是 XConfig 类型（基类 XConfig`2 或实现 IXConfig&lt;T,TUnmanaged&gt;）
        /// </summary>
        private static bool IsXConfigType(Type type)
        {
            var baseType = type.BaseType;
            while (baseType != null)
            {
                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition().Name == "XConfig`2")
                {
                    return true;
                }
                baseType = baseType.BaseType;
            }
            return GetIXConfigInterface(type) != null;
        }

        /// <summary>
        /// 获取类型实现的 IXConfig&lt;T,TUnmanaged&gt; 接口（含从基类继承）
        /// </summary>
        private static Type GetIXConfigInterface(Type type)
        {
            foreach (var iface in type.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition().Name == "IXConfig`2" && iface.GetGenericArguments().Length >= 2)
                {
                    return iface;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取“当前类型自己的” IXConfig&lt;T,TUnmanaged&gt;，即第一个泛型参数 T 等于 type 的接口（派生类用此得到自己的 TUnmanaged，而非基类的）
        /// </summary>
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
            if (info.UnmanagedTypeName != expectedUnmanagedName &&
                info.UnmanagedTypeName != expectedUnmanagedName2)
            {
                throw new ArgumentException(
                    $"类型 {configType.Name} 的 Unmanaged 类型名称不符合约定。" +
                    $"期望: {expectedUnmanagedName} 或 {expectedUnmanagedName2}，" +
                    $"实际: {info.UnmanagedTypeName}");
            }
        }

        /// <summary>
        /// 分析字段
        /// </summary>
        private static void AnalyzeFields(Type configType, ConfigTypeInfo info)
        {
            var fields = configType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var field in fields)
            {
                var fieldInfo = new FieldInfo
                {
                    Name = field.Name,
                    ManagedType = GetTypeName(field.FieldType)
                };

                // 检查 XmlIndex 属性
                var indexAttr = field.GetCustomAttribute<XmlIndexAttribute>();
                if (indexAttr != null)
                {
                    fieldInfo.IsIndexField = true;
                    fieldInfo.IndexName = indexAttr.IndexName;
                    fieldInfo.IndexPosition = indexAttr.Position;
                }

                // 检查 XmlTypeConverter 属性
                var converterAttr = field.GetCustomAttribute<XmlTypeConverterAttribute>();
                if (converterAttr != null && converterAttr.ConverterType != null)
                {
                    // 从转换器类型中提取源类型和目标类型
                    var converterBaseType = converterAttr.ConverterType.BaseType;
                    if (converterBaseType != null && converterBaseType.IsGenericType)
                    {
                        var genericArgs = converterBaseType.GetGenericArguments();
                        if (genericArgs.Length >= 2)
                        {
                            fieldInfo.NeedsConverter = true;
                            // 保存转换器类型的完整名称（包括命名空间）
                            fieldInfo.ConverterTypeName = converterAttr.ConverterType.FullName ?? converterAttr.ConverterType.Name;
                            fieldInfo.ConverterDomain = converterAttr.Domain ?? "";
                            fieldInfo.SourceType = GetTypeName(genericArgs[0]);
                            fieldInfo.TargetType = GetTypeName(genericArgs[1]);
                            
                            // 如果字段标记了转换器，非托管类型使用源类型（存储源类型，通过转换方法转换为目标类型）
                            fieldInfo.UnmanagedType = fieldInfo.SourceType;
                            
                            // 添加转换器类型的命名空间到 RequiredUsings
                            if (!string.IsNullOrEmpty(converterAttr.ConverterType.Namespace))
                            {
                                info.RequiredUsings.Add(converterAttr.ConverterType.Namespace);
                            }
                        }
                    }
                }

                // 检查 XmlGlobalConvert 属性（未设置时）：从 IConfigDataCenter 按域获取转换器，XML 源类型为 string
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

                // 无字段级转换器时：若字段类型在程序集级注册了转换器（如 [assembly: XmlGlobalConvert(..., "global")]），则按域使用
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
                {
                    // 映射到非托管类型
                    fieldInfo.UnmanagedType = MapToUnmanagedType(field.FieldType, field, info);
                }

                // 检查是否需要生成 _Ref 字段（CfgS 类型）
                if (IsConfigKeyType(field.FieldType))
                {
                    fieldInfo.NeedsRefField = true;
                    fieldInfo.RefFieldName = field.Name + "_Ref";
                }

                // 必要字段与默认值（容器暂不参与默认值逻辑）
                var notNullAttr = field.GetCustomAttribute<XmlNotNullAttribute>();
                if (notNullAttr != null)
                    fieldInfo.IsNotNull = true;
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

        /// <summary>
        /// 映射到非托管类型
        /// </summary>
        private static string MapToUnmanagedType(Type managedType, System.Reflection.FieldInfo fieldInfo, ConfigTypeInfo configInfo)
        {
            // 基本类型保持不变
            if (managedType.IsPrimitive || managedType == typeof(decimal))
            {
                return GetTypeName(managedType);
            }

            // string 类型 - 根据 XmlStringMode 属性映射
            if (managedType == typeof(string))
            {
                Type targetType = null;
                string targetTypeName = null;
                
                if (fieldInfo != null)
                {
                    var strModeAttr = fieldInfo.GetCustomAttribute<XmlStringModeAttribute>();
                    if (strModeAttr != null)
                    {
                        // 需要通过反射获取 StrMode 值
                        var strModeField = typeof(XmlStringModeAttribute).GetField("StrMode", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (strModeField != null)
                        {
                            var strMode = (EXmlStrMode)strModeField.GetValue(strModeAttr);
                            switch (strMode)
                            {
                                case EXmlStrMode.EFix32:
                                    targetTypeName = "FixedString32Bytes";
                                    break;
                                case EXmlStrMode.EFix64:
                                    targetTypeName = "FixedString64Bytes";
                                    break;
                                case EXmlStrMode.EStrI:
                                    targetTypeName = "StrI";
                                    break;
                                case EXmlStrMode.ELabelI:
                                    targetTypeName = "LabelI";
                                    break;
                            }
                        }
                    }
                }
                
                // 默认使用 StrI
                if (string.IsNullOrEmpty(targetTypeName))
                {
                    targetTypeName = "StrI";
                }
                
                // 通过反射查找目标类型并添加命名空间
                targetType = FindTypeByName(targetTypeName);
                if (targetType != null && !string.IsNullOrEmpty(targetType.Namespace))
                {
                    configInfo.RequiredUsings.Add(targetType.Namespace);
                }
                
                return targetTypeName;
            }

            // List<T> -> XBlobArray<T>
            if (managedType.IsGenericType && managedType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elementType = managedType.GetGenericArguments()[0];
                var unmanagedElementType = MapToUnmanagedType(elementType, null, configInfo);
                // XBlobArray 在全局命名空间中，不需要添加 using
                return $"XBlobArray<{unmanagedElementType}>";
            }

            // Dictionary<K, V> -> XBlobMap<K, V>
            if (managedType.IsGenericType && managedType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var keyType = managedType.GetGenericArguments()[0];
                var valueType = managedType.GetGenericArguments()[1];
                var unmanagedKeyType = MapToUnmanagedType(keyType, null, configInfo);
                var unmanagedValueType = MapToUnmanagedType(valueType, null, configInfo);
                // XBlobMap 在全局命名空间中，不需要添加 using
                return $"XBlobMap<{unmanagedKeyType}, {unmanagedValueType}>";
            }

            // HashSet<T> -> XBlobSet<T>
            if (managedType.IsGenericType && managedType.GetGenericTypeDefinition() == typeof(HashSet<>))
            {
                var elementType = managedType.GetGenericArguments()[0];
                var unmanagedElementType = MapToUnmanagedType(elementType, null, configInfo);
                // XBlobSet 在全局命名空间中，不需要添加 using
                return $"XBlobSet<{unmanagedElementType}>";
            }

            // CfgS<T> -> CfgI<T>
            if (IsConfigKeyType(managedType))
            {
                var elementType = managedType.GetGenericArguments()[0];
                // CfgI 在全局命名空间中，不需要添加 using
                return $"CfgI<{GetTypeName(elementType)}>";
            }

            // LabelS -> LabelI
            if (managedType.Name == "LabelS")
            {
                var targetType = FindTypeByName("LabelI");
                if (targetType != null && !string.IsNullOrEmpty(targetType.Namespace))
                {
                    configInfo.RequiredUsings.Add(targetType.Namespace);
                }
                return "LabelI";
            }

            // Type -> TypeI (全局映射)
            if (managedType == typeof(Type))
            {
                var targetType = FindTypeByName("TypeI");
                if (targetType != null && !string.IsNullOrEmpty(targetType.Namespace))
                {
                    configInfo.RequiredUsings.Add(targetType.Namespace);
                }
                return "TypeI";
            }

            // 嵌套 XConfig（NestedConfig 等）-> 对应 UnManaged 结构体（NestedConfigUnManaged）
            if (IsXConfigType(managedType))
            {
                Type unmanagedTypeFromGeneric = null;
                var baseType = managedType.BaseType;
                if (baseType != null && baseType.IsGenericType)
                {
                    var genericArgs = baseType.GetGenericArguments();
                    if (genericArgs.Length >= 2)
                    {
                        unmanagedTypeFromGeneric = genericArgs[1];
                    }
                }
                if (unmanagedTypeFromGeneric == null)
                {
                    var ixConfig = GetIXConfigInterface(managedType);
                    if (ixConfig != null && ixConfig.GetGenericArguments().Length >= 2)
                    {
                        unmanagedTypeFromGeneric = ixConfig.GetGenericArguments()[1];
                    }
                }
                if (unmanagedTypeFromGeneric != null)
                {
                    // 优先尝试查找 {TypeName}UnManaged 类型
                    var expectedUnmanagedName = managedType.Name + "UnManaged";
                    var assembly = managedType.Assembly;
                    try
                    {
                        string fullName = string.IsNullOrEmpty(managedType.Namespace)
                            ? expectedUnmanagedName
                            : managedType.Namespace + "." + expectedUnmanagedName;
                        var expectedUnmanagedType = assembly.GetType(fullName, false, false);
                        if (expectedUnmanagedType != null)
                        {
                            return GetTypeName(expectedUnmanagedType);
                        }
                    }
                    catch
                    {
                        // 如果查找失败，使用泛型参数中的类型
                    }
                    return GetTypeName(unmanagedTypeFromGeneric);
                }
            }

            // 对于有命名空间且是值类型的类型（作为兜底处理，如 Unity.Mathematics 类型）
            if (!string.IsNullOrEmpty(managedType.Namespace) && managedType.IsValueType && !managedType.IsEnum)
            {
                // 自动添加该类型的命名空间到 RequiredUsings
                configInfo.RequiredUsings.Add(managedType.Namespace);
                return GetTypeName(managedType);
            }

            // 其他类型直接使用名称
            return GetTypeName(managedType);
        }

        /// <summary>
        /// 检查是否是 CfgS 类型
        /// </summary>
        private static bool IsConfigKeyType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition().Name == "CfgS`1";
        }

        /// <summary>
        /// 检查是否是容器类型（List/Dictionary/HashSet），容器暂不参与 [XmlDefault] 默认值逻辑。
        /// </summary>
        private static bool IsContainerType(Type type)
        {
            if (type == null || !type.IsGenericType) return false;
            var def = type.GetGenericTypeDefinition();
            return def == typeof(System.Collections.Generic.List<>) ||
                   def == typeof(System.Collections.Generic.Dictionary<,>) ||
                   def == typeof(System.Collections.Generic.HashSet<>);
        }

        private static readonly Dictionary<Assembly, Dictionary<Type, string>> ConverterTargetCache =
            new Dictionary<Assembly, Dictionary<Type, string>>();

        /// <summary>
        /// 从转换器类型（实现 ITypeConverter&lt;string, T&gt;）获取目标类型 T。
        /// </summary>
        public static Type GetTargetTypeFromConverterType(Type converterType)
        {
            if (converterType == null) return null;
            foreach (var i in converterType.GetInterfaces())
            {
                if (!i.IsGenericType || i.GetGenericArguments().Length < 2) continue;
                var def = i.GetGenericTypeDefinition();
                if (def.Name != "ITypeConverter`2") continue;
                var args = i.GetGenericArguments();
                if (args[0] == typeof(string))
                    return args[1];
            }
            return null;
        }

        /// <summary>
        /// 获取程序集内“已注册转换器”的目标类型及其域：容器元素类型若在此表中，则统一使用 IConfigDataCenter.GetConverter 解析。
        /// </summary>
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

        private static Dictionary<Type, string> BuildConverterTargetMap(Assembly asm)
        {
            var dict = new Dictionary<Type, string>();
            try
            {
                var attrType = typeof(XmlGlobalConvertAttribute);
                foreach (var attr in asm.GetCustomAttributes(attrType, false))
                {
                    if (!(attr is XmlGlobalConvertAttribute ga) || ga.ConverterType == null) continue;
                    var targetType = GetTargetTypeFromConverterType(ga.ConverterType);
                    if (targetType != null && !dict.ContainsKey(targetType))
                        dict[targetType] = ga.Domain ?? "";
                }
            }
            catch { /* 忽略程序集无该属性 */ }

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
                            string domain = null;
                            if (ga != null && ga.ConverterType != null)
                            {
                                targetType = field.FieldType;
                                domain = ga.Domain ?? "";
                            }
                            else if (ta != null && ta.ConverterType != null)
                            {
                                targetType = GetTargetTypeFromConverterType(ta.ConverterType) ?? field.FieldType;
                                domain = ta.Domain ?? "";
                            }
                            if (targetType != null && !dict.ContainsKey(targetType))
                                dict[targetType] = domain ?? "";
                        }
                    }
                    catch { /* 忽略单类型 */ }
                }
            }
            catch (ReflectionTypeLoadException) { }

            return dict;
        }

        /// <summary>
        /// 获取类型名称（简化版本）
        /// </summary>
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

        /// <summary>
        /// 分析索引组
        /// </summary>
        private static void AnalyzeIndexGroups(ConfigTypeInfo info)
        {
            var indexGroups = new Dictionary<string, IndexGroupInfo>();

            foreach (var field in info.Fields)
            {
                if (field.IsIndexField)
                {
                    if (!indexGroups.ContainsKey(field.IndexName))
                    {
                        indexGroups[field.IndexName] = new IndexGroupInfo
                        {
                            IndexName = field.IndexName,
                            IsMultiValue = false // 需要从属性中获取
                        };
                    }

                    indexGroups[field.IndexName].Fields.Add(field);
                }
            }

            // 设置 IsMultiValue
            foreach (var kvp in indexGroups)
            {
                var group = kvp.Value;
                group.Fields = group.Fields.OrderBy(f => f.IndexPosition).ToList();
                
                // 从第一个字段的属性中获取 IsMultiValue
                var firstField = info.ManagedType.GetField(group.Fields[0].Name);
                if (firstField != null)
                {
                    var indexAttr = firstField.GetCustomAttribute<XmlIndexAttribute>();
                    if (indexAttr != null)
                    {
                        group.IsMultiValue = indexAttr.IsMultiValue;
                    }
                }

                info.IndexGroups.Add(group);
            }
        }

        /// <summary>
        /// 收集所需的 using 语句（根据字段类型填充 HashSet，供模板生成 using）
        /// </summary>
        private static void CollectRequiredUsings(ConfigTypeInfo info)
        {
            info.RequiredUsings.Add("System");
            info.RequiredUsings.Add("System.Collections.Generic");
            info.RequiredUsings.Add("System.Xml");
            info.RequiredUsings.Add("XM");
            info.RequiredUsings.Add("XM.Contracts");
            info.RequiredUsings.Add("XM.Contracts.Config");
            foreach (var f in info.Fields)
            {
                if (f.NeedsConverter) { info.RequiredUsings.Add("XM.Utils"); break; }
            }

            // 从托管字段类型收集命名空间（ClassHelper 与 Unmanaged 共用）
            if (info.ManagedType != null)
            {
                foreach (var field in info.ManagedType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (field.Name == "Data") continue;
                    CollectUsingsFromType(field.FieldType, info);
                }
            }

            // 从字段的 UnmanagedType 字符串收集（非托管生成用）
            foreach (var field in info.Fields)
            {
                CollectUsingsFromTypeString(field.UnmanagedType, info.ManagedType.Assembly, info);
                if (field.NeedsRefField)
                    CollectUsingsFromTypeString("XBlobPtr", info.ManagedType.Assembly, info);
            }
        }

        /// <summary>从 Type 及其泛型参数收集命名空间。</summary>
        private static void CollectUsingsFromType(Type type, ConfigTypeInfo configInfo)
        {
            if (type == null) return;
            if (!string.IsNullOrEmpty(type.Namespace))
                configInfo.RequiredUsings.Add(type.Namespace);
            if (type.IsGenericType)
            {
                foreach (var arg in type.GetGenericArguments())
                    CollectUsingsFromType(arg, configInfo);
            }
        }

        /// <summary>
        /// 从类型字符串中收集所需的命名空间
        /// </summary>
        private static void CollectUsingsFromTypeString(string typeString, Assembly assembly, ConfigTypeInfo configInfo)
        {
            if (string.IsNullOrEmpty(typeString))
                return;

            // 提取类型名称（去除泛型参数）
            string typeName = typeString;
            int genericIndex = typeName.IndexOf('<');
            if (genericIndex > 0)
            {
                typeName = typeName.Substring(0, genericIndex);
            }

            // 尝试在程序集中查找该类型
            try
            {
                // 首先尝试在当前程序集中查找
                var type = assembly.GetType(typeName, false, false);
                if (type == null)
                {
                    // 尝试在所有已加载的程序集中查找
                    foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
                    {
                        try
                        {
                            type = asm.GetType(typeName, false, false);
                            if (type != null)
                                break;
                        }
                        catch
                        {
                            // 忽略无法加载的程序集
                        }
                    }
                }

                if (type != null && !string.IsNullOrEmpty(type.Namespace))
                {
                    configInfo.RequiredUsings.Add(type.Namespace);
                }
            }
            catch
            {
                // 如果查找失败，忽略（可能是全局命名空间的类型）
            }

            // 递归处理泛型参数
            if (typeString.Contains("<"))
            {
                int startIndex = typeString.IndexOf('<') + 1;
                int endIndex = typeString.LastIndexOf('>');
                if (endIndex > startIndex)
                {
                    string genericArgs = typeString.Substring(startIndex, endIndex - startIndex);
                    // 分割泛型参数（简单处理，不考虑嵌套泛型）
                    var args = SplitGenericArguments(genericArgs);
                    foreach (var arg in args)
                    {
                        CollectUsingsFromTypeString(arg.Trim(), assembly, configInfo);
                    }
                }
            }
        }

        /// <summary>
        /// 分割泛型参数字符串
        /// </summary>
        private static List<string> SplitGenericArguments(string genericArgs)
        {
            var result = new List<string>();
            int depth = 0;
            int start = 0;

            for (int i = 0; i < genericArgs.Length; i++)
            {
                char c = genericArgs[i];
                if (c == '<')
                {
                    depth++;
                }
                else if (c == '>')
                {
                    depth--;
                }
                else if (c == ',' && depth == 0)
                {
                    result.Add(genericArgs.Substring(start, i - start));
                    start = i + 1;
                }
            }

            if (start < genericArgs.Length)
            {
                result.Add(genericArgs.Substring(start));
            }

            return result;
        }

        /// <summary>
        /// 通过类型名称查找类型（在所有已加载的程序集中搜索）
        /// </summary>
        private static Type FindTypeByName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return null;

            // 在所有已加载的程序集中查找
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    // 尝试完全匹配类型名称
                    var type = assembly.GetType(typeName, false, false);
                    if (type != null)
                        return type;

                    // 如果没找到，尝试在程序集的所有类型中按简单名称查找
                    var types = assembly.GetTypes();
                    foreach (var t in types)
                    {
                        if (t.Name == typeName || t.FullName == typeName)
                            return t;
                    }
                }
                catch
                {
                    // 忽略无法加载的程序集
                }
            }

            return null;
        }
    }
}
