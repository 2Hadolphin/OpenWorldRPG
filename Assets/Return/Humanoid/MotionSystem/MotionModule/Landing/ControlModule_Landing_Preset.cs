using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
using Return.Humanoid;
using Sirenix.OdinInspector;
using Return.Creature;
using Return.Motions;

public class ControlModule_Landing_Preset : MotionModulePreset_Humanoid
{
    public override Limb[] GetMotionLimbs => new[]
    {
        Limb.Body,
    };



    public override IHumanoidMotionModule Create(GameObject @object)
    {
        return new Landing_MotionModule();

        //return HumanoidMotionModule.Create<Landing_MotionModule>(this,@object);
    }
}
