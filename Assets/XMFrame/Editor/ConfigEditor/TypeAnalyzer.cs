using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using XMFrame;
using XMFrame.Utils.Attribute;

namespace XMFrame.Editor.ConfigEditor
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
    }

    /// <summary>
    /// 类型分析器，负责分析托管类型并映射到非托管类型
    /// </summary>
    public static class TypeAnalyzer
    {
        /// <summary>
        /// 分析配置类型
        /// </summary>
        public static ConfigTypeInfo AnalyzeConfigType(Type configType)
        {
            if (!IsXConfigType(configType))
            {
                throw new ArgumentException($"类型 {configType.Name} 不是 XConfig 类型");
            }

            var info = new ConfigTypeInfo
            {
                ManagedType = configType,
                Namespace = configType.Namespace ?? string.Empty,
                ManagedTypeName = configType.Name
            };

            // 获取泛型参数 TUnmanaged
            var baseType = configType.BaseType;
            if (baseType != null && baseType.IsGenericType)
            {
                var genericArgs = baseType.GetGenericArguments();
                if (genericArgs.Length >= 2)
                {
                    info.UnmanagedType = genericArgs[1];
                    info.UnmanagedTypeName = info.UnmanagedType.Name;
                }
            }

            // 分析字段
            AnalyzeFields(configType, info);

            // 分析索引组
            AnalyzeIndexGroups(info);

            // 收集所需的 using 语句
            CollectRequiredUsings(info);

            return info;
        }

        /// <summary>
        /// 检查是否是 XConfig 类型
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
            return false;
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
                
                if (!fieldInfo.NeedsConverter)
                {
                    // 映射到非托管类型
                    fieldInfo.UnmanagedType = MapToUnmanagedType(field.FieldType, field, info);
                }

                // 检查是否需要生成 _Ref 字段（ConfigKey 类型）
                if (IsConfigKeyType(field.FieldType))
                {
                    fieldInfo.NeedsRefField = true;
                    fieldInfo.RefFieldName = field.Name + "_Ref";
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

            // Unity.Mathematics 类型
            if (managedType.Namespace == "Unity.Mathematics")
            {
                return GetTypeName(managedType);
            }

            // string 类型 - 根据 XmlStringMode 属性映射
            if (managedType == typeof(string))
            {
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
                            configInfo.RequiredUsings.Add("Unity.Collections");
                            switch (strMode)
                            {
                                case EXmlStrMode.EFix32:
                                    return "FixedString32Bytes";
                                case EXmlStrMode.EFix64:
                                    return "FixedString64Bytes";
                                case EXmlStrMode.EStrHandle:
                                    return "StrHandle";
                                case EXmlStrMode.EStrLabel:
                                    return "StrLabelHandle";
                            }
                        }
                    }
                }
                // 默认使用 StrHandle
                configInfo.RequiredUsings.Add("Unity.Collections");
                return "StrHandle";
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

            // ConfigKey<T> -> CfgId<T>
            if (IsConfigKeyType(managedType))
            {
                var elementType = managedType.GetGenericArguments()[0];
                // CfgId 在全局命名空间中，不需要添加 using
                return $"CfgId<{GetTypeName(elementType)}>";
            }

            // StrLabel -> StrLabelHandle
            if (managedType.Name == "StrLabel")
            {
                return "StrLabelHandle";
            }

            // Type -> TypeId (全局映射)
            if (managedType == typeof(Type))
            {
                configInfo.RequiredUsings.Add("XMFrame.Utils");
                return "TypeId";
            }

            // XConfig<T, TUnmanaged> -> 优先查找 {TypeName}UnManaged，否则使用 TUnmanaged
            if (IsXConfigType(managedType))
            {
                var baseType = managedType.BaseType;
                if (baseType != null && baseType.IsGenericType)
                {
                    var genericArgs = baseType.GetGenericArguments();
                    if (genericArgs.Length >= 2)
                    {
                        // 优先尝试查找 {TypeName}UnManaged 类型
                        var expectedUnmanagedName = managedType.Name + "UnManaged";
                        var unmanagedTypeFromGeneric = genericArgs[1];
                        
                        // 在同一个程序集中查找 {TypeName}UnManaged 类型
                        var assembly = managedType.Assembly;
                        try
                        {
                            // 尝试在相同命名空间中查找
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
                        
                        // 如果找不到，使用泛型参数中的类型
                        return GetTypeName(unmanagedTypeFromGeneric);
                    }
                }
            }

            // 其他类型直接使用名称
            return GetTypeName(managedType);
        }

        /// <summary>
        /// 检查是否是 ConfigKey 类型
        /// </summary>
        private static bool IsConfigKeyType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition().Name == "ConfigKey`1";
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
        /// 收集所需的 using 语句
        /// </summary>
        private static void CollectRequiredUsings(ConfigTypeInfo info)
        {
            info.RequiredUsings.Add("System");
            info.RequiredUsings.Add("XMFrame");
            
            // 检查是否有需要转换的字段
            foreach (var field in info.Fields)
            {
                if (field.NeedsConverter)
                {
                    info.RequiredUsings.Add("XMFrame.Interfaces.ConfigMananger");
                    break;
                }
            }
            
            // 从字段的实际类型信息中动态提取命名空间
            foreach (var field in info.Fields)
            {
                CollectUsingsFromTypeString(field.UnmanagedType, info.ManagedType.Assembly, info);
                
                // 如果有 _Ref 字段，也需要检查 XBlobPtr 的命名空间
                if (field.NeedsRefField)
                {
                    CollectUsingsFromTypeString("XBlobPtr", info.ManagedType.Assembly, info);
                }
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
    }
}
