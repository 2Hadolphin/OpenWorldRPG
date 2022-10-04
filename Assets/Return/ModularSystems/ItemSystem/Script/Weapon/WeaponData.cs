using Assets.Scripts.Item.Weapon;
using UnityEngine;
using Return.Items.Weapons;
using Return.CentreModule;

namespace Return.Items.Weapons
{
    [RequireComponent(typeof(WeaponCentral))]
    public class WeaponData: WeaponSystemMethods
    {
        [SerializeField]
        private WeaponGunData GunData;
        [SerializeField]
        private WeaponGunData LocalData;
        public WeaponGunData CurrentData { get { return LocalData; } }

        public struct Data 
        {
            public string LastUserID;
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
            public WeaponGunData.ShootingMotion Motion;
        }


        public void Initialization()
        {
            if (LocalData == null)
                LocalData = new WeaponGunData();
            LocalData.LastUserID = "haha";
            LocalData.DamageZ = new Vector2Int(50, 90);
            LocalData.AttackSpeedZ = new Vector2Int(50, 90);
            LocalData.RangeZ = new Vector2Int(75, 150);

            LocalData.Damage = Random.Range(LocalData.DamageZ.x, LocalData.DamageZ.y);
            LocalData.AttackSpeed = Random.Range(LocalData.AttackSpeedZ.x, LocalData.AttackSpeedZ.y);
            LocalData.Range = Random.Range(LocalData.RangeZ.x, LocalData.RangeZ.y);

            LocalData.FireRate = 1200;
            LocalData.KineticEnergy = 2000;
            LocalData.Hardness = 8;
            LocalData.Penetration = 10;
            LocalData.IterationStep = 20;
            LocalData.CaculateMask = GDR.WeaponPhysicMask;
            LocalData.Motion = WeaponGunData.ShootingMotion.Slot;

        }
    }
}