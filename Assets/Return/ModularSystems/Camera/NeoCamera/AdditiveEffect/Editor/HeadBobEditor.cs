#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace NeoFPS
{
    //[CustomEditor(typeof(HeadBob), true)]
    public class HeadBobEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_HorizontalBobRange"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_VerticalBobRange"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BobCurve"));

            // Bob interval
            var keyProp = serializedObject.FindProperty("m_BobIntervalParamKey");
            EditorGUILayout.PropertyField(keyProp);
            if (string.IsNullOrEmpty(keyProp.stringValue))
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BobInterval"));
            else
            {
                ++EditorGUI.indentLevel;
                var intervalProp = serializedObject.FindProperty("m_BobInterval");
                EditorGUILayout.PropertyField(intervalProp, new GUIContent("Fallback Bob Interval", intervalProp.tooltip));
                --EditorGUI.indentLevel;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif