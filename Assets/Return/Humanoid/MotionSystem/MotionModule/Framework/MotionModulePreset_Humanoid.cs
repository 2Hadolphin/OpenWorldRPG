using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using Return.Humanoid.Motion;
using Return.Creature;

namespace Return.Humanoid
{
    [Serializable]
    public abstract class MotionModulePreset_Humanoid : MotionModulePreset
    {

        [ShowInInspector]
        public virtual HashSet<Limb> MotionLimbs { get; protected set; } = new HashSet<Limb>();

        [HideInInspector]
        public abstract Limb[] GetMotionLimbs { get; }

        public AvatarMask AnimationMask;

    }
}

