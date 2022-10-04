using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Return.Items.Weapons
{
    public enum WeaponType
    {
        Primary,
        Secondary,
        Undefined
    }


    /// <summary>
    /// How the gun can be loaded with ammo.
    /// </summary>
    [Flags]
    public enum ReloadMode
    {
        None=0,
        Magazines=1,
        BulletByBullet=2,
    }

    public enum Ballistics
    {
        [Tooltip("Firearms")]
        PhysicCast,
        [Tooltip("Rocket")]
        Rigidbody,
    }


    /// <summary>
    /// Weapon States. ????????delete
    /// </summary>
    public enum WeaponState
    {
        Idle,
        Firing,
        Reloading,
        MeleeAttacking,
        Interacting
    }
}