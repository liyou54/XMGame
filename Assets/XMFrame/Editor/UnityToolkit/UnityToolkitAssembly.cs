#if UNITY_EDITOR
using UnityEditor;

namespace UnityToolkit
{
    /// <summary>
    /// UnityToolkit 程序集占位。本程序集允许引用 XModToolkit 与 UnityEditor，用于在 Editor 中桥接 Toolkit 与 Unity。
    /// </summary>
    public static class UnityToolkitAssembly
    {
        [MenuItem("UnityToolkit/关于")]
        private static void About()
        {
            EditorUtility.DisplayDialog("UnityToolkit", "可在此程序集中使用 XModToolkit 与 UnityEditor。", "确定");
        }
    }
}
#endif
