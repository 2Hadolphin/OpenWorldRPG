using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Return.Items;
using Return.Humanoid;
using Return.CentreModule;

using Return.Items.Gear;
using Return;
using Return.Gear;
using System;
using TNet;
using Return.Equipment.Slot;

namespace Return.Inventory
{

    /// <summary>
    /// Multi slot with require type.
    /// </summary>
    public class SingleSlot : Inventory, ISingleSlot, IDataNodeSerializable
    {
        [SerializeField]
        Slot[] m_Slots;
        public Slot[] Slots { get => m_Slots; set => m_Slots = value; }


        [SerializeField]
        Category m_category;

        /// <summary>
        /// Limit storage type. **Gun **Medicine
        /// </summary>
        public Category category { get => m_category; set => m_category = value; }


        #region IDataNodeSerializable

        public override void Serialize(DataNode node)
        {
            base.Serialize(node);
        }

        public override void Deserialize(DataNode node)
        {
            base.Deserialize(node);
        }

        #endregion


        #region ISingleSlot

        public override bool CanStorage(object obj)
        {
            if (obj is IPickup pickup)
                if (category && !category.Contains(pickup.Preset.Category))
                    return false;

            return base.CanStorage(obj);
        }

        public override void Store(object obj)
        {
            // store
            base.Store(obj);

            // push into showcase
            foreach (var slot in Slots)
            {
                if(slot.TrySetSlot(obj))
                    break;
            }
        }

        public override void Extract(object obj)
        {
            base.Extract(obj);


        }

        #endregion
    }


}


