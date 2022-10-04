using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Return.Items;
using Return.Gear;

namespace Return.Humanoid.Character
{
    public interface ISlot
    {
        HashSet<SlotType> EnableSlotType { get; }
        //HumanBodyBones TargetHumanBone { get; }
        //PR SlotOffset { get; }
    }

    /// <summary>
    /// Base slot with type limit.
    /// </summary>
    public abstract class BaseSlotBinding : PresetDatabase, ISlot
    {
        public abstract HashSet<SlotType> EnableSlotType { get; }
    }

    /// <summary>
    /// Humanoid slot with type limit.
    /// </summary>
    public abstract class HumanSlotBinding : BaseSlotBinding
    {
        public abstract EquipmentSlot CreateSlot(Animator animator);
    }
}