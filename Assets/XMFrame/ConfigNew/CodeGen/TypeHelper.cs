using System;
using System.Collections.Generic;
using XM.ConfigNew.Metadata;

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
        /// 获取CfgI泛型类型名称
        /// 使用Unmanaged类型作为泛型参数，带全局限定名
        /// </summary>
        public static string GetCfgITypeName(Type targetManagedType)
        {
            if (targetManagedType == null)
                return "CfgI";
            
            // 使用Unmanaged类型的全局限定名
            var unmanagedTypeName = targetManagedType.Name + CodeGenConstants.UnmanagedSuffix;
            var unmanagedNamespace = targetManagedType.Namespace;
            
            if (!string.IsNullOrEmpty(unmanagedNamespace))
            {
                return $"CfgI<global::{unmanagedNamespace}.{unmanagedTypeName}>";
            }
            
            return $"CfgI<{unmanagedTypeName}>";
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
        /// </summary>
        public static string GetUnmanagedTypeNameWithWrapper(Type managedType)
        {
            if (managedType == null)
                return "int";
            
            // 可空类型 T? -> T
            if (IsNullableType(managedType))
            {
                var underlyingType = Nullable.GetUnderlyingType(managedType);
                if (underlyingType != null)
                {
                    managedType = underlyingType;
                }
            }
            
            // CfgS<T> -> CfgI<TUnmanaged>
            if (IsCfgSType(managedType))
            {
                var innerType = GetContainerElementType(managedType);
                if (innerType != null)
                {
                    var unmanagedTypeName = GetGlobalQualifiedTypeName(innerType) + CodeGenConstants.UnmanagedSuffix;
                    return $"CfgI<{unmanagedTypeName}>";
                }
            }
            
            // 枚举类型需要包装，并使用全局限定名
            if (managedType.IsEnum)
                return $"global::XM.ConfigNew.CodeGen.EnumWrapper<{GetGlobalQualifiedTypeName(managedType)}>";
            
            // 字符串类型 -> StrI
            if (managedType == typeof(string))
                return "StrI";
            
            // 其他类型直接返回
            return GetUnmanagedTypeName(managedType);
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
                    var unmanagedTypeName = GetGlobalQualifiedTypeName(innerType) + CodeGenConstants.UnmanagedSuffix;
                    return CodeBuilder.BuildCfgITypeName(unmanagedTypeName);
                }
                return GetGlobalQualifiedTypeName(type);
            }
            else if (IsConfigType(type))
            {
                return GetGlobalQualifiedTypeName(type) + CodeGenConstants.UnmanagedSuffix;
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
