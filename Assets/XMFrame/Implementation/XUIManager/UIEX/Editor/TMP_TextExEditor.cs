using UnityEditor;

namespace XM.UIEX.Editor
{
    [CustomEditor(typeof(TMP_TextEx))]
    [CanEditMultipleObjects]
    public class TMP_TextExEditor : TMPro.EditorUtilities.TMP_EditorPanelUI
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
