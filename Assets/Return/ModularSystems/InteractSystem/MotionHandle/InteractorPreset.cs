using Return.Humanoid;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return.Humanoid.Animation;
using UnityEngine.Assertions;
using Return.Creature;
using Return.Motions;

namespace Return
{
    public class InteractorPreset : MotionModulePreset_Humanoid
    {

        public override HashSet<Limb> MotionLimbs { get; protected set; } = new HashSet<Limb>() { Limb.RightHand, Limb.LeftHand };


        public override Limb[] GetMotionLimbs => new[]
        {
            RightHand,LeftHand,RightFinger,LeftFinger
        };

        public Limb LeftHand = Limb.LeftHand;
        public Limb LeftFinger = Limb.LeftFinger;

        public Limb RightHand = Limb.RightHand; 
        public Limb RightFinger = Limb.RightFinger;

        public override IHumanoidMotionModule Create(GameObject @object)
        {
            return MonoMotionModule_Humanoid.Create<Interactor>(this, @object);
        }
    }
}