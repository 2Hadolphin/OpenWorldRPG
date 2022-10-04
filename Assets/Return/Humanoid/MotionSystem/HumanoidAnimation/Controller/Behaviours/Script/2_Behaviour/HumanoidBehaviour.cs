using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using System;
using UnityEngine.Jobs;
namespace Return.Humanoid.Animation
{
    /// <summary>
    /// Basic class for instance playable behaviour while run time
    /// </summary>
    public abstract class HumanoidBehaviour<T>:IDisposable, IAnimationBehaviour where T: AnimationBehaviourBundle
    {
        public HumanoidBehaviour(AnimationBehaviourBundle bundleSource)
        {
            Bundle = bundleSource as T;
        }
        protected IAdvanceAnimator Animator;
        public abstract float Weight { get; set; }
        public AvatarMask Mask { get; set; }
        /// <summary>
        ///  last playable in this behaviour
        /// </summary>
        public abstract Playable GetOutputPort { get; }
        public abstract Playable GetInputPort { get; }
        public uint Port;
        public readonly T Bundle;
        public abstract void UpdateJobData();
        public abstract void AddSource(Playable playable);

        public virtual void Listener(object sender, AnimationArg e) { }

        public abstract void Dispose();

        public virtual void Init(IAdvanceAnimator IAnimator)
        {
            Animator = IAnimator;
        }

        public void Enable(bool value)
        {
            Animator.SetBehaviour(this, value);
        }


        public abstract void Unload(ITimer timer, float time = 0f);

        AnimationBehaviourBundle IAnimationBehaviour.Bundle => Bundle;
        uint IAnimationBehaviour.Port { get => Port; set => Port=value; }

    }
    public interface AnimationHandler
    {
        event EventHandler<AnimationArg> ParameterHandler;
    }

    public interface IAnimationBehaviour
    {
        AnimationBehaviourBundle Bundle { get; }
        Playable GetOutputPort { get; }
        Playable GetInputPort { get; }
        uint Port { get; set; }
        AvatarMask Mask { get; set; }
        void AddSource(Playable playable);
        void Dispose();
        void Enable(bool value);

        void Listener(object sender, AnimationArg e);
    }

    public class AnimationArg : EventArgs
    {
        public AnimationArg(string key, ValueType valueType)
        {
            Key = key;
            Type = valueType;
        }
        public enum ValueType { Integer,Float,Vector3,}
        public readonly string Key;
        public readonly ValueType Type;

        public int value_Integer;
        public float value_Float;
        public Vector3 value_Vector3;

    }

}
