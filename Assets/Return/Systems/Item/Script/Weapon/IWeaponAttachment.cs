using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return.Items.Weapons
{
    public interface IWeaponMagazine
    {
        mmMagazine magazine { get; }
        Transform tf { get; }

        void Refresh();
    }
}