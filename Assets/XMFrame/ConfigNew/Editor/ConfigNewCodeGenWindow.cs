using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using XM.ConfigNew.Tools;

namespace XM.ConfigNew.Editor
{
    /// <summary>
    /// ConfigNew 代码生成编辑器窗口
    /// 用于测试新的代码生成器
    /// </summary>
    public class ConfigNewCodeGenWindow : EditorWindow
    {
        private List<AssemblyInfo> _allAssemblies = new List<AssemblyInfo>();
        private List<ConfigTypeInfo> _allConfigTypes = new List<ConfigTypeInfo>();
        private Vector2 _scrollPosition;
        private string _customOutputPath = "";
        private bool _useCustomPath = false;
        private int _selectedTab = 0; // 0=按程序集, 1=按类型
        
        [Serializable]
        private class AssemblyInfo
        {
            public string Name;
            public Assembly Assembly;
            public bool Selected;
            public int ConfigTypeCount;
            
            public AssemblyInfo(Assembly assembly, int configTypeCount)
            {
                Name = assembly.GetName().Name;
                Assembly = assembly;
                Selected = false;
                ConfigTypeCount = configTypeCount;
            }
        }
        
        [Serializable] 
        private class ConfigTypeInfo
        {
            public string Name;
            public Type Type;
            public bool Selected;
            public string AssemblyName;
            public string Namespace;
            
            public ConfigTypeInfo(Type type)
            {
                Name = type.Name;
                Type = type;
                Selected = false;
                AssemblyName = type.Assembly.GetName().Name;
                Namespace = type.Namespace ?? "";
            }
        }
        
        [MenuItem("XMFrame/Config/代码生成器")]
        [MenuItem("XMFrame/Config/Generate Code")]
        public static void ShowWindow()
        {
            var window = GetWindow<ConfigNewCodeGenWindow>("配置代码生成器");
            window.minSize = new Vector2(600, 500);
            window.RefreshData();
        }
        
        private void OnEnable()
        {
            RefreshData();
        }
        
        private void RefreshData()
        {
            _allAssemblies.Clear();
            _allConfigTypes.Clear();
            
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var configTypes = assembly.GetTypes()
                        .Where(t => TypeAnalyzer.IsXConfigType(t) && !t.IsAbstract)
                        .ToList();
                    
                    if (configTypes.Count > 0)
                    {
                        _allAssemblies.Add(new AssemblyInfo(assembly, configTypes.Count));
                        
                        foreach (var type in configTypes)
                        {
                            _allConfigTypes.Add(new ConfigTypeInfo(type));
                        }
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    // 忽略无法加载的程序集
                }
            }
            
            // 排序
            _allAssemblies = _allAssemblies.OrderBy(a => a.Name).ToList();
            _allConfigTypes = _allConfigTypes.OrderBy(t => t.Name).ToList();
        }
        
        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            
            // 标题
            EditorGUILayout.LabelField("ConfigNew 代码生成器(测试)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "使用新的元数据系统生成 Unmanaged 结构体\n" +
                "特性:\n" +
                "- 支持枚举、可空类型\n" +
                "- 支持无限层嵌套容器\n" +
                "- 支持容器自定义解析器\n" +
                "- 索引结构体(部分类,每个索引一个文件)",
                MessageType.Info
            );
            
            EditorGUILayout.Space(10);
            
            // Tab选择
            _selectedTab = GUILayout.Toolbar(_selectedTab, new[] { "按程序集选择", "按类型选择" });
            
            EditorGUILayout.Space(5);
            
            // 根据Tab显示不同内容
            if (_selectedTab == 0)
            {
                DrawAssemblyTab();
            }
            else
            {
                DrawTypeTab();
            }
            
            EditorGUILayout.Space(10);
            
            // 输出路径设置
            DrawOutputPathSettings();
            
            EditorGUILayout.Space(10);
            
