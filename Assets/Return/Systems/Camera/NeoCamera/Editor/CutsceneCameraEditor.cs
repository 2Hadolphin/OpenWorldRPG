#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using NeoFPS;

namespace NeoFPSEditor
{
    //[CustomEditor(typeof(CutsceneCamera))]
    public class CutsceneCameraEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            var canSkip = serializedObject.FindProperty("m_CanSkip");
            EditorGUILayout.PropertyField(canSkip);

            if (canSkip.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SkipHold"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnSkip"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif