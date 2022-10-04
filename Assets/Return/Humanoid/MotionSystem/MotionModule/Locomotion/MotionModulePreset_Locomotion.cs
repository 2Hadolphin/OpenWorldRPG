using Return.Humanoid.Animation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Return;
using Return.Humanoid.Motion;
using MxM;
using Sirenix.OdinInspector;
using Return.Creature;
using Return.Motions;

namespace Return.Humanoid
{

    /// <summary>
    /// Locomotion module preset
    /// </summary>
    public class MotionModulePreset_Locomotion : MotionModulePreset_Humanoid
    {
        public override IHumanoidMotionModule Create(GameObject @object)
        {
            return MotionModule_Humanoid.Create<Locomotion>(this);

            //return MonoMotionModule_Humanoid.Create<Locomotion>(this,@object);
        }

        #region Config

        public UTagPicker FirstPersonCamera;
        public UTagPicker ThirdPersonCamera;

        [Tooltip("First person movement requied respond immediately.")]
        public float VelocityBlendRatio_FirstPerson=0;

        [Tooltip("Third person movement requied motion lerp.")]
        public float VelocityBlendRatio_ThirdPerson=1;

        public float BlendRatio_AngularVelocity;

        public float WalkSpeed=8f;
        public float SprintSpeed=15f;
        public float RunSpeed=12f;

        public float CrouchHeight = 1.35f;

        [Header("Inputs Profiles")]
        [SerializeField]
        public MxMInputProfile m_generalLocomotion = null;

        [SerializeField]
        public MxMInputProfile m_strafeLocomotion = null;

        [SerializeField]
        public MxMInputProfile m_sprintLocomotion = null;


        [SerializeField]
        string m_idleTagName = "IdleState";
        [SerializeField]
        string m_walkSpeedTagName = "Walk";
        [SerializeField] 
        string m_runSpeedTagName = "Run";
        [SerializeField]
        string m_sprintSpeedTagName = "Sprint";

        [ShowInInspector]
        float m_favourMultiplier = 0.6f;

        [SerializeField]
        Limb m_rightLeg = Limb.RightLeg;

        [SerializeField]
        Limb m_leftLeg = Limb.LeftLeg;




        public float FavourMultiplier { get => m_favourMultiplier; set => m_favourMultiplier = value; }
        public string IdleTagName { get => m_idleTagName; set => m_idleTagName = value; }
        public string WalkSpeedTagName { get => m_walkSpeedTagName; set => m_walkSpeedTagName = value; }
        public string RunSpeedTagName { get => m_runSpeedTagName; set => m_runSpeedTagName = value; }
        public string SprintSpeedTagName { get => m_sprintSpeedTagName; set => m_sprintSpeedTagName = value; }
        public Limb RightLeg { get => m_rightLeg; set => m_rightLeg = value; }
        public Limb LeftLeg { get => m_leftLeg; set => m_leftLeg = value; }

        public override Limb[] GetMotionLimbs => new Limb[]
        {
            RightLeg,
            LeftLeg,
        };

        #endregion

    }
}