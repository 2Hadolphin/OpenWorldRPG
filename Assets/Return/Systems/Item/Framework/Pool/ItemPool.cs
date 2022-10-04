using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return.Items
{
    public class ItemPool : SingletonMonoManager<ItemPool>
    {
        [SerializeField]
        List<AbstractItem> m_items;

        [SerializeField]
        List<ItemShowcase> m_showcases;

        public List<AbstractItem> Items { get => m_items; set => m_items = value; }
        public List<ItemShowcase> Showcases { get => m_showcases; set => m_showcases = value; }

        public override void LoadInitilization()
        {
            
        }

        public virtual void Return(IPickup pickup)
        {
            

        }



    }
}