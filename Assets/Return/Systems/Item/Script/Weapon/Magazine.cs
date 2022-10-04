using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return.Items.Weapons
{
    [System.Obsolete]
    public class mmMagazine : WeaponAttachment
    {
        public int MaxAmmo = 0;
        public int CurAmmo = 3000;

        public float _Firerate = 0;
        public float _shootRange = 0;


        //movement
        public float _looksensitivity = 0;

        public int BulletDamage = 10;



    }
}