using UnityEditor;

namespace XM.UIEX.Editor
{
    [CustomEditor(typeof(TextEx))]
    [CanEditMultipleObjects]
    public class TextExEditor : UnityEditor.UI.TextEditor
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
