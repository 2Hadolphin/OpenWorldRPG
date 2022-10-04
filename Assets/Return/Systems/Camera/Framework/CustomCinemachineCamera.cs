using UnityEngine;
using Sirenix.OdinInspector;
using Cinemachine;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine.Events;


namespace Return.Cameras
{
    /// <summary>
    /// ???????
    /// </summary>
    [System.Obsolete]
    [DefaultExecutionOrder(ExecuteOrderList.ModuleSystem)]
    [RequireComponent(typeof(CinemachineBrain))]
    public partial class CustomCinemachineCamera : CustomCamera
    {
        [SerializeField]
        public CinemachineVirtualCameraBase CamPrefab;


        protected override void Awake()
        {
            base.Awake();
            gameObject.InstanceIfNull(ref Director);
            //Cursor.lockState = CursorLockMode.Locked;

        }

        protected override void Start()
        {
            Input = InputManager.Input;
            Director.m_UpdateMethod = CinemachineBrain.UpdateMethod.LateUpdate;

            var cam = Director.ActiveVirtualCamera;

            if (cam == null)
            {
                if (CamPrefab != null)
                {
                    cam = Instantiate(CamPrefab);
                    CinemachineBrain.SoloCamera = cam;
                }
                else
                    Director.m_CameraActivatedEvent.AddEvent(OnCamActivate);
            }

            OnCamActivate(cam, null);
        }

        protected virtual void OnCamActivate(ICinemachineCamera cam,ICinemachineCamera cam1)
        {
            Debug.Log(cam);

            Assert.IsNotNull(cam);

            Director.ActiveVirtualCamera.VirtualCameraGameObject.InstanceIfNull(ref mInput);
            Input.FreeCam.ViewPort.performed += mInput.MouseMove_performed;
            Input.Humanoid.Room.performed += mInput.Room_performed;
            mInput.Setting = Setting;

            Director.m_CameraActivatedEvent.RemoveListener(OnCamActivate);
        }

        public override void SetTarget(Transform tf)
        {
            Assert.IsNotNull(tf);

            Director.ActiveVirtualCamera.LookAt = tf;
            Director.ActiveVirtualCamera.Follow = tf;
        }

        VirtualCamInputModule mInput;

        [SerializeField]
        public CameraSetting Setting;



        mPlayerInput Input;

        protected CinemachineBrain Director;
        protected CamWrapper Wrapper;

        public override Quaternion GetCamCorrectedRotation => ICam.State.CorrectedOrientation;


        public ICinemachineCamera ICam => Director.ActiveVirtualCamera;
        public override Transform HostObject => Director.ActiveVirtualCamera.Follow;
    }


    public class CamWrapper
    {
        public CamWrapper(ICinemachineCamera cam)
        {
            Cam = cam;
        }
        public ICinemachineCamera Cam { get; protected set; }

        public Quaternion GetRotation
        {
            get
            {
                return Cam.State.RawOrientation;
            }
        }
    }
}