using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return;
using System;
using UnityEngine.InputSystem;
using Return.Humanoid;
using Return.Humanoid.Motion;
using Sirenix.OdinInspector;
using MxM;
using Return.Creature;
using Return.Motions;

public class ControlModule_Turnround_UserInput : MotionModulePreset_Humanoid
{
    public override IHumanoidMotionModule Create(GameObject @object)
    {
        return MotionModule_Humanoid.Create<Turnround>(this);
    }

    public UTagPicker FirstPersonCamera;

    public UTagPicker ThirdPersonCamera;


    #region States



    #region Setter
    //
    //[VirtualState(VirtualValue.Vector3, true)]
    //public StateWrapper ModuleVector;

    //
    //[VirtualState(VirtualValue.Vector3, true)]
    //public StateWrapper ModuleVelocity;

    //[VirtualState(VirtualValue.Quaternion, true)]
    //
    //public StateWrapper ModuleRotation;

    //[VirtualState(VirtualValue.Vector3, true)]
    //public StateWrapper ModuleDirection;

    //[VirtualState(VirtualValue.Quaternion, true)]
    //public StateWrapper ViewPortRotation;

    //[VirtualState(VirtualValue.Vector3, true)]
    //public StateWrapper ViewPortDirection;

    
    //[VirtualState(VirtualValue.Quaternion, true)]
    //public StateWrapper ViewPortPlanarRotation;

    
    //[VirtualState(VirtualValue.Vector3, true)]
    //public StateWrapper ViewPortPlanarDirection;



    #endregion

    #endregion
    [SerializeField]
    public TurnInPlaceProfile[] m_turnInPlaceProfiles = null;

    public override Limb[] GetMotionLimbs => new[]
    {
        RightLeg,LeftLeg
    };

    public Limb RightLeg=Limb.RightLeg;
    public Limb LeftLeg = Limb.LeftLeg;
}
