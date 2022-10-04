using UnityEngine;
using Sirenix.OdinInspector;
using System;
using UnityEngine.Rendering.Universal;
using System.Linq;
using System.Collections.Generic;

namespace Return.Cameras
{
    /// <summary>
    /// Abstract camera handler.
    /// </summary>
    [DefaultExecutionOrder(ExecuteOrderList.Camera)]
    public abstract class CustomCamera : BaseComponent, ICamWrapper
    {
        public static Action<CustomCamera, bool> OnStateChanged;

        [SerializeField]
        [Tooltip("????")]
        protected UTagPicker m_CameraType;
        public UTagPicker cameraType => m_CameraType;

        /// <summary>
        /// Main preset camera.
        /// </summary>
        [SerializeField,Required]
        protected Camera Camera;

        /// <summary>
        /// Active main camera.
        /// </summary>
        public virtual Camera mainCamera => Camera;


        [SerializeField]
        private Transform m_MainCameraTransform;

        /// <summary>
        /// Active main camera transform.
        /// </summary>
        public virtual Transform mainCameraTransform => m_MainCameraTransform;


        protected virtual void Awake()
        {
            if(m_MainCameraTransform.IsNull())
                m_MainCameraTransform = Camera.transform;

            //Debug.Log(this);
            SetCameraCoordinate();
            BaseCameraManager.RegisterCamera(this);

            enabled = false;
        }

        
        /// <summary>
        /// Set camera conponents.
        /// </summary>
        protected virtual void SetHandles(bool enable)
        {
            Debug.LogError($"Set camera handle {enable} : {this}.");

            if(Camera != null)
                Camera.enabled = enable;
        }

        protected virtual void SetCameraCoordinate()
        {
            ReadOnlyTransform = new WrapTransform(Camera.transform);
        }


        protected virtual void Start() { }


        protected HashSet<ICameraVolumeProvider> Effects=new(5);

        public virtual void InstallEffect(ICameraVolumeProvider volumeProvider)
        {
            if(Effects.Add(volumeProvider))
                volumeProvider.AddEffect(mainCamera);
        }

        public virtual void UnistallEffect(ICameraVolumeProvider volumeProvider)
        {
            if (Effects.Remove(volumeProvider))
                volumeProvider.RemoveEffect(mainCamera);
        }



        /// <summary>
        /// Set camera reference
        /// </summary>
        [Obsolete]
        protected virtual void LoadMainCamera(Camera cam=null)
        {
            if (cam.NotNull())
                Camera = cam;
            else
                InstanceIfNull(ref Camera);
        }

        public ReadOnlyTransform ReadOnlyTransform { get; protected set; }

        public abstract Transform HostObject { get; }

        public virtual Quaternion GetCamCorrectedRotation { get => ReadOnlyTransform.rotation; }

        public abstract void SetTarget(Transform tf);

        [SerializeField]
        UniversalAdditionalCameraData m_Data;

        public UniversalAdditionalCameraData GetData 
        {
            get
            {
                if (m_Data.IsNull())
                    m_Data = Camera.GetUniversalAdditionalCameraData();

                return m_Data;
            }

            set
            {
                m_Data = value;
            }
        }

        public virtual void SetStack(Camera camera)
        {
            var data = GetData;

            if (data.cameraStack.Contains(camera))
                return;

            data.cameraStack.Add(camera);


            //data.cameraStack.OrderBy();
        }

        protected virtual void OnEnable()
        {
            Activate();

            SetHandles(true);

            OnStateChanged?.Invoke(this, true);

            Debug.LogError("Activate Camera : " + this);
        }

        protected virtual void OnDisable()
        {
            Deactivate();

            SetHandles(false);

            if(!Routine.quitting)
                OnStateChanged?.Invoke(this, false);
        }

        /// <summary>
        /// Invoke while enable component.
        /// </summary>
        public virtual void Activate()
        {
            if (!this.CheckEnable())
                return;

            //CameraManager.SwitchCamera(this);
        }

        /// <summary>
        /// Invoke while disable component.
        /// </summary>
        public virtual void Deactivate()
        {
            if (this.CheckEnable(false))
                return;
        }
    }

    public abstract class UniversalCamera : CustomCamera
    {

    }
}