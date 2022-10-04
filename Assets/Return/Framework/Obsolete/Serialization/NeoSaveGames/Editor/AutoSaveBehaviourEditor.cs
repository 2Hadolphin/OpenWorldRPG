#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NeoSaveGames
{
    [CustomEditor(typeof(AutoSaveBehaviour), true)]
    public class AutoSaveBehaviourEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var prop = serializedObject.FindProperty("m_OneShot");

            EditorGUILayout.PropertyField(prop);

            if (!prop.boolValue)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Cooldown"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RetryAttempts"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif