using Return;
using Return.Audios;
using Return.Items.Weapons;
using System;
using UnityEngine.Timeline;
using UnityEngine;

public interface IPerformerHandle
{
    /// <summary>
    /// This function will be invoke while performed cancel this playable
    /// </summary>
    public void Cancel(TimelinePreset preset);

    /// <summary>
    /// This function will be invoke while performed cancel this playable
    /// </summary>
    public void Finish(TimelinePreset preset);
}

public interface IItemPerformerHandle : IPerformerHandle//:IComparable<IItemPerformerHandle>//, IStateCallbackReceiver
{
    //public event Action<TimelinePerformerHandle> OnPerformerPlay;

}

public interface IItemPreloadPerformerHandle : IPerformerHandle //: IItemPerformerHandle
{
    /// <summary>
    /// RegisterHandler all usual performers
    /// </summary>
    public TimelinePreset[] LoadPerformers();
}

/// <summary>
/// Provide timeline preset and on-play event
/// </summary>
public interface IFirearmsPerformerHandle: IItemPreloadPerformerHandle
{

}

/// <summary>
/// Provide persistent timeline preset and on-play event
/// </summary>
public interface IFirearmsPersistentPerformerHandle : IFirearmsPerformerHandle
{

}

public interface IPerformerHandler:IModular
{
    /// <summary>
    /// Add persistent performer state.
    /// </summary>
    void AddState(TimelinePerformerHandle handle);

    /// <summary>
    /// Remove persistent performer state.
    /// </summary>
    void RemoveState(TimelinePreset handle);

    /// <summary>
    /// Set persisten state **IdleMotion **Aiming
    /// </summary>
    void SetIdleState(TimelinePerformerHandle animPreset);

    /// <summary>
    ///  Play state.
    /// </summary>
    /// <param name="handle">State config</param>
    /// <param name="endPerformerCallback">State callback</param>
    void PlayState(TimelinePerformerHandle handle=default, IPerformerHandle endPerformerCallback = null);
    
    /// <summary>
    /// CheckAdd state order.
    /// </summary>
    bool CanPlay(Token token = null);

    /// <summary>
    /// State event callback.
    /// </summary>
    event Action<Marker> OnMarkerPost;
}

public interface IFireamrsPerformerHandler: IPerformerHandler
{
    //event Action OnIdle;

    /// <summary>
    /// Wait performer
    /// </summary>
    void QueuePerformer(TimelinePerformerHandle handle);

    /// <summary>
    /// Firing
    /// </summary>
    void PlayAdditive(TimelinePreset clip, double duration, WrapMode wrapMode = WrapMode.Once);

    /// <summary>
    /// Aiming
    /// </summary>
    void PlayAdditive(TimelinePerformerHandle handle);

    void StopAdditive(TimelinePreset preset);
}

public interface IFirearmsAudioHandle
{
    public event Action<BundleSelecter> OnAudioPlay;

}

/// <summary>
/// ??????
/// </summary>
public struct TimelinePerformerHandle
{
    /// <summary>
    ///  delete ?
    /// </summary>
    [Obsolete]
    public bool cancel;

    public TimelinePreset Preset;
    public double Speed;

    /// <summary>
    /// Time to blend state.
    /// </summary>
    public float blendTime;

    /// <summary>
    /// **Loop **Clamp forever **Loop 
    /// </summary>
    public WrapMode WrapMode;

    public static implicit operator TimelinePerformerHandle(TimelinePreset preset)
    {
        return new TimelinePerformerHandle() { Preset = preset, Speed = 1, blendTime=0.17f, WrapMode = WrapMode.Once };
    }
}


public static class PerformerExtension
{
    public static PerformerConfig GetConfig(this TimelinePreset preset)
    {
        var config = PerformerConfig.config;

        config.handle = preset;

        return config;
    }

    public class PerformerConfig
    {
        public static readonly PerformerConfig config=new();

        public TimelinePerformerHandle handle;

        public static implicit operator TimelinePerformerHandle(PerformerConfig config)
        {
            return config.handle;
        }

        public static implicit operator PerformerConfig(TimelinePreset preset)
        {
            return preset.GetConfig();
        }
    }

    public static PerformerConfig Cancel(this PerformerConfig config)
    {
        var handle = config.handle;

        handle.cancel = true;
        config.handle = handle;

        return config;
    }

    public static PerformerConfig SetWrapMode(this PerformerConfig config,WrapMode mode=WrapMode.ClampForever)
    {
        var handle = config.handle;
        
        handle.WrapMode = mode;
        config.handle = handle;
        return config;
    }

    public static PerformerConfig SetSpeed(this PerformerConfig config, double speed)
    {
        config.handle.Speed = speed;
        return config;
    }

    public static PerformerConfig SetDuration(this PerformerConfig config, float duration)
    {
        var handle = config.handle;

        duration = duration.Max(0.05f);
        Debug.Log(handle.Preset.GetAsset().duration + " : " + duration);
        handle.Speed = handle.Preset.GetAsset().duration / duration;
        Debug.LogError(handle.Speed);

        config.handle = handle;

        return config;
    }


}