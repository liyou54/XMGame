using System.Collections.Generic;

namespace XModToolkit.Config
{
    /// <summary>
    /// 配置类型信息 DTO，用于在 Unity Editor 和 XModToolkit 之间传递配置类型信息
    /// </summary>
    public class ConfigTypeInfoDto
    {
        public string Namespace { get; set; }
        public string ManagedTypeName { get; set; }
        public string UnmanagedTypeName { get; set; }
        public string TableName { get; set; }
        public string ModName { get; set; }
        public string ContainerAllocCode { get; set; }
        public string ContainerAllocHelperMethods { get; set; }
        public string LinkHelperClassName { get; set; }
        public List<string> RequiredUsings { get; set; } = new List<string>();
        public List<UnmanagedFieldDto> Fields { get; set; } = new List<UnmanagedFieldDto>();
        public List<IndexGroupDto> IndexGroups { get; set; } = new List<IndexGroupDto>();
        public List<FieldAssignDto> FieldAssigns { get; set; } = new List<FieldAssignDto>();
        public List<ConverterRegistrationDto> ConverterRegistrations { get; set; } = new List<ConverterRegistrationDto>();
    }

    /// <summary>
    /// 非托管字段 DTO
    /// </summary>
    public class UnmanagedFieldDto
    {
        public string Name { get; set; }
        public string UnmanagedType { get; set; }
        public bool NeedsRefField { get; set; }
        public string RefFieldName { get; set; }
        public bool NeedsConverter { get; set; }
        public string SourceType { get; set; }
        public string TargetType { get; set; }
        public string ConverterDomainEscaped { get; set; }
        
        /// <summary>字段是否是 CfgI 类型（用于 ToString 生成）</summary>
        public bool IsCfgI { get; set; }
        
        /// <summary>字段是否是 XBlobPtr 类型（用于 ToString 生成）</summary>
        public bool IsXBlobPtr { get; set; }
        
        /// <summary>如果是 XBlobPtr，关联的 CfgI 字段名（用于 ToString 生成）</summary>
        public string AssociatedCfgIField { get; set; }
        
        /// <summary>字段是否是容器类型（用于 ToString 生成）</summary>
        public bool IsContainer { get; set; }
        
        /// <summary>容器类型："Array", "Map", "Set"（用于 ToString 生成）</summary>
        public string ContainerKind { get; set; }
        
        /// <summary>容器元素类型（用于 ToString 生成）</summary>
        public string ElementType { get; set; }
        
        /// <summary>Map 的键类型（用于 ToString 生成）</summary>
        public string KeyType { get; set; }
        
        /// <summary>Map 的值类型（用于 ToString 生成）</summary>
        public string ValueType { get; set; }
        
        /// <summary>容器元素类型是否是 CfgI（用于 ToString 生成）</summary>
        public bool ElementTypeIsCfgI { get; set; }
        
        /// <summary>Map 的键类型是否是 CfgI（用于 ToString 生成）</summary>
        public bool KeyTypeIsCfgI { get; set; }
        
        /// <summary>Map 的值类型是否是 CfgI（用于 ToString 生成）</summary>
        public bool ValueTypeIsCfgI { get; set; }
    }

    /// <summary>
    /// 索引组 DTO
    /// </summary>
    public class IndexGroupDto
    {
        public string IndexName { get; set; }
        public bool IsMultiValue { get; set; }
        public List<IndexFieldDto> Fields { get; set; } = new List<IndexFieldDto>();
    }

    /// <summary>
    /// 索引字段 DTO
    /// </summary>
    public class IndexFieldDto
    {
        public string Name { get; set; }
        public string UnmanagedType { get; set; }
        public string ParamName { get; set; }
    }

    /// <summary>
    /// 字段赋值 DTO
    /// </summary>
    public class FieldAssignDto
    {
        public string CallCode { get; set; }
        public string MethodCode { get; set; }
    }

    /// <summary>
    /// 转换器注册 DTO
    /// </summary>
    public class ConverterRegistrationDto
    {
        public string SourceType { get; set; }
        public string TargetType { get; set; }
        public string DomainEscaped { get; set; }
        public string ConverterTypeName { get; set; }
    }
}
