using UnityEngine;
using Return;
using Return.Humanoid;
using System;
using Sirenix.OdinInspector;
using Return.Motions;

[Serializable]
public class Landing_MotionModule : MonoMotionModule_Humanoid
{
    public MotionModulePreset_Humanoid Data { get; protected set; }

    public override MotionModulePreset_Humanoid GetData => Data;

    protected override void LoadData(MotionModulePreset data)
    {
        Data = data as ControlModule_Landing_Preset;
    }




    #region Parameter

    public float Gravity=9.8f;
    public float minDown=-0.1f;
    public float Lift;
    #endregion

    #region Catch
    public float Inertia;
    #endregion





    public override int CompareTo(IHumanoidMotionModule other)
    {
        return -2;
    }
}
