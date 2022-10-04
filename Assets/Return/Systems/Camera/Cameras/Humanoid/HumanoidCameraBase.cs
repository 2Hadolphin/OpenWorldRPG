using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using Return.Humanoid.Motion;
using Return.Agents;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using Return.Motions;

namespace Return.Cameras
{
    /// <summary>
    /// ?????? **focus point **root **fov **CameraSetting
    /// </summary>
    [DefaultExecutionOrder(ExecuteOrderList.Camera)]
    public abstract class HumanoidCameraBase : CustomCamera
    {
        [SerializeField]
        //[Required]
        AudioListener m_Listener;
        public AudioListener Listener { get => m_Listener; set => m_Listener = value; }

        protected event Action<Vector3> LookAtPoint;
        public float Fov = 90f;

        [SerializeField]
        private Transform m_YawTransform;
        [SerializeField]
        private Transform m_AimYawTransform; 
        [SerializeField]
        private Transform m_PitchTransform;

        /// <summary>
        /// Y-Axis (Left-Right)
        /// </summary>
        public Transform YawTransform { get => m_YawTransform; protected set => m_YawTransform = value; }

        /// <summary>
        /// ?????
        /// </summary>
        public Transform AimYawTransform { get => m_AimYawTransform; protected set => m_AimYawTransform = value; }

        /// <summary>
        /// X-Axis (Up-Down)
        /// </summary>
        public Transform PitchTransform { get => m_PitchTransform; protected set => m_PitchTransform = value; }

        public IHumanoidMotionSystem MotionSystem;



        public Camera Cam
        {
            get => Camera;
        }

        #region Init

        public override void Activate()
        {
            base.Activate();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            //ConstCache.PrimeCamDirector = this;
            //CharacterRoot.InstanceIfNull(ref Camera);

            //ViewMask = DemoPreset.Instance.CharacterViewMask;


            //if (!_Beacon)
            //{
            //    enabled = false;
            //    return;
            //}

            //Camera.cullingMask = ViewMask;

            //Camera.fieldOfView = Fov;
            //TargetFov = Fov;

            SetHandles(true);
        }

        protected override void SetHandles(bool enable)
        {
            base.SetHandles(enable);

            //if (InputHandle != null)
            //    InputHandle.enabled = enable;

            //LockMouse(enable);

            if (Listener != null)
                Listener.enabled = enable;
        }


        protected override void OnDisable()
        {
            base.OnDisable();

            SetHandles(false);
        }


        /// <summary>
        /// Perfect camera fix phase
        /// </summary>
        /// <param name="pr">Last controller delta move</param>
        protected virtual void OnAfterControllerMove(PR pr) { }


        #endregion


        #region Delete

        /// <summary>
        /// focus target
        /// </summary>
        [Obsolete]
        public Transform _Beacon = null;
        #endregion

        #region Setting

        /// <summary>
        /// Different setting for custom camera effects.
        /// </summary>
        protected mStack<CameraSetting> Settings = new mStack<CameraSetting>();

        [SerializeField]
        private CameraSetting m_CameraConfig;

        public CameraSetting setting { get=>m_CameraConfig; protected set=>m_CameraConfig=value; }

        /// <summary>
        /// Add new camera setting.
        /// </summary>
        public virtual void AddDirector(CameraSetting camSetting)
        {
            if (camSetting == null)
                return;

            if (camSetting.Priority != null)
                Settings.Push(camSetting, (x) => x.Priority > camSetting.Priority);
            else
                Settings.Push(camSetting);

            Evaluate = false;

            if (token_Director.NotNull())
            {
                token_Director.Cancel(false);
                token_Director = null;
            }

            if (token_Director.IsNull())
            {
                token_Director = new CancellationTokenSource();
                //var token = gameObject.GetCancellationTokenOnDestroy();

                token_Director.RegisterRaiseCancelOnDestroy(this);
            }

            UniTask.Create(ChangeDirector).AttachExternalCancellation(token_Director.Token);
        }

        protected CancellationTokenSource token_Director;

