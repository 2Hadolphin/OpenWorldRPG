using UnityEngine;
using Return;


public abstract class bl_WeaponSwayBase : BaseComponent
{

    /// <summary>
    /// 
    /// </summary>
    /// <param name="multiplier"></param>
    public abstract void SetMotionMultiplier(float multiplier);

    /// <summary>
    /// Reset all the settings to its original value
    /// </summary>
    public abstract void ResetSettings();
}