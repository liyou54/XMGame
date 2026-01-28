using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Scriban;
using Scriban.Runtime;
using UnityEditor;
using UnityEngine;
using XM.Editor;

namespace XM.Editor
{
    /// <summary>
    /// 非托管代码生成器
    /// </summary>
    public static class UnmanagedCodeGenerator
    {
        /// <summary>
        /// 生成所有配置类型的非托管代码
        /// </summary>
        public static void GenerateAllUnmanagedCode(string outputBasePath = null)
        {
            try
            {
                // 查找所有 XConfig 类型
                var configTypes = FindAllXConfigTypes();
                
                if (configTypes.Count == 0)
                {
                    Debug.LogWarning("未找到任何 XConfig 类型");
                    return;
                }

                Debug.Log($"找到 {configTypes.Count} 个配置类型");

                // 使用 Scriban 代码生成器加载模板
                var templatePath = ScribanCodeGenerator.GetTemplatePath("UnmanagedStruct.sbncs");
                if (!ScribanCodeGenerator.TryLoadTemplate(templatePath, out var template))
                {
                    Debug.LogError($"模板加载失败: {templatePath}");
                    return;
                }

                // 按程序集分组处理
                var typesByAssembly = configTypes.GroupBy(t => t.Assembly).ToList();

                foreach (var assemblyGroup in typesByAssembly)
                {
                    var assembly = assemblyGroup.Key;
                    var types = assemblyGroup.ToList();

                    // 查找 asmdef 文件并确定输出目录
                    string outputDir = outputBasePath;
                    if (string.IsNullOrEmpty(outputDir))
                    {
                        outputDir = XM.Editor.ConfigCodeGenCache.GetOutputDirectory(assembly);
                    }

                    if (string.IsNullOrEmpty(outputDir))
                    {
                        Debug.LogWarning($"无法为程序集 {assembly.GetName().Name} 确定输出目录，跳过");
                        continue;
                    }

                    // 确保输出目录存在
                    Directory.CreateDirectory(outputDir);

                    // 为每个类型生成代码
                    foreach (var configType in types)
                    {
                        try
                        {
                            GenerateUnmanagedCodeForType(configType, template, outputDir);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"生成 {configType.Name} 的非托管代码时出错: {ex.Message}");
                        }
                    }
                }

                Debug.Log("非托管代码生成完成");
                AssetDatabase.Refresh();
            }
            catch (Exception ex)
            {
                Debug.LogError($"生成非托管代码时出错: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 为单个类型生成非托管代码
        /// </summary>
        private static void GenerateUnmanagedCodeForType(Type configType, Template template, string outputDir)
        {
            // 分析类型
            var typeInfo = TypeAnalyzer.AnalyzeConfigType(configType);

            // 准备模板数据
            var scriptObject = new ScriptObject();
            scriptObject["namespace"] = typeInfo.Namespace;
            scriptObject["unmanaged_type_name"] = typeInfo.UnmanagedTypeName;
            scriptObject["managed_type_name"] = typeInfo.ManagedTypeName;
            scriptObject["has_base"] = typeInfo.HasBase;
            scriptObject["base_unmanaged_type_name"] = typeInfo.BaseUnmanagedTypeName ?? "";
            scriptObject["required_usings"] = typeInfo.RequiredUsings.OrderBy(x => x).ToList();

            // 准备字段数据
            var fields = new List<ScriptObject>();
            // 继承时：派生 Unmanaged 前置 Id_Base、IdBase_Ref
            if (typeInfo.HasBase && !string.IsNullOrEmpty(typeInfo.BaseUnmanagedTypeName))
            {
                var baseName = typeInfo.BaseUnmanagedTypeName;
                fields.Add(new ScriptObject
                {
                    ["name"] = "Id_Base",
                    ["unmanaged_type"] = $"CfgI<{baseName}>",
                    ["managed_type"] = "",
                    ["needs_ref_field"] = false,
                    ["ref_field_name"] = "",
                    ["needs_converter"] = false,
                    ["converter_type_name"] = "",
                    ["converter_domain"] = "",
                    ["source_type"] = "",
                    ["target_type"] = ""
                });
                fields.Add(new ScriptObject
                {
                    ["name"] = "IdBase_Ref",
                    ["unmanaged_type"] = $"XBlobPtr<{baseName}>",
                    ["managed_type"] = "",
                    ["needs_ref_field"] = false,
                    ["ref_field_name"] = "",
                    ["needs_converter"] = false,
                    ["converter_type_name"] = "",
                    ["converter_domain"] = "",
                    ["source_type"] = "",
                    ["target_type"] = ""
                });
            }
            foreach (var field in typeInfo.Fields)
            {
                var fieldObj = new ScriptObject();
                fieldObj["name"] = field.Name;
                fieldObj["unmanaged_type"] = field.UnmanagedType;
                fieldObj["managed_type"] = field.ManagedType;
                fieldObj["needs_ref_field"] = field.NeedsRefField;
                fieldObj["ref_field_name"] = field.RefFieldName ?? "";
                fieldObj["needs_converter"] = field.NeedsConverter;
                fieldObj["converter_type_name"] = field.ConverterTypeName ?? "";
                fieldObj["converter_domain"] = field.ConverterDomain ?? "";
                fieldObj["converter_domain_escaped"] = (field.ConverterDomain ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
                fieldObj["source_type"] = field.SourceType ?? "";
                fieldObj["target_type"] = field.TargetType ?? "";
                fields.Add(fieldObj);
            }
            scriptObject["fields"] = fields;

            // 准备索引组数据
            var indexGroups = new List<ScriptObject>();
            foreach (var indexGroup in typeInfo.IndexGroups)
            {
                var groupObj = new ScriptObject();
                groupObj["index_name"] = indexGroup.IndexName;
                groupObj["is_multi_value"] = indexGroup.IsMultiValue;

                var groupFields = new List<ScriptObject>();
                foreach (var field in indexGroup.Fields)
                {
                    var fieldObj = new ScriptObject();
                    fieldObj["name"] = field.Name;
                    fieldObj["unmanaged_type"] = field.UnmanagedType;
                    // 生成 camelCase 参数名
                    var paramName = char.ToLowerInvariant(field.Name[0]) + field.Name.Substring(1);
                    fieldObj["param_name"] = paramName;
                    groupFields.Add(fieldObj);
                }
                groupObj["fields"] = groupFields;
                indexGroups.Add(groupObj);
            }
            scriptObject["index_groups"] = indexGroups;

            // 使用 Scriban 代码生成器渲染并写入文件
            var fileName = $"{typeInfo.UnmanagedTypeName}.Gen.cs";
            var filePath = Path.Combine(outputDir, fileName);
            if (!ScribanCodeGenerator.TryRender(template, scriptObject, out var result))
            {
                Debug.LogError($"渲染模板时出错: {typeInfo.ManagedTypeName}");
                return;
            }
            File.WriteAllText(filePath, result);

            Debug.Log($"已生成: {filePath}");
        }

        /// <summary>
        /// 查找所有 XConfig 类型
        /// </summary>
        private static List<Type> FindAllXConfigTypes()
        {
            var configTypes = new List<Type>();

            // 获取所有程序集
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => IsXConfigType(t) && !t.IsAbstract)
                        .ToList();

                    configTypes.AddRange(types);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    // 忽略无法加载的类型
                    Debug.LogWarning($"加载程序集 {assembly.GetName().Name} 的类型时出错: {ex.Message}");
                }
            }

            return configTypes;
        }

