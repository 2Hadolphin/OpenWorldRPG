using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace Return.Editors
{
    [CustomEditor(typeof(UniversalMarker))]
    public class TimelineMarkerDrawer : Editor
    {
        private SerializedProperty m_tagPicker;

        protected void OnEnable()
        {
            m_tagPicker = serializedObject.FindProperty(nameof(UniversalMarker.EventID));
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
                UTagPickerDrawer.DrawTagField(m_tagPicker.FindPropertyRelative(nameof(UTagPicker.Tag)), definition);
        }
    }
}