using System;

namespace XM.ConfigNew.CodeGen.Builders
{
    /// <summary>
    /// 容器元素处理器
    /// 封装容器中元素类型的统一处理逻辑（嵌套容器/配置类型/叶子类型）
    /// </summary>
    public static class ContainerElementHandler
    {
        /// <summary>
        /// 生成 List 元素的处理代码（用于 Array 索引赋值场景）
        /// 自动判断元素类型：嵌套容器 / 配置类型 / 叶子类型
        /// </summary>
        public static void GenerateListElementProcessing(
            CodeBuilder builder,
            Type elementType,
            string elementAccess,
            string arrayVar,
            string indexVar,
            Action<CodeBuilder, Type, string, string, int> nestedContainerCallback,
            int depth = 0)
        {
            if (TypeHelper.IsContainerType(elementType))
            {
                // 嵌套容器：通过 ref 方式分配
                var elementUnmanagedType = GetUnmanagedElementTypeName(elementType);
                var innerSourceVar = $"inner{depth}";
                var tempVar = $"temp_{depth}";
                
                builder.AppendVarDeclaration(innerSourceVar, elementAccess);
                builder.BeginIfBlock(CodeBuilder.BuildNotNullAndNotEmptyCondition(innerSourceVar));
                builder.AppendDefaultVarDeclaration(tempVar, elementUnmanagedType);
                
                // 回调处理嵌套容器分配
                nestedContainerCallback(builder, elementType, innerSourceVar, tempVar, depth + 1);
                
                builder.AppendBlobIndexAssign(arrayVar, indexVar, tempVar);
                builder.EndBlock();
            }
            else
            {
                // 叶子类型（包括配置类型）
                ElementValueGenerator.GenerateIndexAssignment(builder, elementType, elementAccess, arrayVar, indexVar);
            }
        }
        
        /// <summary>
        /// 生成 Dictionary Value 的处理代码
        /// 自动判断 Value 类型：嵌套容器 / 配置类型 / 叶子类型
        /// </summary>
        public static void GenerateDictionaryValueProcessing(
            CodeBuilder builder,
            Type valueType,
            string valueAccess,
            string mapVar,
            string keyExpr,
            string suffix,
            Action<CodeBuilder, Type, string, string, int> nestedContainerCallback,
            int depth = 0)
        {
            if (TypeHelper.IsContainerType(valueType))
            {
                // Value 是嵌套容器
                var valueUnmanagedType = GetUnmanagedElementTypeName(valueType);
                var innerSourceVar = $"innerVal{depth}";
                var tempVar = $"tempVal{suffix}";
                
                builder.AppendVarDeclaration(innerSourceVar, valueAccess);
                builder.BeginIfBlock(CodeBuilder.BuildNotNullAndNotEmptyCondition(innerSourceVar));
                builder.AppendDefaultVarDeclaration(tempVar, valueUnmanagedType);
                
                // 回调处理嵌套容器分配
                nestedContainerCallback(builder, valueType, innerSourceVar, tempVar, depth + 1);
                
                builder.AppendBlobMapAssign(mapVar, keyExpr, tempVar);
                builder.EndBlock();
            }
            else if (TypeHelper.IsConfigType(valueType))
            {
                // Value 是配置类型
                ElementValueGenerator.GenerateConfigMapAssignment(builder, valueType, valueAccess, mapVar, keyExpr, suffix);
            }
            else
            {
                // Value 是叶子类型
                var valueExpr = ElementValueGenerator.GenerateValueExpression(valueType, valueAccess);
                builder.AppendBlobMapAssign(mapVar, keyExpr, valueExpr);
            }
        }
        
        /// <summary>
        /// 获取非托管元素类型名称
        /// </summary>
        private static string GetUnmanagedElementTypeName(Type type)
        {
            // 可空类型 T? -> T
            if (TypeHelper.IsNullableType(type))
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
                var enumTypeName = TypeHelper.GetGlobalQualifiedTypeName(type);
                return $"{CodeGenConstants.EnumWrapperPrefix}{enumTypeName}{CodeGenConstants.EnumWrapperSuffix}";
            }
            else if (TypeHelper.IsCfgSType(type))
            {
                var innerType = TypeHelper.GetContainerElementType(type);
                if (innerType != null)
                {
                    var unmanagedTypeName = TypeHelper.GetGlobalQualifiedTypeName(innerType) + CodeGenConstants.UnmanagedSuffix;
                    return CodeBuilder.BuildCfgITypeName(unmanagedTypeName);
                }
                return TypeHelper.GetGlobalQualifiedTypeName(type);
            }
            else if (TypeHelper.IsConfigType(type))
            {
                return TypeHelper.GetGlobalQualifiedTypeName(type) + CodeGenConstants.UnmanagedSuffix;
            }
            else if (TypeHelper.IsContainerType(type))
            {
                return GetUnmanagedContainerTypeName(type);
            }
            else
            {
                return TypeHelper.GetGlobalQualifiedTypeName(type);
            }
        }
        
        /// <summary>
        /// 获取非托管容器类型名称
        /// </summary>
        private static string GetUnmanagedContainerTypeName(Type containerType)
        {
            if (!TypeHelper.IsContainerType(containerType))
            {
                return GetUnmanagedElementTypeName(containerType);
            }
            
            var elementType = TypeHelper.GetContainerElementType(containerType);
            var elementTypeName = GetUnmanagedElementTypeName(elementType);
            
            if (TypeHelper.IsListType(containerType))
            {
                return $"{CodeGenConstants.XBlobArrayPrefix}{elementTypeName}{CodeGenConstants.GenericClose}";
            }
            else if (TypeHelper.IsDictionaryType(containerType))
            {
                var keyType = TypeHelper.GetDictionaryKeyType(containerType);
                var valueType = TypeHelper.GetDictionaryValueType(containerType);
                var keyTypeName = GetUnmanagedElementTypeName(keyType);
                var valueTypeName = GetUnmanagedElementTypeName(valueType);
                return $"{CodeGenConstants.XBlobMapPrefix}{keyTypeName}, {valueTypeName}{CodeGenConstants.GenericClose}";
            }
            else if (TypeHelper.IsHashSetType(containerType))
            {
                return $"{CodeGenConstants.XBlobSetPrefix}{elementTypeName}{CodeGenConstants.GenericClose}";
            }
            
            return CodeGenConstants.ObjectTypeName;
        }
    }
}
