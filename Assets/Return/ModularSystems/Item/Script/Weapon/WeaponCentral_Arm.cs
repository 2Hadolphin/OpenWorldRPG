using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using Return.Humanoid;

namespace Return.Items.Weapons
{
    public class WeaponCentral_Arm : WeaponCentral,IWeaponBase
    {

        public struct FireMode { bool Safty;bool Single;bool Burst;bool Auto; }
        public enum mode { Safty, Single, Auto }
        private mode SwitchMode;

        private Coroutine autoFire;
        public GunStatement State { get; private set; }



        #region PublicCall
        public override void Initialization()
        {
            Transform tf = transform;
            io = tf.GetComponent<WeaponIO>();
            data = tf.GetComponent<WeaponData>();
            caculator = tf.GetComponent<WeaponCaculator>();

            io.InitializationData();
            data.Initialization();
            caculator.Initialization(this);
            if (State == null)
                State = new GunStatement();
            SwitchMode = new mode();
            SwitchMode = mode.Safty;

            
            //if(!system.getItem.transform.TryGetComponent<AudioSource>(out Audio))
            //{
            //    Audio = system.getItem.CharacterRoot.AddComponent<AudioSource>();
            //}

        }

        public override Transform IKHandle
        {

            get 
            {
                Transform tf = io.ElementRef.Handle.transform;
                return tf; 
            }

        }

        public override void Activate()
        {
            active = true;
        }
        public override void Deactivate()
        {
            active = false;
        }
        public override void AnalyzeData()
        {
            throw new System.NotImplementedException();
        }
        public override bool isReady(out float restTime)
        {
            restTime = 0;
            return State.hasCoolDown;
        }

        #endregion

        #region PublicInvoke
        public override bool TryWeaponMainUse()
        {
            return true;
        }
        public override void WeaponMainUse()
        {
            PullTrigger();
        }
        public override bool TryRest()
        {
            return true;
        }
        public override void Rest()
        {
            if (autoFire != null)
            {
                StopCoroutine(autoFire);
                autoFire = null;
            }
        }

        public override bool TryAim()
        {
            return true;
        }

        public override void Aim()
        {
            print("Aim !");
        }

        public virtual bool TryReload()
        {
            return true;
        }
        public virtual void Reload()
        {            
            /*
            mmMagazine NewMag;
            if (ReloadMagable(out NewMag))
            {
                mmMagazine OldMag = IO.ElementRef.MagazineSlot.magazine;
                if (OldMag != null)
                {
                    // get oldmag to bag;
                }

                NewMag.transform.SetParent(IO.ElementRef.MagazineSlot.tf);
                NewMag.CharacterRoot.SetActive(true);
                IO.ElementRef.MagazineSlot.magazine = NewMag;
            }
            */
            State.Ammo = 30;
            CheckProperty();
        }
        public override bool TryDequit()
        {
            return true;
        }
        public override void Dequit()
        {
            Deactivate();
        }
        public override bool TryDisaim()
        {
            return true;
        }

        public override void Disaim()
        {
            print("Disaim !");
        }

        public override bool TrySwitchState()
        {
            return true;
        }
        public override void SwitchState()
        {
            SwitchSafe();
        }
        #endregion

        #region MainThred
        protected virtual void SwitchSafe()
        {
            Rest();
            switch (SwitchMode)
            {
                case mode.Safty:
                    SwitchMode = mode.Single;
                    break;
                case mode.Single:
                    SwitchMode = mode.Auto;
                    break;
                case mode.Auto:
                    SwitchMode = mode.Safty;
                    break;
            }

        }
        protected virtual void PullTrigger()
        {
            if (CheckShootable())
            {
                PerformShoot();
            }
        }
        protected virtual bool CheckShootable()
        {
            if (State.IsShootable)
                if (State.hasCoolDown)
                    return true;
            print("Weapon Useable : "+ State.IsShootable);
            return false;
        }
        protected virtual void PerformShoot()
        {
            switch (SwitchMode)
            {
                case mode.Safty:
                    //play sound effect
                    break;
                case mode.Single:
                    caculator.Single();
                    Audio.PlayOneShot(io.Clips[0]);
                    UpdateState();
                    break;
                case mode.Auto:
                    autoFire = StartCoroutine(AutoFire());
                    break;
            }
            if (true) //?? PlayerTargetMotionMode hasAim?
            {

            }
            else
            {
                caculator.MathfOperate();
            }


            PlayDebris();
        }
        #endregion

        #region NormalFunction

        protected virtual bool ReloadMagable(out mmMagazine magazine)
        {
            magazine = null; //?? make bag relase mag to Temporary port 
            return true; //?? get bag system
        }
        protected override bool active
        {
            get { return State.isActive; }
            set
            {
                if (value)
                {
                    CheckProperty();
                    Audio.enabled = true;
                    SwitchMode = mode.Auto;
                }
                else
                {
                    Audio.enabled = false;
                    State.IsShootable = false;

                }
                State.isActive = value;
            }
        }

        public virtual bool _cooldown { set { State.hasCoolDown = value; } }

        public override Transform pamaterIKHandle
        {
            get
            {
                if (io.ElementRef.subHandle == null)
                    return IKHandle;
                return io.ElementRef.subHandle.transform;
            }
        }

        public int GetOriginSN => throw new System.NotImplementedException();

        IEnumerator AutoFire()
        {
            for (; ; )
            {
                if (State.Ammo > 0)
                    if (State.hasCoolDown)
                    {
                        State.hasCoolDown = false;
                        caculator.Multi();
                        Audio.PlayOneShot(io.Clips[0]);
                        UpdateState();
                    }

                yield return null;
            }
        }

        protected override void CheckProperty()
        {
            /*
            IO.ElementRef.MagazineSlot.Refresh();
            State.hasMag = IO.ElementRef.mmMagazine.magazine == null;
            if(State.hasMag)
                State.Ammo = IO.ElementRef.mmMagazine.magazine.CurAmmo;
            */
            State.hasCoolDown = true;
            State.IsShootable = true;
            State.Ammo = 30;
            // State.BasicData weapon data

        }
        protected override void UpdateState()
        {
            State.Ammo--;
            State.hasCoolDown = false;
            if (State.Ammo > 0)
            {
                State.hasMag = true;
            }
            else
            {
                State.hasMag = false;
                State.IsShootable = false;
            }
            io.PostMagState();
        }
        protected override void PlayDebris()
        {

        }


        #endregion

    }
}