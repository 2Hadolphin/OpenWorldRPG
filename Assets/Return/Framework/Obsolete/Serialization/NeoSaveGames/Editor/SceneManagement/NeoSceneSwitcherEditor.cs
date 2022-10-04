#if UNITY_EDITOR

using UnityEditor;
using NeoSaveGames.Serialization;

namespace NeoSaveGames.SceneManagement
{
    [CustomEditor(typeof(NeoSceneSwitcher), true)]
    public class NeoSceneSwitcherEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var sceneModeProp = serializedObject.FindProperty("m_Mode");
            var loadingModeProp = serializedObject.FindProperty("m_LoadingSceneMode");
            
            EditorGUILayout.PropertyField(sceneModeProp);
            switch(sceneModeProp.enumValueIndex)
            {
                case 0: // Name
                    NeoSerializationEditorUtilities.LayoutSceneNameField(serializedObject.FindProperty("m_SceneName"), "Target Scene");
                    break;
                case 1: // Index
                    NeoSerializationEditorUtilities.LayoutSceneIndexField(serializedObject.FindProperty("m_SceneIndex"), "Target Scene");
                    break;
            }

            EditorGUILayout.PropertyField(loadingModeProp);
            switch (loadingModeProp.enumValueIndex)
            {
                case 1: // Name
                    NeoSerializationEditorUtilities.LayoutSceneNameField(serializedObject.FindProperty("m_LoadingSceneName"), "Loading Scene");
                    break;
                case 2: // Index
                    NeoSerializationEditorUtilities.LayoutSceneIndexField(serializedObject.FindProperty("m_LoadingSceneIndex"), "Loading Scene");
                    break;
            }

            // Inspect subclass properties
            OnChildInspectorGUI();

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void OnChildInspectorGUI()
        {
            // Grab the last serialized property in base
            var itr = serializedObject.FindProperty("m_LoadingSceneIndex");

            // Iterate through visible properties from here
            while (itr.NextVisible(true))
                EditorGUILayout.PropertyField(itr);
        }
    }
}

#endif
