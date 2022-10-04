
using Assets.Scripts.Item.Weapon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
using Return.Humanoid;

namespace Return.Items.Weapons
{
    public abstract class WeaponCentral : MonoBehaviour
    {

        public WeaponIO io { get; protected set; }
        public WeaponData data { get; protected set; }
        protected WeaponCaculator caculator;
        //protected IInputAdapter adapter;
        protected AudioSource Audio;//?

        #region PublicCall
        public abstract void Initialization();


        public abstract void Activate();
        public abstract void Deactivate();
        public abstract void AnalyzeData();
        public abstract Transform IKHandle { get; }

        public abstract bool isReady(out float restTime);

        #endregion

        #region NormalFunction
        protected abstract bool active { get; set; }

        public abstract Transform pamaterIKHandle { get; }


        protected abstract void UpdateState();
        protected abstract void CheckProperty();
        protected abstract void PlayDebris();
        #endregion


        #region BaseInvokeFunction

        public abstract bool TrySwitchState();
        public abstract bool TryWeaponMainUse();
        public abstract bool TryRest();
        public abstract bool TryAim();
        public abstract bool TryDisaim();
        public abstract bool TryDequit();


        public abstract void SwitchState();
        public abstract void WeaponMainUse();
        public abstract void Rest();
        public abstract void Aim();
        public abstract void Disaim();
        public abstract void Dequit();


        #endregion
    }
}
