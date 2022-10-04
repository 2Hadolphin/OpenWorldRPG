using MxM;
using Return.Creature;
using Return.Motions;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Return.Humanoid
{
    public class ControlModule_Ladder : MotionModulePreset_Humanoid
    {
        public override Limb[] GetMotionLimbs => new[]
        {
            Limb.Body,
            Limb.Chest,
            Limb.RightHand,
            Limb.LeftHand,
            Limb.RightLeg,
            Limb.LeftLeg,
        };

        public override IHumanoidMotionModule Create(GameObject @object)
        {
            return MotionModule_Humanoid.Create<LadderMotion>(this);
        }


        [SerializeField]
        public MxMEventDefinition m_DropDefinition = null;

        public float SlideHeight = 0.8f;
        public float SlideRadius = 0.5f;

        public float LadderClimbSpeed;

        [SerializeField]
        public string m_climbTagName = "Climb";
    }
}