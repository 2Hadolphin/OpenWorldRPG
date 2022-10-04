using UnityEngine;
using Return;
using Return.Items;
using Return.Items.Weapons;
using Return.Modular;

// ** rename as FirearmsAnimationModuleBase

/// <summary>
/// Firearms animation player
/// </summary>
public abstract class BaseFirearmsPerformerPlayer : MonoItemModule
{
    public override ControlMode CycleOption => ControlMode.Register | ControlMode.Activate | ControlMode.Unregister ;

    /// <summary>
    /// Add persistent animation state.
    /// </summary>
    public abstract void AddState(TimelinePerformerHandle preset);

    /// <summary>
    /// Remove performer state.
    /// </summary>
    public abstract void RemoveState(TimelinePreset preset);

    /// <summary>
    /// Play timeline state.
    /// </summary>
    public abstract void PlayState(TimelinePerformerHandle preset, IPerformerHandle endPerformerCallback = null);

    /// <summary>
    /// Handle cancel animations. 
    /// </summary>
    public abstract void CancelState(TimelinePreset preset);


    /// <summary>
    /// Clean playable graph.
    /// </summary>
    protected override void Unregister() { }

}
