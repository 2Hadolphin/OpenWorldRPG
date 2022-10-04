using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Return.Items.Weapons.Firearms
{
    public class FirearmsPreset : ItemPreset
    {
        public override AbstractItem CreateItem(Transform tf = null)
        {
            return base.CreateItem<Firearms>(tf);
        }

    }
}

