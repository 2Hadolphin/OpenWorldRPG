using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Return.Items;
using UnityEngine.Assertions;
using Return.Gear;
using System;

namespace Return.Humanoid.Character
{
    /// <summary>
    /// Humanoid body slot on human bone **Hands **Shoes **Ring **Hat **
    /// </summary>
    public class BodySlotBinding : HumanSlotBinding/*,IEquatable<BodySlotBinding>*/
    {
        public override HashSet<SlotType> EnableSlotType => EnableSlots;

        public HumanBodyBones TargetBone;

        [SerializeField]
        [HideLabel]
        PR m_Coordinate;

        public PR Coordinate => m_Coordinate;


#if UNITY_EDITOR
        // Editor setting
        Transform Slot;
        [ShowInInspector]
        public Transform SetSlot
        {
            get => Slot;
            set
            {
                if (value)
                    m_Coordinate = value.GetLocalPR();
                Slot = value;
            }
        }
#endif

        [ShowInInspector]
        [ListDrawerSettings(Expanded = true)]
        [HideLabel]
        public HashSet<SlotType> EnableSlots = new ();

        [Button("CreateSlotBinding")]
        public override EquipmentSlot CreateSlot(Animator animator)
        {
            Assert.IsNotNull(animator);
            var bone = animator.GetBoneTransform(TargetBone)?.gameObject;
            Assert.IsNotNull(bone);

            var slot = bone.InstanceIfNull<EquipmentSlot>();

            if (slot.Binding && !slot.Equals(this))
                slot = bone.AddComponent<EquipmentSlot>();

            slot.Init(this);
            slot.transform.SetLocalPR(Coordinate);
            return slot;
        }



        //public bool Equals(BodySlotBinding other)
        //{
        //    return Title == other.Title && TargetBone == other.TargetBone;
        //}
    }
}