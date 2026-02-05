using System;
using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen.Builders
{
    /// <summary>
    /// 容器类型解析代码生成器
    /// 支持: List, Dictionary, HashSet, 嵌套容器
    /// </summary>
    public static class ContainerParser
    {
        /// <summary>
        /// 生成容器解析逻辑
        /// </summary>
        public static void GenerateParseLogic(CodeBuilder builder, ConfigFieldMetadata field)
        {
            var typeInfo = field.TypeInfo;
            
            switch (typeInfo.ContainerType)
            {
                case EContainerType.List:
                    GenerateListParse(builder, field);
                    break;
                    
                case EContainerType.Dictionary:
                    GenerateDictionaryParse(builder, field);
                    break;
                    
                case EContainerType.HashSet:
                    GenerateHashSetParse(builder, field);
                    break;
                    
                default:
                    builder.AppendComment($"不支持的容器类型: {typeInfo.ContainerType}");
                    builder.AppendLine($"return new {field.ManagedFieldTypeName}();");
                    break;
            }
        }
        
        #region List 解析
        
        /// <summary>
        /// 生成 List 解析逻辑
        /// </summary>
        private static void GenerateListParse(CodeBuilder builder, ConfigFieldMetadata field)
        {
            var fieldName = field.FieldName;
            var xmlName = field.GetEffectiveXmlName();
            var elementType = field.TypeInfo.NestedValueType;
            var elementTypeName = TypeHelper.GetGlobalQualifiedTypeName(elementType);
            var listTypeName = field.ManagedFieldTypeName;
            
            builder.AppendLine($"var list = new {listTypeName}();");
            builder.AppendLine();
            
            // 1. 尝试从 XML 节点解析
            builder.AppendComment("尝试从 XML 节点解析");
            builder.AppendLine($"var nodes = configItem.SelectNodes(\"{xmlName}\");");
            builder.BeginIfBlock("nodes != null");
            builder.BeginForeachLoop(CodeGenConstants.XmlNodeFullName, "node", "nodes");
            
            builder.AppendVarDeclaration("element", $"node as {CodeGenConstants.XmlElementFullName}");
            builder.AppendNullContinue("element");
            builder.AppendLine();
            
            builder.AppendLine("var text = element.InnerText?.Trim();");
            builder.BeginIfBlock("string.IsNullOrEmpty(text)");
            builder.AppendLine("continue;");
            builder.EndBlock();
            builder.AppendLine();
            
            // 解析元素
            if (GenerateElementParse(builder, elementType, fieldName, "text", "parsedItem"))
            {
                builder.AppendLine("list.Add(parsedItem);");
                builder.EndBlock(); // if TryParse
            }
            
            builder.EndBlock(); // foreach
            builder.EndBlock(); // if nodes != null
            builder.AppendLine();
            
            // 2. 如果没有节点，尝试 CSV 格式（仅支持单层）
            if (!field.TypeInfo.IsValueContainer)
            {
                builder.AppendComment("如果没有节点，尝试 CSV 格式");
                builder.BeginIfBlock("list.Count == 0");
                builder.AppendLine($"var csvValue = {CodeGenConstants.ConfigParseHelperFullName}.GetXmlFieldValue(configItem, \"{xmlName}\");");
                builder.BeginIfBlock("!string.IsNullOrEmpty(csvValue)");
                
                GenerateCSVParse(builder, elementType, fieldName, "csvValue");
                
                builder.EndBlock(); // if csvValue
                builder.EndBlock(); // if list.Count == 0
                builder.AppendLine();
            }
            
            // 3. 处理默认值
            if (!string.IsNullOrEmpty(field.DefaultValue) && !field.TypeInfo.IsValueContainer)
            {
                builder.AppendComment($"如果仍为空，使用默认值: {field.DefaultValue}");
                builder.BeginIfBlock("list.Count == 0");
                builder.AppendLine($"var defaultValue = \"{field.DefaultValue}\";");
                GenerateCSVParse(builder, elementType, fieldName, "defaultValue");
                builder.EndBlock();
                builder.AppendLine();
            }
            
            builder.AppendLine("return list;");
        }
        
        /// <summary>
        /// 生成 CSV 格式解析（用于容器默认值）
        /// </summary>
        private static void GenerateCSVParse(CodeBuilder builder, Type elementType, string fieldName, string csvVariable)
        {
            builder.AppendLine($"var parts = {csvVariable}.Split(new[] {{ {CodeGenConstants.CsvSeparatorsCode} }}, {CodeGenConstants.StringSplitOptionsFullName}.RemoveEmptyEntries);");
            builder.BeginForeachLoop("var", "part", "parts");
            builder.AppendLine("var trimmed = part.Trim();");
            builder.BeginIfBlock("string.IsNullOrEmpty(trimmed)");
            builder.AppendLine("continue;");
            builder.EndBlock();
            builder.AppendLine();
            
            // 需要检查返回值，如果 GenerateElementParse 开始了 if 块，需要关闭它
            if (GenerateElementParse(builder, elementType, fieldName, "trimmed", "parsedItem"))
            {
                builder.AppendLine("list.Add(parsedItem);");
                builder.EndBlock(); // if TryParse
            }
            
            builder.EndBlock(); // foreach
        }
        
        /// <summary>
        /// 生成单个元素的解析代码
        /// </summary>
        /// <returns>是否需要调用方添加 EndBlock()</returns>
        private static bool GenerateElementParse(CodeBuilder builder, Type elementType, string fieldName, string sourceVar, string targetVar)
        {
            // 检查是否是嵌套容器（不支持 CSV 解析）
            if (TypeHelper.IsContainerType(elementType))
            {
                builder.AppendComment($"嵌套容器不支持从文本解析: {TypeHelper.GetSimpleTypeName(elementType)}");
                builder.AppendLine("continue;");
                return false;
            }
            
            if (elementType == typeof(int))
            {
                builder.BeginIfBlock($"{CodeGenConstants.ConfigParseHelperFullName}.TryParseInt({sourceVar}, \"{fieldName}\", out var {targetVar})");
                return true;
            }
            else if (elementType == typeof(float))
            {
                builder.BeginIfBlock($"{CodeGenConstants.ConfigParseHelperFullName}.TryParseFloat({sourceVar}, \"{fieldName}\", out var {targetVar})");
                return true;
            }
            else if (elementType == typeof(bool))
            {
                builder.BeginIfBlock($"{CodeGenConstants.ConfigParseHelperFullName}.TryParseBool({sourceVar}, \"{fieldName}\", out var {targetVar})");
                return true;
            }
            else if (elementType == typeof(long))
            {
                builder.BeginIfBlock($"{CodeGenConstants.ConfigParseHelperFullName}.TryParseLong({sourceVar}, \"{fieldName}\", out var {targetVar})");
                return true;
            }
            else if (elementType == typeof(double))
            {
                builder.BeginIfBlock($"{CodeGenConstants.ConfigParseHelperFullName}.TryParseDouble({sourceVar}, \"{fieldName}\", out var {targetVar})");
                return true;
            }
            else if (elementType == typeof(string))
            {
                builder.AppendLine($"var {targetVar} = {sourceVar};");
                builder.BeginIfBlock($"!string.IsNullOrEmpty({targetVar})");
                return true;
            }
            else if (elementType.IsEnum)
            {
                var enumTypeName = TypeHelper.GetGlobalQualifiedTypeName(elementType);
                builder.BeginIfBlock($"{CodeGenConstants.EnumFullName}.TryParse<{enumTypeName}>({sourceVar}, out var {targetVar})");
                return true;
            }
            else
            {
                // 其他类型（如嵌套配置）不支持从文本解析
                builder.AppendComment($"类型 {TypeHelper.GetSimpleTypeName(elementType)} 不支持从文本解析");
                builder.AppendLine("continue;");
                return false;
            }
        }
        
        #endregion
        
        #region Dictionary 解析
        
        /// <summary>
        /// 生成 Dictionary 解析逻辑
        /// </summary>
        private static void GenerateDictionaryParse(CodeBuilder builder, ConfigFieldMetadata field)
        {
            var fieldName = field.FieldName;
            var xmlName = field.GetEffectiveXmlName();
            var keyType = field.TypeInfo.NestedKeyType;
            var valueType = field.TypeInfo.NestedValueType;
            var dictTypeName = field.ManagedFieldTypeName;
            
            builder.AppendLine($"var dict = new {dictTypeName}();");
            builder.AppendLine();
            
            builder.AppendComment("解析 Dictionary Item 节点");
            builder.AppendLine($"var dictNodes = configItem.SelectNodes(\"{xmlName}/Item\");");
            builder.BeginIfBlock("dictNodes != null");
            builder.BeginForeachLoop(CodeGenConstants.XmlNodeFullName, "node", "dictNodes");
            
            builder.AppendVarDeclaration("element", $"node as {CodeGenConstants.XmlElementFullName}");
            builder.AppendNullContinue("element");
            builder.AppendLine();
            
            // 获取 Key 和 Value
            builder.AppendLine("var keyText = element.GetAttribute(\"Key\");");
            builder.AppendLine("var valueText = element.InnerText?.Trim();");
            builder.AppendLine();
            
            // 解析 Key
            if (GenerateElementParse(builder, keyType, fieldName + ".Key", "keyText", "parsedKey"))
            {
                // 解析 Value  
                if (GenerateElementParse(builder, valueType, fieldName + ".Value", "valueText", "parsedValue"))
                {
                    builder.AppendLine("dict[parsedKey] = parsedValue;");
                    builder.EndBlock(); // if TryParse value
                }
                builder.EndBlock(); // if TryParse key
            }
            
            builder.EndBlock(); // foreach
            builder.EndBlock(); // if dictNodes
            builder.AppendLine();
            
            builder.AppendLine("return dict;");
        }
        
        #endregion
        
        #region HashSet 解析
        
        /// <summary>
        /// 生成 HashSet 解析逻辑
        /// </summary>
        private static void GenerateHashSetParse(CodeBuilder builder, ConfigFieldMetadata field)
        {
            var fieldName = field.FieldName;
            var xmlName = field.GetEffectiveXmlName();
            var elementType = field.TypeInfo.NestedValueType;
            var setTypeName = field.ManagedFieldTypeName;
            
            builder.AppendLine($"var set = new {setTypeName}();");
            builder.AppendLine();
            
            // 从 XML 节点解析
            builder.AppendComment("从 XML 节点解析");
            builder.AppendLine($"var nodes = configItem.SelectNodes(\"{xmlName}\");");
            builder.BeginIfBlock("nodes != null");
            builder.BeginForeachLoop(CodeGenConstants.XmlNodeFullName, "node", "nodes");
            
            builder.AppendVarDeclaration("element", $"node as {CodeGenConstants.XmlElementFullName}");
            builder.AppendNullContinue("element");
            builder.AppendLine();
            
            builder.AppendLine("var text = element.InnerText?.Trim();");
            builder.BeginIfBlock("string.IsNullOrEmpty(text)");
            builder.AppendLine("continue;");
            builder.EndBlock();
            builder.AppendLine();
            
            if (GenerateElementParse(builder, elementType, fieldName, "text", "parsedItem"))
            {
                builder.AppendLine("set.Add(parsedItem);");
                builder.EndBlock(); // if TryParse
            }
            
            builder.EndBlock(); // foreach
            builder.EndBlock(); // if nodes
            builder.AppendLine();
            
            // CSV 格式备用
            if (!field.TypeInfo.IsValueContainer)
            {
                builder.AppendComment("CSV 格式备用");
                builder.BeginIfBlock("set.Count == 0");
                builder.AppendLine($"var csvValue = {CodeGenConstants.ConfigParseHelperFullName}.GetXmlFieldValue(configItem, \"{xmlName}\");");
                builder.BeginIfBlock("!string.IsNullOrEmpty(csvValue)");
                
                builder.AppendLine($"var parts = csvValue.Split(new[] {{ {CodeGenConstants.CsvSeparatorsCode} }}, {CodeGenConstants.StringSplitOptionsFullName}.RemoveEmptyEntries);");
                builder.BeginForeachLoop("var", "part", "parts");
                builder.AppendLine("var trimmed = part.Trim();");
                if (GenerateElementParse(builder, elementType, fieldName, "trimmed", "parsedItem"))
                {
                    builder.AppendLine("set.Add(parsedItem);");
                    builder.EndBlock(); // if TryParse
                }
                builder.EndBlock(); // foreach
                
                builder.EndBlock(); // if csvValue
                builder.EndBlock(); // if set.Count == 0
                builder.AppendLine();
            }
            
            builder.AppendLine("return set;");
        }
        
        #endregion
    }
}
