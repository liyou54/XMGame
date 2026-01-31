using System.Collections.Generic;
using Scriban.Runtime;
using XModToolkit.Config;

namespace XModToolkit
{
    /// <summary>
    /// 从 ConfigTypeInfoDto 构建 ClassHelper.sbncs 模板所需的 ScriptObject，不依赖 Unity。
    /// </summary>
    public static class ClassHelperModelBuilder
    {
        public static ScriptObject Build(ConfigTypeInfoDto dto)
        {
            if (dto == null)
                return null;

            var scriptObject = new ScriptObject();
            scriptObject["namespace"] = dto.Namespace ?? "";
            scriptObject["managed_type_name"] = dto.ManagedTypeName ?? "";
            scriptObject["unmanaged_type_name"] = dto.UnmanagedTypeName ?? "";
            scriptObject["helper_class_name"] = (dto.ManagedTypeName ?? "") + "ClassHelper";
            scriptObject["table_name"] = !string.IsNullOrEmpty(dto.TableName) ? dto.TableName : (dto.ManagedTypeName ?? "");
            scriptObject["mod_name"] = !string.IsNullOrEmpty(dto.ModName) ? dto.ModName : "Default";
            scriptObject["has_base"] = dto.HasBase;
            scriptObject["base_managed_type_name"] = dto.BaseManagedTypeName ?? "";
            scriptObject["required_usings"] = dto.RequiredUsings ?? new List<string>();

            var fieldAssigns = new List<ScriptObject>();
            foreach (var a in dto.FieldAssigns ?? new List<FieldAssignDto>())
            {
                fieldAssigns.Add(new ScriptObject
                {
                    ["call_code"] = a.CallCode ?? "",
                    ["method_code"] = a.MethodCode ?? ""
                });
            }
            scriptObject["field_assigns"] = fieldAssigns;

            var converterRegistrations = new List<ScriptObject>();
            foreach (var r in dto.ConverterRegistrations ?? new List<ConverterRegistrationDto>())
            {
                converterRegistrations.Add(new ScriptObject
                {
                    ["source_type"] = r.SourceType ?? "string",
                    ["target_type"] = r.TargetType ?? "",
                    ["domain_escaped"] = r.DomainEscaped ?? "",
                    ["converter_type_name"] = r.ConverterTypeName ?? ""
                });
            }
            scriptObject["converter_registrations"] = converterRegistrations;

            return scriptObject;
        }
    }
}
