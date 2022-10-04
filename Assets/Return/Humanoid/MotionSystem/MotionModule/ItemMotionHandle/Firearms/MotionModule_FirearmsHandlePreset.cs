using Return.Creature;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Return.Motions;

namespace Return.Humanoid.Motion
{
    public class MotionModule_FirearmsHandlePreset : MotionModule_ItemHandlePreset
    {
        public override Limb[] GetMotionLimbs => new[]
        {
            RightHand,LeftHand,RightFinger,LeftFinger
        };

        [BoxGroup("LimbSequence")]
        public Limb RightFinger = Limb.RightFinger;

        [BoxGroup("LimbSequence")]
        public Limb LeftFinger = Limb.LeftFinger;

        public override IHumanoidMotionModule Create(GameObject @object)
        {
            return MonoMotionModule_Humanoid.Create<FirearmsHandler>(this,@object);
        }

        [Tooltip("Favour tag for motion system.")]
        [BoxGroup("MxM")]
        public UTags FavourTags;
    }
}