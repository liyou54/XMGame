using UnityEditor;

namespace XM.UIEX.Editor
{
    [CustomEditor(typeof(TMP_DropdownEx))]
    [CanEditMultipleObjects]
    public class TMP_DropdownExEditor : TMPro.EditorUtilities.DropdownEditor
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
