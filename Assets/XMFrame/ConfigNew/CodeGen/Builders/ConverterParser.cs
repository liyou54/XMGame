using System;
using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen.Builders
{
    /// <summary>
    /// 转换器支持代码生成器
    /// 处理 [XmlTypeConverter] 特性
    /// </summary>
    public static class ConverterParser
    {
        /// <summary>
        /// 检查字段是否需要转换器
        /// </summary>
        public static bool NeedsConverter(ConfigFieldMetadata field)
        {
            return field.Converter != null && field.Converter.NeedsConverter;
        }
        
        /// <summary>
        /// 生成转换器调用代码（在基本解析之后）
        /// </summary>
        public static void GenerateConverterCall(CodeBuilder builder, ConfigFieldMetadata field, string sourceVar, string targetVar)
        {
            if (!NeedsConverter(field))
                return;
            
            var converter = field.Converter.GetEffectiveConverter();
            if (converter == null)
                return;
            
            var converterTypeName = TypeHelper.GetGlobalQualifiedTypeName(converter.ConverterType);
            
            builder.AppendComment($"应用转换器: {converter.ConverterType.Name}");
            builder.BeginIfBlock($"{converterTypeName}.Convert({sourceVar}, out var {targetVar})");
            builder.AppendLine($"return {targetVar};");
            builder.EndBlock();
            builder.AppendLine();
        }
    }
}
