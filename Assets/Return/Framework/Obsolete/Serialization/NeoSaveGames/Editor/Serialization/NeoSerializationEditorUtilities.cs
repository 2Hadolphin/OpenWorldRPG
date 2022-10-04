#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.SceneManagement;

namespace NeoSaveGames.Serialization
{
    public static class NeoSerializationEditorUtilities
    {
        public static bool IsSceneValid(string sceneName)
        {
            if (Application.isPlaying)
                return Application.CanStreamedLevelBeLoaded(sceneName);
            else
            {
                var scenes = EditorBuildSettings.scenes;
                foreach (var s in scenes)
                {
                    if (s.path == sceneName || Path.GetFileNameWithoutExtension(s.path) == sceneName)
                        return true;
                }
                return false;
            }
        }

        public static bool IsSceneValid(int sceneIndex)
        {
            return (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings);
        }

        public static void LayoutSceneNameField(SerializedProperty property)
        {
            LayoutSceneNameField(property, new GUIContent(property.displayName, property.tooltip));
        }

        public static void LayoutSceneIndexField(SerializedProperty property)
        {
            LayoutSceneIndexField(property, new GUIContent(property.displayName, property.tooltip));
        }

        public static void LayoutSceneNameField(SerializedProperty property, string label)
        {
            LayoutSceneNameField(property, new GUIContent(label, property.tooltip));
        }

        public static void LayoutSceneIndexField(SerializedProperty property, string label)
        {
            LayoutSceneIndexField(property, new GUIContent(label, property.tooltip));
        }

        public static void LayoutSceneNameField(SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.PropertyField(property, label);

            bool isValid = IsSceneValid(property.stringValue);

            // Sort layout for small help box
            var helpRect = EditorGUILayout.BeginVertical(GUILayout.Height(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing));
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

            // Draw help box
            if (isValid)
                EditorGUI.HelpBox(helpRect, "Scene is valid", MessageType.Info);
            else
            {
                var color = GUI.color;
                GUI.color = Color.red;
                EditorGUI.HelpBox(helpRect, "Scene not in build settings", MessageType.Error);
                GUI.color = color;
            }
        }

        public static void LayoutSceneIndexField(SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.PropertyField(property, label);

            bool isValid = IsSceneValid(property.intValue);

            // Sort layout for small help box
            var helpRect = EditorGUILayout.BeginVertical(GUILayout.Height(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing));
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

            // Draw help box
            if (isValid)
                EditorGUI.HelpBox(helpRect, "Scene is valid", MessageType.Info);
            else
            {
                var color = GUI.color;
                GUI.color = Color.red;
                EditorGUI.HelpBox(helpRect, "Scene not in build settings", MessageType.Error);
                GUI.color = color;
            }
        }
    }
}

#endif
