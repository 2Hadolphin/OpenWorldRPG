using KinematicCharacterController;
using MxM;
using Return.Creature;
using Return.Humanoid;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return.Motions
{
    public class Slide_MotionPreset : MotionModulePreset_Humanoid
    {
        public override Limb[] GetMotionLimbs => new[]
        {
            Limb.RightLeg,Limb.LeftLeg
        };


        public override IHumanoidMotionModule Create(GameObject @object)
        {
            var module = new Slide();
            module.LoadData(this);
            return module;
        }


        [SerializeField]
        MxMEventDefinition m_SlideDefinition;

        public MxMEventDefinition SlideDefinition { get => m_SlideDefinition; set => m_SlideDefinition = value; }


        public float SlideHeight = 0.8f;
        public float SlideRadius = 0.5f;
        [ShowInInspector]
        public string[] SlideTags = new[]
        {
            "Run","Sprint"
        };

    }
}