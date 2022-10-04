using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using TNet;
using System;
using Return.Items.Weapons.Firearms;
using Return.Agents;

namespace Return.Items.Weapons
{
    public partial class MarkerModule : FirearmsModule<MarkerPreset>, IFirearmsPersistentPerformerHandle
    {

        [Inject]
        protected ITNO tno;


        protected override void Activate()
        {
            base.Activate();

            Item.resolver.TryGetModule(out performer);

            var isLocalUser = Item.Agent.IsLocalUser();



            if (Sight.IsNull() && gameObject.GetChildNamedWithTag(Tags.Config, FirearmsConfig.str_Sight, out var sightSlot))
            {
                if (!sightTransform && pref.Sight)
                    Sight = Instantiate(pref.Sight, sightSlot.transform);
                else if (isLocalUser)
                {
                    Sight = new GameObject("Default Iron sightTransform").AddComponent<Sight>();

                    Sight.ParentOffset(sightSlot.transform);

                    Sight.Offset = pref.Idle.GetOffset();

                    var point = new GameObject("Aim Point").transform;
                    point.ParentOffset(point, Sight.Offset);
                    Sight.AimPoint = point;
                }
            }

            sightTransform = Sight.transform;

            if (sightTransform.NotNull())
            {
                var sightModule = sightTransform.GetComponent<IItemModule>();

                if (isLocalUser)// _checkCache host
                    sightModule?.Deactivate();
                else
                    sightModule?.Activate();
            }
            else
                sightTransform = transform;


            if (isLocalUser)
            {
                var cameraLog =
                    this.ValidMonoHandle<CameraHandle>(Item).
                    BindLocalUser().
                    Instance(gameObject).
                    SetModule().
                    Register().
                    DebugLog();

                //var inputLog = this.ValidMonoHandle<InputHandle>(Item).
                //    BindLocalUser().
                //    Instance(gameObject).
                //    SetHandler().
                //    Register().
                //    DebugLog();

                if (Item.Agent.IsLocalUser())
                {
                    var input = new InputHandle();

                    input.RegisterInput(InputManager.Input);
                    Item.resolver.RegisterModule(input);
                    input.SetHandler(this);
                    input.enabled = true;
                }
            }
        }


        #region Performer Handle

        protected IFireamrsPerformerHandler performer;


        public TimelinePreset[] LoadPerformers()
        {
            return new[] { pref.Aiming };
        }

        //public event Action<TimelinePerformerHandle> OnPerformerPlay;

        public void Cancel(TimelinePreset performer)
        {
            Debug.Log("On marker cancel "+performer);
        }


        public void Finish(TimelinePreset performer)
        {
            Debug.Log("On marker finish " + performer);

            switch (performer)
            {
                case var _ when performer == pref.Aiming:

                    break;
            }
        }



        #endregion


        [ShowInInspector]
        protected Transform sightTransform;

        [SerializeField]
        protected Sight Sight;

        public float aimingBobRatio=0.13f;


        /// <summary>
        /// Change the mode used to operate **scope ratio **Night Vision **Infrared Imager
        /// </summary>
        public virtual void SwitchAssist()
        {

        }

        public float AimingDuration => Sight.NotNull() ? Sight.aimingTime : 0.3f;


        bool isAiming;
        //bool transitAiming;
        //float lerpPercent;

        public event Action<bool> OnAiming;

        /// <summary>
        /// Using aimming system **iron sight **scope **tracker
        /// </summary>
        public virtual void Aim()
        {
            if (!isAiming)
            {
                isAiming = true;
                PlayAimingPerformerHandle();
                OnAiming?.Invoke(isAiming);
            }
            else
            {
                CancelAim();
            }

        }

        public virtual void CancelAim()
        {
            isAiming = false;

            // play anim
            performer.SetIdleState(pref.Aiming.GetConfig().Cancel());
            //performer.StopAdditive(pref.Aiming);

            OnAiming?.Invoke(isAiming);
            //CalculateProgress();
        }


        [Button, PropertyOrder(100)]
        [RFC]
        protected virtual void PlayAimingPerformerHandle()
        {
            if (Item.isMine)
                tno?.Send(nameof(PlayAimingPerformerHandle), Target.Others);

            // anim -> audio


            if (pref.Aiming)
            {
                //if (performer.CanPlay())
                {
                    performer.SetIdleState(
                        pref.Aiming.GetConfig().
                        SetWrapMode(WrapMode.ClampForever));
                }

                //performer.PlayAdditive(
                //    pref.Aiming.
                //    GetConfig().
                //    SetWrapMode(WrapMode.ClampForever));



                //performer.QueuePerformer(
                //    pref.Aiming.GetConfig().
                //    SetWrapMode(WrapMode.ClampForever));
            }


            //else if (pref.SwitchModeSound)
            //    OnAudioPlay?.Invoke(pref.SwitchModeSound);
        }

        /// <summary>
        /// Adjust the lens magnification
        /// </summary>
        /// <param name="value"></param>
        public virtual void RoomIn(float value)
        {
            // setting roomIn animation **spark light
        }
    }

    public interface IMarkerProvider //IBallisticProvider
    {
        Ray GetRay();
    }

    public interface ICameraStatusReciver 
    {
        void OnCameraStatusChange(UTag type, bool active = true);

    }

}