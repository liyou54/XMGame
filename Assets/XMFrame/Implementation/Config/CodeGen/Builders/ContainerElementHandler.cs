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
                // 嵌套容器：通过 ref 方式分配（使用统一方法）
                var elementUnmanagedType = TypeHelper.GetUnmanagedElementTypeName(elementType);
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
        /// 生成 Dictionary Value 的处理代码（使用统一转换器）
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
                // Value 是嵌套容器（使用统一方法）
                var valueUnmanagedType = TypeHelper.GetUnmanagedElementTypeName(valueType);
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
                // Value 是叶子类型（使用统一转换器）
                var resultPrefix = string.IsNullOrEmpty(suffix) ? "value" : $"val{suffix}";
                var conversion = UnifiedValueConverter.GenerateConversion(
                    builder, valueType, valueAccess, resultPrefix, UnifiedValueConverter.UsageContext.Alloc);
                
                if (conversion.Success)
                {
                    builder.AppendBlobMapAssign(mapVar, keyExpr, conversion.ConvertedValueVar);
                    
                    if (conversion.NeedsCloseBlock)
                    {
                        builder.EndBlock();
                    }
                }
            }
        }
        
    }
}
