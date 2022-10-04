using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;


namespace Return.Items
{
    public class SlotType : PresetDatabase, IEquatable<SlotType>
    {
        [ListDrawerSettings(Expanded =true,DraggableItems =true)]
        public HashSet<ItemCategory> EnableCategories = new();

        public bool Equals(SlotType other)
        {
            return Title == other.Title;
        }


    }

}