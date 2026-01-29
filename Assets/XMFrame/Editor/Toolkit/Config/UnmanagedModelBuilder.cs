using System.Collections.Generic;
using Scriban.Runtime;
using XModToolkit.Config;

namespace XModToolkit
{
    /// <summary>
    /// 从 ConfigTypeInfoDto 构建 UnmanagedStruct.sbncs 模板所需的 ScriptObject，不依赖 Unity。
    /// </summary>
    public static class UnmanagedModelBuilder
    {
        public static ScriptObject Build(ConfigTypeInfoDto dto)
        {
            if (dto == null)
                return null;

            var scriptObject = new ScriptObject();
            scriptObject["namespace"] = dto.Namespace ?? "";
            scriptObject["managed_type_name"] = dto.ManagedTypeName ?? "";
            scriptObject["unmanaged_type_name"] = dto.UnmanagedTypeName ?? "";
            scriptObject["has_base"] = dto.HasBase;
            scriptObject["base_unmanaged_type_name"] = dto.BaseUnmanagedTypeName ?? "";
            scriptObject["required_usings"] = dto.RequiredUsings ?? new List<string>();

            var fields = new List<ScriptObject>();
            foreach (var f in dto.Fields ?? new List<UnmanagedFieldDto>())
            {
                fields.Add(new ScriptObject
                {
                    ["name"] = f.Name ?? "",
                    ["unmanaged_type"] = f.UnmanagedType ?? "",
                    ["needs_ref_field"] = f.NeedsRefField,
                    ["ref_field_name"] = f.RefFieldName ?? "",
                    ["needs_converter"] = f.NeedsConverter,
                    ["source_type"] = f.SourceType ?? "",
                    ["target_type"] = f.TargetType ?? "",
                    ["converter_domain_escaped"] = f.ConverterDomainEscaped ?? ""
                });
            }
            scriptObject["fields"] = fields;

            var indexGroups = new List<ScriptObject>();
            foreach (var g in dto.IndexGroups ?? new List<IndexGroupDto>())
            {
                var groupObj = new ScriptObject();
                groupObj["index_name"] = g.IndexName ?? "";
                groupObj["is_multi_value"] = g.IsMultiValue;
                var groupFields = new List<ScriptObject>();
                foreach (var f in g.Fields ?? new List<IndexFieldDto>())
                {
                    groupFields.Add(new ScriptObject
                    {
                        ["name"] = f.Name ?? "",
                        ["unmanaged_type"] = f.UnmanagedType ?? "",
                        ["param_name"] = f.ParamName ?? ""
                    });
                }
                groupObj["fields"] = groupFields;
                indexGroups.Add(groupObj);
            }
            scriptObject["index_groups"] = indexGroups;

            return scriptObject;
        }
    }
}
