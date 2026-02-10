#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using XM.ConfigNew.CodeGen;
using YooAsset;
using YooAsset.Editor;
using Assembly = System.Reflection.Assembly;

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
        private const string MainProjectModsFolderName = "Mods";

        private string[] _modNames = Array.Empty<string>();
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

        [MenuItem("Mod/生成配置代码", false, 1)]
        public static void GenerateConfigCodeForAllMods()
        {
            var modNames = GetModNames();
            if (modNames.Count == 0)
            {
                EditorUtility.DisplayDialog("生成配置代码", "未找到任何 Mod。请在 Assets/Mods/ 下创建含 ModDefine.xml 的 Mod 目录。", "确定");
                return;
            }
            int done = 0, skipped = 0;
            foreach (string modName in modNames)
            {
                if (GenerateConfigCodeForMod(modName))
                    done++;
                else
                    skipped++;
            }
            EditorUtility.DisplayDialog("生成配置代码", $"已为 {done} 个 Mod 生成配置代码" + (skipped > 0 ? $"，{skipped} 个跳过（无程序集或无 XConfig 类型）" : "") + "。", "确定");
            AssetDatabase.Refresh();
        }

        [MenuItem(AssetsModMenuPrefix + "生成配置代码", false, 1)]
        public static void GenerateConfigCodeFromContext()
        {
            if (!TryGetSelectedModName(out string modName))
                return;
            if (GenerateConfigCodeForMod(modName))
                EditorUtility.DisplayDialog("生成配置代码", $"已为 Mod \"{modName}\" 生成 ClassHelper 与 Unmanaged 配置代码。", "确定");
            else
                EditorUtility.DisplayDialog("生成配置代码", $"无法为 Mod \"{modName}\" 生成代码：未找到程序集或该 Mod 下无 XConfig 类型。请先编译工程。", "确定");
            AssetDatabase.Refresh();
        }

        [MenuItem(AssetsModMenuPrefix + "生成配置代码", true, 1)]
        public static bool GenerateConfigCodeFromContextValidate()
        {
            return TryGetSelectedModName(out _);
        }

        /// <summary>
        /// 为指定 Mod 生成配置代码（ClassHelper + Unmanaged），输出到 Mod 目录下 Config/Code.Gen。
        /// 返回是否成功（找到程序集并至少生成了代码）。
        /// </summary>
        public static bool GenerateConfigCodeForMod(string modName)
        {
            Assembly assembly = GetModAssembly(modName);
            if (assembly == null)
            {
                Debug.LogWarning($"[ModPackAndExport] 未找到 Mod 程序集: {modName}，请先编译工程。");
                return false;
            }
            string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, ModsFolderName, modName, "Config", "Code.Gen"));
            try
            {
                // 直接调用 ConfigNew 代码生成器 API
                int fileCount = CodeGenerationManager.GenerateForAssemblies(new List<Assembly> { assembly }, outputPath);
                Debug.Log($"[ModPackAndExport] 已为 Mod \"{modName}\" 生成配置代码 ({fileCount} 个文件) -> {outputPath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ModPackAndExport] 生成配置代码失败: {modName}, {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        private static Assembly GetModAssembly(string modName)
        {
            if (string.IsNullOrEmpty(modName)) return null;
            string asmName = modName.Replace(" ", "");
            return AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => string.Equals(a.GetName().Name, asmName, StringComparison.OrdinalIgnoreCase));
        }

        private static List<string> GetModNames()
        {
            string modsRoot = Path.Combine(Application.dataPath, ModsFolderName);
            var list = new List<string>();
            if (!Directory.Exists(modsRoot)) return list;
            foreach (string dir in Directory.GetDirectories(modsRoot))
            {
                if (File.Exists(Path.Combine(dir, ModDefineXmlName)))
                    list.Add(Path.GetFileName(dir));
            }
            return list;
        }

        /// <summary>
        /// 先请求编译，编译完成后再打包并导出（仅打包当前选中的 Mod 文件夹）。
        /// 导出前自动生成配置代码（ClassHelper + Unmanaged），再触发编译以使生成代码参与构建。
        /// </summary>
        private static void RequestCompileThenPackAndExport(string modName)
        {
            Debug.Log($"[ModPackAndExport] 请求编译并打包导出: {modName}");
            if (GenerateConfigCodeForMod(modName))
                Debug.Log($"[ModPackAndExport] 已自动生成配置代码: {modName}");
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
        /// 同步执行 YooAsset 打包，打包结束后直接导出。若未配置 YooAsset 包则跳过打包，仅拷贝 Xml/DLL 等。
        /// </summary>
        private static void TriggerYooAssetBuildThenExport(string modName)
        {
            string definePath = Path.Combine(Path.Combine(Application.dataPath, ModsFolderName), modName);
            definePath = Path.Combine(definePath, ModDefineXmlName);
            string packageName = File.Exists(definePath) ? GetPackageNameFromModDefine(definePath, modName) : modName;
            string assetName = File.Exists(definePath) ? GetAssetNameFromModDefine(definePath) : "Asset";

            if (!HasYooAssetPackage(packageName))
            {
                Debug.Log($"[ModPackAndExport] 未找到 YooAsset 包 \"{packageName}\"，跳过打包，仅执行拷贝（Xml/DLL/ModDefine 等）。");
                DoPackAndExportWithDialog(modName);
                return;
            }

            Debug.Log($"[ModPackAndExport] 开始 YooAsset 打包: {modName}, 包名: {packageName}, 资源根: Assets/{ModsFolderName}/{modName}/{assetName}");
            try
            {
                string yooErr = TryTriggerYooAssetBuild(packageName, modName, assetName);
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

        /// <summary>
        /// 检测 YooAsset 收集器配置中是否存在指定包名。若不存在则不进行 YooAsset 打包，仅拷贝。
        /// </summary>
        private static bool HasYooAssetPackage(string packageName)
        {
            if (string.IsNullOrEmpty(packageName)) return false;
            try
            {
                var setting = AssetBundleCollectorSettingData.Setting;
                if (setting?.Packages == null) return false;
                foreach (var pkg in setting.Packages)
                {
                    if (string.Equals(pkg.PackageName, packageName, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private static void DoPackAndExportWithDialog(string modName)
        {
            Debug.Log($"[ModPackAndExport] DoPackAndExportWithDialog: {modName}");
            string definePath = Path.Combine(Path.Combine(Application.dataPath, ModsFolderName), modName);
            definePath = Path.Combine(definePath, ModDefineXmlName);
            string packageName = File.Exists(definePath) ? GetPackageNameFromModDefine(definePath, modName) : modName;
            if (PackAndExport(modName, packageName, out string err))
                EditorUtility.DisplayDialog("打包并导出", $"Mod \"{modName}\" 已导出到主项目 {MainProjectModsFolderName} 文件夹。", "确定");
            else
                EditorUtility.DisplayDialog("打包并导出失败", err, "确定");
        }

        /// <summary>
        /// 直接调用 YooAsset.Editor 构建接口，对指定包名执行打包。参考官方文档 Jenkins 支持示例。
        /// https://www.yooasset.com/docs/guide-editor/AssetBundleBuilder
        /// Mod 打包并导出时固定使用 BuiltinBuildPipeline，避免 SBP 与 Unity BuildContext（如 IBundleExplicitObjectLayout）不兼容导致 CreateBuiltInShadersBundle 报错。
        /// 构建前将收集器的 CollectPath 设为 Mod 文件夹（Assets/Mods/modName/assetName），实现以 Mod 文件夹为资源根目录。
        /// </summary>
        private static string TryTriggerYooAssetBuild(string packageName, string modName, string assetName)
        {
            try
            {
                string modCollectPath = "Assets/" + ModsFolderName + "/" + modName + "/" + assetName;
                string oldCollectPath = SetPackageCollectPathToModFolder(packageName, modCollectPath);
                try
                {
                    BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
                    string buildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
                    string streamingAssetsRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
                    string packageVersion = DateTime.Now.ToString("yyyyMMddHHmmss");

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
                finally
                {
                    if (oldCollectPath != null)
                        SetPackageCollectPathToModFolder(packageName, oldCollectPath);
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 将指定包的收集器 CollectPath 设为 modCollectPath（以 Mod 文件夹为资源根），返回原路径；若未找到包则返回 null。
        /// </summary>
        private static string SetPackageCollectPathToModFolder(string packageName, string modCollectPath)
        {
            try
            {
                var setting = AssetBundleCollectorSettingData.Setting;
                if (setting?.Packages == null) return null;
                foreach (var pkg in setting.Packages)
                {
                    if (pkg.PackageName != packageName) continue;
                    if (pkg.Groups == null || pkg.Groups.Count == 0) continue;
                    var group = pkg.Groups[0];
                    if (group.Collectors == null || group.Collectors.Count == 0) continue;
                    var collector = group.Collectors[0];
                    string oldPath = collector.CollectPath;
                    collector.CollectPath = modCollectPath;
                    EditorUtility.SetDirty(setting);
                    AssetDatabase.SaveAssets();
                    return oldPath;
                }
                Debug.LogWarning($"[ModPackAndExport] 未在收集器中找到包 \"{packageName}\"，请确保 AssetBundleCollectorSetting 中存在同名包且 CollectPath 可指向 Mod 文件夹。");
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ModPackAndExport] 设置收集路径失败: {ex.Message}");
                return null;
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
        /// 当前选中的是否在某个 Mod 目录下（Assets/Mods/xxx 或其任意子路径），且该 Mod 含 ModDefine.xml。返回该 Mod 名称。
        /// 支持在 Mod 文件夹内任意位置（子文件夹、文件）右键打包。
        /// </summary>
        private static bool TryGetSelectedModName(out string modName)
        {
            modName = null;
            string modsRoot = "Assets/" + ModsFolderName + "/";
            if (Selection.assetGUIDs.Length != 1)
                return false;
            string path = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
            if (string.IsNullOrEmpty(path) || !path.StartsWith(modsRoot, StringComparison.OrdinalIgnoreCase) || path.Length <= modsRoot.Length)
                return false;
            string relative = path.Substring(modsRoot.Length).TrimStart('/', '\\');
            if (string.IsNullOrEmpty(relative))
                return false;
            // 取第一段作为 Mod 名（在 Mod 根或任意子目录/文件上右键均可）
            string firstSegment = relative.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (string.IsNullOrEmpty(firstSegment))
                return false;
            string modRootFull = Path.GetFullPath(Path.Combine(Application.dataPath, ModsFolderName, firstSegment));
            string definePath = Path.Combine(modRootFull, ModDefineXmlName);
            if (!File.Exists(definePath))
                return false;
            modName = firstSegment;
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

            // 是否有 YooAsset 包：无包则不打包、不拷贝 YooAssetPackage.xml 与 Asset；有包则打包并添加 Asset
            string definePathForMod = Path.Combine(Application.dataPath, ModsFolderName, modName, ModDefineXmlName);
            string packageNameForMod = File.Exists(definePathForMod) ? GetPackageNameFromModDefine(definePathForMod, modName) : modName;
            bool hasYooPackage = HasYooAssetPackage(packageNameForMod);
            GUI.enabled = false;
            EditorGUILayout.Toggle("是否有 YooAsset 包", hasYooPackage);
            GUI.enabled = true;
            if (!hasYooPackage)
                EditorGUILayout.HelpBox("未配置 YooAsset 包时：不执行打包，导出时不拷贝 YooAssetPackage.xml 与 Asset 文件夹。", MessageType.None);

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
        /// 打包并导出指定 Mod 到主项目 Mods 文件夹。packageName 从 ModDefine 读取（默认 modName），用于 YooAsset 构建输出目录。
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
            string packageName = GetPackageNameFromModDefine(definePath, modName);
            return PackAndExport(modName, packageName, out error);
        }

        /// <summary>
        /// 打包并导出指定 Mod 到主项目 Mods 文件夹，显式指定 YooAsset 包名。返回是否成功。
        /// </summary>
        public static bool PackAndExport(string modName, string packageName, out string error)
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

                bool hasYooPackage = HasYooAssetPackage(packageName);

                // YooAssetPackage.xml：仅当存在对应 YooAsset 包时才拷贝，否则去掉
                if (hasYooPackage)
                {
                    string yooPath = Path.Combine(modRoot, YooAssetPackageXmlName);
                    if (File.Exists(yooPath))
                    {
                        File.Copy(yooPath, Path.Combine(targetRoot, YooAssetPackageXmlName));
                        Debug.Log($"[ModPackAndExport] 已拷贝 {YooAssetPackageXmlName} -> {targetRoot}");
                    }
                }
                else
                {
                    Debug.Log($"[ModPackAndExport] 无 YooAsset 包，不拷贝 {YooAssetPackageXmlName}");
                }

                // Xml 文件夹（配置用，与 YooAsset 包无关，始终拷贝；复制时去掉 .meta 文件）
                string xmlSrc = Path.Combine(modRoot, XmlFolderName);
                string xmlDst = Path.Combine(targetRoot, XmlFolderName);
                if (Directory.Exists(xmlSrc))
                {
                    CopyDirectory(xmlSrc, xmlDst, excludeMetaFiles: true);
                    Debug.Log($"[ModPackAndExport] 已拷贝文件夹 {XmlFolderName} -> {xmlDst}（不含 .meta）");
                }

                // Asset（Bundles）：仅当存在 YooAsset 包时才拷贝/创建，否则去掉 Asset 项
                if (hasYooPackage)
                {
                    string buildOutputDir = GetModBuildOutputDirectory(packageName);
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
                }
                else
                {
                    Debug.Log($"[ModPackAndExport] 无 YooAsset 包，不创建/拷贝 Asset 文件夹");
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

        /// <summary>
        /// 从 ModDefine.xml 读取 YooAsset 包名（PackageName），若未配置则返回 modName。
        /// </summary>
        private static string GetPackageNameFromModDefine(string definePath, string modName)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(definePath);
                var node = doc.DocumentElement?.SelectSingleNode("PackageName");
                var raw = node?.InnerText?.Trim();
                return string.IsNullOrEmpty(raw) ? modName : raw;
            }
            catch
            {
                return modName;
            }
        }

        /// <summary>
        /// 从 ModDefine.xml 读取资源目录名（AssetName），若未配置则返回 "Asset"。
        /// </summary>
        private static string GetAssetNameFromModDefine(string definePath)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(definePath);
                var node = doc.DocumentElement?.SelectSingleNode("AssetName");
                var raw = node?.InnerText?.Trim();
                return string.IsNullOrEmpty(raw) ? "Asset" : raw;
            }
            catch
            {
                return "Asset";
            }
        }

        /// <summary>与 CreateModWindow 一致：asm 名为 Mod 名去空格，DLL 名为 asm 名 + .dll</summary>
        private static string GetModAsmDllName(string modName)
        {
            return (modName ?? "").Replace(" ", "") + ".dll";
        }

        /// <summary>
        /// 获取该 Mod 的 YooAsset 构建输出目录（最新一次构建的版本目录）。路径为 BuildOutputRoot/BuildTarget/PackageName/PackageVersion。
        /// 注意：必须排除 OutputCache 目录（构建缓存），仅使用 yyyyMMddHHmmss 格式的版本目录（含 manifest）。
        /// 若有多个可能位置（当前项目 Bundles、XModTest Bundles），从所有位置收集版本目录并取最新的。
        /// </summary>
        private static string GetModBuildOutputDirectory(string packageName)
        {
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var candidatePackageRoots = new List<string>();

            // 主构建输出
            string buildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            string mainPackageRoot = Path.Combine(buildOutputRoot, buildTarget.ToString(), packageName);
            if (Directory.Exists(mainPackageRoot))
                candidatePackageRoots.Add(mainPackageRoot);

            // 当主项目为 XMGame 时，XModTest 中可能也有构建输出
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string xmodTestBundles = Path.Combine(projectRoot, "XModTest", "Bundles");
            string xmodTestPackageRoot = Path.Combine(xmodTestBundles, buildTarget.ToString(), packageName);
            if (Directory.Exists(xmodTestPackageRoot) && !candidatePackageRoots.Contains(xmodTestPackageRoot))
                candidatePackageRoots.Add(xmodTestPackageRoot);

            if (candidatePackageRoots.Count == 0)
                return null;

            try
            {
                var allVersionDirs = candidatePackageRoots
                    .SelectMany(root => Directory.GetDirectories(root))
                    .Where(d => !string.Equals(Path.GetFileName(d), "OutputCache", StringComparison.OrdinalIgnoreCase))
                    .Where(d => System.Text.RegularExpressions.Regex.IsMatch(Path.GetFileName(d), @"^\d{14}$"))
                    .ToList();
                if (allVersionDirs.Count == 0)
                    return null;
                // 版本目录名为 yyyyMMddHHmmss，取名称最大即最新
                return allVersionDirs.OrderByDescending(Path.GetFileName).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        private static void CopyDirectory(string sourceDir, string targetDir, bool excludeMetaFiles = false)
        {
            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string name = Path.GetFileName(file);
                if (excludeMetaFiles && name.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                    continue;
                File.Copy(file, Path.Combine(targetDir, name));
            }
            foreach (string sub in Directory.GetDirectories(sourceDir))
            {
                string name = Path.GetFileName(sub);
                CopyDirectory(sub, Path.Combine(targetDir, name), excludeMetaFiles);
            }
        }
    }
}
#endif
