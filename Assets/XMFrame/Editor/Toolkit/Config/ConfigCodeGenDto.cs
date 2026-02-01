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