        /// <summary>
        /// 为指定的程序集生成非托管代码
        /// </summary>
        public static void GenerateUnmanagedCodeForAssemblies(List<Assembly> assemblies, string outputBasePath = null)
        {
            try
            {
                if (assemblies == null || assemblies.Count == 0)
                {
                    Debug.LogWarning("未指定任何程序集");
                    return;
                }

                // 查找所有 XConfig 类型（仅从指定程序集）
                var configTypes = new List<Type>();
                foreach (var assembly in assemblies)
                {
                    try
                    {
                        var types = assembly.GetTypes()
                            .Where(t => IsXConfigType(t) && !t.IsAbstract)
                            .ToList();
                        configTypes.AddRange(types);
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        Debug.LogWarning($"加载程序集 {assembly.GetName().Name} 的类型时出错: {ex.Message}");
                    }
                }

                if (configTypes.Count == 0)
                {
                    Debug.LogWarning("在选定的程序集中未找到任何 XConfig 类型");
                    return;
                }

                Debug.Log($"找到 {configTypes.Count} 个配置类型");

                // 使用 Scriban 代码生成器加载模板
                var templatePath = ScribanCodeGenerator.GetTemplatePath("UnmanagedStruct.sbncs");
                if (!ScribanCodeGenerator.TryLoadTemplate(templatePath, out var template))
                {
                    Debug.LogError($"模板加载失败: {templatePath}");
                    return;
                }

                // 按程序集分组处理
                var typesByAssembly = configTypes.GroupBy(t => t.Assembly).ToList();

                foreach (var assemblyGroup in typesByAssembly)
                {
                    var assembly = assemblyGroup.Key;
                    var types = assemblyGroup.ToList();

                    // 查找 asmdef 文件并确定输出目录
                    string outputDir = outputBasePath;
                    if (string.IsNullOrEmpty(outputDir))
                    {
                        outputDir = XM.Editor.ConfigCodeGenCache.GetOutputDirectory(assembly);
                    }

                    if (string.IsNullOrEmpty(outputDir))
                    {
                        Debug.LogWarning($"无法为程序集 {assembly.GetName().Name} 确定输出目录，跳过");
                        continue;
                    }

                    // 确保输出目录存在
                    Directory.CreateDirectory(outputDir);

                    // 为每个类型生成代码
                    foreach (var configType in types)
                    {
                        try
                        {
                            GenerateUnmanagedCodeForType(configType, template, outputDir);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"生成 {configType.Name} 的非托管代码时出错: {ex.Message}");
                        }
                    }
                }

                Debug.Log("非托管代码生成完成");
                AssetDatabase.Refresh();
            }
            catch (Exception ex)
            {
                Debug.LogError($"生成非托管代码时出错: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// 检查是否是 XConfig 类型（基类 XConfig`2 或实现 IXConfig&lt;T,TUnmanaged&gt;）
        /// </summary>
        public static bool IsXConfigType(Type type)
        {
            var baseType = type.BaseType;
            while (baseType != null)
            {
                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition().Name == "XConfig`2")
                {
                    return true;
                }
                baseType = baseType.BaseType;
            }
            foreach (var iface in type.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition().Name == "IXConfig`2" && iface.GetGenericArguments().Length >= 2)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
