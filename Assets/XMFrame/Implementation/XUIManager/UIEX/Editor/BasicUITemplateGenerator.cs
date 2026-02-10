using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace XM.UIEX.Editor
{
    /// <summary>
    /// 生成 Basic UI 模板预制体，确保各组件具备完整子结构
    /// </summary>
    public static class BasicUITemplateGenerator
    {
        private const string BasicPath = "Assets/Core/Asset/UITemplate/Basic";

        [MenuItem("XM/UIEx/生成 Basic 模板预制体")]
        public static void GenerateAll()
        {
            GenerateBasicImage();
            GenerateBasicText();
            GenerateBasicButton();
            GenerateBasicInputField();
            GenerateBasicSlider();
            GenerateBasicToggle();
            GenerateBasicRawImage();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[BasicUITemplateGenerator] 所有 Basic 模板已生成完成");
        }

        private static void GenerateBasicImage()
        {
            var go = CreateUIExRoot<ImageEx>("Basic_Image", new Vector2(100, 100));
            SavePrefab(go, "Basic_Image");
        }

        private static void GenerateBasicText()
        {
            var go = CreateUIExRoot<TextEx>("Basic_Text", new Vector2(100, 30));
            var text = go.GetComponent<TextEx>();
            if (text != null) text.text = "Text";
            SavePrefab(go, "Basic_Text");
        }

        private static void GenerateBasicRawImage()
        {
            var go = CreateUIExRoot<RawImageEx>("Basic_RawImage", new Vector2(100, 100));
            SavePrefab(go, "Basic_RawImage");
        }

        private static void GenerateBasicButton()
        {
            var go = CreateUIExRoot<ButtonEx>("Basic_Button", new Vector2(160, 30));
            var btn = go.GetComponent<ButtonEx>();

            var bg = new GameObject("Background");
            bg.transform.SetParent(go.transform, false);
            var bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImage = bg.AddComponent<ImageEx>();
            bgImage.color = new Color(1, 1, 1, 1);

            btn.targetGraphic = bgImage;
            SavePrefab(go, "Basic_Button");
        }

        private static void GenerateBasicInputField()
        {
            var go = CreateUIExRoot<InputFieldEx>("Basic_InputField", new Vector2(160, 30));
            var input = go.GetComponent<InputFieldEx>();

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0.5f);
            textRect.anchorMax = new Vector2(1, 0.5f);
            textRect.offsetMin = new Vector2(10, -12);
            textRect.offsetMax = new Vector2(-10, 12);
            var text = textGo.AddComponent<TextEx>();
            text.fontSize = 14;
            text.color = Color.black;

            var placeholderGo = new GameObject("Placeholder");
            placeholderGo.transform.SetParent(go.transform, false);
            var phRect = placeholderGo.AddComponent<RectTransform>();
            phRect.anchorMin = new Vector2(0, 0.5f);
            phRect.anchorMax = new Vector2(1, 0.5f);
            phRect.offsetMin = new Vector2(10, -12);
            phRect.offsetMax = new Vector2(-10, 12);
            var placeholder = placeholderGo.AddComponent<TextEx>();
            placeholder.text = "Enter text...";
            placeholder.fontSize = 14;
            placeholder.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

            input.textComponent = text;
            input.placeholder = placeholder;
            SavePrefab(go, "Basic_InputField");
        }

        private static void GenerateBasicSlider()
        {
            var go = CreateUIExRoot<SliderEx>("Basic_Slider", new Vector2(160, 20));
            var slider = go.GetComponent<SliderEx>();

            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(go.transform, false);
            var bgRect = bgGo.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0.25f);
            bgRect.anchorMax = new Vector2(1, 0.75f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImage = bgGo.AddComponent<ImageEx>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1);

            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(go.transform, false);
            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1, 0.75f);
            fillAreaRect.offsetMin = new Vector2(5, 0);
            fillAreaRect.offsetMax = new Vector2(-5, 0);

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(fillArea.transform, false);
            var fillRect = fillGo.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImage = fillGo.AddComponent<ImageEx>();
            fillImage.color = new Color(0.2f, 0.6f, 1, 1);

            var handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(go.transform, false);
            var handleAreaRect = handleArea.AddComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(10, 0);
            handleAreaRect.offsetMax = new Vector2(-10, 0);

            var handleGo = new GameObject("Handle");
            handleGo.transform.SetParent(handleArea.transform, false);
            var handleRect = handleGo.AddComponent<RectTransform>();
            handleRect.anchorMin = new Vector2(0, 0.5f);
            handleRect.anchorMax = new Vector2(0, 0.5f);
            handleRect.offsetMin = new Vector2(-10, -10);
            handleRect.offsetMax = new Vector2(10, 10);
            var handleImage = handleGo.AddComponent<ImageEx>();
            handleImage.color = Color.white;

            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            SavePrefab(go, "Basic_Slider");
        }

        private static void GenerateBasicToggle()
        {
            var go = CreateUIExRoot<ToggleEx>("Basic_Toggle", new Vector2(160, 30));
            var toggle = go.GetComponent<ToggleEx>();

            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(go.transform, false);
            var bgRect = bgGo.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0.5f);
            bgRect.anchorMax = new Vector2(0, 0.5f);
            bgRect.anchoredPosition = new Vector2(10, 0);
            bgRect.sizeDelta = new Vector2(20, 20);
            var bgImage = bgGo.AddComponent<ImageEx>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1);

            var checkGo = new GameObject("Checkmark");
            checkGo.transform.SetParent(bgGo.transform, false);
            var checkRect = checkGo.AddComponent<RectTransform>();
            checkRect.anchorMin = Vector2.zero;
            checkRect.anchorMax = Vector2.one;
            checkRect.offsetMin = Vector2.zero;
            checkRect.offsetMax = Vector2.zero;
            var checkImage = checkGo.AddComponent<ImageEx>();
            checkImage.color = Color.white;

            toggle.targetGraphic = bgImage;
            toggle.graphic = checkImage;
            SavePrefab(go, "Basic_Toggle");
        }

        private static GameObject CreateUIExRoot<T>(string name, Vector2 size) where T : Component
        {
            var go = new GameObject(name);
            var t = go.AddComponent<T>();
            var rect = t.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = size;
            }
            return go;
        }

        private static void SavePrefab(GameObject go, string prefabName)
        {
            EnsureFolder(BasicPath);
            var path = $"{BasicPath}/{prefabName}.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            if (prefab != null)
                Debug.Log($"[BasicUITemplateGenerator] 已生成: {path}");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var normalized = path.Replace('\\', '/');
            var idx = normalized.LastIndexOf('/');
            if (idx <= 0) return;
            var parent = normalized.Substring(0, idx);
            var name = normalized.Substring(idx + 1);
            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
