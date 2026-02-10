using System;
using XM.ConfigNew.CodeGen.Builders;
using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen.Strategies.Parse
{
    /// <summary>
    /// XmlKey 字段解析策略
    /// XmlKey 字段从 configName 参数读取，而不是从 XML field 读取
    /// </summary>
    public class XmlKeyParseStrategy : IParseStrategy
    {
        public bool CanHandle(ConfigFieldMetadata field)
        {
            if (field == null)
                return false;
            
            // 只处理标记了 [XmlKey] 的字段
            return field.IsXmlKey;
        }
        
        public void Generate(CodeGenContext ctx)
        {
            var field = ctx.FieldMetadata;
            var builder = ctx.Builder;
            
            // XmlKey 字段直接从 configName 参数读取
            builder.AppendComment($"XmlKey 字段: 从 configName 参数读取");
            
            var typeInfo = field.TypeInfo;
            var actualType = typeInfo.ManagedFieldType;
            
            // 特殊处理：如果 XmlKey 字段是 CfgS 类型
            if (actualType != null && TypeHelper.IsCfgSType(actualType))
            {
                GenerateCfgSParse(builder, field);
                return;
            }
            
            bool isNullable = typeInfo.IsNullable;
            
            if (isNullable && typeInfo.UnderlyingType != null)
            {
                actualType = typeInfo.UnderlyingType;
            }
            
            // 根据字段类型生成转换代码
            if (actualType == typeof(int))
            {
                GenerateIntParse(builder, field.FieldName, isNullable);
            }
            else if (actualType == typeof(long))
            {
                GenerateLongParse(builder, field.FieldName, isNullable);
            }
            else if (actualType == typeof(float))
            {
                GenerateFloatParse(builder, field.FieldName, isNullable);
            }
            else if (actualType == typeof(double))
            {
                GenerateDoubleParse(builder, field.FieldName, isNullable);
            }
            else if (actualType == typeof(bool))
            {
                GenerateBoolParse(builder, field.FieldName, isNullable);
            }
            else if (actualType == typeof(string))
            {
                GenerateStringParse(builder);
            }
            else if (actualType.IsEnum)
            {
                GenerateEnumParse(builder, field.FieldName, actualType, isNullable);
            }
            else if (actualType.Name == "StrI")
            {
                GenerateStrIParse(builder);
            }
            else if (actualType.Name == "LabelI")
            {
                GenerateLabelIParse(builder);
            }
            else
            {
                // 其他类型暂时返回 configName 字符串
                builder.AppendComment($"未知 XmlKey 类型: {actualType?.FullName}，返回 configName 字符串");
                builder.AppendLine("return configName;");
            }
        }
        
        #region 类型转换方法
        
        private void GenerateIntParse(CodeBuilder builder, string fieldName, bool isNullable)
        {
            builder.BeginIfBlock($"{CodeGenConstants.ConfigParseHelperFullName}.TryParseInt(configName, \"{fieldName}\", out var parsedValue)");
            builder.AppendLine("return parsedValue;");
            builder.EndBlock();
            builder.AppendLine();
            builder.AppendLine(isNullable ? "return null;" : "return default;");
        }
        
        private void GenerateLongParse(CodeBuilder builder, string fieldName, bool isNullable)
        {
            builder.BeginIfBlock($"{CodeGenConstants.ConfigParseHelperFullName}.TryParseLong(configName, \"{fieldName}\", out var parsedValue)");
            builder.AppendLine("return parsedValue;");
            builder.EndBlock();
            builder.AppendLine();
            builder.AppendLine(isNullable ? "return null;" : "return default;");
        }
        
        private void GenerateStringParse(CodeBuilder builder)
        {
            builder.AppendLine("return configName ?? string.Empty;");
        }
        
        private void GenerateFloatParse(CodeBuilder builder, string fieldName, bool isNullable)
        {
            builder.BeginIfBlock($"{CodeGenConstants.ConfigParseHelperFullName}.TryParseFloat(configName, \"{fieldName}\", out var parsedValue)");
            builder.AppendLine("return parsedValue;");
            builder.EndBlock();
            builder.AppendLine();
            builder.AppendLine(isNullable ? "return null;" : "return default;");
        }
        
        private void GenerateDoubleParse(CodeBuilder builder, string fieldName, bool isNullable)
        {
            builder.BeginIfBlock($"{CodeGenConstants.ConfigParseHelperFullName}.TryParseDouble(configName, \"{fieldName}\", out var parsedValue)");
            builder.AppendLine("return parsedValue;");
            builder.EndBlock();
            builder.AppendLine();
            builder.AppendLine(isNullable ? "return null;" : "return default;");
        }
        
        private void GenerateBoolParse(CodeBuilder builder, string fieldName, bool isNullable)
        {
            builder.BeginIfBlock($"{CodeGenConstants.ConfigParseHelperFullName}.TryParseBool(configName, \"{fieldName}\", out var parsedValue)");
            builder.AppendLine("return parsedValue;");
            builder.EndBlock();
            builder.AppendLine();
            builder.AppendLine(isNullable ? "return null;" : "return default;");
        }
        
        private void GenerateEnumParse(CodeBuilder builder, string fieldName, Type enumType, bool isNullable)
        {
            var enumTypeName = TypeHelper.GetGlobalQualifiedTypeName(enumType);
            builder.BeginIfBlock($"{CodeGenConstants.ConfigParseHelperFullName}.TryParseEnum<{enumTypeName}>(configName, \"{fieldName}\", out var parsedValue)");
            builder.AppendLine("return parsedValue;");
            builder.EndBlock();
            builder.AppendLine();
            builder.AppendLine(isNullable ? "return null;" : "return default;");
        }
        
        private void GenerateStrIParse(CodeBuilder builder)
        {
            builder.AppendComment("StrI 类型：通过字符串查找 StrI");
            builder.BeginIfBlock("string.IsNullOrEmpty(configName)");
            builder.AppendLine("return default;");
            builder.EndBlock();
            builder.AppendLine();
            builder.AppendLine("return global::XM.StrI.Get(configName);");
        }
        
        private void GenerateLabelIParse(CodeBuilder builder)
        {
            builder.AppendComment("LabelI 类型：通过字符串查找 LabelI");
            builder.BeginIfBlock("string.IsNullOrEmpty(configName)");
            builder.AppendLine("return default;");
            builder.EndBlock();
            builder.AppendLine();
            builder.AppendLine("return global::XM.LabelI.Get(configName);");
        }
        
        private void GenerateCfgSParse(CodeBuilder builder, ConfigFieldMetadata field)
        {
            // 从 CfgS<T> 类型中提取泛型参数 T
            var cfgSType = field.TypeInfo.ManagedFieldType;
            Type targetType = null;
            
            if (cfgSType.IsGenericType)
            {
                var genericArgs = cfgSType.GetGenericArguments();
                if (genericArgs.Length > 0)
                {
                    targetType = genericArgs[0];
                }
            }
            
            // 如果有 XmlLinkTargetType，优先使用它
            if (field.XmlLinkTargetType != null)
            {
                targetType = field.XmlLinkTargetType;
            }
            
            if (targetType == null)
            {
                builder.AppendComment("无法确定 CfgS 目标类型");
                builder.AppendLine("return default;");
                return;
            }
            
            // CfgS<T> 应该使用托管类型
            var targetNamespace = targetType.Namespace;
            var qualifiedManagedType = !string.IsNullOrEmpty(targetNamespace)
                ? $"global::{targetNamespace}.{targetType.Name}"
                : targetType.Name;
            
            builder.AppendComment("CfgS 类型：从 configName 参数读取并解析");
            builder.BeginIfBlock("string.IsNullOrEmpty(configName)");
            builder.AppendLine("return default;");
            builder.EndBlock();
            builder.AppendLine();
            
            builder.AppendComment("尝试解析 CfgS 格式（ModName::ConfigName）");
            builder.BeginIfBlock($"{CodeGenConstants.ConfigParseHelperFullName}.TryParseCfgSString(configName, \"{field.FieldName}\", out var modName, out var cfgName)");
            builder.AppendLine($"return new global::XM.Contracts.Config.CfgS<{qualifiedManagedType}>(new global::XM.Contracts.Config.ModS(modName), cfgName);");
            builder.EndBlock();
            builder.AppendLine();
            
            builder.AppendComment("如果 configName 不包含 :: 分隔符，使用当前 mod.Name 补充");
            builder.AppendLine($"return new global::XM.Contracts.Config.CfgS<{qualifiedManagedType}>(mod, configName);");
        }
        
        #endregion
    }
}
