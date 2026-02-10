using System;
using XM.ConfigNew.CodeGen;
using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen.Builders
{
    /// <summary>
    /// 基本类型解析代码生成器
    /// 支持: int, float, bool, string, 枚举, 可空类型
    /// </summary>
    public static class BasicTypeParser
    {
        /// <summary>
        /// 生成基本类型的解析逻辑代码
        /// </summary>
        /// <param name="builder">代码构建器</param>
        /// <param name="field">字段元数据</param>
        public static void GenerateParseLogic(CodeBuilder builder, ConfigFieldMetadata field)
        {
            var typeInfo = field.TypeInfo;
            var fieldName = field.FieldName;
            var xmlName = field.GetEffectiveXmlName();
            
            // 获取实际类型（处理可空）
            Type actualType = typeInfo.ManagedFieldType;
            bool isNullable = typeInfo.IsNullable;
            
            if (isNullable && typeInfo.UnderlyingType != null)
            {
                actualType = typeInfo.UnderlyingType;
            }
            
            // 获取 XML 值
            builder.AppendVarDeclaration("xmlValue", $"{CodeGenConstants.ConfigParseHelperFullName}.GetXmlFieldValue(configItem, \"{xmlName}\")");
            builder.AppendLine();
            
            // 处理默认值
            if (!string.IsNullOrEmpty(field.DefaultValue))
            {
                builder.AppendComment($"默认值: {field.DefaultValue}");
                builder.BeginIfBlock("string.IsNullOrEmpty(xmlValue)");
                builder.AppendAssignment("xmlValue", $"\"{field.DefaultValue}\"");
                builder.EndBlock();
                builder.AppendLine();
            }
            
            // 如果没有值
            if (isNullable)
            {
                builder.BeginIfBlock("string.IsNullOrEmpty(xmlValue)");
                builder.AppendLine("return null;");
                builder.EndBlock();
                builder.AppendLine();
            }
            else if (string.IsNullOrEmpty(field.DefaultValue))
            {
                builder.BeginIfBlock("string.IsNullOrEmpty(xmlValue)");
                builder.AppendLine("return default;");
                builder.EndBlock();
                builder.AppendLine();
            }
            
            // 根据类型生成解析代码
            if (actualType == typeof(int))
            {
                GenerateIntParse(builder, fieldName, isNullable);
            }
            else if (actualType == typeof(float))
            {
                GenerateFloatParse(builder, fieldName, isNullable);
            }
            else if (actualType == typeof(bool))
            {
                GenerateBoolParse(builder, fieldName, isNullable);
            }
            else if (actualType == typeof(long))
            {
                GenerateLongParse(builder, fieldName, isNullable);
            }
            else if (actualType == typeof(double))
            {
                GenerateDoubleParse(builder, fieldName, isNullable);
            }
            else if (actualType == typeof(short))
            {
                GenerateShortParse(builder, fieldName, isNullable);
            }
            else if (actualType == typeof(byte))
            {
                GenerateByteParse(builder, fieldName, isNullable);
            }
            else if (actualType == typeof(string))
            {
                GenerateStringParse(builder, field);
            }
            else if (actualType.IsEnum)
            {
                GenerateEnumParse(builder, actualType, fieldName, isNullable);
            }
            else if (TryGenerateConverterParse(builder, field, actualType, isNullable))
            {
                // 已通过 XmlTypeConverter（字段级或程序集级 [assembly: XmlTypeConverter(typeof(Xxx), true)]）生成
            }
            else
            {
                builder.AppendComment($"未知类型: {actualType?.FullName}");
                builder.AppendLine("return default;");
            }
        }
        
        /// <summary>
        /// 尝试通过 XmlTypeConverter 生成解析代码
        /// 优先使用字段的 Converter.Registrations（来自字段级特性或程序集 [assembly: XmlTypeConverter(typeof(Xxx), true)]）
        /// </summary>
        private static bool TryGenerateConverterParse(CodeBuilder builder, ConfigFieldMetadata field, Type targetType, bool isNullable)
        {
            var effective = field?.Converter?.GetEffectiveConverter();
            if (effective?.ConverterType == null)
                return false;
            
            // 验证转换器为 ITypeConverter&lt;string, targetType&gt;（XML 字符串 -> 目标类型）
            foreach (var i in effective.ConverterType.GetInterfaces())
            {
                if (!i.IsGenericType || i.GetGenericTypeDefinition().Name != "ITypeConverter`2")
                    continue;
                var args = i.GetGenericArguments();
                if (args.Length != 2 || args[0] != typeof(string) || args[1] != targetType)
                    continue;
                
                var converterTypeName = TypeHelper.GetGlobalQualifiedTypeName(effective.ConverterType);
                var converterInvoke = TypeHelper.IsTypeConverterBase(effective.ConverterType)
                    ? $"{converterTypeName}.I.Convert(xmlValue, mod.Name, out var parsedValue)"
                    : $"new {converterTypeName}().Convert(xmlValue, mod.Name, out var parsedValue)";
                builder.AppendComment($"通过 XmlTypeConverter: {effective.ConverterType.Name}");
                builder.BeginIfBlock(converterInvoke);
                builder.AppendLine("return parsedValue;");
                builder.EndBlock();
                builder.AppendLine();
                builder.AppendLine(isNullable ? "return null;" : "return default;");
                return true;
            }
            return false;
        }
        
        #region 数值类型解析
        
        private static void GenerateIntParse(CodeBuilder builder, string fieldName, bool isNullable)
        {
            builder.BeginIfBlock($"{CodeGenConstants.ConfigParseHelperFullName}.TryParseInt(xmlValue, \"{fieldName}\", out var parsedValue)");
            builder.AppendLine("return parsedValue;");
            builder.EndBlock();
            builder.AppendLine();
            builder.AppendLine(isNullable ? "return null;" : "return default;");
        }
        
        private static void GenerateFloatParse(CodeBuilder builder, string fieldName, bool isNullable)
        {
            builder.BeginIfBlock($"{CodeGenConstants.ConfigParseHelperFullName}.TryParseFloat(xmlValue, \"{fieldName}\", out var parsedValue)");
            builder.AppendLine("return parsedValue;");
            builder.EndBlock();
            builder.AppendLine();
            builder.AppendLine(isNullable ? "return null;" : "return default;");
        }
        
        private static void GenerateBoolParse(CodeBuilder builder, string fieldName, bool isNullable)
        {
            builder.BeginIfBlock($"{CodeGenConstants.ConfigParseHelperFullName}.TryParseBool(xmlValue, \"{fieldName}\", out var parsedValue)");
            builder.AppendLine("return parsedValue;");
            builder.EndBlock();
            builder.AppendLine();
            builder.AppendLine(isNullable ? "return null;" : "return default;");
        }
        
        private static void GenerateLongParse(CodeBuilder builder, string fieldName, bool isNullable)
        {
            builder.BeginIfBlock($"{CodeGenConstants.ConfigParseHelperFullName}.TryParseLong(xmlValue, \"{fieldName}\", out var parsedValue)");
            builder.AppendLine("return parsedValue;");
            builder.EndBlock();
            builder.AppendLine();
            builder.AppendLine(isNullable ? "return null;" : "return default;");
        }
        
        private static void GenerateDoubleParse(CodeBuilder builder, string fieldName, bool isNullable)
        {
            builder.BeginIfBlock($"{CodeGenConstants.ConfigParseHelperFullName}.TryParseDouble(xmlValue, \"{fieldName}\", out var parsedValue)");
            builder.AppendLine("return parsedValue;");
            builder.EndBlock();
            builder.AppendLine();
            builder.AppendLine(isNullable ? "return null;" : "return default;");
        }
        
        private static void GenerateShortParse(CodeBuilder builder, string fieldName, bool isNullable)
        {
            builder.BeginIfBlock($"{CodeGenConstants.ConfigParseHelperFullName}.TryParseShort(xmlValue, \"{fieldName}\", out var parsedValue)");
            builder.AppendLine("return parsedValue;");
            builder.EndBlock();
            builder.AppendLine();
            builder.AppendLine(isNullable ? "return null;" : "return default;");
        }
        
        private static void GenerateByteParse(CodeBuilder builder, string fieldName, bool isNullable)
        {
            builder.BeginIfBlock($"{CodeGenConstants.ConfigParseHelperFullName}.TryParseByte(xmlValue, \"{fieldName}\", out var parsedValue)");
            builder.AppendLine("return parsedValue;");
            builder.EndBlock();
            builder.AppendLine();
            builder.AppendLine(isNullable ? "return null;" : "return default;");
        }
        
        #endregion
        
        #region 字符串解析
        
        /// <summary>
        /// 生成字符串类型解析
        /// </summary>
        private static void GenerateStringParse(CodeBuilder builder, ConfigFieldMetadata field)
        {
            // 字符串直接返回（转换在 AllocContainerWithFillImpl 中处理）
            builder.AppendComment("字符串类型直接返回");
            builder.AppendLine("return xmlValue ?? string.Empty;");
        }
        
        #endregion
        
        #region 枚举解析
        
        /// <summary>
        /// 生成枚举类型解析
        /// </summary>
        private static void GenerateEnumParse(CodeBuilder builder, Type enumType, string fieldName, bool isNullable)
        {
            var enumTypeName = TypeHelper.GetGlobalQualifiedTypeName(enumType);
            
            builder.BeginIfBlock($"{CodeGenConstants.EnumFullName}.TryParse<{enumTypeName}>(xmlValue, out var parsedValue)");
            builder.AppendLine("return parsedValue;");
            builder.EndBlock();
            builder.AppendLine();
            
            // 解析失败
            builder.AppendLine($"{CodeGenConstants.ConfigParseHelperFullName}.LogParseError(context, \"{fieldName}\", $\"无法解析枚举值: {{xmlValue}}\");");
            builder.AppendLine(isNullable ? "return null;" : "return default;");
        }
        
        #endregion
    }
}
