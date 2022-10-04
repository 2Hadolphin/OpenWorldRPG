using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return.Humanoid.Motion;
using Return.Agents;
using Sirenix.OdinInspector;
using System;

namespace Return.Items
{
    /// <summary>
    /// Preset to set adjust data and animation about **Equip **Dequip **hit
    /// </summary>
    public class ItemPostureHandlerPreset : ConfigurableItemModulePreset
    {
        [PropertyTooltip("Provide motion system config **Limb sequence **Favour/Require Tags.")]
        public MotionModule_ItemHandlePreset MotionModuleItemHandler;


        [SerializeField]
        string m_ItemHierarchyName="m_items";
        public string HierarchyName { get => m_ItemHierarchyName; set => m_ItemHierarchyName = value; }


        public UTags IdleID;
        public ItemPerformerPreset Idle;

        public UTags EquipID;
        public ItemPerformerPreset Equip;

        public UTags UnequipID;
        public ItemPerformerPreset Unequip;

        /// <summary>
        /// under porformer?
        /// </summary>
        [TabGroup("Posture Adjust Preset")]
        [Obsolete]
        public ItemPostureAdjustData PoseAdjustData;


        public override IConfigurableItemModule LoadModule(GameObject @object, IAgent agent = null)
        {
            return InstanceModule<ItemPostureHandlerModule>(@object);
        }

    }


}