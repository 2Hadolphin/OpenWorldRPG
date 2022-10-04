using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.AddressableAssets;
using Zenject;
using System;

namespace Return.Cameras
{
    /// <summary>
    /// Manage custom cameras. **SO-USingleton
    /// </summary>
    public abstract class BaseCameraManager : SingletonSOManager<BaseCameraManager>, IStart
    {
        public static UTag FirstPersonCamera => Instance.m_FirstPersonCamera;
        [SerializeField]
        protected UTagPicker m_FirstPersonCamera;

        public static UTag ThirdPersonCamera => Instance.m_ThirdPersonCamera;


        [SerializeField]
        protected UTagPicker m_ThirdPersonCamera;

        #region IStart

        void IStart.Initialize()
        {
            Initialize();
        }

        #endregion

        public override void Initialize()
        {
            Assert.IsNotNull(Instance);

            Cameras.Clear();

            foreach (var asset in PreloadCameras)
            {
                var valid = asset.LoadInstanceGameObject(LoadCamera);

                //if (!valid)
                //    Debug.LogError("Failure to load preset cameras");
                //else
                //    Debug.Log("Loading camera : " + asset);
            }

            // subscribe camera status
            CustomCamera.OnStateChanged += SetCamera;
        }


        void LoadCamera(GameObject obj)
        {
            var newCam = obj.GetComponentInChildren<CustomCamera>(true);

            //Debug.Log(newCam);

            RegisterCamera(newCam);
            newCam.enabled = false;


            DontDestroyOnLoad(newCam.gameObject);
        }

        /// <summary>
        /// On custome camera status changed.
        /// </summary>
        protected virtual void SetCamera(CustomCamera camera,bool enable)
        {
            if (enable)
                RegisterCamera(camera);
            else
                UnregisterCamera(camera);
        }

        public static void RegisterCamera(CustomCamera camera)
        {
            Instance.Register(camera);
        }

        public static void UnregisterCamera(CustomCamera camera)
        {
#if UNITY_EDITOR
            if (Instance)
#endif
                Instance.Unregister(camera);
        }

        /// <summary>
        /// Add camera to list.
        /// </summary>
        protected virtual void Register(CustomCamera camera)
        {
            Cameras.Add(camera);
        }


        /// <summary>
        /// Remove camera from list.
        /// </summary>
        protected virtual void Unregister(CustomCamera camera)
        {
            Cameras.Remove(camera);
        }





        [SerializeField]
        List<AssetReference> m_preloadCameras;
        public List<AssetReference> PreloadCameras { get => m_preloadCameras; set => m_preloadCameras = value; }


        /// <summary>
        /// Search current camera which force on target
        /// </summary>
        //[Obsolete]
        //public static CustomCamera GetCamera(GameObject fouceTarget=null)
        //{
        //    return Instance.SearchCamera(fouceTarget? fouceTarget.transform:null);
        //}

        public static T GetCamera<T>()where T : CustomCamera
        {
            var cams = Cameras;

            foreach (var cam in cams)
            {
                if (cam is T target)
                    return target;
            }


            Debug.LogException(new NotSupportedException($"Failure to find camera {typeof(T)}."),Instance);
            return null;
        }

        [Obsolete]
        protected CustomCamera SearchCamera(Transform cameraFocusTarget=null)
        {
            if (cameraFocusTarget)
            {
                foreach (var cam in Cameras)
                    if(cam&&cam.HostObject)
                        if (cameraFocusTarget.IsChildOf(cam.HostObject))
                            return cam;

                var mainCam = Camera.main;

                foreach (var cam in Cameras)
                {
                    if (cam&& cam.mainCamera == mainCam)
                        return cam;
                }

                if (!mainCam)
                {
                    return null;
                }
                else if (mainCam.TryGetComponent<CustomCamera>(out var mCam))
                    return mCam;
                else
                    return mainCam.gameObject.InstanceIfNull<CameraWrapper>();
            }
            else
            {
                foreach (var cam in Cameras)
                    if (cam.isActiveAndEnabled)
                        return cam;
            }

            throw new KeyNotFoundException(Cameras.Count.ToString());

        }




        [ShowInInspector]
        protected static HashSet<CustomCamera> Cameras = new(); 

        protected virtual void Clean()
        {
            var breakCamRef = Cameras.Where(x => !x).ToArray();
            foreach (var cam in breakCamRef)
            {
                if (cam)
                    Cameras.Remove(cam);
            }
        }

    }
}

