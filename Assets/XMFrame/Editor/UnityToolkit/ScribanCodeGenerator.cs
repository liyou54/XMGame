using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Scriban;
using Scriban.Runtime;
using UnityEditor;
using UnityEngine;

namespace UnityToolkit
{
    /// <summary>配置代码生成用缓存：输出目录按程序集缓存。</summary>
    public static class ConfigCodeGenCache
    {
        private static readonly Dictionary<Assembly, string> _outputDirByAssembly = new Dictionary<Assembly, string>();

        public static void ClearOutputDirectoryCache()
        {
            lock (_outputDirByAssembly) { _outputDirByAssembly.Clear(); }
        }

        public static string GetOutputDirectory(Assembly assembly)
        {
            if (assembly == null) return null;
            lock (_outputDirByAssembly)
            {
                if (_outputDirByAssembly.TryGetValue(assembly, out var dir))
                    return dir;
            }
            var dirComputed = ComputeOutputDirectory(assembly);
            lock (_outputDirByAssembly) { _outputDirByAssembly[assembly] = dirComputed; }
            return dirComputed;
        }

        private static string ComputeOutputDirectory(Assembly assembly)
        {
            var assemblyName = assembly.GetName().Name;
            var projectPath = Application.dataPath.Replace("/Assets", "").Replace("\\Assets", "");
            var assetsPath = Path.Combine(projectPath, "Assets");
            var asmdefFiles = Directory.GetFiles(assetsPath, "*.asmdef", SearchOption.AllDirectories);
            foreach (var asmdefFile in asmdefFiles)
            {
                try
                {
                    var content = File.ReadAllText(asmdefFile);
                    if (content.Contains($"\"name\": \"{assemblyName}\""))
                    {
                        var asmdefDir = Path.GetDirectoryName(asmdefFile);
                        return Path.Combine(asmdefDir, "Config", "Code.Gen");
                    }
                }
                catch { }
            }
            try
            {
                foreach (var t in assembly.GetTypes())
                {
                    if (string.IsNullOrEmpty(t.Namespace)) continue;
                    var namespaceParts = t.Namespace.Split('.');
                    var searchDirs = Directory.GetDirectories(assetsPath, namespaceParts[0], SearchOption.AllDirectories);
                    foreach (var dir in searchDirs)
                    {
                        var asmdefInDir = Directory.GetFiles(dir, "*.asmdef", SearchOption.TopDirectoryOnly);
                        if (asmdefInDir.Length > 0)
                            return Path.Combine(dir, "Config", "Code.Gen");
                    }
                    break;
                }
            }
            catch (ReflectionTypeLoadException) { }
            return null;
        }
    }

    /// <summary>基于 Scriban 的代码生成器工具类，负责加载模板、绑定数据、渲染。</summary>
    public static class ScribanCodeGenerator
    {
        public const string DefaultTemplateRoot = "Assets/XMFrame/Editor/ConfigEditor/Templates";

        private static readonly Dictionary<string, Template> _templateCache = new Dictionary<string, Template>(StringComparer.OrdinalIgnoreCase);

        public static void ClearTemplateCache()
        {
            lock (_templateCache) { _templateCache.Clear(); }
        }

        public static bool TryLoadTemplate(string templatePath, out Template template)
        {
            template = null;
            if (string.IsNullOrEmpty(templatePath)) return false;
            var fullPath = templatePath;
            if (!Path.IsPathRooted(templatePath))
            {
                var projectRoot = Application.dataPath.Replace("/Assets", "").Replace("\\Assets", "");
                fullPath = Path.Combine(projectRoot, templatePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
            }
            lock (_templateCache)
            {
                if (_templateCache.TryGetValue(fullPath, out template))
                    return true;
            }
            string contentToParse = null;
            string cacheKey = fullPath;
            if (File.Exists(fullPath))
            {
                try
                {
                    contentToParse = File.ReadAllText(fullPath);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ScribanCodeGenerator] 读取模板失败: {ex.Message}");
                    return false;
                }
            }
            else
            {
                var fileName = Path.GetFileName(templatePath);
                contentToParse = GetEmbeddedTemplateContent(fileName);
                if (contentToParse != null)
                    cacheKey = "embedded:" + fileName;
                else
                {
                    Debug.LogWarning($"[ScribanCodeGenerator] 模板文件不存在: {fullPath}");
                    return false;
                }
            }
            try
            {
                var parsed = Template.Parse(contentToParse);
                if (parsed.HasErrors)
                {
                    Debug.LogError($"[ScribanCodeGenerator] 模板解析错误: {string.Join("; ", parsed.Messages)}");
                    return false;
                }
                lock (_templateCache) { _templateCache[cacheKey] = parsed; }
                template = parsed;
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ScribanCodeGenerator] 解析模板失败: {ex.Message}");
                return false;
            }
        }

        public static bool TryParseTemplate(string templateContent, out Template template)
        {
            template = null;
            if (string.IsNullOrEmpty(templateContent)) return false;
            try
            {
                template = Template.Parse(templateContent);
                if (template.HasErrors) { Debug.LogError($"[ScribanCodeGenerator] 模板解析错误: {string.Join("; ", template.Messages)}"); return false; }
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ScribanCodeGenerator] 解析模板失败: {ex.Message}");
                return false;
            }
        }

        public static bool TryRender(Template template, ScriptObject model, out string result)
        {
            result = null;
            if (template == null || model == null) return false;
            try
            {
                var context = new TemplateContext();
                context.PushGlobal(model);
                result = template.Render(context);
                if (template.HasErrors) { Debug.LogError($"[ScribanCodeGenerator] 渲染错误: {string.Join("; ", template.Messages)}"); return false; }
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ScribanCodeGenerator] 渲染失败: {ex.Message}");
                return false;
            }
        }

        public static bool GenerateToFile(string templatePath, ScriptObject model, string outputFilePath, bool refreshAssets = false)
        {
            if (!TryLoadTemplate(templatePath, out var template)) return false;
            if (!TryRender(template, model, out var code)) return false;
            try
            {
                var dir = Path.GetDirectoryName(outputFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllText(outputFilePath, code);
                if (refreshAssets) AssetDatabase.Refresh();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ScribanCodeGenerator] 写入文件失败: {ex.Message}");
                return false;
            }
        }

        public static string GetTemplatePath(string templateFileName)
        {
            if (string.IsNullOrEmpty(templateFileName)) return null;
            var root = DefaultTemplateRoot.Replace("/", Path.DirectorySeparatorChar.ToString());
            return Path.Combine(root, templateFileName).Replace("\\", "/");
        }

        /// <summary>当模板文件不存在时（如 mod 工程）使用内嵌模板内容。</summary>
        private static string GetEmbeddedTemplateContent(string templateFileName)
        {
            if (string.IsNullOrEmpty(templateFileName)) return null;
            if (string.Equals(templateFileName, "ClassHelper.sbncs", StringComparison.OrdinalIgnoreCase))
                return EmbeddedTemplates.ClassHelperSbncs;
            if (string.Equals(templateFileName, "UnmanagedStruct.sbncs", StringComparison.OrdinalIgnoreCase))
                return EmbeddedTemplates.UnmanagedStructSbncs;
            return null;
        }
    }
}
