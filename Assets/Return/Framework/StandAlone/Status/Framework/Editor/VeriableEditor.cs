using MalbersAnimations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Return.Framework.Stats
{
    public class VariableEditor : Editor
    {
        public static GUIStyle StyleBlue => MTools.Style(new Color(0, 0.5f, 1f, 0.3f));

        protected SerializedProperty value, Description, debug;

        private void OnEnable()
        {
            value = serializedObject.FindProperty("value");
            Description = serializedObject.FindProperty("Description");
            debug = serializedObject.FindProperty("debug");
        }

        public virtual void PaintInspectorGUI(string title)
        {
            serializedObject.Update();
            MalbersEditor.DrawDescription(title);


            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(value, new GUIContent("Value", "The current value"));
                    MalbersEditor.DrawDebugIcon(debug);
                }
                EditorGUILayout.PropertyField(Description);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}