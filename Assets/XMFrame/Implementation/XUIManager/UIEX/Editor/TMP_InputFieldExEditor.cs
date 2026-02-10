using UnityEditor;

namespace XM.UIEX.Editor
{
    [CustomEditor(typeof(TMP_InputFieldEx))]
    [CanEditMultipleObjects]
    public class TMP_InputFieldExEditor : TMPro.EditorUtilities.TMP_InputFieldEditor
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
