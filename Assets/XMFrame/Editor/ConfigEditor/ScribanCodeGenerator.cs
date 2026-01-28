using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Scriban;
using Scriban.Runtime;
using UnityEditor;
using UnityEngine;

namespace XM.Editor
{
    /// <summary>
    /// 配置代码生成用缓存：输出目录按程序集缓存，避免重复扫描 asmdef。
    /// </summary>
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
                catch { /* 忽略 */ }
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

    /// <summary>
    /// 基于 Scriban 的代码生成器工具类。
    /// 负责加载模板、绑定数据、渲染并可选写入文件。模板按路径缓存以加快重复生成。
    /// </summary>
    public static class ScribanCodeGenerator
    {
        /// <summary>
        /// 默认模板根目录（相对于项目 Assets，与 XMFrame 目录一致）
        /// </summary>
        public const string DefaultTemplateRoot = "Assets/XMFrame/Editor/ConfigEditor/Templates";

        private static readonly Dictionary<string, Template> _templateCache = new Dictionary<string, Template>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 清除模板缓存（如模板文件修改后需重新解析时调用）。
        /// </summary>
        public static void ClearTemplateCache()
        {
            lock (_templateCache) { _templateCache.Clear(); }
        }

        /// <summary>
        /// 从文件路径加载 Scriban 模板（带缓存，同一路径重复加载直接返回缓存）。
        /// </summary>
        /// <param name="templatePath">模板文件路径（绝对或相对于项目根）</param>
        /// <param name="template">成功时返回解析后的模板</param>
        /// <returns>是否成功</returns>
        public static bool TryLoadTemplate(string templatePath, out Template template)
        {
            template = null;
            if (string.IsNullOrEmpty(templatePath))
                return false;

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

            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"[ScribanCodeGenerator] 模板文件不存在: {fullPath}");
                return false;
            }

            try
            {
                var content = File.ReadAllText(fullPath);
                var parsed = Template.Parse(content);
                if (parsed.HasErrors)
                {
                    Debug.LogError($"[ScribanCodeGenerator] 模板解析错误: {string.Join("; ", parsed.Messages)}");
                    return false;
                }
                lock (_templateCache) { _templateCache[fullPath] = parsed; }
                template = parsed;
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ScribanCodeGenerator] 读取模板失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 从字符串解析 Scriban 模板。
        /// </summary>
        public static bool TryParseTemplate(string templateContent, out Template template)
        {
            template = null;
            if (string.IsNullOrEmpty(templateContent))
                return false;
            try
            {
                template = Template.Parse(templateContent);
                if (template.HasErrors)
                {
                    Debug.LogError($"[ScribanCodeGenerator] 模板解析错误: {string.Join("; ", template.Messages)}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ScribanCodeGenerator] 解析模板失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 使用 ScriptObject 渲染模板，返回生成文本。
        /// </summary>
        /// <param name="template">已解析的模板</param>
        /// <param name="model">模板变量（ScriptObject 或可迭代键值）</param>
        /// <param name="result">成功时返回渲染结果</param>
        /// <returns>是否成功</returns>
        public static bool TryRender(Template template, ScriptObject model, out string result)
        {
            result = null;
            if (template == null || model == null)
                return false;
            try
            {
                var context = new TemplateContext();
                context.PushGlobal(model);
                result = template.Render(context);
                if (template.HasErrors)
                {
                    Debug.LogError($"[ScribanCodeGenerator] 渲染错误: {string.Join("; ", template.Messages)}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ScribanCodeGenerator] 渲染失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 使用模板路径 + 模型生成代码并写入文件。
        /// </summary>
        /// <param name="templatePath">模板路径（相对 DefaultTemplateRoot 或绝对）</param>
        /// <param name="model">模板变量</param>
        /// <param name="outputFilePath">输出文件路径</param>
        /// <param name="refreshAssets">是否在写入后调用 AssetDatabase.Refresh</param>
        /// <returns>是否成功</returns>
        public static bool GenerateToFile(string templatePath, ScriptObject model, string outputFilePath, bool refreshAssets = false)
        {
            if (!TryLoadTemplate(templatePath, out var template))
                return false;
            if (!TryRender(template, model, out var code))
                return false;

            try
            {
                var dir = Path.GetDirectoryName(outputFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllText(outputFilePath, code);
                if (refreshAssets)
                    AssetDatabase.Refresh();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ScribanCodeGenerator] 写入文件失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取默认模板完整路径（位于 ConfigEditor/Templates 下）。
        /// </summary>
        public static string GetTemplatePath(string templateFileName)
        {
            if (string.IsNullOrEmpty(templateFileName))
                return null;
            var root = DefaultTemplateRoot.Replace("/", Path.DirectorySeparatorChar.ToString());
            return Path.Combine(root, templateFileName).Replace("\\", "/");
        }
    }
}
