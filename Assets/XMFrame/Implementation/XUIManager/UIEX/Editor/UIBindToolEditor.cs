using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using XM.UIEX;

namespace XM.UIManager.Editor
{
    [CustomEditor(typeof(UIBindTool))]
    public class UIBindToolEditor : UnityEditor.Editor
    {
        private SerializedProperty _uiCtrlProp;
        private SerializedProperty _bindEntriesProp;
        private Vector2 _scrollPos;

        private void OnEnable()
        {
            _uiCtrlProp = serializedObject.FindProperty("_uiCtrl");
            _bindEntriesProp = serializedObject.FindProperty("_bindEntries");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_uiCtrlProp, new GUIContent("UICtrl", "关联的 UICtrl"));

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("绑定收集", EditorStyles.boldLabel);

            var tool = (UIBindTool)target;
            var count = tool.BindEntries?.Count ?? 0;
            EditorGUILayout.LabelField($"已收集: {count} 项");

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("收集绑定", GUILayout.Height(24)))
            {
                CollectBindings(tool);
            }
            if (GUILayout.Button("生成视图代码", GUILayout.Height(24)))
            {
                GenerateViewCode(tool);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            if (_bindEntriesProp != null && _bindEntriesProp.arraySize > 0)
            {
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.MaxHeight(200));
                EditorGUI.indentLevel++;
                for (int i = 0; i < _bindEntriesProp.arraySize; i++)
                {
                    var elem = _bindEntriesProp.GetArrayElementAtIndex(i);
                    var path = elem.FindPropertyRelative("Path")?.stringValue ?? "";
                    var bindName = elem.FindPropertyRelative("BindName")?.stringValue ?? "";
                    var compType = elem.FindPropertyRelative("ComponentType")?.stringValue ?? "";
                    EditorGUILayout.LabelField($"{bindName} ({compType})", path);
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.EndScrollView();
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 递归收集子节点下 bind=true 且非 UICtrl 的节点
        /// </summary>
        private void CollectBindings(UIBindTool tool)
        {
            var root = tool.transform;
            var uiCtrl = tool.UICtrl;
            if (uiCtrl == null)
                uiCtrl = root.GetComponentInParent<UICtrlBase>();

            var rootForPath = uiCtrl != null ? uiCtrl.transform : root;
            var entries = new List<UIBindEntry>();
            CollectRecursive(rootForPath, rootForPath, "", entries);

            tool.SetBindEntries(entries);
            EditorUtility.SetDirty(tool);

            Debug.Log($"[UIBindTool] 收集到 {entries.Count} 个绑定项");
        }

        private void CollectRecursive(Transform root, Transform current, string pathPrefix, List<UIBindEntry> outEntries)
        {
            for (int i = 0; i < current.childCount; i++)
            {
                var child = current.GetChild(i);

                // 若为 UICtrl，跳过且不递归其子节点（子节点属于该 UICtrl）
                if (child.GetComponent<UICtrlBase>() != null)
                    continue;

                var relPath = string.IsNullOrEmpty(pathPrefix) ? child.name : pathPrefix + "/" + child.name;

                // 查找实现 IUIEx 且 Bind=true 的组件
                var uiEx = child.GetComponent<IUIEx>();
                if (uiEx != null && uiEx.Bind)
                {
                    var compType = uiEx.GetType().Name;
                    var bindName = !string.IsNullOrEmpty(uiEx.BindName) ? uiEx.BindName : SanitizeFieldName(child.name);
                    outEntries.Add(new UIBindEntry(relPath, bindName, compType, child.name));
                }

                // 继续递归子节点
                CollectRecursive(root, child, relPath, outEntries);
            }
        }

        private static string SanitizeFieldName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "unnamed";
            var sb = new StringBuilder();
            foreach (var c in name)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                    sb.Append(c);
            }
            var s = sb.ToString();
            if (s.Length > 0 && char.IsDigit(s[0]))
                s = "_" + s;
            return string.IsNullOrEmpty(s) ? "unnamed" : s;
        }

