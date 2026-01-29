#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using YooAsset;
using YooAsset.Editor;

namespace XMod.Editor
{
    /// <summary>
    /// 打包 Mod 并导出到主项目根目录 mods 文件夹下。
    /// 打包内容：ModDefine.xml、YooAssetPackage.xml、Xml 文件夹、Asset（YooAsset 打包后的输出，不拷贝原始 Asset 源）、DLL（按 ModDefine 的 DllPath）。
    /// </summary>
    public class ModPackAndExport : EditorWindow
    {
        private const string ModsFolderName = "Mods";
        private const string ModDefineXmlName = "ModDefine.xml";
        private const string YooAssetPackageXmlName = "YooAssetPackage.xml";
        private const string XmlFolderName = "Xml";
        private const string AssetFolderName = "Asset";
        private const string ScriptAssembliesFolder = "Library/ScriptAssemblies";
        /// <summary>与主项目 XModManager.ModsFolder 一致，运行时从 BaseDirectory/Mods 加载。</summary>
        private const string MainProjectModsFolderName = "Mods";

        private string[] _modNames = new string[0];
        private int _selectedIndex;
        private Vector2 _scrollPos;
        private string _lastError;

        private const string AssetsModMenuPrefix = "Assets/Mod/";

        [MenuItem("Mod/打包并导出到主项目 Mods")]
        public static void Open()
        {
            var win = GetWindow<ModPackAndExport>(true, "打包并导出 Mod", true);
            win.minSize = new Vector2(360, 200);
            win.RefreshModList();
        }

        [MenuItem(AssetsModMenuPrefix + "打包并导出到主项目", false, 0)]
        public static void PackAndExportFromContext()
        {
            if (!TryGetSelectedModName(out string modName))
                return;
            RequestCompileThenPackAndExport(modName);
        }

        [MenuItem(AssetsModMenuPrefix + "打包并导出到主项目", true, 0)]
        public static bool PackAndExportFromContextValidate()
        {
            return TryGetSelectedModName(out _);
        }

        /// <summary>
        /// 先请求编译，编译完成后再打包并导出（仅打包当前选中的 Mod 文件夹）。
        /// </summary>
        private static void RequestCompileThenPackAndExport(string modName)
        {
            Debug.Log($"[ModPackAndExport] 请求编译并打包导出: {modName}");
            CompilationPipeline.RequestScriptCompilation();
            EditorApplication.delayCall += OnFirstDelayCall;
            void OnFirstDelayCall()
            {
                EditorApplication.delayCall -= OnFirstDelayCall;
                if (EditorApplication.isCompiling)
                {
                    Debug.Log($"[ModPackAndExport] 等待编译完成: {modName}");
                    void OnCompilationFinished(object _)
                    {
                        CompilationPipeline.compilationFinished -= OnCompilationFinished;
                        Debug.Log($"[ModPackAndExport] 编译完成，触发打包并导出: {modName}");
                        TriggerYooAssetBuildThenExport(modName);
                    }
                    CompilationPipeline.compilationFinished += OnCompilationFinished;
                }
                else
                {
                    Debug.Log($"[ModPackAndExport] 无需等待编译，直接触发打包并导出: {modName}");
                    TriggerYooAssetBuildThenExport(modName);
                }
            }
        }

        /// <summary>
        /// 同步执行 YooAsset 打包，打包结束后直接导出。
        /// </summary>
        private static void TriggerYooAssetBuildThenExport(string modName)
        {
            Debug.Log($"[ModPackAndExport] 开始 YooAsset 打包: {modName}");
            try
            {
                string yooErr = TryTriggerYooAssetBuild(modName);
                if (!string.IsNullOrEmpty(yooErr))
                    Debug.LogWarning($"[ModPackAndExport] YooAsset 打包未执行: {yooErr}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ModPackAndExport] YooAsset 打包异常，仍将执行导出: {ex.Message}");
            }
            Debug.Log($"[ModPackAndExport] 打包结束，直接导出: {modName}");
            DoPackAndExportWithDialog(modName);
        }

        private static void DoPackAndExportWithDialog(string modName)
        {
            Debug.Log($"[ModPackAndExport] DoPackAndExportWithDialog: {modName}");
            if (PackAndExport(modName, out string err))
                EditorUtility.DisplayDialog("打包并导出", $"Mod \"{modName}\" 已导出到主项目 {MainProjectModsFolderName} 文件夹。", "确定");
            else
                EditorUtility.DisplayDialog("打包并导出失败", err, "确定");
        }

