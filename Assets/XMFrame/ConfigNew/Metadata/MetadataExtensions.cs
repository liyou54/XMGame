using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XM.ConfigNew.Metadata
{
    /// <summary>
    /// FieldTypeInfo 扩展方法
    /// </summary>
    public static class FieldTypeInfoExtensions
    {
        /// <summary>
        /// 获取类型的完整描述(用于调试)
        /// </summary>
        public static string GetTypeDescription(this FieldTypeInfo typeInfo)
        {
            if (typeInfo == null)
                return "null";
            
            var sb = new StringBuilder();
            
            if (typeInfo.IsContainer)
            {
                sb.Append($"{typeInfo.ContainerType}");
                
                if (typeInfo.IsNestedContainer)
                    sb.Append($" (嵌套层级: {typeInfo.NestedLevel})");
                
                if (typeInfo.ContainerType == EContainerType.Dictionary)
                    sb.Append($"<{typeInfo.NestedKeyType?.Name}, {typeInfo.NestedValueType?.Name}>");
                else
                    sb.Append($"<{typeInfo.NestedValueType?.Name}>");
            }
            else
            {
                sb.Append(typeInfo.SingleValueType?.Name ?? "unknown");
            }
            
            if (typeInfo.IsNestedConfig)
                sb.Append(" (嵌套配置)");
            
            return sb.ToString();
        }
        
        /// <summary>
        /// 获取最终元素类型(递归到最深层)
        /// </summary>
        public static Type GetFinalElementType(this FieldTypeInfo typeInfo)
        {
            if (typeInfo == null)
                return null;
            
            // 如果是嵌套容器,递归获取
            if (typeInfo.IsNestedContainer && typeInfo.NestedValueTypeInfo != null)
                return typeInfo.NestedValueTypeInfo.GetFinalElementType();
            
            return typeInfo.SingleValueType;
        }
        
        /// <summary>
        /// 获取容器嵌套链的描述
        /// </summary>
        public static string GetContainerChainDescription(this FieldTypeInfo typeInfo)
        {
            if (typeInfo == null || !typeInfo.IsContainer)
                return string.Empty;
            
            if (typeInfo.NestedContainerChain == null || typeInfo.NestedContainerChain.Count == 0)
                return typeInfo.ContainerType.ToString();
            
            return string.Join(" -> ", typeInfo.NestedContainerChain);
        }
    }
    
    /// <summary>
    /// ConfigFieldMetadata 扩展方法
    /// </summary>
    public static class ConfigFieldMetadataExtensions
    {
        /// <summary>
        /// 获取字段的完整描述(用于调试)
        /// </summary>
        public static string GetFieldDescription(this ConfigFieldMetadata field)
        {
            if (field == null)
                return "null";
            
            var sb = new StringBuilder();
            sb.Append($"{field.FieldName}: ");
            
            if (field.TypeInfo != null)
                sb.Append(field.TypeInfo.GetTypeDescription());
            
            if (field.IsNotNull)
                sb.Append(" [必填]");
            
            if (!string.IsNullOrEmpty(field.DefaultValue))
                sb.Append($" [默认: {field.DefaultValue}]");
            
            if (field.IsIndexField)
                sb.Append($" [索引: {string.Join(", ", field.IndexNames?.Select(i => i.Item2.IndexName) ?? Enumerable.Empty<string>())}]");
            
            if (field.IsXmlLink)
                sb.Append($" [Link -> {field.XmlLinkTargetType?.Name}]");
            
            return sb.ToString();
        }
        
        /// <summary>
        /// 获取有效的XML字段名(优先使用XmlName,否则使用FieldName)
        /// </summary>
        public static string GetEffectiveXmlName(this ConfigFieldMetadata field)
        {
            return !string.IsNullOrEmpty(field?.XmlName) ? field.XmlName : field?.FieldName;
        }
        
        /// <summary>
        /// 获取字段参与的所有索引
        /// </summary>
        public static List<ConfigIndexMetadata> GetIndexes(this ConfigFieldMetadata field)
        {
            if (field?.IndexNames == null)
                return new List<ConfigIndexMetadata>();
            
            return field.IndexNames.Select(i => i.Item2).ToList();
        }
        
        /// <summary>
        /// 获取字段在指定索引中的位置
        /// </summary>
        public static int GetPositionInIndex(this ConfigFieldMetadata field, string indexName)
        {
            if (field?.IndexNames == null)
                return -1;
            
            var indexInfo = field.IndexNames.FirstOrDefault(i => i.Item2.IndexName == indexName);
            return indexInfo.Item2 != null ? indexInfo.Item1 : -1;
        }
        
        /// <summary>
        /// 是否需要Alloc方法(容器类型)
        /// </summary>
        public static bool NeedsAllocMethod(this ConfigFieldMetadata field)
        {
            return field != null && field.IsContainer;
        }
        
        /// <summary>
        /// 是否需要Fill方法(嵌套配置)
        /// </summary>
        public static bool NeedsFillMethod(this ConfigFieldMetadata field)
        {
            return field != null && field.IsNestedConfig && !field.IsContainer;
        }
        
        /// <summary>
        /// 获取有效的Key转换器
        /// </summary>
        public static ConverterRegistration GetEffectiveKeyConverter(this ConfigFieldMetadata field)
        {
            return field?.Converter?.GetEffectiveKeyConverter();
        }
        
        /// <summary>
        /// 获取有效的Value转换器
        /// </summary>
        public static ConverterRegistration GetEffectiveValueConverter(this ConfigFieldMetadata field)
        {
            return field?.Converter?.GetEffectiveValueConverter();
        }
    }
    
    /// <summary>
    /// ConverterInfo 扩展方法
    /// </summary>
    public static class ConverterInfoExtensions
    {
        /// <summary>
        /// 按优先级获取有效的转换器
        /// 优先级: 按Priority排序,Priority越小优先级越高
        /// 字段级(Priority=0) > Mod级(Priority=1) > 全局级(Priority=2)
        /// </summary>
        public static ConverterRegistration GetEffectiveConverter(this ConverterInfo converterInfo)
        {
            if (converterInfo?.Registrations == null || converterInfo.Registrations.Count == 0)
                return null;
            
            // 直接返回Priority最小的(列表已按Priority排序)
            return converterInfo.Registrations[0];
        }
        
        /// <summary>
        /// 获取有效的Key转换器(容器字段)
        /// </summary>
        public static ConverterRegistration GetEffectiveKeyConverter(this ConverterInfo converterInfo)
        {
            if (converterInfo?.KeyRegistrations == null || converterInfo.KeyRegistrations.Count == 0)
                return null;
            
            return converterInfo.KeyRegistrations[0];
        }
        
        /// <summary>
        /// 获取有效的Value转换器(容器字段)
        /// </summary>
        public static ConverterRegistration GetEffectiveValueConverter(this ConverterInfo converterInfo)
        {
            if (converterInfo?.ValueRegistrations == null || converterInfo.ValueRegistrations.Count == 0)
                return null;
            
            return converterInfo.ValueRegistrations[0];
        }
        
        /// <summary>
        /// 获取转换器的优先级描述(用于调试)
        /// </summary>
        public static string GetPriorityDescription(this ConverterInfo converterInfo)
        {
            var effective = converterInfo.GetEffectiveConverter();
            if (effective == null)
                return "无转换器";
            
            var scope = effective.IsGlobal ? "全局" : "字段";
            return $"{effective.Location}: {effective.ConverterType?.Name} (作用域: {scope}, Priority: {effective.Priority})";
        }
    }
    
    /// <summary>
    /// ConfigIndexMetadata 扩展方法
    /// </summary>
    public static class ConfigIndexMetadataExtensions
    {
        /// <summary>
        /// 获取索引字段列表
        /// </summary>
        public static List<ConfigFieldMetadata> GetFields(this ConfigIndexMetadata index)
        {
            return index?.IndexFields ?? new List<ConfigFieldMetadata>();
        }
        
        /// <summary>
        /// 获取索引字段的类型列表
        /// </summary>
        public static List<Type> GetFieldTypes(this ConfigIndexMetadata index)
        {
            if (index?.IndexFields == null)
                return new List<Type>();
            
            return index.IndexFields
                .Select(f => f.TypeInfo?.ManagedFieldType)
                .Where(t => t != null)
                .ToList();
        }
        
        /// <summary>
        /// 获取索引字段的名称列表
        /// </summary>
        public static List<string> GetFieldNames(this ConfigIndexMetadata index)
        {
            if (index?.IndexFields == null)
                return new List<string>();
            
            return index.IndexFields.Select(f => f.FieldName).ToList();
        }
        
        /// <summary>
        /// 获取索引的完整描述(用于调试)
        /// </summary>
        public static string GetIndexDescription(this ConfigIndexMetadata index)
        {
            if (index == null)
                return "null";
            
            var sb = new StringBuilder();
            sb.Append($"{index.IndexName}: ");
            
            if (index.IndexFields != null && index.IndexFields.Count > 0)
            {
                var fieldNames = string.Join(", ", index.IndexFields.Select(f => f.FieldName));
                sb.Append($"({fieldNames})");
            }
            
            if (index.IsUnique)
                sb.Append(" [唯一]");
            
            if (index.IsMultiValue)
                sb.Append(" [多值]");
            
            if (index.IsCompositeIndex)
                sb.Append(" [复合索引]");
            
            return sb.ToString();
        }
        
        /// <summary>
        /// 生成索引的类型签名(用于代码生成)
        /// 例如: (string name, int level) -> "string_name_int_level"
        /// </summary>
        public static string GenerateTypeSignature(this ConfigIndexMetadata index, bool includeFieldNames = false)
        {
            if (index?.IndexFields == null || index.IndexFields.Count == 0)
                return string.Empty;
            
            var parts = new List<string>();
            
            foreach (var field in index.IndexFields)
            {
                var typeName = field.TypeInfo?.ManagedFieldType?.Name ?? "unknown";
                parts.Add(typeName);
                
                if (includeFieldNames)
                    parts.Add(field.FieldName);
            }
            
            return string.Join("_", parts);
        }
    }
    
    /// <summary>
    /// ConfigLinkMetadata 扩展方法
    /// </summary>
    public static class ConfigLinkMetadataExtensions
    {
        /// <summary>
        /// 添加Link字段
        /// </summary>
        public static void AddLinkField(this ConfigLinkMetadata link, ConfigFieldMetadata field)
        {
            if (link == null || field == null || !field.IsXmlLink)
                return;
            
            if (link.LinkFields == null)
                link.LinkFields = new List<ConfigFieldMetadata>();
            
            link.LinkFields.Add(field);
            
            // 更新IsMultiLink标志
            if (field.IsMultiLink)
                link.IsMultiLink = true;
        }
        
        /// <summary>
        /// 添加SubLink(被引用关系)
        /// </summary>
        public static void AddSubLink(this ConfigLinkMetadata link, Type subLinkType, Type subLinkHelperType)
        {
            if (link == null || subLinkType == null)
                return;
            
            if (link.SubLinkTypes == null)
                link.SubLinkTypes = new List<Type>();
            
            if (link.SubLinkHelperTypes == null)
                link.SubLinkHelperTypes = new List<Type>();
            
            if (!link.SubLinkTypes.Contains(subLinkType))
            {
                link.SubLinkTypes.Add(subLinkType);
                if (subLinkHelperType != null)
                    link.SubLinkHelperTypes.Add(subLinkHelperType);
            }
        }
        
        /// <summary>
        /// 获取所有Link目标类型
        /// </summary>
        public static List<Type> GetAllLinkTargetTypes(this ConfigLinkMetadata link)
        {
            var targets = new List<Type>();
            
            if (link?.LinkFields == null)
                return targets;
            
            foreach (var field in link.LinkFields)
            {
                if (field.XmlLinkTargetType != null)
                    targets.Add(field.XmlLinkTargetType);
            }
            
            return targets;
        }
        
        /// <summary>
        /// 获取Link的完整描述(用于调试)
        /// </summary>
        public static string GetLinkDescription(this ConfigLinkMetadata link)
        {
            if (link == null)
                return "null";
            
            var sb = new StringBuilder();
            
            if (link.HasLinkFields)
            {
                sb.AppendLine($"Link字段数: {link.LinkFields.Count}");
                foreach (var field in link.LinkFields)
                {
                    sb.AppendLine($"  - {field.FieldName} -> {field.XmlLinkTargetType?.Name}");
                }
            }
            
            if (link.HasSubLinks)
            {
                sb.AppendLine($"被引用数: {link.SubLinkTypes.Count}");
                foreach (var subType in link.SubLinkTypes)
                {
                    sb.AppendLine($"  - {subType?.Name}");
                }
            }
            
            if (link.IsMultiLink)
                sb.AppendLine("[多重Link]");
            
            return sb.ToString();
        }
        
        /// <summary>
        /// 获取所有相关的配置类型(父、Link目标、SubLink)
        /// </summary>
        public static List<Type> GetAllRelatedTypes(this ConfigLinkMetadata link)
        {
            var types = new List<Type>();
            
            if (link == null)
                return types;
            
            if (link.LinkFields != null)
            {
                foreach (var field in link.LinkFields)
                {
                    if (field.XmlLinkTargetType != null)
                        types.Add(field.XmlLinkTargetType);
                }
            }
            
            if (link.SubLinkTypes != null)
                types.AddRange(link.SubLinkTypes.Where(t => t != null));
            
            return types.Distinct().ToList();
        }
    }
    
    /// <summary>
    /// ConfigClassMetadata 扩展方法
    /// </summary>
    public static class ConfigClassMetadataExtensions
    {
        /// <summary>
        /// 获取字段元数据
        /// </summary>
        public static ConfigFieldMetadata GetField(this ConfigClassMetadata metadata, string fieldName)
        {
            if (metadata?.FieldByName == null)
                return null;
            
            metadata.FieldByName.TryGetValue(fieldName, out var field);
            return field;
        }
        
        /// <summary>
        /// 获取索引元数据
        /// </summary>
        public static ConfigIndexMetadata GetIndex(this ConfigClassMetadata metadata, string indexName)
        {
            if (metadata?.IndexByName == null)
                return null;
            
            metadata.IndexByName.TryGetValue(indexName, out var index);
            return index;
        }
        
        /// <summary>
        /// 获取所有参与索引的字段
        /// </summary>
        public static List<ConfigFieldMetadata> GetIndexedFields(this ConfigClassMetadata metadata)
        {
            if (metadata?.Fields == null)
                return new List<ConfigFieldMetadata>();
            
            return metadata.Fields.Where(f => f.IsIndexField).ToList();
        }
        
        /// <summary>
        /// 获取所有Link字段
        /// </summary>
        public static List<ConfigFieldMetadata> GetLinkFields(this ConfigClassMetadata metadata)
        {
            if (metadata?.Fields == null)
                return new List<ConfigFieldMetadata>();
            
            return metadata.Fields.Where(f => f.IsXmlLink).ToList();
        }
        
        /// <summary>
        /// 获取所有嵌套配置字段
        /// </summary>
        public static List<ConfigFieldMetadata> GetNestedConfigFields(this ConfigClassMetadata metadata)
        {
            if (metadata?.Fields == null)
                return new List<ConfigFieldMetadata>();
            
            return metadata.Fields.Where(f => f.IsNestedConfig).ToList();
        }
        
        /// <summary>
        /// 获取配置类的完整摘要(用于调试)
        /// </summary>
        public static string GetSummary(this ConfigClassMetadata metadata)
        {
            if (metadata == null)
                return "null";
            
            var sb = new StringBuilder();
            sb.AppendLine($"配置类: {metadata.ManagedTypeName}");
            sb.AppendLine($"  表名: {metadata.TableName}");
            sb.AppendLine($"  Mod: {metadata.ModName}");
            sb.AppendLine($"  命名空间: {metadata.Namespace}");
            sb.AppendLine($"  字段数: {metadata.Fields?.Count ?? 0}");
            sb.AppendLine($"  索引数: {metadata.Indexes?.Count ?? 0}");
            sb.AppendLine($"  有Link: {metadata.HasLink}");
            
            return sb.ToString();
        }
        
        /// <summary>
        /// 获取配置类的详细报告
        /// </summary>
        public static string GetDetailedReport(this ConfigClassMetadata metadata)
        {
            if (metadata == null)
                return "null";
            
            var sb = new StringBuilder();
            sb.AppendLine(metadata.GetSummary());
            
            // 字段详情
            if (metadata.Fields != null && metadata.Fields.Count > 0)
            {
                sb.AppendLine("\n字段列表:");
                foreach (var field in metadata.Fields)
                {
                    sb.AppendLine($"  {field.GetFieldDescription()}");
                }
            }
            
            // 索引详情
            if (metadata.Indexes != null && metadata.Indexes.Count > 0)
            {
                sb.AppendLine("\n索引列表:");
                foreach (var index in metadata.Indexes)
                {
                    sb.AppendLine($"  {index.GetIndexDescription()}");
                }
            }
            
            // Link详情
            if (metadata.Link != null && metadata.HasLink)
            {
                sb.AppendLine("\nLink信息:");
                sb.Append(metadata.Link.GetLinkDescription());
            }
            
            return sb.ToString();
        }
        
        /// <summary>
        /// 获取所有需要转换器的字段
        /// </summary>
        public static List<ConfigFieldMetadata> GetFieldsNeedingConverter(this ConfigClassMetadata metadata)
        {
            if (metadata?.Fields == null)
                return new List<ConfigFieldMetadata>();
            
            return metadata.Fields.Where(f => f.NeedsConverter).ToList();
        }
        
        /// <summary>
        /// 获取所有需要转换器的字段及其转换器信息
        /// </summary>
        public static Dictionary<ConfigFieldMetadata, ConverterInfo> GetFieldsWithConverters(this ConfigClassMetadata metadata)
        {
            var result = new Dictionary<ConfigFieldMetadata, ConverterInfo>();
            
            if (metadata?.Fields == null)
                return result;
            
            foreach (var field in metadata.Fields)
            {
                if (field.NeedsConverter)
                    result[field] = field.Converter;
            }
            
            return result;
        }
        
        /// <summary>
        /// 构建快速查找表
        /// </summary>
        public static void BuildLookupTables(this ConfigClassMetadata metadata)
        {
            if (metadata == null)
                return;
            
            // 构建字段查找表
            if (metadata.Fields != null)
            {
                metadata.FieldByName = new Dictionary<string, ConfigFieldMetadata>();
                foreach (var field in metadata.Fields)
                {
                    if (!string.IsNullOrEmpty(field.FieldName))
                        metadata.FieldByName[field.FieldName] = field;
                }
            }
            
            // 构建索引查找表
            if (metadata.Indexes != null)
            {
                metadata.IndexByName = new Dictionary<string, ConfigIndexMetadata>();
                foreach (var index in metadata.Indexes)
                {
                    if (!string.IsNullOrEmpty(index.IndexName))
                        metadata.IndexByName[index.IndexName] = index;
                }
            }
        }
        
        /// <summary>
        /// 检查是否有循环引用(嵌套配置或Link)
        /// </summary>
        public static bool HasCircularReference(this ConfigClassMetadata metadata, HashSet<Type> visited = null)
        {
            if (metadata?.ManagedType == null)
                return false;
            
            visited = visited ?? new HashSet<Type>();
            
            if (visited.Contains(metadata.ManagedType))
                return true;
            
            visited.Add(metadata.ManagedType);
            
            // 检查嵌套配置
            if (metadata.Fields != null)
            {
                foreach (var field in metadata.Fields)
                {
                    if (field.IsNestedConfig && field.TypeInfo?.NestedConfigMetadata != null)
                    {
                        if (field.TypeInfo.NestedConfigMetadata.HasCircularReference(visited))
                            return true;
                    }
                }
            }
            
            visited.Remove(metadata.ManagedType);
            return false;
        }
    }
}
