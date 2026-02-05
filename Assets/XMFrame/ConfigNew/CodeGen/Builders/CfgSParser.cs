using System;
using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen.Builders
{
    /// <summary>
    /// CfgS 和 Link 字段解析代码生成器
    /// </summary>
    public static class CfgSParser
    {
        /// <summary>
        /// 生成 CfgS 字段解析逻辑
        /// </summary>
        public static void GenerateParseLogic(CodeBuilder builder, ConfigFieldMetadata field)
        {
            var fieldName = field.FieldName;
            var xmlName = field.GetEffectiveXmlName();
            var targetType = field.XmlLinkTargetType;
            
            if (targetType == null)
            {
                builder.AppendComment("Link 目标类型未知");
                builder.AppendLine("return default;");
                return;
            }
            
            // CfgS<T> 应该使用托管类型，而不是非托管类型
            var targetNamespace = targetType.Namespace;
            var qualifiedManagedType = !string.IsNullOrEmpty(targetNamespace)
                ? $"global::{targetNamespace}.{targetType.Name}"
                : targetType.Name;
            
            builder.AppendComment("解析 CfgS 引用字符串");
            builder.AppendVarDeclaration("cfgSString", $"{CodeGenConstants.ConfigParseHelperFullName}.GetXmlFieldValue(configItem, \"{xmlName}\")");
            builder.BeginIfBlock("string.IsNullOrEmpty(cfgSString)");
            builder.AppendLine("return default;");
            builder.EndBlock();
            builder.AppendLine();
            
            builder.BeginIfBlock($"{CodeGenConstants.ConfigParseHelperFullName}.TryParseCfgSString(cfgSString, \"{fieldName}\", out var modName, out var cfgName)");
            builder.AppendLine($"return new global::XM.Contracts.Config.CfgS<{qualifiedManagedType}>(new global::XM.Contracts.Config.ModS(modName), cfgName);");
            builder.EndBlock();
            builder.AppendLine();
            
            builder.AppendLine($"{CodeGenConstants.ConfigParseHelperFullName}.LogParseError(context, \"{fieldName}\", $\"无法解析 CfgS 字符串: {{cfgSString}}\");");
            builder.AppendLine("return default;");
        }
    }
}
