using UnityEngine;
using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine.Rendering.Universal;


namespace Return.Cameras
{
    public class CinemachineFirstPersonCamera : FirstPersonCamera,IVirtualCamera
    {
        [SerializeField,Required]
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
                    FieldOfView= Camera.fieldOfView,
                    FarClipPlane= Camera.farClipPlane,
                    NearClipPlane = Camera.nearClipPlane,
                };


                //brainCam.fieldOfView = Camera.fieldOfView;
                //brainCam.farClipPlane = Camera.farClipPlane;


                //mainCamera.enabled = false;

                var stack = Camera.GetUniversalAdditionalCameraData()?.cameraStack;

                if (stack == null)
                    stack = StackOverlap;

                // set overlap
                var data = brainCam.GetUniversalAdditionalCameraData();

                foreach (var overlap in stack)
                    data.cameraStack.CheckAdd(overlap);
            }
            else
                Debug.LogError("Missing main preset camera");

        }

        #endregion

        public override void Activate()
        {
            base.Activate();

            Camera.gameObject.SetActive(false);
            
        }

        protected override void Awake()
        {
            Camera.enabled = false;

            base.Awake();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }


        protected override void OnDisable()
        {
            base.OnDisable();
        }

        protected override void SetHandles(bool enable)
        {
            //base.SetHandles(enable);

            virtualCamera.enabled = enable;

            if (Listener != null)
                Listener.enabled = enable;
        }

 

    }

}