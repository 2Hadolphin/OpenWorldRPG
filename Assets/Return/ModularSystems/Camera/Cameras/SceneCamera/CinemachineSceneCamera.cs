using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Return.Cameras
{
    public class CinemachineSceneCamera : SceneCamera, IVirtualCamera
    {
        [SerializeField, Required]
        CinemachineVirtualCamera m_virtualCamera;
        public CinemachineVirtualCamera virtualCamera { get => m_virtualCamera; set => m_virtualCamera = value; }

        public override Camera mainCamera => cinemachineCam;
        protected Camera cinemachineCam;


        #region IVitrualCamera

        public void SetBrain(CinemachineBrain brain)
        {
            brain.m_UpdateMethod = CinemachineBrain.UpdateMethod.LateUpdate;
            CinemachineBrain.SoloCamera = virtualCamera;
        }

        public virtual void SetCamera(Camera brainCam)
        {
            cinemachineCam = brainCam;

            if (Camera)
            {
                brainCam.CopyFrom(Camera);
                brainCam.cullingMask = Camera.cullingMask;


                virtualCamera.m_Lens = new LensSettings()
                {
                    FieldOfView = Camera.fieldOfView,
                    FarClipPlane = Camera.farClipPlane,
                    NearClipPlane = Camera.nearClipPlane,
                };
            }
            else
                Debug.LogError("Missing main preset camera");
        }

        #endregion


        public override void Activate()
        {
            base.Activate();

            Camera.enabled = false;

            if (AudioListener != null)
                AudioListener.enabled = false;
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        // ignore base camera
        protected override void SetHandles(bool enable)
        {
            base.SetHandles(enabled);

            virtualCamera.enabled = enable;

        }
    }
}

