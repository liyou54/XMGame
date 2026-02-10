using UnityEditor;

namespace XM.UIEX.Editor
{
    [CustomEditor(typeof(RawImageEx))]
    [CanEditMultipleObjects]
    public class RawImageExEditor : UnityEditor.UI.RawImageEditor
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
