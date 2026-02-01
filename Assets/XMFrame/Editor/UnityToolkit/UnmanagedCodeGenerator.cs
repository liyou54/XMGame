using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Scriban;
using Scriban.Runtime;
using UnityEditor;
using UnityEngine;
using XModToolkit;
using XModToolkit.Config;

namespace UnityToolkit
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
                        outputDir = ConfigCodeGenCache.GetOutputDirectory(assembly);
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
            var typeInfo = TypeAnalyzer.AnalyzeConfigType(configType);
            var dto = ToUnmanagedDto(typeInfo);
            var scriptObject = UnmanagedModelBuilder.Build(dto);
            if (scriptObject == null)
            {
                Debug.LogError($"构建 Unmanaged 模板模型失败: {typeInfo.ManagedTypeName}");
                return;
            }

            if (!ScribanCodeGeneratorCore.TryRender(template, scriptObject, out var result))
            {
                Debug.LogError($"渲染模板时出错: {typeInfo.ManagedTypeName}");
                return;
            }

            var fileName = $"{typeInfo.UnmanagedTypeName}.Gen.cs";
            var filePath = Path.Combine(outputDir, fileName);
            File.WriteAllText(filePath, result);
            Debug.Log($"已生成: {filePath}");
        }

        /// <summary>将 Editor 侧类型信息转为 Toolkit 用 DTO，供 XModToolkit 渲染 Unmanaged 模板。</summary>
        private static ConfigTypeInfoDto ToUnmanagedDto(ConfigTypeInfo typeInfo)
        {
            var dto = new ConfigTypeInfoDto
            {
                Namespace = typeInfo.Namespace ?? "",
                ManagedTypeName = typeInfo.ManagedTypeName,
                UnmanagedTypeName = typeInfo.UnmanagedTypeName,
                RequiredUsings = typeInfo.RequiredUsings?.OrderBy(x => x).ToList() ?? new List<string>()
            };

            var currentUnmanaged = typeInfo.UnmanagedTypeName ?? "";
            foreach (var field in typeInfo.Fields ?? new List<FieldInfo>())
            {
                if (field.IsXmlLink && !string.IsNullOrEmpty(field.XmlLinkDstUnmanagedType))
                {
                    dto.Fields.Add(new UnmanagedFieldDto { Name = field.Name + "_Dst", UnmanagedType = $"CfgI<{field.XmlLinkDstUnmanagedType}>", NeedsRefField = false, RefFieldName = "", NeedsConverter = false, SourceType = "", TargetType = "", ConverterDomainEscaped = "" });
                    dto.Fields.Add(new UnmanagedFieldDto { Name = field.Name + "_Ref", UnmanagedType = $"CfgI<{field.XmlLinkDstUnmanagedType}>", NeedsRefField = false, RefFieldName = "", NeedsConverter = false, SourceType = "", TargetType = "", ConverterDomainEscaped = "" });
                    dto.Fields.Add(new UnmanagedFieldDto { Name = field.Name, UnmanagedType = $"CfgI<{currentUnmanaged}>", NeedsRefField = false, RefFieldName = "", NeedsConverter = false, SourceType = "", TargetType = "", ConverterDomainEscaped = "" });
                }
                else
                {
                    dto.Fields.Add(new UnmanagedFieldDto
                    {
                        Name = field.Name,
                        UnmanagedType = field.UnmanagedType ?? "",
                        NeedsRefField = field.NeedsRefField,
                        RefFieldName = field.RefFieldName ?? "",
                        NeedsConverter = field.NeedsConverter,
                        SourceType = field.SourceType ?? "",
                        TargetType = field.TargetType ?? "",
                        ConverterDomainEscaped = (field.ConverterDomain ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"")
                    });
                }
            }
            foreach (var indexGroup in typeInfo.IndexGroups ?? new List<IndexGroupInfo>())
            {
                var groupDto = new IndexGroupDto { IndexName = indexGroup.IndexName, IsMultiValue = indexGroup.IsMultiValue };
                foreach (var field in indexGroup.Fields ?? new List<FieldInfo>())
                {
                    var paramName = string.IsNullOrEmpty(field.Name) ? "" : char.ToLowerInvariant(field.Name[0]) + field.Name.Substring(1);
                    groupDto.Fields.Add(new IndexFieldDto { Name = field.Name, UnmanagedType = field.UnmanagedType ?? "", ParamName = paramName });
                }
                dto.IndexGroups.Add(groupDto);
            }
            return dto;
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
                        outputDir = ConfigCodeGenCache.GetOutputDirectory(assembly);
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
