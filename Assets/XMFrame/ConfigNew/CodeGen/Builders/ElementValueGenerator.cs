using System;

namespace XM.ConfigNew.CodeGen.Builders
{
    /// <summary>
    /// 元素值生成器 - 统一处理不同类型元素的代码生成
    /// 消除 GenerateLeafElementAssignment、GenerateLeafValueExpression、GenerateLeafSetAdd 等方法中的重复逻辑
    /// </summary>
    public static class ElementValueGenerator
    {
        
        /// <summary>
        /// 生成元素赋值代码（用于数组/List索引赋值）
        /// </summary>
        /// <param name="builder">代码构建器</param>
        /// <param name="elementType">元素类型</param>
        /// <param name="elementAccess">元素访问表达式（如 config.list[i]）</param>
        /// <param name="arrayVar">目标数组变量名</param>
        /// <param name="indexVar">索引变量名</param>
        public static void GenerateIndexAssignment(CodeBuilder builder, Type elementType, string elementAccess, string arrayVar, string indexVar)
        {
            // 配置类型需要通过 Helper 处理（使用统一方法）
            if (TypeHelper.IsConfigType(elementType))
            {
                var actualType = NullableTypeHelper.GetActualType(elementType);
                ConfigHelperInvoker.GenerateIndexAssignment(builder, actualType, elementAccess, arrayVar, indexVar);
                return;
            }
            
            // 容器类型不应该在这里处理
            if (TypeHelper.IsContainerType(elementType))
            {
                builder.AppendComment($"嵌套容器需要特殊处理: {TypeHelper.GetSimpleTypeName(elementType)}");
                return;
            }
            
            // 其他类型使用统一转换器
            var resultPrefix = $"elem{indexVar}";
            var conversion = UnifiedValueConverter.GenerateConversion(
                builder, elementType, elementAccess, resultPrefix, UnifiedValueConverter.UsageContext.Alloc);
            
            if (conversion.Success)
            {
                builder.AppendBlobIndexAssign(arrayVar, indexVar, conversion.ConvertedValueVar);
                
                if (conversion.NeedsCloseBlock)
                {
                    builder.EndBlock();
                }
            }
        }
        
        
        /// <summary>
        /// 生成元素添加代码（用于 Set.Add）
        /// </summary>
        /// <param name="builder">代码构建器</param>
        /// <param name="elementType">元素类型</param>
        /// <param name="itemAccess">元素访问表达式</param>
        /// <param name="setVar">Set 变量名</param>
        public static void GenerateSetAdd(CodeBuilder builder, Type elementType, string itemAccess, string setVar)
        {
            // 使用统一转换器
            var resultPrefix = "item";
            var conversion = UnifiedValueConverter.GenerateConversion(
                builder, elementType, itemAccess, resultPrefix, UnifiedValueConverter.UsageContext.Alloc);
            
            if (conversion.Success)
            {
                builder.AppendBlobSetAdd(setVar, conversion.ConvertedValueVar);
                
                if (conversion.NeedsCloseBlock)
                {
                    builder.EndBlock();
                }
            }
        }
        
        /// <summary>
        /// 生成元素值表达式（用于简单赋值场景，不支持需要 Try 的类型）
        /// 注意：对于 CfgS, string, LabelS 应使用 UnifiedValueConverter.GenerateConversion
        /// </summary>
        /// <param name="elementType">元素类型</param>
        /// <param name="valueAccess">值访问表达式</param>
        /// <returns>转换后的值表达式</returns>
        public static string GenerateValueExpression(Type elementType, string valueAccess)
        {
            var nullableInfo = NullableTypeHelper.Analyze(elementType, valueAccess);
            var actualType = nullableInfo.ActualType;
            var access = nullableInfo.ValueAccessExpr;
            
            // 枚举类型
            if (actualType.IsEnum)
            {
                var enumTypeName = TypeHelper.GetGlobalQualifiedTypeName(actualType);
                return CodeBuilder.BuildEnumWrapper(enumTypeName, access);
            }
            
            // 其他类型直接返回
            return access;
        }
        
        /// <summary>
        /// 生成配置类型的 Map 赋值代码（委托给统一方法）
        /// </summary>
        public static void GenerateConfigMapAssignment(CodeBuilder builder, Type configType, string sourceAccess, string targetMapVar, string keyExpr, string suffix)
        {
            ConfigHelperInvoker.GenerateMapAssignment(builder, configType, sourceAccess, targetMapVar, keyExpr, suffix);
        }
    }
}
