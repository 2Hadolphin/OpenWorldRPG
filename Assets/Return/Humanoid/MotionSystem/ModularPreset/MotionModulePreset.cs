using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return.Humanoid.Modular;
using Sirenix.OdinInspector;
using Return.Humanoid.Controller;


namespace Return.Humanoid.Motion
{
    /// <summary>
    /// 
    /// </summary>
    public class MotionSystemPreset : BaseModularPreset
    {
        public override void LoadModule(GameObject @object)
        {

        }


        #region Modules

        [ListDrawerSettings(Expanded =true)]
        [SerializeField]
        [ShowInInspector]
        HashSet<MotionModulePreset> m_MotionModules = new ();

        public HashSet<MotionModulePreset> MotionModules { get => m_MotionModules; set => m_MotionModules = value; }

        #endregion


        #region MxM Config

        //[SerializeField, Range(0f, 1f)]
        //float m_VelocityBlendRatio;
        //[SerializeField, Range(0f, 1f)]
        //float m_RotationBlendRatio;
        //[SerializeField, Range(0f, 1f)]
        //float m_PositionBias=1f;
        //[SerializeField, Range(0f, 1f)]
        //float m_RotationBias=1f;

        //[VirtualState(VirtualValue.Float)]
        //public StateWrapper VelocityBlendRatio;

        //[VirtualState(VirtualValue.Float, true)]
        //public StateWrapper RotationBlendRatio;

        //[VirtualState(VirtualValue.Float,true)]
        //public StateWrapper PositionBias;

        //[VirtualState(VirtualValue.Float)]
        //public StateWrapper RotationBias;

        #endregion


        #region Require Motion State


        //[VirtualState(VirtualValue.Trigger,true)]
        //public StateWrapper EnableMotion;

        #region ViewPort

        //[VirtualState(VirtualValue.Vector3, true)]
        //[BoxGroup(ViewPort)]
        //public StateWrapper ViewDirection;

        //[VirtualState(VirtualValue.Quaternion, true)]
        //[BoxGroup(ViewPort)]
        //public StateWrapper ViewRotation;

        //[VirtualState(VirtualValue.Vector3, true)]
        //public StateWrapper ViewPortPlanarDirection;

        //[VirtualState(VirtualValue.Quaternion, true)]
        //[BoxGroup(ViewPort)]
        //public StateWrapper ViewPortPlanarRotation;

        #endregion

        #region Agent

        //[VirtualState(VirtualValue.Vector3, true)]
        //[BoxGroup(AgentState)]
        //public StateWrapper Gravity;

        //[VirtualState(VirtualValue.Float, true)]
        //[BoxGroup(AgentState)]
        //public StateWrapper Energy;

        //
        //[VirtualState(VirtualValue.Bool)]
        //public StateWrapper InputUpdate;


        //
        //[VirtualState(VirtualValue.Bool,true)]
        //public StateWrapper MotionUpdate;

        #endregion

        #region Module

        //[VirtualState(VirtualValue.Vector3)]
        //public StateWrapper ModuleVector;

        //[VirtualState(VirtualValue.Vector3)]
        //public StateWrapper ModuleVelocity;

        //[VirtualState(VirtualValue.Quaternion)]
        //public StateWrapper ModuleRotation;


        #endregion




        #region MxM

        //[VirtualState(VirtualValue.Vector3, true)]
        //
        //public StateWrapper AnimationDeltaPosition;

        //[VirtualState(VirtualValue.Quaternion, true)]
        //
        //public StateWrapper AnimationDeltaRotation;

        #endregion


        //[VirtualState(VirtualValue.Float,  true)]
        //
        //public StateWrapper MaxStableMoveSpeed;



        #endregion

        #region KCC
        //public const string KCCPost = "KCC";



        //[VirtualState(VirtualValue.Generic,  true)]
        //[BoxGroup(KCCPost)]
        //public StateWrapper GroundingStatus;

        //[VirtualState(VirtualValue.Trigger, true)]
        //[BoxGroup(KCCPost)]
        //public StateWrapper GroundedPost;

        //[VirtualState(VirtualValue.Vector3, true)]
        //[BoxGroup(KCCPost)]
        //public StateWrapper CharacterUp;

        //[VirtualState(VirtualValue.Vector3, true)]
        //
        //public StateWrapper MotorVelocity;

        //[VirtualState(VirtualValue.Quaternion, true)]
        //
        //public StateWrapper MotorRotation;

        #endregion
    }
}