        /// <summary>
        /// 直接调用 YooAsset.Editor 构建接口，对指定包名执行打包。参考官方文档 Jenkins 支持示例。
        /// https://www.yooasset.com/docs/guide-editor/AssetBundleBuilder
        /// Mod 打包并导出时固定使用 BuiltinBuildPipeline，避免 SBP 与 Unity BuildContext（如 IBundleExplicitObjectLayout）不兼容导致 CreateBuiltInShadersBundle 报错。
        /// </summary>
        private static string TryTriggerYooAssetBuild(string packageName)
        {
            try
            {
                BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
                string buildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
                string streamingAssetsRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
                string packageVersion = DateTime.Now.ToString("yyyyMMddHHmmss");

                // Mod 打包固定使用内置管线，避免 ScriptableBuildPipeline 与 Unity SBP 上下文不兼容
                BuiltinBuildParameters buildParameters = new BuiltinBuildParameters
                {
                    BuildOutputRoot = buildOutputRoot,
                    BuildinFileRoot = streamingAssetsRoot,
                    BuildPipeline = EBuildPipeline.BuiltinBuildPipeline.ToString(),
                    BuildBundleType = (int)EBuildBundleType.AssetBundle,
                    BuildTarget = buildTarget,
                    PackageName = packageName,
                    PackageVersion = packageVersion,
                    VerifyBuildingResult = true,
                    EnableSharePackRule = true,
                    FileNameStyle = EFileNameStyle.BundleName,
                    BuildinFileCopyOption = EBuildinFileCopyOption.None,
                    BuildinFileCopyParams = string.Empty,
                    EncryptionServices = null,
                    CompressOption = ECompressOption.LZ4,
                    ClearBuildCacheFiles = false,
                    UseAssetDependencyDB = true
                };
                var pipeline = new BuiltinBuildPipeline();
                BuildResult result = pipeline.Run(buildParameters, true);

                if (result.Success)
                    return null;
                return result.ErrorInfo ?? "构建失败";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 保留：若需按包设置使用 SBP/RawFile 时可恢复分支；当前 Mod 打包固定用 Builtin 避免 SBP 上下文错误。
        /// </summary>
        private static string TryTriggerYooAssetBuildByPipeline(string packageName, string pipelineName)
        {
            try
            {
                BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
                string buildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
                string streamingAssetsRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
                string packageVersion = DateTime.Now.ToString("yyyyMMddHHmmss");
                BuildResult result;

                if (pipelineName == nameof(EBuildPipeline.BuiltinBuildPipeline))
                {
                    BuiltinBuildParameters buildParameters = new BuiltinBuildParameters
                    {
                        BuildOutputRoot = buildOutputRoot,
                        BuildinFileRoot = streamingAssetsRoot,
                        BuildPipeline = EBuildPipeline.BuiltinBuildPipeline.ToString(),
                        BuildBundleType = (int)EBuildBundleType.AssetBundle,
                        BuildTarget = buildTarget,
                        PackageName = packageName,
                        PackageVersion = packageVersion,
                        VerifyBuildingResult = true,
                        EnableSharePackRule = true,
                        FileNameStyle = EFileNameStyle.HashName,
                        BuildinFileCopyOption = EBuildinFileCopyOption.None,
                        BuildinFileCopyParams = string.Empty,
                        EncryptionServices = null,
                        CompressOption = ECompressOption.LZ4,
                        ClearBuildCacheFiles = false,
                        UseAssetDependencyDB = true
                    };
                    var pipeline = new BuiltinBuildPipeline();
                    result = pipeline.Run(buildParameters, true);
                }
                else if (pipelineName == nameof(EBuildPipeline.ScriptableBuildPipeline))
                {
                    ScriptableBuildParameters buildParameters = new ScriptableBuildParameters
                    {
                        BuildOutputRoot = buildOutputRoot,
                        BuildinFileRoot = streamingAssetsRoot,
                        BuildPipeline = EBuildPipeline.ScriptableBuildPipeline.ToString(),
                        BuildBundleType = (int)EBuildBundleType.AssetBundle,
                        BuildTarget = buildTarget,
                        PackageName = packageName,
                        PackageVersion = packageVersion,
                        VerifyBuildingResult = true,
                        EnableSharePackRule = true,
                        FileNameStyle = EFileNameStyle.HashName,
                        BuildinFileCopyOption = EBuildinFileCopyOption.None,
                        BuildinFileCopyParams = string.Empty,
                        EncryptionServices = null,
                        CompressOption = ECompressOption.LZ4,
                        ClearBuildCacheFiles = false,
                        UseAssetDependencyDB = true,
                        BuiltinShadersBundleName = GetBuiltinShaderBundleName(packageName)
                    };
                    var pipeline = new ScriptableBuildPipeline();
                    result = pipeline.Run(buildParameters, true);
                }
                else if (pipelineName == nameof(EBuildPipeline.RawFileBuildPipeline))
                {
                    RawFileBuildParameters buildParameters = new RawFileBuildParameters
                    {
                        BuildOutputRoot = buildOutputRoot,
                        BuildinFileRoot = streamingAssetsRoot,
                        BuildPipeline = EBuildPipeline.RawFileBuildPipeline.ToString(),
                        BuildBundleType = (int)EBuildBundleType.AssetBundle,
                        BuildTarget = buildTarget,
                        PackageName = packageName,
                        PackageVersion = packageVersion,
                        VerifyBuildingResult = true,
                        FileNameStyle = EFileNameStyle.HashName,
                        BuildinFileCopyOption = EBuildinFileCopyOption.None,
                        BuildinFileCopyParams = string.Empty,
                        EncryptionServices = null,
                        ClearBuildCacheFiles = false,
                        UseAssetDependencyDB = true
                    };
                    var pipeline = new RawFileBuildPipeline();
                    result = pipeline.Run(buildParameters, true);
                }
                else
                {
                    return $"不支持的构建管线: {pipelineName}";
                }

                if (result.Success)
                    return null;
                return result.ErrorInfo ?? "构建失败";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// SBP 构建管线需设置内置着色器资源包名称，与自动收集的着色器资源包名保持一致。参考官方文档注意事项。
        /// </summary>
        private static string GetBuiltinShaderBundleName(string packageName)
        {
            try
            {
                var uniqueBundleName = AssetBundleCollectorSettingData.Setting.UniqueBundleName;
                var packRuleResult = DefaultPackRule.CreateShadersPackRuleResult();
                return packRuleResult.GetBundleName(packageName, uniqueBundleName);
            }
            catch
            {
                return "unity_builtin_shaders";
            }
        }

        /// <summary>
        /// 当前选中的是否为单个 Mod 目录（Assets/Mods/xxx 且含 ModDefine.xml）。返回该 Mod 名称。
        /// </summary>
        private static bool TryGetSelectedModName(out string modName)
        {
            modName = null;
            string modsRoot = "Assets/" + ModsFolderName;
            if (Selection.assetGUIDs.Length != 1)
                return false;
            string path = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
            if (string.IsNullOrEmpty(path) || !path.StartsWith(modsRoot) || path.Length <= modsRoot.Length + 1)
                return false;
            string relative = path.Substring(modsRoot.Length).TrimStart('/');
            if (relative.Contains("/") || relative.Contains("\\"))
                return false;
            string fullPath = Path.Combine(Application.dataPath, "..", path).Replace('/', Path.DirectorySeparatorChar);
            fullPath = Path.GetFullPath(fullPath);
            string definePath = Path.Combine(fullPath, ModDefineXmlName);
            if (!File.Exists(definePath))
                return false;
            modName = relative;
            return true;
        }

        private void RefreshModList()
        {
            string modsRoot = Path.Combine(Application.dataPath, ModsFolderName);
            if (!Directory.Exists(modsRoot))
            {
                _modNames = new string[0];
                return;
            }

            var list = new List<string>();
            foreach (string dir in Directory.GetDirectories(modsRoot))
            {
                string definePath = Path.Combine(dir, ModDefineXmlName);
                if (File.Exists(definePath))
                    list.Add(Path.GetFileName(dir));
            }

            _modNames = list.ToArray();
            if (_selectedIndex >= _modNames.Length)
                _selectedIndex = 0;
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("打包并导出到主项目 Mods", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            if (GUILayout.Button("刷新 Mod 列表"))
                RefreshModList();

            if (_modNames.Length == 0)
            {
                EditorGUILayout.HelpBox($"未找到 Mod。请在 Assets/{ModsFolderName}/ 下创建含 {ModDefineXmlName} 的 Mod 目录。", MessageType.Info);
                EditorGUILayout.EndScrollView();
                return;
            }

            _selectedIndex = EditorGUILayout.Popup("选择 Mod", _selectedIndex, _modNames);
            string modName = _modNames[_selectedIndex];

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("导出目标", EditorStyles.miniLabel);
            string mainModsPath = GetMainProjectModsPath();
            EditorGUILayout.SelectableLabel(mainModsPath, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));

            if (!string.IsNullOrEmpty(_lastError))
                EditorGUILayout.HelpBox(_lastError, MessageType.Error);

            EditorGUILayout.Space(12);

            GUI.enabled = _modNames.Length > 0;
            if (GUILayout.Button("打包并导出到主项目", GUILayout.Height(28)))
            {
                _lastError = null;
                RequestCompileThenPackAndExport(modName);
            }
            GUI.enabled = true;

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 主项目根目录下的 Mods 文件夹路径（与 XModManager.ModsFolder 一致）。
        /// 若当前工程在 XMGame 内（如 XMGame/XModTest），则主项目为父目录；若当前工程名为 XModTest 且与 XMGame 同级，则主项目为同级 XMGame。
        /// </summary>
        private static string GetMainProjectModsPath()
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            if (string.IsNullOrEmpty(projectRoot))
                return "";
            string parentDir = Path.GetDirectoryName(projectRoot);
            string mainProjectRoot;
            if (string.Equals(Path.GetFileName(parentDir), "XMGame", StringComparison.OrdinalIgnoreCase))
                mainProjectRoot = parentDir; // 当前工程在 XMGame 内，如 XMGame/XModTest
            else if (string.Equals(Path.GetFileName(projectRoot), "XModTest", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(parentDir))
                mainProjectRoot = Path.Combine(parentDir, "XMGame"); // XModTest 与 XMGame 同级
            else
                mainProjectRoot = string.IsNullOrEmpty(parentDir) ? projectRoot : parentDir;
            return Path.Combine(mainProjectRoot, MainProjectModsFolderName);
        }

        /// <summary>
        /// 打包并导出指定 Mod 到主项目 Mods 文件夹。返回是否成功。
        /// </summary>
        public static bool PackAndExport(string modName, out string error)
        {
            error = null;
            string modsRoot = Path.Combine(Application.dataPath, ModsFolderName);
            string modRoot = Path.Combine(modsRoot, modName);
            if (!Directory.Exists(modRoot))
            {
                error = $"Mod 目录不存在: {modRoot}";
                return false;
            }

            string definePath = Path.Combine(modRoot, ModDefineXmlName);
            if (!File.Exists(definePath))
            {
                error = $"未找到 {ModDefineXmlName}";
                return false;
            }

            string dllFileName;
            if (!TryGetDllPathFromModDefine(definePath, out dllFileName))
                dllFileName = null;

            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string scriptAssemblies = Path.Combine(projectRoot, ScriptAssembliesFolder.Replace('/', Path.DirectorySeparatorChar));
            string dllSource = null;
            if (!string.IsNullOrEmpty(dllFileName))
                dllSource = Path.Combine(scriptAssemblies, dllFileName);
            if (string.IsNullOrEmpty(dllSource) || !File.Exists(dllSource))
            {
                string asmDllName = GetModAsmDllName(modName);
                string asmDllPath = Path.Combine(scriptAssemblies, asmDllName);
                if (File.Exists(asmDllPath))
                {
                    dllFileName = asmDllName;
                    dllSource = asmDllPath;
                }
            }
            if (string.IsNullOrEmpty(dllSource) || !File.Exists(dllSource))
            {
                error = string.IsNullOrEmpty(dllSource)
                    ? $"DLL 不存在，请先编译工程: {Path.Combine(scriptAssemblies, GetModAsmDllName(modName))}"
                    : $"DLL 不存在，请先编译工程: {dllSource}";
                return false;
            }

            string mainModsPath = GetMainProjectModsPath();
            if (string.IsNullOrEmpty(mainModsPath))
            {
                error = "无法解析主项目 Mods 路径";
                return false;
            }

            string targetRoot = Path.Combine(mainModsPath, modName);
            Debug.Log($"[ModPackAndExport] 主项目 Mods 路径: {mainModsPath}");
            try
            {
                Debug.Log($"[ModPackAndExport] 开始导出 Mod: {modName} -> {targetRoot}");

                if (!Directory.Exists(mainModsPath))
                    Directory.CreateDirectory(mainModsPath);
                if (Directory.Exists(targetRoot))
                    Directory.Delete(targetRoot, recursive: true);
                Directory.CreateDirectory(targetRoot);

                // ModDefine.xml
                File.Copy(definePath, Path.Combine(targetRoot, ModDefineXmlName));
                Debug.Log($"[ModPackAndExport] 已拷贝 {ModDefineXmlName} -> {targetRoot}");

                // YooAssetPackage.xml
                string yooPath = Path.Combine(modRoot, YooAssetPackageXmlName);
                if (File.Exists(yooPath))
                {
                    File.Copy(yooPath, Path.Combine(targetRoot, YooAssetPackageXmlName));
                    Debug.Log($"[ModPackAndExport] 已拷贝 {YooAssetPackageXmlName} -> {targetRoot}");
                }

                // Xml 文件夹
                string xmlSrc = Path.Combine(modRoot, XmlFolderName);
                string xmlDst = Path.Combine(targetRoot, XmlFolderName);
                if (Directory.Exists(xmlSrc))
                {
                    CopyDirectory(xmlSrc, xmlDst);
                    Debug.Log($"[ModPackAndExport] 已拷贝文件夹 {XmlFolderName} -> {xmlDst}");
                }

                // Bundles：将 YooAsset 构建输出（Bundles/.../包名/版本号）拷贝到 Mods/<modName>/Asset 下
                string buildOutputDir = GetModBuildOutputDirectory(modName);
                string assetDst = Path.Combine(targetRoot, AssetFolderName);
                if (!string.IsNullOrEmpty(buildOutputDir) && Directory.Exists(buildOutputDir))
                {
                    CopyDirectory(buildOutputDir, assetDst);
                    Debug.Log($"[ModPackAndExport] 已拷贝 Bundles 到 Mods 对应文件夹: {buildOutputDir} -> {assetDst}");
                }
                else
                {
                    if (!Directory.Exists(assetDst))
                        Directory.CreateDirectory(assetDst);
                    if (string.IsNullOrEmpty(buildOutputDir))
                        Debug.LogWarning($"[ModPackAndExport] 未找到构建输出目录，跳过 Bundles 拷贝。请先执行 YooAsset 打包。");
                    else
                        Debug.LogWarning($"[ModPackAndExport] 构建输出目录不存在: {buildOutputDir}，跳过 Bundles 拷贝。");
                }

                // DLL（DllPath 为相对 Mod 根目录的文件名，如 MyMod.dll）
                string dllDst = Path.Combine(targetRoot, dllFileName);
                File.Copy(dllSource, dllDst);
                Debug.Log($"[ModPackAndExport] 已拷贝 DLL: {dllFileName} -> {dllDst}");

                Debug.Log($"[ModPackAndExport] 导出完成: {modName} -> {targetRoot}");
                return true;
            }
            catch (System.Exception ex)
            {
                error = ex.Message;
                Debug.LogError($"[ModPackAndExport] 导出失败: {modName}, {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        private static bool TryGetDllPathFromModDefine(string definePath, out string dllFileName)
        {
            dllFileName = null;
            try
            {
                var doc = new XmlDocument();
                doc.Load(definePath);
                var node = doc.DocumentElement?.SelectSingleNode("DllPath");
                if (node == null || string.IsNullOrWhiteSpace(node.InnerText))
                    return false;
                string raw = node.InnerText.Trim();
                dllFileName = Path.GetFileName(raw);
                if (string.IsNullOrEmpty(dllFileName))
                    return false;
                if (!dllFileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    dllFileName = null;
                return dllFileName != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>与 CreateModWindow 一致：asm 名为 Mod 名去空格，DLL 名为 asm 名 + .dll</summary>
        private static string GetModAsmDllName(string modName)
        {
            return (modName ?? "").Replace(" ", "") + ".dll";
        }

        /// <summary>
        /// 获取该 Mod 的 YooAsset 构建输出目录（最新一次构建的版本目录）。路径为 BuildOutputRoot/BuildTarget/PackageName/PackageVersion。
        /// </summary>
        private static string GetModBuildOutputDirectory(string packageName)
        {
            string buildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            string packageRoot = Path.Combine(buildOutputRoot, buildTarget.ToString(), packageName);
            if (!Directory.Exists(packageRoot))
                return null;
            try
            {
                var versionDirs = Directory.GetDirectories(packageRoot);
                if (versionDirs.Length == 0)
                    return null;
                // 版本目录名为 yyyyMMddHHmmss，取名称最大即最新
                string latest = versionDirs.OrderByDescending(Path.GetFileName).FirstOrDefault();
                return latest;
            }
            catch
            {
                return null;
            }
        }

        private static void CopyDirectory(string sourceDir, string targetDir)
        {
            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string name = Path.GetFileName(file);
                File.Copy(file, Path.Combine(targetDir, name));
            }
            foreach (string sub in Directory.GetDirectories(sourceDir))
            {
                string name = Path.GetFileName(sub);
                CopyDirectory(sub, Path.Combine(targetDir, name));
            }
        }
    }
}
#endif
