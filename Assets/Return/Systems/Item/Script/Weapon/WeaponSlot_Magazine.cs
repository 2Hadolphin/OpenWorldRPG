using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Return.Items.Weapons
{

    public class WeaponSlot_Magazine : WeaponSlot_Element
    {
        public mmMagazine magazine { get;private set; }
        public Transform tf { get; }

        public void Refresh()
        {
            magazine = gameObject.GetComponentInChildren<mmMagazine>();
        }
    }
}