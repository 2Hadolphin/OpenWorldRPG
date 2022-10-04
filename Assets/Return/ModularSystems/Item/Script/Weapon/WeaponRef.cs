using UnityEngine;
using System;

namespace Return.Items.Weapons
{
    public abstract class WeaponRef
    {
       public  WeaponGunData Data;
    }

    [Serializable]
    public partial class WeaponElementRef
    {
        public IWeaponMagazine Magazine;
        public WeaponSlot_Element Sight;
        public WeaponSlot_Element Barrel;
        public WeaponSlot_Element Ejector;
        public WeaponSlot_Element Animator;
        public WeaponSlot_Element MagazineSlot;
        public WeaponSlot_Element Handle;
        public WeaponSlot_Element subHandle;


        public LineRenderer rayline;
    }

    [Serializable]
    public partial class WeaponGunData
    {
        [Header("BasicInfo")]
        public string LastUserID = null;

        [Header("Elements")]
        
        public Vector2Int DamageZ;
        public Vector2Int AttackSpeedZ;
        public Vector2Int RangeZ;
        public int Damage;
        public int AttackSpeed;
        public int Range;
        public float FireRate; //1800 per minute shot
        public int KineticEnergy;
        public float Hardness;
        public int Penetration;
        public int IterationStep;
        public LayerMask CaculateMask;


        public enum ShootingMotion { Slot,Cracker,Stick,Storm}
        [Header("Behavior")]
        public ShootingMotion Motion;
    }



}