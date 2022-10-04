using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TNet;
using Sirenix.OdinInspector;
using System;
using Return.Audios;
using Return.Items.Weapons.Firearms;
using Return.Humanoid;
using System.Threading.Tasks;
using System.Linq;

namespace Return.Items.Weapons
{

    /// <summary>
    /// Control fire mode **Net
    /// </summary>
    public partial class FireControlModule: FirearmsModule<FireControlPreset>,IFirearmsRecoilPort, IFirearmsPersistentPerformerHandle, IFirearmsAudioHandle,IMuzzleParticleProvider
    {
        #region Setup

        public void LoadModuleData(FireControlPreset preset)
        {
            pref = preset;
        }

        #endregion

        [Inject]
        protected ITNO tno;

        #region Routine

        protected override void Register()
        {
            base.Register();

            InstanceIfNull(ref AmmoModule);

            if (Item.isMine)
            {
                Control = this.ValidHandle<HostHandler>(Item).
                    Instance().
                    SetModule().
                    Register().
                    //DebugLog().
                    AsHandle<HostHandler>();
            }

            Control?.Register();

        }

        protected override void Unregister()
        {
            Control?.UnRegister();


            base.Unregister();
        }

        protected override void Activate()
        {
            base.Activate();

            Item.resolver.TryGetModule(out performer);

            if (Item.isMine)
            {
                m_FireMode = pref.FireModes.FirstOrDefault();
            }

            Control?.Activate();
        }

        protected override void Deactivate()
        {
            base.Deactivate();

            Control?.Deactivate();

            base.Deactivate();
            // stop anim
        }

        #endregion


        #region Modules

        protected AmmunitionModule AmmoModule;

        /// <summary>
        /// Fire control handle  **User  **AI   **Remote
        /// </summary>
        protected IItemModuleHandle Control;

        #endregion


        #region EventPost

        public event Action<bool> OnFiring;

        #endregion


        #region Muzzle

        public event Action OnMuzzlePlay;

        #endregion


   
        #region PerformerHandle
        // cache for stop
        IFireamrsPerformerHandler performer;

        //public event Action<TimelinePerformerHandle> OnPerformerPlay;

        public void Cancel(TimelinePreset performer)
        {
            switch (performer)
            {
                case var _ when performer == pref.Firing:
                    
                    break;
            }
        }

        public void Finish(TimelinePreset performer)
        {
            switch (performer)
            {
                case var _ when performer == pref.Firing:


                    Debug.Log("On performer end "+performer);
                    //AmmoModule.ChargingBolt();

                    break;
            }

            Debug.Log("On performer end");

        }


        public TimelinePreset[] LoadPerformers()
        {
            return new[]
            {
                //pref.IdleMotion,
                //pref.Trigger,
                pref.Firing,
            };
        }

        #endregion

        #region Audio Port

        public event Action<BundleSelecter> OnAudioPlay;

        #endregion

        #region Recoil IK Port

        public event Action<bl_RecoilBase.RecoilData> OnRecoil;

        /// <summary>
        /// Play IK and camera recoil.
        /// </summary>
        protected virtual void StartRecoil()
        {
            //if(Firearms.isMine) // no need
                OnRecoil?.Invoke(new());
        }

        #endregion


    

        #region Firing

        [Button]
        [RFC]
        protected virtual void PlayFiringPerformerHandle()
        {
            if (Item.isMine)
                tno?.Send(nameof(PlayFiringPerformerHandle), Target.Others);

            // anim
            if (pref.Firing)
            {
                performer.PlayAdditive(pref.Firing, m_FireMode.Rof);

                //var handle =
                //    pref.Firing.GetConfig().
                //    SetDuration(m_FireMode.Rof);

                //performer.PlayState(handle, this);
            }

            // particle
            OnMuzzlePlay?.Invoke();
            

            // audio ammo config => muzzle mixer
            if (pref.MuzzleSounds)
                OnAudioPlay?.Invoke(pref.MuzzleSounds);
        }


        [Button]
        [RFC]
        protected virtual void PlayEmptyTrigger()
        {
            if (Item.isMine)
                tno?.Send(nameof(PlayEmptyTrigger), Target.Others);

            //if (pref.Trigger)
            //    performer.PlayState(pref.Trigger);
            //else 
            if (pref.EmptyTriggerSound)
                OnAudioPlay?.Invoke(pref.EmptyTriggerSound);
        }

        #endregion

        #region Fire Mode

        [Tooltip("Current fire mode")]
        [HideLabel]
        public FireModeBinding m_FireMode;

        public virtual void SwitchMode()
        {
            // logic
            ChangeFireMode(pref.FireModes.Loop(m_FireMode));

            // audio
            PlaySwitchPerformerHandle(m_FireMode.FireMode);
        }


        [RFC]
        protected virtual void ChangeFireMode(FireModeBinding fireMode)
        {
            if (Item.isMine)
                tno?.Send(nameof(ChangeFireMode), Target.OthersSaved, fireMode);

            m_FireMode = fireMode;
        }

        [Button, PropertyOrder(100)]
        [RFC]
        protected virtual void PlaySwitchPerformerHandle(FireMode fireMode)
        {
            if (Item.isMine)
                tno?.Send(nameof(PlaySwitchPerformerHandle), Target.OthersSaved, fireMode);

            // anim -> audio

            if (pref.SwitchModeSound)
                OnAudioPlay?.Invoke(pref.SwitchModeSound);



            if (fireMode.HasFlag(FireMode.Safty) && pref.Safety.NotNull())
            {
                if (performer.CanPlay())
                    performer.PlayState(pref.Safety);
                else
                    performer.QueuePerformer(pref.Safety);
            }
        }

#endregion
    }


}