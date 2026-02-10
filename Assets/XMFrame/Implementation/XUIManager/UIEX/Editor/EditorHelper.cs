using UnityEditor;
using UnityEngine;

namespace XM.UIEX.Editor
{ 
    /// <summary>
    /// UIEx 编辑器辅助类，用于绘制 Bind/BindName 自定义字段
    /// </summary>
    public static class UIExEditorHelper 
    {
        private static readonly string[] ExcludeProperties = { "m_Script", "_bind", "_bindName" };

        /// <summary>
        /// 绘制 UIEx 绑定字段区域
        /// </summary>
        public static void DrawBindFields(SerializedObject serializedObject)
        {
            var bindProperty = serializedObject.FindProperty("_bind");
            var bindNameProperty = serializedObject.FindProperty("_bindName");

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("UIEx 绑定", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            if (bindProperty != null)
            {
                EditorGUILayout.PropertyField(bindProperty, new GUIContent("Bind", "是否参与 UI 绑定"));
            }

            if (bindNameProperty != null)
            {
                EditorGUILayout.PropertyField(bindNameProperty, new GUIContent("Bind Name", "绑定名称，用于代码查找"));
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space(8);
        }

        /// <summary>
        /// 获取需排除绘制的属性列表（用于 DrawPropertiesExcluding）
        /// </summary>
        public static string[] GetExcludeProperties()
        {
            return ExcludeProperties;
        }
    }
}
