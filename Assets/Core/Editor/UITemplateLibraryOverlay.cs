using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace XM.UITemplate.Editor
{
    /// <summary>
    /// UI 模板库 - 使用 Overlay 系统，与 AI Navigation 相同
    /// 启动时自动添加到 Scene View，常驻显示
    /// </summary>
    [Overlay(typeof(SceneView), "UITemplateLibrary", "UI 模板库", true,
        defaultDockZone = DockZone.RightColumn,
        defaultDockPosition = DockPosition.Bottom,
        defaultLayout = Layout.Panel)]
    public class UITemplateLibraryOverlay : Overlay
    {
        public override VisualElement CreatePanelContent()
        {
            return new UITemplateLibraryPanelElement();
        }
    }

    [InitializeOnLoad]
    public static class UITemplateLibraryOverlayBootstrap
    {
        private static readonly HashSet<SceneView> _checkedViews = new HashSet<SceneView>();

        static UITemplateLibraryOverlayBootstrap()
        {
            EditorApplication.delayCall += () =>
            {
                EnsureOverlayVisible();
                EditorApplication.update += OnUpdate;
            };
            SceneView.duringSceneGui += sceneView =>
            {
                if (!_checkedViews.Contains(sceneView) && EnsureOverlayVisibleFor(sceneView))
                    _checkedViews.Add(sceneView);
            };
        }

        private static void OnUpdate()
        {
            if (EnsureOverlayVisible())
                EditorApplication.update -= OnUpdate;
        }

        private static bool EnsureOverlayVisible()
        {
            var sv = SceneView.lastActiveSceneView;
            return sv != null && EnsureOverlayVisibleFor(sv);
        }

        private static bool EnsureOverlayVisibleFor(SceneView sv)
        {
            if (TryGetOverlay(sv, "UITemplateLibrary", out var existing))
            {
                // 已存在则确保常驻显示
                if (existing != null)
                {
                    existing.displayed = true;
                    existing.collapsed = false;
                }
                return true;
            }

            var canvasProp = sv.GetType().GetProperty("overlayCanvas", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (canvasProp == null)
                canvasProp = typeof(EditorWindow).GetProperty("overlayCanvas", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (canvasProp?.GetGetMethod()?.Invoke(sv, null) is not OverlayCanvas overlayCanvas)
                return false;

            var overlay = new UITemplateLibraryOverlay();
            overlayCanvas.Add(overlay);
            overlay.displayed = true;   // 二级模板常驻显示，无需点击一级按钮
            overlay.collapsed = false;  // 保持展开状态
            return true;
        }

        private static bool TryGetOverlay(EditorWindow window, string id, out Overlay result)
        {
            result = null;
            try
            {
                var method = window.GetType().GetMethod("TryGetOverlay",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                    null, new[] { typeof(string), typeof(Overlay).MakeByRefType() }, null);
                if (method == null)
                    return false;
                var args = new object[] { id, null };
                var found = (bool)method.Invoke(window, args);
                if (found)
                    result = (Overlay)args[1];
                return found;
            }
            catch
            {
                return false;
            }
        }
    }

    internal class TreeNode
    {
        public string Name;
        public bool IsFolder;
        public GameObject Prefab;
        public readonly List<TreeNode> Children = new List<TreeNode>();
    }

    internal class UITemplateLibraryPanelElement : VisualElement
    {
        private readonly ScrollView _scroll;
        private readonly TextField _searchField;
        private readonly VisualElement _body;
        private readonly Button _collapseBtn;
        private bool _collapsed;
        private readonly Dictionary<string, bool> _foldoutStates = new Dictionary<string, bool>();

        private const int MaxPanelHeight = 480;

        public UITemplateLibraryPanelElement()
        {
            style.minWidth = 240;
            style.maxWidth = 380;
            style.minHeight = 220;
            style.maxHeight = MaxPanelHeight;
            style.paddingTop = 6;
            style.paddingBottom = 6;
            style.paddingLeft = 8;
            style.paddingRight = 8;

            var headerRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 6 } };
            var header = new Label("UI 模板库");
            header.style.fontSize = 12;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.flexGrow = 1;
            headerRow.Add(header);
            _collapseBtn = new Button(ToggleCollapse) { text = "▼" };
            _collapseBtn.style.width = 24;
            _collapseBtn.style.height = 20;
            headerRow.Add(_collapseBtn);
            Add(headerRow);

            _body = new VisualElement();
            _body.style.flexGrow = 1;
            _body.style.minHeight = 0;  // 允许在 flex 布局中收缩，使 ScrollView 正确滚动
            Add(_body);

            var searchRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 6 } };
            _searchField = new TextField { style = { flexGrow = 1 } };
            _searchField.RegisterValueChangedCallback(_ => Refresh());
            var refreshBtn = new Button(() => Refresh()) { text = "刷新" };
            refreshBtn.style.width = 44;
            searchRow.Add(_searchField);
            searchRow.Add(refreshBtn);
            _body.Add(searchRow);

            _scroll = new ScrollView(ScrollViewMode.Vertical);
            _scroll.style.flexGrow = 1;
            _scroll.style.minHeight = 0;  // 配合 flex 布局，超出时显示滚动条
            _body.Add(_scroll);

            Refresh();
        }

        private void ToggleCollapse()
        {
            _collapsed = !_collapsed;
            if (_collapsed)
            {
                _body.style.display = DisplayStyle.None;
                style.minHeight = 28;
                _collapseBtn.text = "▶";
            }
            else
            {
                _body.style.display = DisplayStyle.Flex;
                style.minHeight = 220;
                _collapseBtn.text = "▼";
            }
        }

        public void Refresh()
        {
            var filter = (_searchField?.value ?? "").Trim();
            var root = BuildFileTree(UITemplateLibraryPanel.TemplateRootPath, filter);
            _scroll.Clear();
            if (root.Children.Count == 0)
            {
                _scroll.Add(new Label("未找到 UI 模板\n路径: " + UITemplateLibraryPanel.TemplateRootPath));
                return;
            }
            BuildTreeUI(root, _scroll, "", !string.IsNullOrEmpty(filter));
        }

        private void BuildTreeUI(TreeNode node, VisualElement parent, string pathKey, bool searchExpandAll = false)
        {
            var list = node.Children.OrderBy(c => c.IsFolder ? 0 : 1).ThenBy(c => c.Name, StringComparer.OrdinalIgnoreCase).ToList();
            foreach (var child in list)
            {
                var key = string.IsNullOrEmpty(pathKey) ? child.Name : pathKey + "/" + child.Name;
                if (child.IsFolder)
                {
                    var expanded = searchExpandAll || (_foldoutStates.TryGetValue(key, out var v) ? v : true);
                    if (!searchExpandAll)
                        _foldoutStates[key] = expanded;
                    var foldout = new Foldout { text = child.Name, value = expanded };
                    foldout.RegisterValueChangedCallback(evt => _foldoutStates[key] = evt.newValue);
                    foldout.style.marginTop = 1;
                    foldout.style.marginBottom = 1;
                    parent.Add(foldout);
                    BuildTreeUI(child, foldout, key, searchExpandAll);
                }
                else
                {
                    parent.Add(CreatePrefabItem(child.Prefab));
                }
            }
        }

        private static TreeNode BuildFileTree(string rootPath, string filter)
        {
            var root = new TreeNode { Name = "", IsFolder = true };
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { rootPath }).Distinct().ToArray();
            var baseLen = rootPath.TrimEnd('/').Length + 1;
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid).Replace("\\", "/");
                if (assetPath.Length <= baseLen) continue;
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab == null) continue;
                if (!string.IsNullOrEmpty(filter) && prefab.name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                var relPath = assetPath.Substring(baseLen);
                var parts = relPath.Split('/');
                var cur = root;
                for (var i = 0; i < parts.Length - 1; i++)
                {
                    var folderName = parts[i];
                    var next = cur.Children.Find(c => c.IsFolder && c.Name == folderName);
                    if (next == null)
                    {
                        next = new TreeNode { Name = folderName, IsFolder = true };
                        cur.Children.Add(next);
                    }
                    cur = next;
                }
                var existing = cur.Children.Find(c => !c.IsFolder && c.Prefab == prefab);
                if (existing == null)
                    cur.Children.Add(new TreeNode { Name = prefab.name, IsFolder = false, Prefab = prefab });
            }
            PruneEmptyFolders(root);
            return root;
        }

        private static void PruneEmptyFolders(TreeNode node)
        {
            foreach (var child in node.Children.Where(c => c.IsFolder).ToList())
                PruneEmptyFolders(child);
            node.Children.RemoveAll(c => c.IsFolder && c.Children.Count == 0);
        }

        private VisualElement CreatePrefabItem(GameObject prefab)
        {
            var row = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    paddingTop = 1, paddingBottom = 1, paddingLeft = 4,
                    marginTop = 1, marginBottom = 1, backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.5f),
                    borderTopLeftRadius = 2, borderBottomLeftRadius = 2, borderTopRightRadius = 2, borderBottomRightRadius = 2
                }
            };
            row.AddToClassList("prefab-item");
            row.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button != 0) return;
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new[] { prefab };
                DragAndDrop.paths = new[] { AssetDatabase.GetAssetPath(prefab) };
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                DragAndDrop.StartDrag(prefab.name);
                evt.StopPropagation();
            });
            row.RegisterCallback<ClickEvent>(evt =>
            {
                if (evt.button != 0) return;
                CreatePrefabInScene(prefab);
            });
            row.Add(new Label(prefab.name) { style = { fontSize = 14, flexGrow = 1 } });
            return row;
        }

        private static void CreatePrefabInScene(GameObject prefab)
        {
            if (prefab == null) return;
            var parent = FindOrCreateUIContainer();
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            if (instance == null) return;
            Undo.RegisterCreatedObjectUndo(instance, "Create UI Template " + prefab.name);
            var rt = instance.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = Vector2.zero;
                rt.localScale = Vector3.one;
            }
            Selection.activeGameObject = instance;
            EditorGUIUtility.PingObject(instance);
        }

        private static Transform FindOrCreateUIContainer()
        {
            var canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (canvas != null) return canvas.transform;
            var root = new GameObject("UI Root");
            var c = root.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            root.AddComponent<UnityEngine.UI.CanvasScaler>();
            root.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            if (UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
            Undo.RegisterCreatedObjectUndo(root, "Create UI Root");
            return root.transform;
        }
    }

    public static class UITemplateLibraryPanel
    {
        public const string TemplateRootPath = "Assets/Core/Asset/UITemplate";
    }

    public static class UITemplateLibraryMenu
    {
        [MenuItem("Window/UI 模板库 %#u")]
        public static void FocusSceneView()
        {
            var sv = SceneView.lastActiveSceneView;
            if (sv != null) sv.Focus();
        }
    }
}
