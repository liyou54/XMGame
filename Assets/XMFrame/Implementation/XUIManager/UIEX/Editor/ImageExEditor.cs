using UnityEditor;

namespace XM.UIEX.Editor
{
    [CustomEditor(typeof(ImageEx))]
    [CanEditMultipleObjects]
    public class ImageExEditor : UnityEditor.UI.ImageEditor
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
