using System;
using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen.Builders
{
    /// <summary>
    /// 统一值转换器
    /// 输入：源类型（Managed）+ 使用场景（Alloc/Parse）
    /// 输出：目标类型（Unmanaged）+ 转换代码
    /// 适用于所有场景：字段、容器元素、Dictionary Key/Value
    /// </summary>
    public static class UnifiedValueConverter
    {
        /// <summary>
        /// 使用场景
        /// </summary>
        public enum UsageContext
        {
            /// <summary>分配场景（Alloc）- 从 Managed 到 Unmanaged</summary>
            Alloc,
            
            /// <summary>解析场景（Parse）- 从 XML 到 Managed（保留，暂未实现）</summary>
            Parse
        }
        
        /// <summary>
        /// 转换结果
        /// </summary>
        public class ConversionResult
        {
            /// <summary>目标类型名（Unmanaged，全局限定）</summary>
            public string TargetTypeName { get; set; }
            
            /// <summary>转换后的值变量名</summary>
            public string ConvertedValueVar { get; set; }
            
            /// <summary>是否需要关闭 if 块</summary>
            public bool NeedsCloseBlock { get; set; }
            
            /// <summary>是否转换成功</summary>
            public bool Success { get; set; }
        }
        
        /// <summary>
        /// 生成值转换代码（统一入口，适用于所有场景）
        /// </summary>
        /// <param name="builder">代码构建器</param>
        /// <param name="sourceType">源类型（Managed）</param>
        /// <param name="sourceExpr">源值表达式</param>
        /// <param name="resultVarPrefix">结果变量名前缀</param>
        /// <param name="context">使用场景</param>
        /// <returns>转换结果</returns>
        public static ConversionResult GenerateConversion(
            CodeBuilder builder,
            Type sourceType,
            string sourceExpr,
            string resultVarPrefix,
            UsageContext context = UsageContext.Alloc)
        {
            var result = new ConversionResult { Success = false };
            
            if (sourceType == null || context != UsageContext.Alloc)
            {
                return result;
            }
            
            // 获取实际类型（处理可空）
            var nullableInfo = NullableTypeHelper.Analyze(sourceType, sourceExpr);
            var actualType = nullableInfo.ActualType;
            var valueExpr = nullableInfo.ValueAccessExpr;
            
            // 根据类型生成转换
            if (TypeHelper.IsCfgSType(actualType))
            {
                // CfgS<T> → CfgI<TUnmanaged>
                return GenerateCfgSConversion(builder, actualType, nullableInfo.SourceExpr, resultVarPrefix);
            }
            else if (actualType == typeof(string))
            {
                // string → StrI
                return GenerateStringConversion(builder, nullableInfo.SourceExpr, resultVarPrefix);
            }
            else if (actualType.Name == "LabelS")
            {
                // LabelS → LabelI
                return GenerateLabelSConversion(builder, nullableInfo.SourceExpr, resultVarPrefix);
            }
            else if (actualType.IsEnum)
            {
                // enum → EnumWrapper<T>
                return GenerateEnumConversion(builder, actualType, valueExpr, resultVarPrefix);
            }
            else if (TypeHelper.IsConfigType(actualType))
            {
                // IXConfig → TUnmanaged (不应该出现在值转换中，应该通过 Helper)
                result.TargetTypeName = TypeHelper.GetConfigUnmanagedTypeName(actualType);
                result.ConvertedValueVar = valueExpr;
                result.Success = true;
                return result;
            }
            else
            {
                // 基本类型、非托管结构体（int, int2, LabelI 等）→ 直接使用
                return GenerateDirectConversion(builder, actualType, valueExpr, resultVarPrefix);
            }
        }
        
        #region 各类型转换实现
        
        /// <summary>
        /// 生成 CfgS 转换：CfgS&lt;T&gt; → CfgI&lt;TUnmanaged&gt;
        /// </summary>
        private static ConversionResult GenerateCfgSConversion(CodeBuilder builder, Type cfgSType, string sourceExpr, string resultPrefix)
        {
            var result = new ConversionResult();
            
            // 获取目标 Unmanaged 类型名
            result.TargetTypeName = TypeHelper.GetCfgSUnmanagedTypeName(cfgSType);
            if (string.IsNullOrEmpty(result.TargetTypeName))
            {
                result.Success = false;
                return result;
            }
            
            // 生成转换代码：TryGetCfgI + As<TUnmanaged>()
            var cfgIVar = $"{resultPrefix}CfgI";
            var convertedVar = $"{resultPrefix}Converted";
            
            builder.BeginIfBlock($"{CodeGenConstants.TryGetCfgIMethod}({sourceExpr}, out var {cfgIVar})");
            builder.AppendVarDeclaration(convertedVar, $"{cfgIVar}.{CodeGenConstants.AsMethod}<{result.TargetTypeName}>()");
            
            result.ConvertedValueVar = convertedVar;
            result.NeedsCloseBlock = true;
            result.Success = true;
            
            return result;
        }
        
        /// <summary>
        /// 生成 string 转换：string → StrI
        /// </summary>
        private static ConversionResult GenerateStringConversion(CodeBuilder builder, string sourceExpr, string resultPrefix)
        {
            var result = new ConversionResult();
            result.TargetTypeName = CodeGenConstants.StrITypeName;
            result.ConvertedValueVar = $"{resultPrefix}StrI";
            
            builder.BeginIfBlock($"{CodeGenConstants.TryGetStrIMethod}({sourceExpr}, out var {result.ConvertedValueVar})");
            
            result.NeedsCloseBlock = true;
            result.Success = true;
            
            return result;
        }
        
        /// <summary>
        /// 生成 LabelS 转换：LabelS → LabelI
        /// </summary>
        private static ConversionResult GenerateLabelSConversion(CodeBuilder builder, string sourceExpr, string resultPrefix)
        {
            var result = new ConversionResult();
            result.TargetTypeName = "global::XM.LabelI";
            result.ConvertedValueVar = $"{resultPrefix}LabelI";
            
            builder.BeginIfBlock($"TryGetLabelI({sourceExpr}, out var {result.ConvertedValueVar})");
            
            result.NeedsCloseBlock = true;
            result.Success = true;
            
            return result;
        }
        
        /// <summary>
        /// 生成枚举转换：enum → EnumWrapper&lt;T&gt;
        /// </summary>
        private static ConversionResult GenerateEnumConversion(CodeBuilder builder, Type enumType, string valueExpr, string resultPrefix)
        {
            var result = new ConversionResult();
            var enumTypeName = TypeHelper.GetGlobalQualifiedTypeName(enumType);
            result.TargetTypeName = $"{CodeGenConstants.EnumWrapperPrefix}{enumTypeName}{CodeGenConstants.EnumWrapperSuffix}";
            result.ConvertedValueVar = $"{resultPrefix}Enum";
            
            builder.AppendVarDeclaration(result.ConvertedValueVar, CodeBuilder.BuildEnumWrapper(enumTypeName, valueExpr));
            
            result.NeedsCloseBlock = false;
            result.Success = true;
            
            return result;
        }
        
        /// <summary>
        /// 生成直接转换：基本类型、非托管结构体等
        /// </summary>
        private static ConversionResult GenerateDirectConversion(CodeBuilder builder, Type type, string valueExpr, string resultPrefix)
        {
            var result = new ConversionResult();
            result.TargetTypeName = TypeHelper.IsUnmanagedStructType(type)
                ? TypeHelper.GetGlobalQualifiedTypeName(type)
                : TypeHelper.GetUnmanagedTypeName(type);
            result.ConvertedValueVar = $"{resultPrefix}Direct";
            
            builder.AppendVarDeclaration(result.ConvertedValueVar, valueExpr);
            
            result.NeedsCloseBlock = false;
            result.Success = true;
            
            return result;
        }
        
        #endregion
    }
}
