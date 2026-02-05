using System;
using System.Collections.Generic;
using XM.ConfigNew.Metadata;
using XM.Utils.Attribute;

namespace XM.ConfigNew.CodeGen
{
    /// <summary>
    /// 类型辅助工具 - 用于代码生成器的类型判断和转换
    /// 避免魔法字符串,使用 typeof 和 nameof
    /// </summary>
    public static class TypeHelper
    {
        #region 类型判断
        
        /// <summary>
        /// 判断是否是List类型
        /// </summary>
        public static bool IsListType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }
        
        /// <summary>
        /// 判断是否是Dictionary类型
        /// </summary>
        public static bool IsDictionaryType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
        }
        
        /// <summary>
        /// 判断是否是HashSet类型
        /// </summary>
        public static bool IsHashSetType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>);
        }
        
        /// <summary>
        /// 判断是否是可空类型
        /// </summary>
        public static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
        
        /// <summary>
        /// 判断是否是CfgS类型
        /// </summary>
        public static bool IsCfgSType(Type type)
        {
            if (!type.IsGenericType)
                return false;
            
            var genericTypeDef = type.GetGenericTypeDefinition();
            // 使用 typeof 避免魔法字符串
            return genericTypeDef.Name == typeof(CfgS<>).Name;
        }
        
        /// <summary>
        /// 判断是否是容器类型
        /// </summary>
        public static bool IsContainerType(Type type)
        {
            return IsListType(type) || IsDictionaryType(type) || IsHashSetType(type);
        }
        
        /// <summary>
        /// 判断是否是配置类型（实现了 IXConfig 接口）
        /// </summary>
        public static bool IsConfigType(Type type)
        {
            if (type == null)
                return false;
            
            return typeof(XM.IXConfig).IsAssignableFrom(type);
        }
        
        /// <summary>
        /// 获取容器的元素类型（用于 List 和 HashSet）
        /// </summary>
        public static Type GetContainerElementType(Type containerType)
        {
            if (containerType == null || !containerType.IsGenericType)
                return null;
            
            var args = containerType.GetGenericArguments();
            return args.Length > 0 ? args[0] : null;
        }
        
        /// <summary>
        /// 获取 Dictionary 的 Key 类型
        /// </summary>
        public static Type GetDictionaryKeyType(Type dictionaryType)
        {
            if (dictionaryType == null || !IsDictionaryType(dictionaryType))
                return null;
            
            var args = dictionaryType.GetGenericArguments();
            return args.Length >= 2 ? args[0] : null;
        }
        
        /// <summary>
        /// 获取 Dictionary 的 Value 类型
        /// </summary>
        public static Type GetDictionaryValueType(Type dictionaryType)
        {
            if (dictionaryType == null || !IsDictionaryType(dictionaryType))
                return null;
            
            var args = dictionaryType.GetGenericArguments();
            return args.Length >= 2 ? args[1] : null;
        }
        
        #endregion
        
        #region 类型名称获取
        
        /// <summary>
        /// 获取类型的简化名称(用于代码生成)
        /// </summary>
        public static string GetSimpleTypeName(Type type)
        {
            if (type == null)
                return "object";
            
            // 处理可空类型
            if (IsNullableType(type))
            {
                var underlyingType = Nullable.GetUnderlyingType(type);
                return GetSimpleTypeName(underlyingType) + "?";
            }
            
            // 处理泛型类型
            if (type.IsGenericType)
            {
                var name = type.Name.Split('`')[0];
                var args = type.GetGenericArguments();
                var argNames = string.Join(", ", Array.ConvertAll(args, GetSimpleTypeName));
                return $"{name}<{argNames}>";
            }
            
            // 基本类型
            return type.Name;
        }
        
        /// <summary>
        /// 获取类型的完整名称(包含命名空间)
        /// </summary>
        public static string GetFullTypeName(Type type)
        {
            if (type == null)
                return "object";
            
            if (type.IsGenericType)
            {
                var name = type.FullName?.Split('`')[0] ?? type.Name.Split('`')[0];
                var args = type.GetGenericArguments();
                var argNames = string.Join(", ", Array.ConvertAll(args, GetFullTypeName));
                return $"{name}<{argNames}>";
            }
            
            return type.FullName ?? type.Name;
        }
        
        /// <summary>
        /// 判断命名空间是否需要 global:: 前缀
        /// 所有命名空间都使用 global:: 前缀，彻底避免命名冲突
        /// </summary>
        private static bool NeedsGlobalPrefix(string namespaceName)
        {
            // 所有有命名空间的类型都使用 global:: 前缀
            return !string.IsNullOrEmpty(namespaceName);
        }
        
        /// <summary>
        /// 获取类型的全局限定名称(使用 global:: 前缀，避免命名冲突)
        /// </summary>
        public static string GetGlobalQualifiedTypeName(Type type)
        {
            if (type == null)
                return "object";
            
            // 处理可空类型
            if (IsNullableType(type))
            {
                var underlyingType = Nullable.GetUnderlyingType(type);
                return GetGlobalQualifiedTypeName(underlyingType) + "?";
            }
            
            // C# 基本类型关键字映射
            if (type == typeof(int)) return "int";
            if (type == typeof(float)) return "float";
            if (type == typeof(double)) return "double";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(string)) return "string";
            if (type == typeof(long)) return "long";
            if (type == typeof(short)) return "short";
            if (type == typeof(byte)) return "byte";
            if (type == typeof(decimal)) return "decimal";
            if (type == typeof(object)) return "object";
            if (type == typeof(void)) return "void";
            
            // 处理泛型类型
            if (type.IsGenericType)
            {
                var name = type.Name.Split('`')[0];
                
                // 构建类型名称前缀
                string typePrefix;
                if (!string.IsNullOrEmpty(type.Namespace))
                {
                    var needsGlobal = NeedsGlobalPrefix(type.Namespace);
                    typePrefix = needsGlobal ? $"global::{type.Namespace}.{name}" : $"{type.Namespace}.{name}";
                }
                else
                {
                    typePrefix = $"global::{name}";
                }
                
                // 处理泛型参数
                var args = type.GetGenericArguments();
                
                // 如果是开放泛型（如 List<>），直接返回不带参数的类型名
                if (type.IsGenericTypeDefinition)
                {
                    var paramCount = args.Length;
                    if (paramCount == 1)
                        return $"{typePrefix}<>";
                    else if (paramCount == 2)
                        return $"{typePrefix}<,>";
                    else
                        return $"{typePrefix}<{new string(',', paramCount - 1)}>";
                }
                
                // 封闭泛型（如 List<int>），递归处理参数
                var argNames = string.Join(", ", Array.ConvertAll(args, GetGlobalQualifiedTypeName));
                return $"{typePrefix}<{argNames}>";
            }
            
            // 普通类型: global::Namespace.TypeName
            if (!string.IsNullOrEmpty(type.Namespace))
            {
                // System 和 Unity.Collections 命名空间不需要 global 前缀
                if (NeedsGlobalPrefix(type.Namespace))
                    return $"global::{type.Namespace}.{type.Name}";
                return $"{type.Namespace}.{type.Name}";
            }
            
            return type.Name;
        }
        
        #endregion
        
        #region 非托管类型映射
        
        /// <summary>
        /// 获取托管类型对应的非托管类型名称
        /// </summary>
        public static string GetUnmanagedTypeName(Type managedType)
        {
            if (managedType == null)
                return "int";
            
            // 基本类型映射
            if (managedType == typeof(int)) return "int";
            if (managedType == typeof(float)) return "float";
            if (managedType == typeof(bool)) return "bool";
            if (managedType == typeof(long)) return "long";
            if (managedType == typeof(double)) return "double";
            if (managedType == typeof(byte)) return "byte";
            if (managedType == typeof(short)) return "short";
            if (managedType == typeof(string)) return CodeGenConstants.StrITypeName;
            
            // 枚举类型保持原样
            if (managedType.IsEnum)
                return managedType.Name;
            
            // 默认
            return "int";
        }
        
        #endregion
        
        #region 常量定义
        
        /// <summary>
        /// Link父指针后缀
        /// </summary>
        public static string LinkParentPtrSuffix => "_ParentPtr";
        
        /// <summary>
        /// Link父索引后缀
        /// </summary>
        public static string LinkParentIndexSuffix => "_ParentIndex";
        
        /// <summary>
        /// 从 CfgS&lt;T&gt; 类型获取其泛型参数的 Unmanaged 类型名
        /// 用于容器 Key/Value 的类型转换
        /// </summary>
        /// <param name="cfgSType">CfgS&lt;T&gt; 类型</param>
        /// <returns>T 的 Unmanaged 类型名</returns>
        public static string GetCfgSUnmanagedTypeName(Type cfgSType)
        {
            if (!IsCfgSType(cfgSType))
                return null;
            
            var innerType = GetContainerElementType(cfgSType);
            if (innerType == null)
                return null;
            
            return GetConfigUnmanagedTypeName(innerType);
        }
        
        /// <summary>
        /// 从配置类型获取其 Unmanaged 类型的全局限定名
        /// 统一方法：优先从泛型参数获取，确保使用 global:: 前缀
        /// </summary>
        /// <param name="configType">配置类型（Managed）</param>
        /// <returns>Unmanaged 类型的全局限定名</returns>
        public static string GetConfigUnmanagedTypeName(Type configType)
        {
            if (configType == null)
                return CodeGenConstants.ObjectTypeName;
            
            // 尝试从泛型参数获取 Unmanaged 类型（正确方式）
            var unmanagedType = Tools.TypeAnalyzer.GetUnmanagedTypeFromConfig(configType);
            if (unmanagedType != null)
                return GetGlobalQualifiedTypeName(unmanagedType);
            
            // 回退到名称拼接（确保全局限定）
            var configTypeName = GetGlobalQualifiedTypeName(configType);
            return EnsureUnmanagedSuffix(configTypeName);
        }
        
        /// <summary>
        /// 获取CfgI泛型类型名称
        /// 优先从 IXConfig&lt;T, TUnmanaged&gt; 泛型参数获取，避免拼接导致的大小写问题
        /// </summary>
        public static string GetCfgITypeName(Type targetManagedType)
        {
            if (targetManagedType == null)
                return "CfgI";
            
            // 尝试从泛型参数获取 Unmanaged 类型（正确方式）
            var unmanagedType = Tools.TypeAnalyzer.GetUnmanagedTypeFromConfig(targetManagedType);
            if (unmanagedType != null)
            {
                var unmanagedTypeName = GetGlobalQualifiedTypeName(unmanagedType);
                return CodeBuilder.BuildCfgITypeName(unmanagedTypeName);
            }
            
            // 回退到名称拼接方式
            var fallbackUnmanagedTypeName = EnsureUnmanagedSuffix(targetManagedType.Name);
            var unmanagedNamespace = targetManagedType.Namespace;
            
            if (!string.IsNullOrEmpty(unmanagedNamespace))
            {
                return $"CfgI<global::{unmanagedNamespace}.{fallbackUnmanagedTypeName}>";
            }
            
            return $"CfgI<{fallbackUnmanagedTypeName}>";
        }
        
        #endregion
        
        #region XBlob类型名称
        
        /// <summary>
        /// 获取XBlob容器类型名称(泛型,支持嵌套)
        /// XBlobArray等类型在全局命名空间，直接使用类型名
        /// </summary>
        public static string GetXBlobTypeName(FieldTypeInfo typeInfo)
        {
            if (typeInfo == null || !typeInfo.IsContainer)
            {
                return "global::XBlobPtr";
            }
            
            switch (typeInfo.ContainerType)
            {
                case EContainerType.List:
                    return $"global::XBlobArray<{GetElementTypeName(typeInfo)}>";
                    
                case EContainerType.Dictionary:
                    var keyTypeName = GetUnmanagedTypeNameWithWrapper(typeInfo.NestedKeyType);
                    var valueTypeName = GetElementTypeName(typeInfo);
                    return $"global::XBlobMap<{keyTypeName}, {valueTypeName}>";
                    
                case EContainerType.HashSet:
                    var elementTypeName = GetUnmanagedTypeNameWithWrapper(typeInfo.SingleValueType);
                    return $"global::XBlobSet<{elementTypeName}>";
                    
                default:
                    return "global::XBlobPtr";
            }
        }
        
        /// <summary>
        /// 获取容器元素类型名称(递归处理嵌套容器)
        /// </summary>
        private static string GetElementTypeName(FieldTypeInfo typeInfo)
        {
            // 如果Value是嵌套容器,递归生成
            if (typeInfo.IsValueContainer && typeInfo.NestedValueTypeInfo != null)
            {
                return GetXBlobTypeName(typeInfo.NestedValueTypeInfo);
            }
            
            // 如果是嵌套配置
            if (typeInfo.IsNestedConfig && typeInfo.NestedConfigMetadata != null)
            {
                // 使用全局限定名
                if (typeInfo.NestedConfigMetadata.UnmanagedType != null)
                    return GetGlobalQualifiedTypeName(typeInfo.NestedConfigMetadata.UnmanagedType);
                return typeInfo.NestedConfigMetadata.UnmanagedTypeName;
            }
            
            // 普通类型（需要包装枚举）
            return GetUnmanagedTypeNameWithWrapper(typeInfo.SingleValueType);
        }
        
        /// <summary>
        /// 获取非托管类型名称(枚举需要包装)
        /// 用于需要IEquatable约束的容器(XBlobSet, XBlobMap的Key)
        /// 已统一到 GetUnmanagedElementTypeName，此方法仅作委托
        /// </summary>
        public static string GetUnmanagedTypeNameWithWrapper(Type managedType)
        {
            // 委托给统一方法
            return GetUnmanagedElementTypeName(managedType);
        }
        
        #endregion
        
        #region Unmanaged 类型名称处理
        
        /// <summary>
        /// 确保类型名称带有 Unmanaged 后缀（智能添加，避免重复）
        /// 用于兼容两种情况：CfgS&lt;TestConfig&gt; 和 CfgS&lt;TestConfigUnmanaged&gt;
        /// </summary>
        /// <param name="typeName">类型名称（可能已带 Unmanaged 后缀）</param>
        /// <returns>确保带 Unmanaged 后缀的类型名</returns>
        public static string EnsureUnmanagedSuffix(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return typeName;
            
            // 如果已经以 Unmanaged 结尾，直接返回
            if (typeName.EndsWith(CodeGenConstants.UnmanagedSuffix))
                return typeName;
            
            // 否则添加 Unmanaged 后缀
            return typeName + CodeGenConstants.UnmanagedSuffix;
        }
        
        /// <summary>
        /// 从类型获取 Unmanaged 类型名称
        /// 配置类型优先从泛型参数获取，其他类型使用名称拼接
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>Unmanaged 类型的全局限定名</returns>
        public static string GetUnmanagedTypeNameSafe(Type type)
        {
            if (type == null)
                return CodeGenConstants.ObjectTypeName;
            
            // 配置类型委托给专用方法（从泛型参数获取）
            if (IsConfigType(type))
                return GetConfigUnmanagedTypeName(type);
            
            // 其他类型使用名称拼接
            return EnsureUnmanagedSuffix(GetGlobalQualifiedTypeName(type));
        }
        
        #endregion
        
        #region 非托管类型名称生成（用于代码生成）
        
        /// <summary>
        /// 获取非托管元素类型名称（用于容器元素类型声明）
        /// 处理：string->StrI, enum->EnumWrapper, CfgS->CfgI, Config->Unmanaged, Container->XBlob*
        /// </summary>
        public static string GetUnmanagedElementTypeName(Type type)
        {
            // 可空类型 T? -> T
            if (IsNullableType(type))
            {
                var underlyingType = Nullable.GetUnderlyingType(type);
                if (underlyingType != null)
                {
                    type = underlyingType;
                }
            }
            
            if (type == typeof(string))
            {
                return CodeGenConstants.StrITypeName;
            }
            else if (type.IsEnum)
            {
                var enumTypeName = GetGlobalQualifiedTypeName(type);
                return $"{CodeGenConstants.EnumWrapperPrefix}{enumTypeName}{CodeGenConstants.EnumWrapperSuffix}";
            }
            else if (IsCfgSType(type))
            {
                var innerType = GetContainerElementType(type);
                if (innerType != null)
                {
                    // 智能添加 Unmanaged 后缀（兼容 managed 和 unmanaged 类型）
                    var unmanagedTypeName = GetUnmanagedTypeNameSafe(innerType);
                    return CodeBuilder.BuildCfgITypeName(unmanagedTypeName);
                }
                return GetGlobalQualifiedTypeName(type);
            }
            else if (IsConfigType(type))
            {
                // 尝试从泛型参数获取（正确方式）
                var unmanagedType = Tools.TypeAnalyzer.GetUnmanagedTypeFromConfig(type);
                if (unmanagedType != null)
                    return GetGlobalQualifiedTypeName(unmanagedType);
                
                // 回退到名称推断
                return GetUnmanagedTypeNameSafe(type);
            }
            else if (IsContainerType(type))
            {
                return GetUnmanagedContainerTypeName(type);
            }
            else
            {
                return GetGlobalQualifiedTypeName(type);
            }
        }
        
        /// <summary>
        /// 获取非托管容器类型名称（用于嵌套容器）
        /// 返回 XBlobArray/XBlobMap/XBlobSet 的完整类型名
        /// </summary>
        public static string GetUnmanagedContainerTypeName(Type containerType)
        {
            if (!IsContainerType(containerType))
            {
                return GetUnmanagedElementTypeName(containerType);
            }
            
            var elementType = GetContainerElementType(containerType);
            var elementTypeName = GetUnmanagedElementTypeName(elementType);
            
            if (IsListType(containerType))
            {
                return $"{CodeGenConstants.XBlobArrayPrefix}{elementTypeName}{CodeGenConstants.GenericClose}";
            }
            else if (IsDictionaryType(containerType))
            {
                var keyType = GetDictionaryKeyType(containerType);
                var valueType = GetDictionaryValueType(containerType);
                var keyTypeName = GetUnmanagedElementTypeName(keyType);
                var valueTypeName = GetUnmanagedElementTypeName(valueType);
                return $"{CodeGenConstants.XBlobMapPrefix}{keyTypeName}, {valueTypeName}{CodeGenConstants.GenericClose}";
            }
            else if (IsHashSetType(containerType))
            {
                return $"{CodeGenConstants.XBlobSetPrefix}{elementTypeName}{CodeGenConstants.GenericClose}";
            }
            
            return CodeGenConstants.ObjectTypeName;
        }
        
        #endregion
        
        #region 字段类型获取（统一入口）
        
        /// <summary>
        /// 获取字段的非托管类型名称（统一方法，所有生成器都应使用此方法）
        /// </summary>
        /// <param name="field">字段元数据</param>
        /// <param name="getStringModeTypeName">字符串模式类型名称获取函数（可选）</param>
        /// <returns>非托管类型名称</returns>
        public static string GetUnmanagedFieldTypeName(ConfigFieldMetadata field, Func<EXmlStrMode, string> getStringModeTypeName = null)
        {
            var typeInfo = field.TypeInfo;
            
            // 如果已经预计算，直接使用
            if (!string.IsNullOrEmpty(field.UnmanagedFieldTypeName))
            {
                return field.UnmanagedFieldTypeName;
            }
            
            // 获取实际类型（处理可空类型）
            Type targetType = typeInfo.ManagedFieldType;
            if (typeInfo.IsNullable && typeInfo.UnderlyingType != null)
            {
                targetType = typeInfo.UnderlyingType;
            }
            
            // 1. CfgS<T> 类型 -> CfgI<TUnmanaged>（优先级最高）
            if (IsCfgSType(targetType))
            {
                var innerType = GetContainerElementType(targetType);
                return GetCfgITypeName(innerType);
            }
            
            // 2. 容器类型 -> XBlobArray<T> / XBlobMap<K,V> / XBlobSet<T>
            if (typeInfo.IsContainer)
            {
                return GetXBlobTypeName(typeInfo);
            }
            
            // 3. 嵌套配置 -> 嵌套的Unmanaged类型
            if (typeInfo.IsNestedConfig)
            {
                // 优先使用元数据中的类型名
                if (typeInfo.NestedConfigMetadata != null)
                    return typeInfo.NestedConfigMetadata.UnmanagedTypeName;
                
                // 尝试从 IXConfig<T, TUnmanaged> 泛型参数获取（正确方式）
                if (typeInfo.SingleValueType != null)
                {
                    var unmanagedType = Tools.TypeAnalyzer.GetUnmanagedTypeFromConfig(typeInfo.SingleValueType);
                    if (unmanagedType != null)
                        return GetGlobalQualifiedTypeName(unmanagedType);
                    
                    // 如果获取失败，回退到类型名推断
                    return EnsureUnmanagedSuffix(typeInfo.SingleValueType.Name);
                }
                
                return CodeGenConstants.ObjectTypeName;
            }
            
            // 4. XMLLink 类型 -> CfgI<T>（已由 CfgS 处理，保留兼容）
            if (field.IsXmlLink && !IsCfgSType(targetType))
            {
                return GetCfgITypeName(field.XmlLinkTargetType);
            }
            
            // 5. 枚举类型 -> 使用全局限定名
            if (targetType.IsEnum)
            {
                return GetGlobalQualifiedTypeName(targetType);
            }
            
            // 6. 字符串类型 -> 根据 StringMode
            if (targetType == typeof(string))
            {
                if (getStringModeTypeName != null)
                    return getStringModeTypeName(field.StringMode);
                
                // 默认使用 StrI
                return CodeGenConstants.StrITypeName;
            }
            
            // 7. 特殊转换类型（LabelS -> LabelI, ModS -> ModI 等）
            if (targetType.Name == "LabelS")
            {
                // LabelS 转换为 LabelI
                var ns = targetType.Namespace;
                return !string.IsNullOrEmpty(ns) ? $"global::{ns}.LabelI" : "LabelI";
            }
            if (targetType.Name == "ModS")
            {
                // ModS 转换为 ModI
                var ns = targetType.Namespace;
                return !string.IsNullOrEmpty(ns) ? $"global::{ns}.ModI" : "ModI";
            }
            if (targetType.Name == "TblS")
            {
                // TblS 转换为 TblI
                var ns = targetType.Namespace;
                return !string.IsNullOrEmpty(ns) ? $"global::{ns}.TblI" : "TblI";
            }
            
            // 8. 非托管结构体类型（int2, float2, Labels, LabelI 等）-> 保持原类型
            if (IsUnmanagedStructType(targetType))
            {
                return GetGlobalQualifiedTypeName(targetType);
            }
            
            // 9. 基本类型（int, float, bool 等）
            return GetUnmanagedTypeName(targetType);
        }
        
        /// <summary>
        /// 判断是否是非托管结构体类型（可直接在 Unmanaged 结构体中使用）
        /// 包括：Unity.Mathematics 类型、XM 命名空间的非托管值类型、已生成的 Unmanaged 结构体
        /// </summary>
        public static bool IsUnmanagedStructType(Type type)
        {
            if (type == null)
                return false;
            
            // 检查是否是值类型且不是基本类型
            if (!type.IsValueType || type.IsPrimitive || type.IsEnum)
                return false;
            
            var typeName = type.Name;
            
            // 检查命名空间
            var ns = type.Namespace;
            if (ns != null)
            {
                // Unity.Mathematics (int2, float2, quaternion, etc.)
                if (ns.StartsWith("Unity.Mathematics"))
                    return true;
                
                // Unity.Collections 的非托管类型（FixedString 已在 string 处理）
                if (ns == "Unity.Collections")
                {
                    // 排除托管集合类型
                    if (!typeName.StartsWith("Native"))
                        return true;
                }
                
                // XM 命名空间的值类型
                if (ns.StartsWith("XM"))
                {
                    // 排除托管类型：LabelS, ModS, TblS, CfgS（这些需要转换为 I 结尾的）
                    if (typeName == "LabelS" || typeName == "ModS" || typeName == "TblS")
                        return false;
                    
                    // CfgS 是泛型，已在前面处理
                    if (typeName.StartsWith("CfgS"))
                        return false;
                    
                    // 其他 XM 类型（LabelI, ModI, TblI, CfgI, Labels 等）可以直接使用
                    return true;
                }
            }
            
            // 检查是否实现 IConfigUnManaged 接口（已生成的 Unmanaged 结构体）
            var interfaces = type.GetInterfaces();
            foreach (var i in interfaces)
            {
                if (i.IsGenericType && i.GetGenericTypeDefinition().Name == "IConfigUnManaged`1")
                    return true;
            }
            
            return false;
        }
        
        #endregion
    }
    
    /// <summary>
    /// CfgS泛型定义(用于typeof)
    /// </summary>
    public struct CfgS<T> { }
    
    /// <summary>
    /// CfgI泛型定义(用于typeof)
    /// </summary>
    public struct CfgI<T> { }
    
}
