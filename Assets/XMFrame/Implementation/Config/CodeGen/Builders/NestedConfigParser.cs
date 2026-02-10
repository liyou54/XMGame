using System;
using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen.Builders
{
    /// <summary>
    /// 嵌套配置解析代码生成器
    /// </summary>
    public static class NestedConfigParser
    {
        /// <summary>
        /// 生成嵌套配置解析逻辑（单个）
        /// </summary>
        public static void GenerateSingleParse(CodeBuilder builder, ConfigFieldMetadata field)
        {
            var fieldName = field.FieldName;
            var xmlName = field.GetEffectiveXmlName();
            var nestedType = field.TypeInfo.SingleValueType;
            var nestedTypeName = TypeHelper.GetGlobalQualifiedTypeName(nestedType);
            
            builder.AppendComment("解析嵌套配置");
            builder.AppendVarDeclaration("element", $"configItem.SelectSingleNode(\"{xmlName}\") as {CodeGenConstants.XmlElementFullName}");
            builder.BeginIfBlock(CodeBuilder.BuildNotNullCondition("element") + " == false");
            builder.AppendLine("return null;");
            builder.EndBlock();
            builder.AppendLine();
            
            builder.AppendVarDeclaration("helper", $"global::XM.Contracts.IConfigManager.I?.GetClassHelper(typeof({nestedTypeName}))");
            builder.BeginIfBlock(CodeBuilder.BuildNotNullCondition("helper") + " == false");
            builder.AppendLine($"{CodeGenConstants.ConfigParseHelperFullName}.LogParseError(context, \"{fieldName}\", \"无法获取嵌套配置 Helper\");");
            builder.AppendLine("return null;");
            builder.EndBlock();
            builder.AppendLine();
            
            builder.AppendVarDeclaration("nestedConfigName", $"configName + \"_{fieldName}\"");
            builder.AppendLine($"return ({nestedTypeName})helper.DeserializeConfigFromXml(element, mod, nestedConfigName, context);");
        }
        
        /// <summary>
        /// 生成嵌套配置列表解析逻辑
        /// </summary>
        public static void GenerateListParse(CodeBuilder builder, ConfigFieldMetadata field)
        {
            var fieldName = field.FieldName;
            var xmlName = field.GetEffectiveXmlName();
            var nestedType = field.TypeInfo.SingleValueType;
            var nestedTypeName = TypeHelper.GetGlobalQualifiedTypeName(nestedType);
            var listTypeName = field.ManagedFieldTypeName;
            
            builder.AppendNewVarDeclaration("list", listTypeName);
            builder.AppendLine();
            
            builder.AppendComment("解析嵌套配置列表");
            builder.AppendVarDeclaration("nodes", $"configItem.SelectNodes(\"{xmlName}\")");
            builder.BeginIfBlock(CodeBuilder.BuildNotNullCondition("nodes"));
            
            builder.AppendVarDeclaration("helper", $"global::XM.Contracts.IConfigManager.I?.GetClassHelper(typeof({nestedTypeName}))");
            builder.BeginIfBlock(CodeBuilder.BuildNotNullCondition("helper"));
            
            builder.BeginForeachLoop(CodeGenConstants.XmlNodeFullName, "node", "nodes");
            builder.AppendVarDeclaration("element", $"node as {CodeGenConstants.XmlElementFullName}");
            builder.AppendNullContinue("element");
            builder.AppendLine();
            
            builder.AppendVarDeclaration("nestedConfigName", $"configName + \"_{fieldName}_\" + list.{CodeGenConstants.CountProperty}");
            builder.AppendVarDeclaration("item", $"({nestedTypeName})helper.DeserializeConfigFromXml(element, mod, nestedConfigName, context)");
            builder.BeginIfBlock(CodeBuilder.BuildNotNullCondition("item"));
            builder.AppendLine("list.Add(item);");
            builder.EndBlock();
            
            builder.EndBlock(); // foreach
            builder.EndBlock(); // if helper
            builder.EndBlock(); // if nodes
            builder.AppendLine();
            
            builder.AppendLine("return list;");
        }
    }
}
