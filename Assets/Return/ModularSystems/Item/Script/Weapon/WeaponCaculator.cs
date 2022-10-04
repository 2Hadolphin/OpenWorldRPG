using Assets.Scripts.Item.Weapon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Return.Items.Weapons
{

    public abstract class WeaponCaculator : WeaponSystemMethods, IWeaponCaculator
    {
        //Ref 
        protected IWeaponBase central;
        protected WeaponGunData.ShootingMotion Motion;

        protected Transform chamber;
        protected float FireRate;
        public abstract bool Initialization(IWeaponBase central);
        public abstract void Single();
        public abstract void Multi();
        public abstract void CheckMotion();
        public abstract void MathfOperate();

    }
}