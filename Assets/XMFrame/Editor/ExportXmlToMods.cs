#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace XM.Editor
{
    /// <summary>
    /// 将 Assets/Xml 导出到 Mods/Core/Xml/ 文件夹。
    /// 通过菜单「XMFrame/导出 Xml 到 Mods」手动执行。
    /// </summary>
    public static class ExportXmlToMods
    {
        private const string SourcePath = "Assets/Mods/Core/Xml";
        private const string DestPath = "Mods/Core/Xml";

        [MenuItem("XMFrame/导出 Xml 到 Mods")]
        public static void ExportXml()
        {
            try
            {
                string projectRoot = Path.GetDirectoryName(Application.dataPath);
                if (string.IsNullOrEmpty(projectRoot)) return;

                string sourceDir = Path.Combine(projectRoot, SourcePath.Replace('/', Path.DirectorySeparatorChar));
                string destDir = Path.Combine(projectRoot, DestPath.Replace('/', Path.DirectorySeparatorChar));

                if (!Directory.Exists(sourceDir))
                {
                    Debug.LogWarning($"[ExportXmlToMods] 源目录不存在: {sourceDir}");
                    EditorUtility.DisplayDialog("导出失败", $"源目录不存在：{SourcePath}", "确定");
                    return;
                }

                if (!Directory.Exists(destDir))
                {
                    try { Directory.CreateDirectory(destDir); }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[ExportXmlToMods] 创建目标目录失败: {destDir}\n{ex.Message}");
                        EditorUtility.DisplayDialog("导出失败", $"无法创建目标目录：{ex.Message}", "确定");
                        return;
                    }
                }

                int copiedCount = 0;
                var files = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);

                foreach (string srcPath in files)
                {
                    if (srcPath.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                        continue;

                    string relativePath = srcPath.Substring(sourceDir.Length).TrimStart(Path.DirectorySeparatorChar);
                    string destPath = Path.Combine(destDir, relativePath);

                    string destFileDir = Path.GetDirectoryName(destPath);
                    if (!string.IsNullOrEmpty(destFileDir) && !Directory.Exists(destFileDir))
                        Directory.CreateDirectory(destFileDir);

                    try
                    {
                        File.Copy(srcPath, destPath, overwrite: true);
                        copiedCount++;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[ExportXmlToMods] 拷贝失败: {relativePath}\n{ex.Message}");
                    }
                }

                Debug.Log($"[ExportXmlToMods] 已将 {SourcePath} 导出到 {DestPath}，共 {copiedCount} 个文件。");
                EditorUtility.DisplayDialog("导出完成", $"已导出 {copiedCount} 个文件到 {DestPath}", "确定");
            }
            catch (Exception ex)
            {
                Debug.LogError("[ExportXmlToMods] 导出异常。");
                Debug.LogException(ex);
                EditorUtility.DisplayDialog("导出失败", ex.Message, "确定");
            }
        }
    }
}
#endif