        private void GenerateViewCode(UIBindTool tool)
        {
            var entries = tool.BindEntries;
            if (entries == null || entries.Count == 0)
            {
                EditorUtility.DisplayDialog("生成视图代码", "请先执行「收集绑定」", "确定");
                return;
            }

            var root = tool.transform;
            var uiCtrl = tool.UICtrl;
            if (uiCtrl == null)
                uiCtrl = root.GetComponentInParent<UICtrlBase>();

            var ctrlTypeName = uiCtrl != null ? uiCtrl.GetType().Name : "UICtrlBase";
            var viewClassName = ctrlTypeName.Replace("Ctrl", "View").Replace("Base", "");
            if (string.IsNullOrEmpty(viewClassName))
                viewClassName = "UIView";

            var sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using XM.UIEX;");
            sb.AppendLine();
            sb.AppendLine("namespace XM");
            sb.AppendLine("{");
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// {viewClassName} 视图层，由 UIBindTool 自动生成");
            sb.AppendLine($"    /// </summary>");
            sb.AppendLine($"    public class {viewClassName}");
            sb.AppendLine("    {");
            sb.AppendLine("        private Transform _root;");
            sb.AppendLine();

            foreach (var e in entries)
            {
                sb.AppendLine($"        public {e.ComponentType} {e.BindName} {{ get; private set; }}");
            }
            sb.AppendLine();

            sb.AppendLine("        public void Bind(Transform root)");
            sb.AppendLine("        {");
            sb.AppendLine("            _root = root;");
            foreach (var e in entries)
            {
                var pathArg = string.IsNullOrEmpty(e.Path) ? "\"\"" : $"\"{e.Path}\"";
                sb.AppendLine($"            {e.BindName} = root.Find({pathArg})?.GetComponent<{e.ComponentType}>();");
            }
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            var assetPath = GetDefaultOutputPath(tool);
            var fullPath = Path.Combine(Application.dataPath, assetPath.Replace("Assets/", "").Replace("Assets\\", ""));
            var dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(fullPath, sb.ToString(), Encoding.UTF8);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("生成视图代码", $"已生成: {assetPath}", "确定");
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath));
        }

        private static string GetDefaultOutputPath(UIBindTool tool)
        {
            var scriptPath = GetScriptPath(tool);
            if (!string.IsNullOrEmpty(scriptPath))
            {
                var dir = Path.GetDirectoryName(scriptPath);
                var asmDir = FindAsmdefDirectory(dir);
                if (!string.IsNullOrEmpty(asmDir))
                {
                    var rel = Path.Combine(asmDir, "Generated", "UI", $"{tool.GetType().Name}_View.cs");
                    return rel.Replace("\\", "/");
                }
            }
            return "Assets/Generated/UI/UIBindTool_View.cs";
        }

        private static string GetScriptPath(UIBindTool tool)
        {
            var ms = MonoScript.FromMonoBehaviour(tool);
            if (ms != null)
                return AssetDatabase.GetAssetPath(ms);
            return null;
        }

        private static string FindAsmdefDirectory(string startDir)
        {
            if (string.IsNullOrEmpty(startDir)) return null;
            var fullDir = Path.Combine(Application.dataPath, startDir.Replace("Assets/", "").Replace("Assets\\", ""));
            var dir = fullDir;
            while (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
            {
                var files = Directory.GetFiles(dir, "*.asmdef");
                if (files.Length > 0)
                {
                    var rel = dir.Replace(Application.dataPath, "").Replace("\\", "/").TrimStart('/', '\\');
                    return string.IsNullOrEmpty(rel) ? "Assets" : "Assets/" + rel;
                }
                dir = Path.GetDirectoryName(dir);
            }
            return null;
        }
    }
}
