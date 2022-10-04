using UnityEngine;
using Cinemachine;
using Sirenix.OdinInspector;


namespace Return.Cameras
{
    /// <summary>
    /// Cinemachine brain wrapper
    /// </summary>
    public class VirtualCamera : BaseComponent
    {
        [SerializeField]
        CinemachineBrain m_Brain;
        public CinemachineBrain Brain { get => m_Brain; set => m_Brain = value; }

        [SerializeField]
        Camera m_Camera;
        public Camera Camera { get => m_Camera; set => m_Camera = value; }

        [SerializeField]
        AudioListener m_AudioListener;
        public AudioListener AudioListener { get => m_AudioListener; set => m_AudioListener = value; }


        protected virtual void Awake()
        {
            Init();
        }

        [Button]
        protected virtual void Init()
        {
            InstanceIfNull(ref m_Brain);
            InstanceIfNull(ref m_Camera);
            InstanceIfNull(ref m_AudioListener);
        }

        protected virtual void OnEnable()
        {
            Brain.enabled = true;
            Camera.enabled = true;
            AudioListener.enabled = true;

        }

        protected virtual void OnDisable()
        {
            Brain.enabled = false;
            Camera.enabled = false;
            AudioListener.enabled = false;
        }




    }
}

