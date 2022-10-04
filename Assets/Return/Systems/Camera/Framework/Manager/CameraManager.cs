using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine.Assertions;


namespace Return.Cameras
{

    public class CameraManager : BaseCameraManager //IInitializable
    {
        public delegate void CameraStateChange(CustomCamera cam, UTag type, bool enable);

        /// <summary>
        /// 
        /// </summary>
        public static event Action<UTag,bool> OnCamerasChange;

        public static event CameraStateChange OnCameraStateChange;
        //public static event Action<CustomCamera,UTag, bool> OnCameraStateChange;

        /// <summary>
        /// Subscribe camera change event and push initialize function.
        /// </summary>
        /// <typeparam name="T">Camera type to init callback.</typeparam>
        /// <param name="callback">Function to recive camera change post.</param>
        /// <param name="initTargetOnly">Whether init callback with target camera only.</param>
        public static CustomCamera SubscribeCamera<T>(CameraStateChange callback,bool initTargetOnly=false) where T:CustomCamera
        {
            OnCameraStateChange -= callback;
            OnCameraStateChange += callback;

            CustomCamera cam;

            if (typeof(T) == typeof(CustomCamera))
                cam = ps_mainCameraHandler;
            else
                cam = GetCamera<T>();

            if (cam == null)
            {
                if (initTargetOnly)
                    return cam;

                cam = ps_mainCameraHandler;
            }

            callback(cam, cam.cameraType, cam.enabled);

            return cam;
        }


        public static UTag ActivateCameraTypes { get; protected set; }

        protected static CustomCamera ps_mainCameraHandler;

        public static event Action<CustomCamera> OnMainCameraHandlerChanged;

        /// <summary>
        /// Current camera handler. **FirstPerson **ThirdPerson **Vehicle **Free *Scene **Timeline
        /// </summary>
        public static CustomCamera mainCameraHandler
        {
            get => ps_mainCameraHandler;
            set
            {
                if (ps_mainCameraHandler == value)
                    return;

                ps_mainCameraHandler = value;
                OnMainCameraHandlerChanged?.Invoke(value);
            }
        }

        static HashSet<ICameraVolumeProvider> cacheEffects=new(10);

        public static void AddVolume(ICameraVolumeProvider volumeProvider)
        {
            if (cacheEffects.Add(volumeProvider))
                mainCameraHandler.InstallEffect(volumeProvider);
        }

        public static void RemoveVolume(ICameraVolumeProvider volumeProvider)
        {
            if (Routine.quitting)
                return;

            if (!cacheEffects.Remove(volumeProvider))
                return;

            foreach (var cam in Cameras)
            {
                try
                {
                    cam.UnistallEffect(volumeProvider);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }


        public override void Initialize()
        {
            base.Initialize();

            Debug.Log("Initialize camera manager.");
            //SwitchCamera(GetCamera<FreeCamera>());

            if (virtualCamera.IsNull())
            {
                virtualCamera =
                    m_CinemachineCameraPreset != null ?
                    Instantiate(m_CinemachineCameraPreset) :
                    new GameObject(nameof(VirtualCamera)).AddComponent<VirtualCamera>();

                DontDestroyOnLoad(virtualCamera.gameObject);
                //m_virtualCamera.gameObject.hideFlags = HideFlags.DontSave;
            }
        }

        /// <summary>
        /// Set camera as target instance.
        /// </summary>
        public static void SwitchCamera(CustomCamera activeCamera)
        {
            Assert.IsFalse(activeCamera == null);

            if (mainCameraHandler == activeCamera)
            {
                Debug.LogWarning(new InvalidOperationException($"Repeat set camera activate {activeCamera}."));
                return;
            }

            foreach (var cam in Cameras)
            {
                try
                {
                    if (activeCamera != cam)
                        cam.enabled = false;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            mainCameraHandler = activeCamera;

            if (activeCamera)
                activeCamera.enabled = true;
        }


        protected override void SetCamera(CustomCamera camera, bool enable)
        {


            Debug.Log($"Set camera {camera} : {enable}.");
            if (enable)
            {
                if (mainCameraHandler != camera)
                    SwitchCamera(camera);

                UTag type = camera.cameraType;
                OnCameraStateChange?.Invoke(camera, type, true);

                ActivateCameraTypes |= type;
                OnCamerasChange?.Invoke(ActivateCameraTypes, true);

                if (camera is IVirtualCamera virtualHandler)
                {
                    virtualHandler.SetCamera(virtualCamera.Camera);
                    virtualHandler.SetBrain(virtualCamera.Brain);

                    virtualCamera.enabled = true;
                }
                else
                {
                    if(virtualCamera)
                        virtualCamera.enabled = false;
                }
            }
            else
            {
                UTag type = camera.cameraType;
                OnCameraStateChange?.Invoke(camera, type, false);

                ActivateCameraTypes ^= type;
                OnCamerasChange?.Invoke(ActivateCameraTypes, false);


                if (camera is IVirtualCamera)
                    virtualCamera.enabled = false;
            }
        }


        protected override void Register(CustomCamera camera)
        {
            base.Register(camera);

            SetCamera(camera,true);
        }

        protected override void Unregister(CustomCamera camera)
        {
            base.Unregister(camera);

            SetCamera(camera,false);
        }


        #region Cinemachine

        [SerializeField,Required]
        VirtualCamera m_CinemachineCameraPreset;

        [ShowInInspector,ReadOnly]
        VirtualCamera m_virtualCamera;

        public VirtualCamera virtualCamera { get => m_virtualCamera; protected set => m_virtualCamera = value; }




        #endregion
    }
}

