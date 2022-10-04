using System;
using UnityEngine;
using System.Collections;
using Return.Items;
using UnityEngine.Playables;
using UnityEngine.Animations;

using Return;

/// <summary>
/// MFPS default script to handle the FPWeapon animations.
/// You can use your own script to handle the animations if you inherit your script from bl_WeaponAnimationBase.cs
/// </summary>
public class bl_WeaponAnimation : MonoItemModule
{
    public AnimationScriptPlayable TopPlayer;




    #region Public members

    public FireBlendMethod fireBlendMethod = FireBlendMethod.FireSpeed;

    #endregion

  
  


    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public void PlayParticle(int id)
    {
        //ParticleSystem.EmissionModule m = Particles[id].emission;
        //m.rateOverTime = ParticleRate;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public void StopParticle(int id)
    {
        //ParticleSystem.EmissionModule m = Particles[id].emission;
        //m.rateOverTime = 0;
    }


    private Animator _Animator;
    private Animator animator
    {
        get
        {
            if (!_Animator)
                InstanceIfNull(ref _Animator);

            return _Animator;
        }
    }

    private Animation _Anim;
    private Animation Anim
    {
        get
        {
            if(!_Anim)
                InstanceIfNull(ref _Anim);

            return _Anim;
        }
    }
}



[Serializable]
public enum AnimationType
{
    Animation,
    Animator,
}


public enum FireBlendMethod
{
    FireRate,
    FireSpeed,
    FireRateCrossFade,
    FireSpeedCrossFade
}
