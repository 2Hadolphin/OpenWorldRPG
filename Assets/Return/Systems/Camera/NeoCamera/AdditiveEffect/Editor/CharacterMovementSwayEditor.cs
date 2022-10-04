#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS;

namespace NeoFPSEditor
{
    [CustomEditor(typeof (CharacterMovementSway), true)]
    public class CharacterMovementSwayEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            GUI.enabled = false;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            GUI.enabled = true;

            DrawSwayProperty(serializedObject.FindProperty("m_ForwardsSway"), "Forwards");
            DrawSwayProperty(serializedObject.FindProperty("m_ReverseSway"), "Reverse");
            DrawSwayProperty(serializedObject.FindProperty("m_LeftSway"), "Left");
            DrawSwayProperty(serializedObject.FindProperty("m_RightSway"), "Right");

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DampingTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AimingMultiplier"));

            serializedObject.ApplyModifiedProperties();
        }

        static void DrawSwayProperty(SerializedProperty prop, string label)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            //++EditorGUI.indentLevel;
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("offset"));
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("roll"));
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("speed"));
            //--EditorGUI.indentLevel;

            EditorGUILayout.EndVertical();
        }
    }
}

#endif