using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;

namespace Return.Items
{
    /// <summary>
    /// m_items data with modules
    /// </summary>
    public abstract class ItemPreset : ItemInfo, IValue<ItemInfo>
    {
        ItemInfo IValue<ItemInfo>.GetValue() => this;

        [Button("Create Showcase")]
        public virtual ItemShowcase CreateShowCase(Transform tf=null)
        {
            var prefab = Model;
            var ob=tf? Instantiate(prefab, tf):Instantiate(prefab);
            var showcase = ob.AddComponent<ItemShowcase>();
            showcase.LoadInfo(this);
            return showcase;
        }

        [Button("Create m_items")]
        public virtual AbstractItem CreateItem(Transform tf = null)
        {
            return CreateItem<AbstractItem>(tf); 
        }

        /// <summary>
        /// Using generic type to create item handler. **preset cache **modular handler **parameter blackboard
        /// </summary>
        protected virtual AbstractItem CreateItem<T>(Transform parent = null) where T: AbstractItem
        {
            // get prefab
            var prefab = Prefab != null ? Prefab:Model;

            // instantiate as child or root
            var go = parent != null ? Instantiate(prefab, parent) : Instantiate(prefab);
            
            // get handler
            var item = go.InstanceIfNull<T>();

            // bind preset
            item.Preset=this;

            return item;
        }



        [Tooltip("Common modules for each character.")]
        [SerializeField]
        ModularConfigs m_NormalPlayerConfig;
        public ModularConfigs UniversalConfig { get => m_NormalPlayerConfig; set => m_NormalPlayerConfig = value; }


        [Tooltip("Special modules for host player.")]
        [SerializeField]
        ModularConfigs m_HostPlayerConfig;
        public virtual ModularConfigs LocalUserConfig { get => m_HostPlayerConfig; set => m_HostPlayerConfig = value; }

    }
}