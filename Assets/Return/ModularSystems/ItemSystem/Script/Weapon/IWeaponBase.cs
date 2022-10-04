using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return.Items.Weapons
{
    public interface IWeaponBase
    {
        WeaponIO io { get; }
        GunStatement State { get; }
        WeaponData data { get; }


        // Pass m_Value
        bool _cooldown { set; }
    }
}