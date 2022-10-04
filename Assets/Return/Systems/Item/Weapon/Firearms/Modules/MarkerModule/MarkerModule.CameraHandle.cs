using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using System;
using Return.Cameras;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.Assertions;
using Return.Motions;

namespace Return.Items.Weapons
{
    public partial class MarkerModule   //Camera **local user
    {
        /// <summary>
        /// MonoModule to local user camera. **Ray ** Aiming ** Shaking
        /// </summary>
        public class CameraHandle : MonoItemModuleHandle<MarkerModule>, IMarkerProvider, ICameraStatusReciver
        {
            protected override void ActivateHandle(IItem item, MarkerModule module)
            {
                base.ActivateHandle(item, module);

                CameraManager.OnCamerasChange += OnCameraStatusChange;

                var firstPersonCam = CameraManager.GetCamera<FirstPersonCamera>();

                OnCameraStatusChange(firstPersonCam.cameraType, firstPersonCam.isActiveAndEnabled);

                ItemCamHandle = firstPersonCam.FirstPersonHandleTransform;

                ViewSlot = item.transform.Traverse().FirstOrDefault(x => x.name.Contains("view", StringComparison.CurrentCultureIgnoreCase));

                Assert.IsNotNull(Module.Sight);

                module.OnAiming += Module_OnAiming;

                module.performer.OnMarkerPost += Performer_OnMarkerPost;

                // bind camera bob with motion system
                if(item.Agent.Resolver.TryGetModule<IMotionSystem>(out var motion))
                {
                    // handler
                    InstanceIfNull(ref AdditiveHandler);
                    item.resolver.RegisterModule(AdditiveHandler);
                    AdditiveHandler.ItemCameraTransform = ItemCamHandle;//= CameraManager.GetCamera<FirstPersonCamera>()?.HandleCameraTransform;


                    #region Bob

                    InstanceIfNull(ref PositionBob);
                    PositionBob.velocityGetter = motion.LocalSelfVelocity;
                    PositionBob.BobData = module.pref.PositionBobData;
                    AdditiveHandler.ApplyAdditiveEffect(PositionBob);
                    SetBlend(0);

                    InstanceIfNull(ref RotationBob);
                    RotationBob.velocityGetter = motion.LocalSelfVelocity;
                    RotationBob.BobData = module.pref.RotationBobData;
                    AdditiveHandler.ApplyAdditiveEffect(RotationBob);


                    #endregion


                    #region Sway
 
                    InstanceIfNull(ref SwayAmplifier);
                    SwayAmplifier.Init(firstPersonCam);
                    SwayAmplifier.data = module.pref.SwayData;
                    AdditiveHandler.ApplyAdditiveEffect(SwayAmplifier);

                    #endregion
                }
            }


            protected override void Deactivate()
            {
                base.Deactivate();


                AdditiveHandler.enabled = false;

                PositionBob.enabled = false;
                RotationBob.enabled = false;
                SwayAmplifier.enabled = false;
            }

            #region Marker Raycast

            MarkerCalculator Calculator;

            public virtual Ray GetRay()
            {
                return Calculator.Calculate();
            }

            public void OnCameraStatusChange(UTag type, bool active=true)
            {
                var isFirstPerson = CameraManager.ActivateCameraTypes.HasFlag(CameraManager.FirstPersonCamera);
                if (isFirstPerson)
                    Calculator = new FirstPersonMarkerCalculator();

                Calculator.Init(this);
            }

            protected abstract class MarkerCalculator
            {
                public virtual void Init(CameraHandle cameraHandle)
                {
                    handle = cameraHandle;
                }

                protected CameraHandle handle;

                protected virtual Transform marker => handle.Module.sightTransform;
                public abstract Ray Calculate();
            }

            protected class FirstPersonMarkerCalculator: MarkerCalculator
            {
                public override void Init(CameraHandle cameraHandle)
                {
                    base.Init(cameraHandle);

                    var cam = CameraManager.GetCamera<FirstPersonCamera>();

                    fpHandleCam = cam.HandleCamera;
                    viewCam = cam.Cam;
                }

                Camera fpHandleCam;
                Camera viewCam;



