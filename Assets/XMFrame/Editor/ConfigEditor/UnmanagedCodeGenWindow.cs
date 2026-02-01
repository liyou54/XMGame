using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityToolkit;

namespace XM.Editor
{
    /// <summary>
    /// 非托管代码生成编辑器窗口
    /// </summary>
    public class UnmanagedCodeGenWindow : EditorWindow
    {
        private List<AssemblyInfo> allAssemblies = new List<AssemblyInfo>();
        private Vector2 scrollPosition;
        private string customOutputPath = "";
        private bool useCustomPath = false;

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

        [MenuItem("XMFrame/Config/Generate Code (Select Assemblies)")]
        [MenuItem("UnityToolkit/Config/Generate Code (Select Assemblies)")]
        public static void ShowWindow()
        {
            var window = GetWindow<UnmanagedCodeGenWindow>("代码生成器");
            window.minSize = new Vector2(500, 400);
            window.RefreshAssemblies();
        }

        private void OnEnable()
        {
            RefreshAssemblies();
        }

        private void RefreshAssemblies()
        {
            allAssemblies.Clear();

            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var configTypes = assembly.GetTypes()
                        .Where(t => UnityToolkit.UnmanagedCodeGenerator.IsXConfigType(t) && !t.IsAbstract)
                        .ToList();

                    if (configTypes.Count > 0)
                    {
                        allAssemblies.Add(new AssemblyInfo(assembly, configTypes.Count));
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    // 忽略无法加载的程序集
                }
            }

            // 按名称排序
            allAssemblies = allAssemblies.OrderBy(a => a.Name).ToList();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);

            // 标题：共用下方 toggle 同时生成 Unmanaged + 配置（ConfigClassHelper 静态代码）
            EditorGUILayout.LabelField("选择要生成代码的程序集", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("勾选程序集后点击生成：将生成 Unmanaged 结构体 + ConfigClassHelper（配置解析，静态代码生成，无反射）", MessageType.None);
            EditorGUILayout.Space(5);

            // 全选/取消全选按钮
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("全选", GUILayout.Width(80)))
            {
                foreach (var info in allAssemblies)
                {
                    info.Selected = true;
                }
            }
            if (GUILayout.Button("取消全选", GUILayout.Width(80)))
            {
                foreach (var info in allAssemblies)
                {
                    info.Selected = false;
                }
            }
            if (GUILayout.Button("刷新", GUILayout.Width(80)))
            {
                RefreshAssemblies();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // 程序集列表
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (var info in allAssemblies)
            {
                EditorGUILayout.BeginHorizontal();
                info.Selected = EditorGUILayout.Toggle(info.Selected, GUILayout.Width(20));
                EditorGUILayout.LabelField(info.Name, GUILayout.Width(300));
                EditorGUILayout.LabelField($"({info.ConfigTypeCount} 个配置类型)", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);

            // 输出路径选项
            EditorGUILayout.LabelField("输出路径设置", EditorStyles.boldLabel);
            useCustomPath = EditorGUILayout.Toggle("使用自定义输出路径", useCustomPath);
            if (useCustomPath)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("输出目录:", GUILayout.Width(80));
                customOutputPath = EditorGUILayout.TextField(customOutputPath);
                if (GUILayout.Button("浏览", GUILayout.Width(60)))
                {
                    var path = EditorUtility.OpenFolderPanel("选择输出目录", "Assets", "");
                    if (!string.IsNullOrEmpty(path))
                    {
                        var projectPath = Application.dataPath.Replace("/Assets", "").Replace("\\Assets", "");
                        if (path.StartsWith(projectPath))
                        {
                            customOutputPath = path.Substring(projectPath.Length).TrimStart('\\', '/');
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("错误", "选择的目录必须在项目目录内", "确定");
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(10);

            // 生成按钮
            var selectedCount = allAssemblies.Count(a => a.Selected);
            GUI.enabled = selectedCount > 0;
            if (GUILayout.Button($"生成代码 ({selectedCount} 个程序集)", GUILayout.Height(30)))
            {
                GenerateCode();
            }
            GUI.enabled = true;

            EditorGUILayout.Space(5);
            var selectedConfigTypeCount = allAssemblies.Where(a => a.Selected).Sum(a => a.ConfigTypeCount);
            EditorGUILayout.HelpBox($"已选择 {selectedCount} 个程序集，共 {selectedConfigTypeCount} 个配置类型", MessageType.Info);
        }

        private void GenerateCode()
        {
            var selectedAssemblies = allAssemblies.Where(a => a.Selected).Select(a => a.Assembly).ToList();
            
            if (selectedAssemblies.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "请至少选择一个程序集", "确定");
                return;
            }

            string outputPath = useCustomPath && !string.IsNullOrEmpty(customOutputPath) ? customOutputPath : null;
            
            try
            {
                UnityToolkit.UnmanagedCodeGenerator.GenerateUnmanagedCodeForAssemblies(selectedAssemblies, outputPath);
                UnityToolkit.ClassHelperCodeGenerator.GenerateClassHelperForAssemblies(selectedAssemblies, outputPath);
                EditorUtility.DisplayDialog("成功", $"已成功为 {selectedAssemblies.Count} 个程序集生成 Unmanaged 结构体与 ConfigClassHelper（配置解析静态代码）", "确定");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("错误", $"生成代码时出错:\n{ex.Message}", "确定");
            }
        }
    }
}
