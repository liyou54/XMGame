#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace XM.Editor
{
    /// <summary>
    /// 将 XMFrame 必要 DLL 拷贝到 XModTest 工程；运行时 DLL 到 Plugins，工具集到 Mod Editor。
    /// 不拷贝 YooAsset（Mod 工程通过 Package 引用）。仅通过菜单「XMFrame/拷贝 DLL 到 XModTest」手动执行。
    /// </summary>
    public static class CopyDllsToXModTest
    {
        /// <summary>拷贝目标项目路径：XModTest 的 Assets 目录</summary>
        private const string CopyProjectPath = "XModTest/Assets";
        private const string ScriptAssembliesFolder = "ScriptAssemblies";
        private const string PluginsSubPath = "Plugins/XMFrame";
        private const string EditorPluginsSubPath = "Plugins/Editor/XMFrame";

        private static readonly string[] RuntimeDllNames = { "XM.Utils.dll", "XM.Contracts.dll", "XM.ModAPI.dll", "XM.Runtime.dll" };
        /// <summary>运行时依赖（仅 UniTask）；不拷贝 YooAsset，Mod 工程用 Package</summary>
        private static readonly string[] RuntimeDependencyDllNames = { "UniTask.dll" };
        private static readonly string[] EditorToolDllNames = { "XModToolkit.dll", "UnityToolkit.dll", "XM.Editor.dll" };
        /// <summary>Editor 依赖；不拷贝 YooAsset.Editor</summary>
        private static readonly string[] EditorDependencyDllNames = Array.Empty<string>();

        private static bool CopyForce(string srcPath, string destPath, string logLabel)
        {
            if (!File.Exists(srcPath)) return false;
            string destDir = Path.GetDirectoryName(destPath);
            if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);
            try
            {
                File.Copy(srcPath, destPath, overwrite: true);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CopyDllsToXModTest] 拷贝 {logLabel}: {ex.Message}");
                return false;
            }
        }

        [MenuItem("XMFrame/拷贝 DLL 到 XModTest")]
        public static void CopyDlls()
        {
            try
            {
                string projectRoot = Path.GetDirectoryName(Application.dataPath);
                if (string.IsNullOrEmpty(projectRoot)) return;

                string sourceDir = Path.Combine(projectRoot, "Library", ScriptAssembliesFolder);
                string copyProjectRoot = Path.Combine(projectRoot, CopyProjectPath.Replace('/', Path.DirectorySeparatorChar));

                if (!Directory.Exists(copyProjectRoot))
                {
                    try { Directory.CreateDirectory(copyProjectRoot); }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[CopyDllsToXModTest] 创建目录失败: {copyProjectRoot}\n{ex.Message}");
                        return;
                    }
                }

                if (!Directory.Exists(sourceDir))
                {
                    Debug.LogWarning($"[CopyDllsToXModTest] 源目录不存在（请先编译主工程）: {sourceDir}");
                    return;
                }

                int totalCopied = 0;
                string destRuntime = Path.Combine(copyProjectRoot, PluginsSubPath);
                string destEditor = Path.Combine(copyProjectRoot, EditorPluginsSubPath);

                foreach (string dllName in RuntimeDllNames)
                    if (CopyForce(Path.Combine(sourceDir, dllName), Path.Combine(destRuntime, dllName), $"运行时 {dllName}")) totalCopied++;
                foreach (string dllName in RuntimeDependencyDllNames)
                    if (CopyForce(Path.Combine(sourceDir, dllName), Path.Combine(destRuntime, dllName), $"运行时依赖 {dllName}")) totalCopied++;
                foreach (string dllName in EditorToolDllNames)
                    if (CopyForce(Path.Combine(sourceDir, dllName), Path.Combine(destEditor, dllName), $"工具 {dllName}")) totalCopied++;
                foreach (string dllName in EditorDependencyDllNames)
                    if (CopyForce(Path.Combine(sourceDir, dllName), Path.Combine(destEditor, dllName), $"Editor 依赖 {dllName}")) totalCopied++;

                string scribanInMain = Path.Combine(Application.dataPath, "XMFrame", "Editor", "Plugins", "Scriban.dll");
                if (CopyForce(scribanInMain, Path.Combine(destEditor, "Scriban.dll"), "Scriban.dll")) totalCopied++;

                AssetDatabase.Refresh();
                Debug.Log($"[CopyDllsToXModTest] 已拷贝 {totalCopied} 个文件到 XModTest（未拷贝 YooAsset，Mod 工程用 Package）。");
            }
            catch (Exception ex)
            {
                Debug.LogError("[CopyDllsToXModTest] 拷贝异常。");
                Debug.LogException(ex);
            }
        }
    }
}
#endif
