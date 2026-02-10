using System;
using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen.Builders
{
    /// <summary>
    /// CfgS 和 XMLLink 字段解析代码生成器
    /// </summary>
    public static class CfgSParser
    {
        /// <summary>
        /// 生成 CfgS/XMLLink 字段解析逻辑
        /// CfgS 和 XMLLink 字段都从 configName 参数读取（id 属性值），而不是从 XML field 读取
        /// XMLLink 字段解析出的 CfgS 会被转换为 CfgI 并存储到 _Parent 字段
        /// </summary>
        public static void GenerateParseLogic(CodeBuilder builder, ConfigFieldMetadata field)
        {
            var fieldName = field.FieldName;
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
            
            builder.AppendComment("CfgS 字段：从 configName 参数读取（id 属性值）");
            builder.BeginIfBlock("string.IsNullOrEmpty(configName)");
            builder.AppendLine("return default;");
            builder.EndBlock();
            builder.AppendLine();
            
            builder.AppendComment("尝试解析 CfgS 格式（ModName::ConfigName）");
            builder.BeginIfBlock($"{CodeGenConstants.ConfigParseHelperFullName}.TryParseCfgSString(configName, \"{fieldName}\", out var modName, out var cfgName)");
            builder.AppendLine($"return new global::XM.Contracts.Config.CfgS<{qualifiedManagedType}>(new global::XM.Contracts.Config.ModS(modName), cfgName);");
            builder.EndBlock();
            builder.AppendLine();
            
            builder.AppendComment("如果 configName 不是 CfgS 格式（如 \"item_003\"），直接使用当前 mod");
            builder.AppendLine($"return new global::XM.Contracts.Config.CfgS<{qualifiedManagedType}>(mod, configName);");
        }
    }
}
