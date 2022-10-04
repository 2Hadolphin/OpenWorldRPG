using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using System;
using Sirenix.OdinInspector;

namespace Return.Humanoid.Animation
{
    /// <summary>
    /// Basic class for playable behaviour preset
    /// </summary>
    [System.Serializable]
    public abstract class AnimationBehaviourBundle : PresetDatabase
    {
        /// <summary>
        /// Lerp might break humanoid normal rigging
        /// </summary>

        public enum TransitType { Lerp, Reset, }

        [Flags]
        public enum ParameterType { Script,MotionChannel}
        /// <summary>
        /// ??????? IdleState=>, Locomotion=>walk, Operate?, Adjust=>Feet IK 
        /// </summary>
        public enum BehaviourType { Idle, Locomotion, Operate, Adjust,LastType }
        /// <summary>
        /// Inherit => overwrite ,RootTransform=> root Source,Blend=>Blend with other stream
        /// </summary>
        public enum StreamType { Source, Inherit, Blend,LastType }
        public ParameterType _ParameterType;
        public BehaviourType _BehaviourType;
        public StreamType _StreamType;
        public byte Priority;
        public bool Exclusive;
        public AvatarMask HumanoidMask;
        [SerializeField]
        public AnimationBehaviourBundle Dependence;

        public abstract void LoadBehaviour(IAdvanceAnimator animator, out IAnimationBehaviour humanoidBehaviour);
    }


}

