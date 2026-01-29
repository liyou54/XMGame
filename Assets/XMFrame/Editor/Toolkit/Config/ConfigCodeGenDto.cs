using System.Collections.Generic;

namespace XModToolkit.Config
{
    /// <summary>
    /// 配置代码生成用 DTO，不依赖 Unity/反射，供 Toolkit 渲染模板使用。
    /// </summary>
    public sealed class ConfigTypeInfoDto
    {
        public string Namespace { get; set; }
        public string ManagedTypeName { get; set; }
        public string UnmanagedTypeName { get; set; }
        public bool HasBase { get; set; }
        public string BaseManagedTypeName { get; set; }
        public string BaseUnmanagedTypeName { get; set; }
        public List<string> RequiredUsings { get; set; } = new List<string>();

        /// <summary>ClassHelper 模板：字段赋值与 Parse 方法。</summary>
        public List<FieldAssignDto> FieldAssigns { get; set; } = new List<FieldAssignDto>();
        /// <summary>ClassHelper 模板：构造函数中注册的转换器。</summary>
        public List<ConverterRegistrationDto> ConverterRegistrations { get; set; } = new List<ConverterRegistrationDto>();

        /// <summary>Unmanaged 模板：字段列表。</summary>
        public List<UnmanagedFieldDto> Fields { get; set; } = new List<UnmanagedFieldDto>();
        /// <summary>Unmanaged 模板：索引组。</summary>
        public List<IndexGroupDto> IndexGroups { get; set; } = new List<IndexGroupDto>();
    }

    public sealed class FieldAssignDto
    {
        public string CallCode { get; set; }
        public string MethodCode { get; set; }
    }

    public sealed class ConverterRegistrationDto
    {
        public string SourceType { get; set; }
        public string TargetType { get; set; }
        public string DomainEscaped { get; set; }
        public string ConverterTypeName { get; set; }
    }

    public sealed class UnmanagedFieldDto
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

    public sealed class IndexGroupDto
    {
        public string IndexName { get; set; }
        public bool IsMultiValue { get; set; }
        public List<IndexFieldDto> Fields { get; set; } = new List<IndexFieldDto>();
    }

    public sealed class IndexFieldDto
    {
        public string Name { get; set; }
        public string UnmanagedType { get; set; }
        public string ParamName { get; set; }
    }
}
