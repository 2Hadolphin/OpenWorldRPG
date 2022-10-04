using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return;
using System;
using Return.Humanoid;
using Return.Humanoid.Animation;
using Return.Humanoid.Controller;
using MxMGameplay;
using Sirenix.OdinInspector;
using MxM;
using Return.Creature;

namespace Return.Motions
{
    public class Jump_MotionPreset : MotionModulePreset_Humanoid
    {

        public override IHumanoidMotionModule Create(GameObject obj)
        {
            var jumping = new Jumping();
            jumping.LoadData(this);
            return jumping;
            //MotionModule_Humanoid.Create<Jumping>(this);
        }

        #region States

        #region Getter

        [SerializeField]
        [Range(0,20)]
        float m_MaxStableMoveSpeed =10;

        public float MaxStableMoveSpeed { get => m_MaxStableMoveSpeed; set => m_MaxStableMoveSpeed = value; }

        #endregion


        #endregion

        [SerializeField]
        MxMEventDefinition m_jumpDefinition;

        public float JumpDistance = 4.5f;
        public float JumpHeight = 0.7f;

        [SerializeField]
        VaultDefinition[] m_vaultDefinitions;

        [SerializeField]
        VaultDetectionConfig[] m_vaultConfigurations;

        [SerializeField]
        float m_minStepUpDepth = 1f;

        //[SerializeField]
        //private float m_minStepOffDepth = 1f;

        [SerializeField]
        LayerMask m_layerMask;

        [SerializeField]
        float m_minAdvance = 0.1f; //The minimum advance required to trigger a vault.

        [SerializeField]
        float m_advanceSmoothing = 10f;

        [SerializeField]
        float m_maxApproachAngle = 60f;



        public override Limb[] GetMotionLimbs => new[]
        {
            RightHand,
            LeftHand,
            RightLeg,
            LeftLeg
         };

        public MxMEventDefinition JumpDefinition { get => m_jumpDefinition; set => m_jumpDefinition = value; }
        public VaultDefinition[] VaultDefinitions { get => m_vaultDefinitions; set => m_vaultDefinitions = value; }
        public VaultDetectionConfig[] VaultConfigurations { get => m_vaultConfigurations; set => m_vaultConfigurations = value; }
        public float MinStepUpDepth { get => m_minStepUpDepth; set => m_minStepUpDepth = value; }
        public LayerMask LayerMask { get => m_layerMask; set => m_layerMask = value; }
        public float MinAdvance { get => m_minAdvance; set => m_minAdvance = value; }
        public float AdvanceSmoothing { get => m_advanceSmoothing; set => m_advanceSmoothing = value; }
        public float MaxApproachAngle { get => m_maxApproachAngle; set => m_maxApproachAngle = value; }

        public Limb RightHand = Limb.RightHand;
        public Limb LeftHand = Limb.LeftHand;
        public Limb RightLeg = Limb.RightLeg;
        public Limb LeftLeg = Limb.LeftLeg;

    }
}