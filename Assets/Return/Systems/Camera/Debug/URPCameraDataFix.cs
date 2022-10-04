#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine.Rendering.Universal;
using Sirenix.OdinInspector;

namespace Return.Cameras
{
    class URPCameraDataFix : ScriptableObject
    {




        protected virtual void Awake()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }


        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log("OnSceneLoaded: " + scene.name);
            UpdateSceneCam();
        }

        [Button]
        public static void UpdateSceneCam()
        {
            var editorCam = SceneView.lastActiveSceneView.camera;

            Debug.Log($"Editor camera {editorCam}");

            if (!editorCam)
                return;

            if (!editorCam.TryGetComponent<UniversalAdditionalCameraData>(out var data))
            {
                Debug.LogError($"Missing URP camera data, create new one.");
                data = editorCam.gameObject.AddComponent<UniversalAdditionalCameraData>();
                editorCam.UpdateVolumeStack(data);
            }
            else
                Debug.Log(data);

        }
    }
}

#endif