using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return;
using Sirenix.OdinInspector;
using Return.Creature;
using MxM;
using System;
using Return.Humanoid.Motion;
using Return.Motions;

[Obsolete]
public class MotionModule_FirearmsHandle_Detail : MotionModule_FirearmsHandlePreset
{
    public override IHumanoidMotionModule Create(GameObject @object)
    {
        return null;
        //return MonoMotionModule_Humanoid.Create<MotionModule_FirearmsHandle>(this, @object);
    }

    #region Config

    [Header("Inputs Profiles")]
    [SerializeField]
    public MxMInputProfile m_generalLocomotion = null;

    [SerializeField]
    public MxMInputProfile m_animmingLocomotion = null;

    [SerializeField]
    public string m_WeaponTagName = "Firearms";

    [ShowInInspector]
    public float m_favourMultiplier { get; protected set; } = 1.5f;

    public override Limb[] GetMotionLimbs => new Limb[]
    {
            RightHand,
            LeftHand,
            RightFinger,
            LeftFinger,
    };

    #endregion
}
