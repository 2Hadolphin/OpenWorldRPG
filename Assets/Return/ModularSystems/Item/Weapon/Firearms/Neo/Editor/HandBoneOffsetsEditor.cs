#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using NeoFPS;


namespace NeoFPSEditor
{
    [CustomEditor(typeof(HandBoneOffsets))]
    public class HandBoneOffsetsEditor : Editor
    {
        GUIContent[] m_FingerLabels = null;
        GUIStyle m_FoldoutStyle = null;

        private static Vector3 m_ClipBoard = Vector3.zero;

        public override void OnInspectorGUI()
        {
            InspectOffsets();
        }

        public void InspectOffsets()
        {
            serializedObject.UpdateIfRequiredOrScript();

            InitialiseLabels();

            var fingers = serializedObject.FindProperty("m_FingerOffsets");

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                ++EditorGUI.indentLevel;

                var expand = serializedObject.FindProperty("expandLeftHand");
                var rect = EditorGUILayout.GetControlRect();
                expand.boolValue = EditorGUI.Foldout(rect, expand.boolValue, "Left Hand Offsets", true, m_FoldoutStyle);

                --EditorGUI.indentLevel;

                if (expand.boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_LeftHandPositionOffset"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_LeftHandRotationOffset"));

                    var prop = serializedObject.FindProperty("m_OffsetLeftFingers");
                    EditorGUILayout.PropertyField(prop);
                    if (prop.boolValue)
                    {
                        for (int i = 0; i < 15; ++i)
                            InspectFingerOffset(fingers.GetArrayElementAtIndex(i), m_FingerLabels[i]);
                    }
                }

            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                ++EditorGUI.indentLevel;

                var expand = serializedObject.FindProperty("expandRightHand");
                expand.boolValue = EditorGUILayout.Foldout(expand.boolValue, "Right Hand Offsets", true, m_FoldoutStyle);

                --EditorGUI.indentLevel;

                if (expand.boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RightHandPositionOffset"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RightHandRotationOffset"));

                    var prop = serializedObject.FindProperty("m_OffsetRightFingers");
                    EditorGUILayout.PropertyField(prop);
                    if (prop.boolValue)
                    {
                        for (int i = 0; i < 15; ++i)
                            InspectFingerOffset(fingers.GetArrayElementAtIndex(i + 15), m_FingerLabels[i]);
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        void InspectFingerOffset(SerializedProperty prop, GUIContent label)
        {
            var rect = EditorGUILayout.GetControlRect();
            rect.width -= 2f * EditorGUIUtility.singleLineHeight + 4f;

            // Property field
            EditorGUI.PropertyField(rect, prop, label);

            rect.x += rect.width + 2f;
            rect.width = EditorGUIUtility.singleLineHeight;

            // Copy button
            if (GUI.Button(rect, EditorGUIUtility.FindTexture("SceneLoadIn"), EditorStyles.label))
            {
                m_ClipBoard = prop.vector3Value;
            }

            rect.x += EditorGUIUtility.singleLineHeight + 2f;

            // Paste button
            if (GUI.Button(rect, EditorGUIUtility.FindTexture("SceneLoadOut"), EditorStyles.label))
            {
                prop.vector3Value = m_ClipBoard;
            }
        }

        void InitialiseLabels()
        {
            if (m_FoldoutStyle == null)
            {
                m_FoldoutStyle = new GUIStyle(EditorStyles.foldout);
                m_FoldoutStyle.fontStyle = FontStyle.Bold;
            }

            if (m_FingerLabels == null)
            {
                m_FingerLabels = new GUIContent[] {
                    new GUIContent("Thumb 1 (Proximal)"),
                    new GUIContent("Thumb 2 (Intermediate)"),
                    new GUIContent("Thumb 3 (Distal)"),
                    new GUIContent("Index 1 (Proximal)"),
                    new GUIContent("Index 2 (Intermediate)"),
                    new GUIContent("Index 3 (Distal)"),
                    new GUIContent("Middle 1 (Proximal)"),
                    new GUIContent("Middle 2 (Intermediate)"),
                    new GUIContent("Middle 3 (Distal)"),
                    new GUIContent("Ring 1 (Proximal)"),
                    new GUIContent("Ring 2 (Intermediate)"),
                    new GUIContent("Ring 3 (Distal)"),
                    new GUIContent("Pinky 1 (Proximal)"),
                    new GUIContent("Pinky 2 (Intermediate)"),
                    new GUIContent("Pinky 3 (Distal)")
                };
            }
        }
    }
}

#endif