        /// <summary>
        /// Remove camera setting.
        /// </summary>
        public virtual void RemoveDirector(CameraSetting setting)
        {
            if (setting == null)
                return;

            var last = Settings.Peek();

            if (last == setting)
                if (Evaluate && token_Director.NotNull())
                    token_Director.Cancel();

            Settings.Remove(setting);
        }



        protected virtual async UniTask ChangeDirector()
        {
            var current = Settings.Peek();

            Evaluate = true;
            float duringTime = 0.13f;
            float time = 0;

            Vector3 deltaOffsetPos = default;
            Vector3 deltaOffsetRot = default;
            float dampSensitivity_H = default;
            float dampSensitivity_V = default;

            float dampSmoothLerpPos = default;
            float dampSmoothLerpRot = default;

            var max = float.PositiveInfinity;

            while (Evaluate && enabled)
            {
                if (time < duringTime)
                {
                    var delta = ConstCache.deltaTime;
                    time += delta;

                    setting.Offset_pos = Vector3.SmoothDamp(setting.Offset_pos, current.Offset_pos, ref deltaOffsetPos, duringTime, max, delta);
                    setting.Offset_rot = MathfUtility.SmoothDampQuaternion(setting.Offset_rot, current.Offset_rot, ref deltaOffsetRot, duringTime);

                    setting.Sensitivity_H = Mathf.SmoothDamp(setting.Sensitivity_H, current.Sensitivity_H, ref dampSensitivity_H, duringTime, max, delta);
                    setting.Sensitivity_V = Mathf.SmoothDamp(setting.Sensitivity_V, current.Sensitivity_V, ref dampSensitivity_V, duringTime, max, delta);


                    setting.SmoothLerp_Position = Mathf.SmoothDamp(setting.SmoothLerp_Position, current.SmoothLerp_Position, ref dampSmoothLerpPos, duringTime, max, delta);
                    setting.SmoothLerp_Rotation = Mathf.SmoothDamp(setting.SmoothLerp_Rotation, current.SmoothLerp_Rotation, ref dampSmoothLerpRot, duringTime, max, delta);

                }
                else
                {
                    setting.Offset_pos =  current.Offset_pos;
                    setting.Offset_rot = current.Offset_rot;

                    setting.Sensitivity_H = current.Sensitivity_H;
                    setting.Sensitivity_V = current.Sensitivity_V;


                    setting.SmoothLerp_Position =  current.SmoothLerp_Position;
                    setting.SmoothLerp_Rotation = current.SmoothLerp_Rotation;

                    break;
                }
                await UniTask.NextFrame();
            }


            return;
        }

        #endregion

        protected bool Evaluate = false;

        #region Routine

        protected virtual void Update() { }

        protected virtual void LateUpdate()
        {
            ProcessFOV();
        }
        #endregion

        #region Main
        protected virtual void ProcessFOV()
        {
            float angle = Mathf.Abs(Fov / zoomMultiplier - Fov);
            var fov= Mathf.MoveTowards(Camera.fieldOfView, TargetFov, angle / zoomDuration * ConstCache.deltaTime);
            fov = Mathf.Clamp(fov, 43f, 77f);

            if (float.IsNaN(fov))
                fov = 55f;

            Camera.fieldOfView = fov;
        }

        protected float TargetFov, zoomDuration, zoomMultiplier;

        public virtual void Zoom(float fov, float time = 2f, float multiplier = 2f)
        {
            if (fov > 0)
                TargetFov = fov;
            else
                TargetFov = Fov;

            zoomDuration = time;
            zoomMultiplier = multiplier;
        }
        #endregion

        //protected virtual void RebuildBeacon()
        //{
        //    var go = new GameObject();
        //    go.hideFlags = HideFlags.NotEditable;

        //    //var fb = go.AddComponent<FocusBall>();

        //    ////fb.Init(this, new Injector.Void(RebuildBeacon));
        //    //_FocusPoint = fb.transform;
        //}



        #region Overwrite 
        [ShowInInspector,BoxGroup("State")]
        protected Vector3 Overwrite_pos;
        [ShowInInspector, BoxGroup("State")]
        protected Vector3 Overwrite_rot;
        #endregion


    }
}