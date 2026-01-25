using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Scriban;
using Scriban.Runtime;
using UnityEditor;
using UnityEngine;
using XMFrame.Editor.ConfigEditor;

namespace XMFrame.Editor.ConfigEditor
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

                // 加载 Scriban 模板
                var templatePath = "Assets/XMFrame/Editor/ConfigEditor/Templates/UnmanagedStruct.sbncs";
                if (!File.Exists(templatePath))
                {
                    Debug.LogError($"模板文件不存在: {templatePath}");
                    return;
                }

                var templateContent = File.ReadAllText(templatePath);
                var template = Template.Parse(templateContent);

                if (template.HasErrors)
                {
                    Debug.LogError($"模板解析错误: {string.Join(", ", template.Messages)}");
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
                        outputDir = FindOutputDirectory(assembly);
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
            scriptObject["required_usings"] = typeInfo.RequiredUsings.ToList();

            // 准备字段数据
            var fields = new List<ScriptObject>();
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

            // 渲染模板
            var context = new TemplateContext();
            context.PushGlobal(scriptObject);
            var result = template.Render(context);

            // 检查模板渲染错误（通过 template.Messages 检查）
            if (template.HasErrors)
            {
                Debug.LogError($"渲染模板时出错: {string.Join(", ", template.Messages.Select(m => m.Message))}");
                return;
            }

            // 保存生成的文件
            var fileName = $"{typeInfo.UnmanagedTypeName}.Gen.cs";
            var filePath = Path.Combine(outputDir, fileName);
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

                // 加载 Scriban 模板
                var templatePath = "Assets/XMFrame/Editor/ConfigEditor/Templates/UnmanagedStruct.sbncs";
                if (!File.Exists(templatePath))
                {
                    Debug.LogError($"模板文件不存在: {templatePath}");
                    return;
                }

                var templateContent = File.ReadAllText(templatePath);
                var template = Template.Parse(templateContent);

                if (template.HasErrors)
                {
                    Debug.LogError($"模板解析错误: {string.Join(", ", template.Messages)}");
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
                        outputDir = FindOutputDirectory(assembly);
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
        /// 检查是否是 XConfig 类型
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
            return false;
        }

        /// <summary>
        /// 查找输出目录（基于 asmdef 文件位置）
        /// </summary>
        private static string FindOutputDirectory(Assembly assembly)
        {
            var assemblyName = assembly.GetName().Name;
            var projectPath = Application.dataPath.Replace("/Assets", "").Replace("\\Assets", "");

            // 在 Assets 目录下查找 asmdef 文件
            var assetsPath = Path.Combine(projectPath, "Assets");
            var asmdefFiles = Directory.GetFiles(assetsPath, "*.asmdef", SearchOption.AllDirectories);

            foreach (var asmdefFile in asmdefFiles)
            {
                try
                {
                    // 读取 asmdef 文件内容，检查 name 字段
                    var content = File.ReadAllText(asmdefFile);
                    if (content.Contains($"\"name\": \"{assemblyName}\""))
                    {
                        // 找到匹配的 asmdef 文件，在同级目录创建 Config/Code.Gen
                        var asmdefDir = Path.GetDirectoryName(asmdefFile);
                        var configDir = Path.Combine(asmdefDir, "Config", "Code.Gen");
                        return configDir;
                    }
                }
                catch
                {
                    // 忽略读取错误
                }
            }

            // 如果找不到，尝试根据程序集中的类型位置推断
            var types = assembly.GetTypes().Where(t => !string.IsNullOrEmpty(t.Namespace)).ToList();
            if (types.Count > 0)
            {
                // 使用第一个类型的命名空间来推断路径
                var firstType = types[0];
                var namespaceParts = firstType.Namespace.Split('.');
                
                // 查找可能的目录
                var searchDirs = Directory.GetDirectories(assetsPath, namespaceParts[0], SearchOption.AllDirectories);
                foreach (var dir in searchDirs)
                {
                    var asmdefInDir = Directory.GetFiles(dir, "*.asmdef", SearchOption.TopDirectoryOnly);
                    if (asmdefInDir.Length > 0)
                    {
                        var configDir = Path.Combine(dir, "Config", "Code.Gen");
                        return configDir;
                    }
                }
            }

            return null;
        }
    }
}
