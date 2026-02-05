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
        /// 元素类型分类
        /// </summary>
        public enum ElementCategory
        {
            /// <summary>字符串类型 -> StrI</summary>
            String,
            
            /// <summary>枚举类型 -> EnumWrapper</summary>
            Enum,
            
            /// <summary>CfgS 类型 -> CfgI</summary>
            CfgS,
            
            /// <summary>配置类型（需要通过 Helper）</summary>
            Config,
            
            /// <summary>嵌套容器类型</summary>
            Container,
            
            /// <summary>基本类型（直接赋值）</summary>
            Primitive
        }
        
        /// <summary>
        /// 获取元素类型分类
        /// </summary>
        public static ElementCategory GetCategory(Type type)
        {
            // 处理可空类型，获取实际类型
            var actualType = TypeHelper.IsNullableType(type) 
                ? Nullable.GetUnderlyingType(type) ?? type 
                : type;
            
            if (actualType == typeof(string))
                return ElementCategory.String;
            
            if (actualType.IsEnum)
                return ElementCategory.Enum;
            
            if (TypeHelper.IsCfgSType(actualType))
                return ElementCategory.CfgS;
            
            if (TypeHelper.IsConfigType(actualType))
                return ElementCategory.Config;
            
            if (TypeHelper.IsContainerType(actualType))
                return ElementCategory.Container;
            
            return ElementCategory.Primitive;
        }
        
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
            var category = GetCategory(elementType);
            var isNullable = TypeHelper.IsNullableType(elementType);
            var actualType = isNullable ? Nullable.GetUnderlyingType(elementType) ?? elementType : elementType;
            var valueAccess = isNullable ? CodeBuilder.BuildGetValueOrDefault(elementAccess) : elementAccess;
            
            switch (category)
            {
                case ElementCategory.String:
                    builder.BeginIfBlock($"{CodeGenConstants.TryGetStrIMethod}({elementAccess}, out var strI)");
                    builder.AppendBlobIndexAssign(arrayVar, indexVar, "strI");
                    builder.EndBlock();
                    break;
                
                case ElementCategory.Enum:
                    var enumTypeName = TypeHelper.GetGlobalQualifiedTypeName(actualType);
                    builder.AppendBlobIndexAssign(arrayVar, indexVar, CodeBuilder.BuildEnumWrapper(enumTypeName, valueAccess));
                    break;
                
                case ElementCategory.CfgS:
                    var innerType = TypeHelper.GetContainerElementType(actualType);
                    var unmanagedTypeName = innerType != null 
                        ? TypeHelper.GetGlobalQualifiedTypeName(innerType) + CodeGenConstants.UnmanagedSuffix 
                        : CodeGenConstants.ObjectTypeName;
                    builder.BeginIfBlock($"{CodeGenConstants.TryGetCfgIMethod}({elementAccess}, out var cfgI)");
                    builder.AppendBlobIndexAssign(arrayVar, indexVar, $"cfgI.{CodeGenConstants.AsMethod}<{unmanagedTypeName}>()");
                    builder.EndBlock();
                    break;
                
                case ElementCategory.Config:
                    GenerateConfigIndexAssignment(builder, actualType, elementAccess, arrayVar, indexVar);
                    break;
                
                case ElementCategory.Container:
                    // 嵌套容器不应该在这里处理，应该使用递归方法
                    builder.AppendComment($"嵌套容器需要特殊处理: {TypeHelper.GetSimpleTypeName(actualType)}");
                    break;
                
                case ElementCategory.Primitive:
                default:
                    builder.AppendBlobIndexAssign(arrayVar, indexVar, valueAccess);
                    break;
            }
        }
        
        /// <summary>
        /// 生成配置类型的索引赋值（需要通过 ClassHelper）
        /// </summary>
        private static void GenerateConfigIndexAssignment(CodeBuilder builder, Type configType, string elementAccess, string arrayVar, string indexVar)
        {
            var configTypeName = TypeHelper.GetGlobalQualifiedTypeName(configType);
            var unmanagedTypeName = configTypeName + CodeGenConstants.UnmanagedSuffix;
            var helperTypeName = configTypeName + CodeGenConstants.ClassHelperSuffix;
            
            builder.BeginIfBlock(CodeBuilder.BuildNotNullCondition(elementAccess));
            builder.AppendVarDeclaration($"leafHelper_{indexVar}", $"{helperTypeName}.{CodeGenConstants.InstanceProperty}");
            builder.BeginIfBlock(CodeBuilder.BuildNotNullCondition($"leafHelper_{indexVar}"));
            builder.AppendNewVarDeclaration($"leafItemData_{indexVar}", unmanagedTypeName);
            builder.AppendLine($"leafHelper_{indexVar}.{CodeGenConstants.AllocContainerWithFillImplMethod}({elementAccess}, {CodeGenConstants.DefaultTblI}, {CodeGenConstants.CfgIVar}, ref leafItemData_{indexVar}, {CodeGenConstants.ConfigHolderDataVar});");
            builder.AppendBlobIndexAssign(arrayVar, indexVar, $"leafItemData_{indexVar}");
            builder.EndBlock();
            builder.EndBlock();
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
            var category = GetCategory(elementType);
            var isNullable = TypeHelper.IsNullableType(elementType);
            var actualType = isNullable ? Nullable.GetUnderlyingType(elementType) ?? elementType : elementType;
            var valueAccess = isNullable ? CodeBuilder.BuildGetValueOrDefault(itemAccess) : itemAccess;
            
            switch (category)
            {
                case ElementCategory.String:
                    builder.BeginIfBlock($"{CodeGenConstants.TryGetStrIMethod}({itemAccess}, out var strI)");
                    builder.AppendBlobSetAdd(setVar, "strI");
                    builder.EndBlock();
                    break;
                
                case ElementCategory.Enum:
                    var enumTypeName = TypeHelper.GetGlobalQualifiedTypeName(actualType);
                    builder.AppendBlobSetAdd(setVar, CodeBuilder.BuildEnumWrapper(enumTypeName, valueAccess));
                    break;
                
                case ElementCategory.CfgS:
                    var innerType = TypeHelper.GetContainerElementType(actualType);
                    var unmanagedTypeName = innerType != null 
                        ? TypeHelper.GetGlobalQualifiedTypeName(innerType) + CodeGenConstants.UnmanagedSuffix 
                        : CodeGenConstants.ObjectTypeName;
                    builder.BeginIfBlock($"{CodeGenConstants.TryGetCfgIMethod}({itemAccess}, out var cfgI)");
                    builder.AppendBlobSetAdd(setVar, $"cfgI.{CodeGenConstants.AsMethod}<{unmanagedTypeName}>()");
                    builder.EndBlock();
                    break;
                
                case ElementCategory.Primitive:
                default:
                    builder.AppendBlobSetAdd(setVar, valueAccess);
                    break;
            }
        }
        
        /// <summary>
        /// 生成元素值表达式（用于 Dictionary Value 等直接赋值场景）
        /// </summary>
        /// <param name="elementType">元素类型</param>
        /// <param name="valueAccess">值访问表达式</param>
        /// <returns>转换后的值表达式</returns>
        public static string GenerateValueExpression(Type elementType, string valueAccess)
        {
            var category = GetCategory(elementType);
            var isNullable = TypeHelper.IsNullableType(elementType);
            var actualType = isNullable ? Nullable.GetUnderlyingType(elementType) ?? elementType : elementType;
            var access = isNullable ? CodeBuilder.BuildGetValueOrDefault(valueAccess) : valueAccess;
            
            switch (category)
            {
                case ElementCategory.Enum:
                    var enumTypeName = TypeHelper.GetGlobalQualifiedTypeName(actualType);
                    return CodeBuilder.BuildEnumWrapper(enumTypeName, access);
                
                case ElementCategory.Primitive:
                case ElementCategory.String:
                default:
                    return access;
            }
        }
        
        /// <summary>
        /// 生成配置类型的 Map 赋值代码（用于 Dictionary Value）
        /// </summary>
        /// <param name="builder">代码构建器</param>
        /// <param name="configType">配置类型</param>
        /// <param name="sourceAccess">源值访问表达式</param>
        /// <param name="targetMapVar">目标 Map 变量名</param>
        /// <param name="keyExpr">键表达式</param>
        /// <param name="suffix">变量名后缀（用于唯一命名）</param>
        public static void GenerateConfigMapAssignment(CodeBuilder builder, Type configType, string sourceAccess, string targetMapVar, string keyExpr, string suffix)
        {
            var configTypeName = TypeHelper.GetGlobalQualifiedTypeName(configType);
            var unmanagedTypeName = configTypeName + CodeGenConstants.UnmanagedSuffix;
            var helperTypeName = configTypeName + CodeGenConstants.ClassHelperSuffix;
            
            builder.BeginIfBlock(CodeBuilder.BuildNotNullCondition(sourceAccess));
            builder.AppendVarDeclaration($"cfgHelper{suffix}", $"{helperTypeName}.{CodeGenConstants.InstanceProperty}");
            builder.BeginIfBlock(CodeBuilder.BuildNotNullCondition($"cfgHelper{suffix}"));
            builder.AppendNewVarDeclaration($"cfgItemData{suffix}", unmanagedTypeName);
            builder.AppendLine($"cfgHelper{suffix}.{CodeGenConstants.AllocContainerWithFillImplMethod}({sourceAccess}, {CodeGenConstants.DefaultTblI}, {CodeGenConstants.CfgIVar}, ref cfgItemData{suffix}, {CodeGenConstants.ConfigHolderDataVar});");
            builder.AppendBlobMapAssign(targetMapVar, keyExpr, $"cfgItemData{suffix}");
            builder.EndBlock();
            builder.EndBlock();
        }
    }
}
