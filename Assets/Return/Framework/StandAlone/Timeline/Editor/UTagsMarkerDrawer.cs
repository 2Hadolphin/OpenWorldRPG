using UnityEditor;

namespace Return.Editors
{
    [CustomEditor(typeof(DefinitionMarker))]
    public class UTagsMarkerDrawer : Editor
    {
        private SerializedProperty m_tagPicker;

        protected void OnEnable()
        {
            m_tagPicker = serializedObject.FindProperty(nameof(DefinitionMarker.EventID));
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var difinition = m_tagPicker.FindPropertyRelative("Definition");
            EditorGUILayout.ObjectField(difinition);
            //EditorGUILayout.EnumFlagsField(m_tagPicker.FindPropertyRelative("Tags"));
            difinition.serializedObject.ApplyModifiedProperties();
            EditorGUILayout.Space();

            if (difinition.objectReferenceValue is UniversalTagDefinition definition)
                UTagPickerDrawer.DrawTagField(m_tagPicker.FindPropertyRelative(nameof(UTags.Tag)), definition);
        }
    }
}