using Return.Humanoid;
using UnityEngine.InputSystem;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Playables;
using System;
using Return.Creature;
using Return.Motions;

/// <summary>
/// Provide layer mixer and apply mxm tag
/// </summary>
[Obsolete]
public class MotionModule_FirearmsHandle : Component//MonoMotionModule_Humanoid
{



    //public override MotionModulePreset_Humanoid GetData => pref;

    protected MotionModule_FirearmsHandle_Detail pref;

    //protected override void LoadData(MotionModulePreset data)
    //{
    //    data.Parse(out pref);
    //}

    //public override void SetHandler(IMotionSystem motionSystem)
    //{
    //    base.SetHandler(motionSystem);

    //    var graph = Motions.GetGraph;

    //    graph.CreateBehaviour(ref FirearmsPlayable);

    //    MixerID=Motions.mxm.AddLayer(FirearmsPlayable,1,false,pref.AnimationMask);

    //}

    /// <summary>
    /// ?? debug
    /// </summary>
    [Button]
    [Obsolete]
    public virtual void ExecuteAnim(AnimationClip clip,float speed=1f,Action callcback=null)
    {
        FirearmsPlayable.GetBehaviour().PlayClip(clip, speed);
    }

    #region Playables


    int MixerID=-1;
    ScriptPlayable<FirearmsPlayableBehaviour> FirearmsPlayable;

    #endregion

    bool FirearmsMode;

    private void OnDestroy()
    {
        //if (MixerID > 0)
        //    Motions.mxm.RemoveLayer(MixerID);
    }
}