            // 生成按钮
            DrawGenerateButton();
        }
        
        #region 按程序集选择Tab
        
        private void DrawAssemblyTab()
        {
            // 操作按钮
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("全选", GUILayout.Width(80)))
            {
                foreach (var info in _allAssemblies)
                    info.Selected = true;
            }
            if (GUILayout.Button("取消全选", GUILayout.Width(80)))
            {
                foreach (var info in _allAssemblies)
                    info.Selected = false;
            }
            if (GUILayout.Button("刷新", GUILayout.Width(80)))
            {
                RefreshData();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // 程序集列表
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            foreach (var info in _allAssemblies)
            {
                EditorGUILayout.BeginHorizontal();
                info.Selected = EditorGUILayout.Toggle(info.Selected, GUILayout.Width(20));
                EditorGUILayout.LabelField(info.Name, GUILayout.Width(300));
                EditorGUILayout.LabelField($"({info.ConfigTypeCount} 个配置类型)", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }
        
        #endregion
        
        #region 按类型选择Tab
        
        private void DrawTypeTab()
        {
            // 搜索框
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("搜索:", GUILayout.Width(50));
            var searchText = EditorGUILayout.TextField("", GUILayout.Width(200));
            if (GUILayout.Button("全选", GUILayout.Width(60)))
            {
                foreach (var info in _allConfigTypes)
                    info.Selected = true;
            }
            if (GUILayout.Button("取消", GUILayout.Width(60)))
            {
                foreach (var info in _allConfigTypes)
                    info.Selected = false;
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // 类型列表
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            var filteredTypes = string.IsNullOrEmpty(searchText)
                ? _allConfigTypes
                : _allConfigTypes.Where(t => t.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            
            foreach (var info in filteredTypes)
            {
                EditorGUILayout.BeginHorizontal();
                info.Selected = EditorGUILayout.Toggle(info.Selected, GUILayout.Width(20));
                EditorGUILayout.LabelField(info.Name, GUILayout.Width(200));
                EditorGUILayout.LabelField($"[{info.AssemblyName}]", EditorStyles.miniLabel, GUILayout.Width(150));
                EditorGUILayout.LabelField(info.Namespace, EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        #endregion
        
        #region 输出路径设置
        
        private void DrawOutputPathSettings()
        {
            EditorGUILayout.LabelField("输出路径设置", EditorStyles.boldLabel);
            _useCustomPath = EditorGUILayout.Toggle("使用自定义输出路径", _useCustomPath);
            
            if (_useCustomPath)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("输出目录:", GUILayout.Width(80));
                _customOutputPath = EditorGUILayout.TextField(_customOutputPath);
                if (GUILayout.Button("浏览", GUILayout.Width(60)))
                {
                    var path = EditorUtility.OpenFolderPanel("选择输出目录", "Assets", "");
                    if (!string.IsNullOrEmpty(path))
                    {
                        var projectPath = Application.dataPath.Replace("/Assets", "").Replace("\\Assets", "");
                        if (path.StartsWith(projectPath))
                        {
                            _customOutputPath = path.Substring(projectPath.Length).TrimStart('\\', '/');
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("错误", "选择的目录必须在项目目录内", "确定");
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("将使用程序集所在目录作为输出目录", MessageType.None);
            }
        }
        
        #endregion
        
        #region 生成按钮
        
        private void DrawGenerateButton()
        {
            int selectedCount;
            List<Type> selectedTypes;
            
            if (_selectedTab == 0)
            {
                // 按程序集
                selectedCount = _allAssemblies.Count(a => a.Selected);
                selectedTypes = _allAssemblies
                    .Where(a => a.Selected)
                    .SelectMany(a => GetConfigTypesFromAssembly(a.Assembly))
                    .ToList();
            }
            else
            {
                // 按类型
                selectedCount = _allConfigTypes.Count(t => t.Selected);
                selectedTypes = _allConfigTypes
                    .Where(t => t.Selected)
                    .Select(t => t.Type)
                    .ToList();
            }
            
            GUI.enabled = selectedCount > 0;
            
            if (GUILayout.Button($"生成 Unmanaged 代码 ({selectedTypes.Count} 个类型)", GUILayout.Height(40)))
            {
                GenerateCode(selectedTypes);
            }
            
            GUI.enabled = true;
            
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox($"将生成 {selectedTypes.Count} 个 Unmanaged 结构体文件", MessageType.Info);
        }
        
        #endregion
        
        #region 代码生成逻辑
        
        private List<Type> GetConfigTypesFromAssembly(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes()
                    .Where(t => TypeAnalyzer.IsXConfigType(t) && !t.IsAbstract)
                    .ToList();
            }
            catch
            {
                return new List<Type>();
            }
        }
        
        private void GenerateCode(List<Type> types)
        {
            if (types == null || types.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "没有选择任何类型", "确定");
                return;
            }
            
            try
            {
                var manager = new CodeGen.CodeGenerationManager();
                var allGeneratedFiles = new List<string>();
                var successCount = 0;
                var errorCount = 0;
                
                foreach (var type in types)
                {
                    try
                    {
                        // 确定输出目录
                        var outputDir = GetOutputDirectory(type);
                        
                        // 生成代码
                        var files = manager.GenerateForType(type, outputDir);
                        allGeneratedFiles.AddRange(files);
                        successCount++;
                        
                        Debug.Log($"[ConfigNew] 已生成: {type.Name} -> {files.Count} 个文件");
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        Debug.LogError($"[ConfigNew] 生成失败: {type.Name}\n{ex.Message}");
                    }
                }
                
                // 刷新AssetDatabase
                AssetDatabase.Refresh();
                
                // 显示结果
                var message = $"生成完成!\n" +
                             $"成功: {successCount} 个类型\n" +
                             $"失败: {errorCount} 个类型\n" +
                             $"生成文件: {allGeneratedFiles.Count} 个";
                
                EditorUtility.DisplayDialog("生成完成", message, "确定");
                
                // 在控制台输出文件列表
                Debug.Log($"[ConfigNew] 生成的文件列表:\n{string.Join("\n", allGeneratedFiles)}");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("错误", $"生成代码时出错:\n{ex.Message}\n{ex.StackTrace}", "确定");
            }
        }
        
        /// <summary>
        /// 获取输出目录
        /// </summary>
        private string GetOutputDirectory(Type type)
        {
            if (_useCustomPath && !string.IsNullOrEmpty(_customOutputPath))
            {
                return Path.Combine(Application.dataPath.Replace("Assets", ""), _customOutputPath);
            }
            
            // 使用类型所在程序集的目录
            var assembly = type.Assembly;
            var assemblyName = assembly.GetName().Name;
            
            // 查找asmdef文件
            var asmdefFiles = Directory.GetFiles(Application.dataPath, $"{assemblyName}.asmdef", SearchOption.AllDirectories);
            
            if (asmdefFiles.Length > 0)
            {
                var asmdefDir = Path.GetDirectoryName(asmdefFiles[0]);
                return Path.Combine(asmdefDir, "Generated");
            }
            
            // 默认输出到ConfigNew目录
            return Path.Combine(Application.dataPath, "XMFrame", "ConfigNew", "Generated");
        }
        
        #endregion
    }
}
