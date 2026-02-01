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
            scriptObject["table_name"] = dto.TableName ?? "";
            scriptObject["mod_name"] = dto.ModName ?? "Default";
            scriptObject["container_alloc_code"] = dto.ContainerAllocCode ?? "";
            scriptObject["container_alloc_helper_methods"] = dto.ContainerAllocHelperMethods ?? "";
            scriptObject["required_usings"] = dto.RequiredUsings ?? new List<string>();

            var fieldAssigns = new List<ScriptObject>();
            foreach (var f in dto.FieldAssigns ?? new List<FieldAssignDto>())
            {
                fieldAssigns.Add(new ScriptObject
                {
                    ["call_code"] = f.CallCode ?? "",
                    ["method_code"] = f.MethodCode ?? ""
                });
            }
            scriptObject["field_assigns"] = fieldAssigns;

            var converterRegistrations = new List<ScriptObject>();
            foreach (var c in dto.ConverterRegistrations ?? new List<ConverterRegistrationDto>())
            {
                converterRegistrations.Add(new ScriptObject
                {
                    ["source_type"] = c.SourceType ?? "",
                    ["target_type"] = c.TargetType ?? "",
                    ["domain_escaped"] = c.DomainEscaped ?? "",
                    ["converter_type_name"] = c.ConverterTypeName ?? ""
                });
            }
            scriptObject["converter_registrations"] = converterRegistrations;

            return scriptObject;
        }
    }
}