                // first person =>
                // 1. marker annex position to first person handle camera screen space
                // 2. set as first person main camera screen space
                // 3. transform screen position to main camera space (depth)
                public override Ray Calculate()
                {
                    var point = marker.position;
                    var viewPoint = fpHandleCam.WorldToScreenPoint(point);
                    return viewCam.ScreenPointToRay(viewPoint);
                }
            }

            #endregion



            #region Camera Align

            private void Performer_OnMarkerPost(UnityEngine.Timeline.Marker marker)
            {
                if (marker is not DefinitionMarker definition)
                    return;

                if (definition.EventID.Equals(Module.pref.Events.OnCameraAnimationStart))
                    animCamera = true;
                else if (definition.EventID.Equals(Module.pref.Events.OnCameraAnimationStop))
                    animCamera = false;
            }

            ///// <summary>
            ///// GUIDs for FPHandle camera to align.
            ///// </summary>
            //public ICoordinate m_SightPivot;



            [ShowInInspector]
            protected Transform ItemCamHandle;
            [ShowInInspector]
            protected Transform ViewSlot;


            [ShowInInspector]
            bool animCamera
            { 
                get => m_animCamera;
                set
                {
                    if (!m_animCamera.SetAs(value))
                        return;

                    float end = value ? 1 : 0;
                    var seq = LerpMath(ref cam_anim_uid, ref cam_anim_durationTransit, end);

                    var duration = end.Difference(cam_anim_blend) / 1f * 0.1f;

                    Debug.Log(duration);

                    RotationBob.isAiming = value;

                    seq.Append(
                        DOTween.To(
                            () => cam_anim_blend, x => cam_anim_blend = x, end, duration).
                            SetEase(Ease.OutQuad).
                            OnComplete(() => cam_anim_durationTransit = false));
                }
            }

          
            float cam_anim_blend;
            private Guid cam_anim_uid = Guid.Empty;
            bool cam_anim_durationTransit;

            bool m_animCamera;

            [SerializeField]
            Vector3 m_CameraOffset;

            private void LateUpdate()
            {
                if (ItemCamHandle.IsNull())
                    return;

                Vector3 hold, align;

                hold = Vector3.Lerp(
                                     ViewSlot.TransformPoint(m_CameraOffset),
                                     ItemCamHandle.position,
                                     cam_anim_blend);

                if (Module.Sight.NotNull())
                    align = Module.Sight.GetAimPoint();
                else
                    align = Module.sightTransform.position;

                hold.LerpTo(align, blend);

                ItemCamHandle.position = hold;

                AdditiveHandler.UpdateTransforms();
            }

            [ShowInInspector,NonSerialized]
            float blend;
            [ShowInInspector, NonSerialized]
            private Guid uid=Guid.Empty;
            [ShowInInspector, NonSerialized]
            bool durationTransit;

            private void Module_OnAiming(bool isAiming)
            {
                float end = isAiming ? 1 : 0;
                var seq=LerpMath(ref uid,ref durationTransit,end);

                var duration = (end.Difference(blend) / 1f) * Module.AimingDuration;

                //Debug.Log(duration);

                seq.Append(
                    DOTween.To(
                        () => blend, SetBlend, end, duration).
                        SetEase(Ease.OutQuad).
                        OnComplete(() => durationTransit = false));
            }

            /// <summary>
            /// Set bob
            /// </summary>
            void SetBlend(float aimingRatio)
            {
                blend = aimingRatio;

                if (PositionBob.NotNull())
                    PositionBob.SetIncrease((1f-aimingRatio).Max(Module.aimingBobRatio));
            }

            protected virtual Sequence LerpMath(ref Guid uid,ref bool duringTransit,float end)
            {
                if (duringTransit)
                    DOTween.Kill(uid);
                else
                    duringTransit = true;

                Sequence seq = DOTween.Sequence();
                uid = Guid.NewGuid();
                seq.id = uid;

                return seq;
            }

            #endregion


            #region Camera Additive Effect **Bob **Kick

            FirstPersonCameraAdditiveHandler AdditiveHandler;

            // bob
            FirearmsPositionBob PositionBob;
            FirearmsRotationBob RotationBob;

            // sway
            FirearmsAimAmplifier SwayAmplifier;

            #endregion
        }
    }

}