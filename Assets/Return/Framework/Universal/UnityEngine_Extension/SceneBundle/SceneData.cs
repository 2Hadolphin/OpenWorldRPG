using UnityEngine;
using Sirenix.OdinInspector;
using System;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Return.Editors;
using UnityEngine.Assertions;
using System.Linq;

namespace Return.Scenes
{
    [HideLabel]
    [Serializable]
    public struct SceneData
    {
        [SerializeField]
        string m_sceneID;
        public string SceneID { get => m_sceneID; set => m_sceneID = value; }


#if UNITY_EDITOR
        [OnValueChanged(nameof(EditorPushSceneID))]
        [Required]
        [SerializeField]
        SceneAsset m_Scene;
        public SceneAsset EditorSceneAsset { get => m_Scene; set => m_Scene = value; }

        void EditorPushSceneID()
        {
            if(EditorSceneAsset)
                SceneID = EditorSceneAsset.name;
        }

        /// <summary>
        /// Check scene enable in build project.
        /// </summary>
        void ValidScene()
        {
            var scenes=EditorBuildSettings.scenes;
            var path = AssetDatabase.GetAssetPath(EditorSceneAsset);
            try
            {
                Assert.IsTrue(scenes.Any(x => x.path == path));
            }
            catch (Exception e)
            {
                Debug.LogException(e,EditorSceneAsset);
                EditorBuildSettings.scenes = EditorBuildSettings.scenes.Append(new EditorBuildSettingsScene(path, true)).ToArray();
                
            }
        }

        [Button]
#endif
        public AsyncOperation LoadSceneAsync()
        {
            ValidScene();


#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
                return SceneManager.LoadSceneAsync(SceneID,LoadSceneMode.Single);
            else
                return EditorSceneManager.LoadSceneAsync(SceneID);
#else


#endif
        }


        [Button]
        public void LoadScene()
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
                SceneManager.LoadScene(SceneID);
            else
                EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(EditorSceneAsset), OpenSceneMode.Single);
#else


#endif
        }
    }
}