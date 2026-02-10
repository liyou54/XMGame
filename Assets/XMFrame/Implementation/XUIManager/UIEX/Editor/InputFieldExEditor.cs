using UnityEditor;

namespace XM.UIEX.Editor
{
    [CustomEditor(typeof(InputFieldEx))]
    [CanEditMultipleObjects]
    public class InputFieldExEditor : UnityEditor.UI.InputFieldEditor
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
