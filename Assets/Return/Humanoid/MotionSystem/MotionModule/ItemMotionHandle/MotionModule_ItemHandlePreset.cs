using Return.Creature;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Return.Motions;

namespace Return.Humanoid.Motion
{
    /// <summary>
    /// Basic item motion handle which inherit humanoid motion module (**inspect **throw **hit **avoid collosion 
    /// </summary>
    public class MotionModule_ItemHandlePreset : MotionModulePreset_Humanoid
    {
        public override Limb[] GetMotionLimbs => new[]
        {
            RightHand,
            LeftHand,
        };

        [BoxGroup("LimbSequence")]
        public Limb RightHand = Limb.RightHand;
        [BoxGroup("LimbSequence")]
        public Limb LeftHand = Limb.LeftHand;

        public override IHumanoidMotionModule Create(GameObject @object)
        {
            return MonoMotionModule_Humanoid.Create<MotionModule_ItemHandle>(this,@object);
        }
    }
}