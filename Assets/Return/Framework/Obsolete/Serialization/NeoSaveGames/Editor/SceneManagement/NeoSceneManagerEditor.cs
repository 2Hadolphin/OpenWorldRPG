#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeoSaveGames.SceneManagement
{
    [CustomEditor(typeof(NeoSceneManager))]
    public class NeoSceneManagerEditor : Editor
    {
        private const int k_SceneOK = 0;
        private const int k_SceneNotSet = 1;
        private const int k_SceneNotBuilt = 2;

        public bool CheckIsValid()
        {
            var scene = serializedObject.FindProperty("m_DefaultLoadingScreen");
            return (CheckScenePath(scene.stringValue) == 0);                
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.HelpBox("Loading screens are scenes which are loaded before loading the target scene asynchronously, and unloaded once the target scene loading is complete.\n\nThe default loading screen is stored by scene build index. If you modify the build settings, make sure to check the default loading screen below is still correct.", MessageType.Info);
            EditorGUILayout.Space();

            var indexProperty = serializedObject.FindProperty("m_DefaultLoadingScreenIndex");
            var buildScenes = EditorBuildSettings.scenes;

            SceneAsset sceneObject = null;

            if (indexProperty.intValue > -1)
            {
                if (indexProperty.intValue >= buildScenes.Length)
                {
                    indexProperty.intValue = -1;
                    Debug.LogError("Default loading scene index was out of bounds for build settings, setting to -1.");
                }
                else
                    sceneObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(buildScenes[indexProperty.intValue].path);
            }

            // Show the scene field
            var newSceneObj = EditorGUILayout.ObjectField("Default Loading Screen", sceneObject, typeof(SceneAsset), false);

            // Get path from new scene
            if (newSceneObj != sceneObject)
            {
                if (newSceneObj == null)
                    indexProperty.intValue = -1;
                else
                {
                    bool found = false;
                    var path = AssetDatabase.GetAssetPath(newSceneObj);
                    for (int i = 0; i < buildScenes.Length; ++i)
                    {
                        if (buildScenes[i].path == path)
                        {
                            indexProperty.intValue = i;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                        Debug.LogError("Loading scene must be added to build settings");
                }
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MinLoadScreenTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnSceneLoaded"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnSceneLoadFailed"));

            serializedObject.ApplyModifiedProperties();
        }

        int CheckScenePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return 1;

            var scenes = EditorBuildSettings.scenes;
            foreach (var s in scenes)
            {
                if (s.path == path)
                    return 0;
            }

            return 2;
        }
    }
}

#endif
