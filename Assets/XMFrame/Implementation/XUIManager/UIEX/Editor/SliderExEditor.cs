using UnityEditor;

namespace XM.UIEX.Editor
{
    [CustomEditor(typeof(SliderEx))]
    [CanEditMultipleObjects]
    public class SliderExEditor : UnityEditor.UI.SliderEditor
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
