using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Return.Cameras
{
    public class SceneCamera : CustomCamera
    {
        public override Transform HostObject => null;

        [SerializeField]
        AudioListener m_audioListener;

        public AudioListener AudioListener { get => m_audioListener; set => m_audioListener = value; }



        public override void SetTarget(Transform tf)
        {
            
        }

        protected override void Awake()
        {
            base.Awake();

            bool useSceneCam = CameraManager.mainCameraHandler == this;

            SetHandles(useSceneCam);
        }

        protected override void SetHandles(bool enable)
        {
            base.SetHandles(enable);

            if (AudioListener)
                AudioListener.enabled = enable;
        }

        void Reset()
        {
            AudioListener = GetComponent<AudioListener>();
            Camera = GetComponent<Camera>();
        }

    }
}

