using UnityEditor;

namespace XM.UIEX.Editor
{
    [CustomEditor(typeof(ButtonEx))]
    [CanEditMultipleObjects]
    public class ButtonExEditor : UnityEditor.UI.ButtonEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            UIExEditorHelper.DrawBindFields(serializedObject);
            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}
