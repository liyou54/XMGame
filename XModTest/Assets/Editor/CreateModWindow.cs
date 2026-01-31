#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace XMod.Editor
{
    /// <summary>
    /// Mod 工程中用于创建新 Mod 的编辑器窗口。
    /// 创建 Mod 目录、ModDefine.xml、Xml 文件夹、Asset 文件夹、YooAsset 包配置、模板入口脚本与 asmdef。
    /// </summary>
    public class CreateModWindow : EditorWindow
    {
        private const string ModsFolderName = "Mods";
        private const string ModDefineXmlName = "ModDefine.xml";
        private const string ScriptsFolderName = "Scripts";
        private const string XmlFolderName = "Xml";
        private const string AssetFolderName = "Asset";

        /// <summary>YooAsset 资源路径固定为 Asset 文件夹（相对 Mod 根目录）</summary>
        private const string YooAssetPathFixed = "Asset";

        private string _modName = "MyMod";
        private string _version = "1.0.0";
        private string _author = "";
        private string _description = "";
        private string _dllPath = ""; // 空则用 {ModName}.dll
        private string _packageName = ""; // 空则与 Mod 名称一致，用于 YooAsset 包名
        private string _assetName = "Asset"; // YooAsset 资源目录名（相对 Mod 根目录）
        private string _iconPath = "";
        private string _homePageLink = "";
        private string _imagePath = "";
        private Vector2 _scrollPos;

        [MenuItem("Mod/创建 Mod")]
        public static void Open()
        {
            var win = GetWindow<CreateModWindow>(true, "创建 Mod", true);
            win.minSize = new Vector2(360, 420);
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("新建 Mod", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            _modName = EditorGUILayout.TextField("Mod 名称", _modName);
            if (string.IsNullOrWhiteSpace(_modName))
            {
                EditorGUILayout.HelpBox("Mod 名称不能为空。", MessageType.Warning);
            }

            _version = EditorGUILayout.TextField("版本", _version);
            _author = EditorGUILayout.TextField("作者", _author);
            _description = EditorGUILayout.TextField("描述", _description);
            _dllPath = EditorGUILayout.TextField("DLL 路径（相对 Mod 目录）", _dllPath);
            if (string.IsNullOrWhiteSpace(_dllPath))
                EditorGUILayout.HelpBox("留空则使用 \"{ModName}.dll\"。", MessageType.None);
            _packageName = EditorGUILayout.TextField("YooAsset 包名（可选）", _packageName);
            if (string.IsNullOrWhiteSpace(_packageName))
                EditorGUILayout.HelpBox("留空则与 Mod 名称一致。", MessageType.None);
            _assetName = EditorGUILayout.TextField("资源目录名（AssetName）", _assetName);
            if (string.IsNullOrWhiteSpace(_assetName))
                EditorGUILayout.HelpBox("留空则使用 \"Asset\"。", MessageType.None);
            _iconPath = EditorGUILayout.TextField("图标路径（相对 Mod 目录）", _iconPath);
            _homePageLink = EditorGUILayout.TextField("主页链接", _homePageLink);
            _imagePath = EditorGUILayout.TextField("图片路径（相对 Mod 目录）", _imagePath);

            EditorGUILayout.Space(12);

            GUI.enabled = !string.IsNullOrWhiteSpace(_modName);
            if (GUILayout.Button("创建 Mod", GUILayout.Height(28)))
            {
                CreateMod();
            }
            GUI.enabled = true;

            EditorGUILayout.EndScrollView();
        }

        private void CreateMod()
        {
            string modName = _modName.Trim();
            string version = string.IsNullOrWhiteSpace(_version) ? "1.0.0" : _version.Trim();
            string author = _author?.Trim() ?? "";
            string description = _description?.Trim() ?? "";
            string dllPath = string.IsNullOrWhiteSpace(_dllPath) ? $"{modName}.dll" : _dllPath.Trim();
            string packageName = string.IsNullOrWhiteSpace(_packageName) ? modName : _packageName.Trim();
            string assetName = string.IsNullOrWhiteSpace(_assetName) ? "Asset" : _assetName.Trim();
            string iconPath = _iconPath?.Trim() ?? "";
            string homePageLink = _homePageLink?.Trim() ?? "";
            string imagePath = _imagePath?.Trim() ?? "";

            string assetsPath = Application.dataPath;
            string modsRoot = Path.Combine(assetsPath, ModsFolderName);
            string modRoot = Path.Combine(modsRoot, modName);
            string scriptsDir = Path.Combine(modRoot, ScriptsFolderName);
            string xmlDir = Path.Combine(modRoot, XmlFolderName);
            string assetDir = Path.Combine(modRoot, assetName);

            if (Directory.Exists(modRoot))
            {
                if (!EditorUtility.DisplayDialog("创建 Mod", $"目录已存在: {modRoot}\n是否覆盖并重新生成？", "覆盖", "取消"))
                    return;
            }

            try
            {
                if (!Directory.Exists(modsRoot))
                    Directory.CreateDirectory(modsRoot);
                if (!Directory.Exists(modRoot))
                    Directory.CreateDirectory(modRoot);
                if (!Directory.Exists(scriptsDir))
                    Directory.CreateDirectory(scriptsDir);
                if (!Directory.Exists(xmlDir))
                    Directory.CreateDirectory(xmlDir);
                if (!Directory.Exists(assetDir))
                    Directory.CreateDirectory(assetDir);

                WriteModDefineXml(modRoot, modName, version, author, description, dllPath, packageName, assetName, iconPath, homePageLink, imagePath);
                WriteYooAssetPackageConfig(modRoot, packageName, assetName);
                WriteModEntryCs(scriptsDir, modName);
                WriteAssemblyModName(scriptsDir, modName);
                WriteAsmdef(modRoot, modName);

                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("创建 Mod", $"Mod \"{modName}\" 已创建。\n路径: Assets/{ModsFolderName}/{modName}\n含: ModDefine.xml、Xml、Asset、Scripts（含 AssemblyModName.cs）、YooAssetPackage.xml", "确定");
                Close();
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("创建 Mod 失败", ex.Message, "确定");
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// 生成 ModDefine.xml，节点与 ModConfig 对应：Name→ModName, Version, Author, Description, DllPath, PackageName, AssetName, IconPath, HomePageLink, ImagePath。ConfigFiles 由运行时读 Xml 目录自动获取。
        /// </summary>
        private static void WriteModDefineXml(string modRoot, string modName, string version, string author, string description, string dllPath, string packageName, string assetName, string iconPath, string homePageLink, string imagePath)
        {
            string path = Path.Combine(modRoot, ModDefineXmlName);
            string xml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<ModDefine>
  <Name>{EscapeXml(modName)}</Name>
  <Version>{EscapeXml(version)}</Version>
  <Author>{EscapeXml(author)}</Author>
  <Description>{EscapeXml(description)}</Description>
  <DllPath>{EscapeXml(dllPath)}</DllPath>
  <PackageName>{EscapeXml(packageName)}</PackageName>
  <AssetName>{EscapeXml(assetName)}</AssetName>
  <IconPath>{EscapeXml(iconPath)}</IconPath>
  <HomePageLink>{EscapeXml(homePageLink)}</HomePageLink>
  <ImagePath>{EscapeXml(imagePath)}</ImagePath>
</ModDefine>
";
            File.WriteAllText(path, xml);
        }

        /// <summary>
        /// 生成 YooAsset 包配置；YooAsset 资源路径为固定路径 Asset。
        /// </summary>
        private static void WriteYooAssetPackageConfig(string modRoot, string modName, string yooAssetPathFixed)
        {
            string path = Path.Combine(modRoot, "YooAssetPackage.xml");
            string xml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<YooAssetPackage>
  <PackageName>{EscapeXml(modName)}</PackageName>
  <AssetPath>{EscapeXml(yooAssetPathFixed)}</AssetPath>
  <Description>Mod 资源包，资源路径固定为 Asset 目录。</Description>
</YooAssetPackage>
";
            File.WriteAllText(path, xml);
        }

        private static string EscapeXml(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }

        private static void WriteModEntryCs(string scriptsDir, string modName)
        {
            string className = modName + "Entry";
            string path = Path.Combine(scriptsDir, className + ".cs");
            string ns = modName.Replace(" ", "");
            string content = $@"using XM.Contracts;

namespace {ns}
{{
    public class {className} : ModBase
    {{
        public override void OnCreate()
        {{
        }}

        public override void OnInit()
        {{
        }}
    }}
}}
";
            File.WriteAllText(path, content);
        }

        /// <summary>
        /// 生成程序集级 Mod 名称特性文件，供主工程通过反射读取该 Mod 程序集对应的 Mod 名。
        /// </summary>
        private static void WriteAssemblyModName(string scriptsDir, string modName)
        {
            string path = Path.Combine(scriptsDir, "AssemblyModName.cs");
            string content = $@"// 由「创建 Mod」生成，用于标记程序集对应的 Mod 名称；主工程可通过反射读取。
[assembly: XM.Contracts.ModNameAttribute(""{modName.Replace("\"", "\\\"")}"")]
";
            File.WriteAllText(path, content);
        }

        private static void WriteAsmdef(string modRoot, string modName)
        {
            string asmName = modName.Replace(" ", "");
            string path = Path.Combine(modRoot, asmName + ".asmdef");
            string json = $@"{{
    ""name"": ""{asmName}"",
    ""rootNamespace"": """",
    ""references"": [
        ""XM.Contracts""
    ],
    ""includePlatforms"": [],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": true,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}}
";
            File.WriteAllText(path, json);
        }
    }
}
#endif
