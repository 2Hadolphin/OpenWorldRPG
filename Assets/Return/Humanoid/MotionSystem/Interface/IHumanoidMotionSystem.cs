using UnityEngine;
using MxM;
using Return.Creature;
using UnityEngine.Playables;
using Return.Framework.Parameter;
using Return.Modular;
using Return.Humanoid;
using System;

namespace Return.Motions
{


    public interface IHumanoidMotionSystem : IMotionSystem, IHumanoidAnimator, IHumanoidController
    {
        event Action<float> UpdatePost;
        event Action<float> OnFixedUpdate;

        /// <summary>
        /// Whether the motion system is driven by the local player.
        /// </summary>
        bool IsLocalUser { get; }

        #region Net
        TNet.ITNO tno { get; }

        /// <summary>
        /// Whether the motion system control by remote device.
        /// </summary>
        Parameters<bool> isMine { get; }
        #endregion

        LimbSequence this[Limb limb] { get; }

        LimbIKWrapper GetIK(int hash, Limb limb);

        MxMAnimator mxm { get; }

        IModularResolver Resolver { get; }

        bool TryGetModule<T>(out T module) where T : IHumanoidMotionModule;

        /// <summary>
        /// Add module from preset and return in require type.
        /// </summary>
        bool InstallModule<T>(MotionModulePreset motionModuleData, out T module) where T : IMotionModule;

        void UnistallModule(IHumanoidMotionModule module);

        /// <summary>
        /// SetHandler module.
        /// </summary>
        /// <returns>Return qulify of module activation.</returns>
        //bool InstallModule(IHumanoidMotionModule module);

        /// <summary>
        /// 
        /// </summary>
        void AddAnimationLayer(Playable animPlayable, AvatarMask mask, float blendTime = 0.2f);

        /// <summary>
        /// 
        /// </summary>
        void RemoveAnimationLayer(Playable playable, float blendTime = 0.2f);

    }
}
