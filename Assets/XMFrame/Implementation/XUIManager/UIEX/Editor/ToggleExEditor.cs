using UnityEditor;

namespace XM.UIEX.Editor
{
    [CustomEditor(typeof(ToggleEx))]
    [CanEditMultipleObjects]
    public class ToggleExEditor : UnityEditor.UI.ToggleEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            XM.UIEX.Editor.UIExEditorHelper.DrawBindFields(serializedObject);
            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}
