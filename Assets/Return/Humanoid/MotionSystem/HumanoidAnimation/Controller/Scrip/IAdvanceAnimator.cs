using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Unity.Collections;
using UnityEngine.Animations;
using System;

namespace Return.Humanoid.Animation
{
    /// <summary>
    /// ?????? custom anim graph
    /// </summary>
    public interface IAdvanceAnimator
    {
        Animator GetAnimator { get; }
        PlayableGraph GetGraph { get; }
        FreeFormBlendNode CreateBlendNode();

        public NativeArray<TransformStreamHandle> MirrorBones { get; }
        public event Action<Vector2> Locomotion_Level;
        public event Action<float> JumpSchedule;
        public event Action<Vector3> RootScale;
        public event Action<float> MovementVelocity;
        public HumanBone GetHumanBone(HumanBodyBones humanBodyBone);
        public SkeletonBone GetSkeletonBone(HumanBodyBones humanBodyBone);
        public float GetSkeketonPathDistance(HumanBodyBones parent, HumanBodyBones target, Dictionary<Transform, SkeletonBone> map = null);
        void SetBehaviour(IAnimationBehaviour behaviour, bool enable);
        IMotionChannel MotionChannel { get; }
        Transform RootBone { get; }
        Transform IKGoalRootBone { get; }
        Vector3 ToLoacl(Vector3 vector);
        public TransformStreamHandle GetBindingBone(HumanBodyBones humanBodyBone);
        Dictionary<Transform, HumanBodyBones> TransformMap { get; }
        Dictionary<Transform, SkeletonBone> SkeletonMap { get; }

        void AddBehaviourBundle(AnimationBehaviourBundle bundle, out IAnimationBehaviour behaviour);
    }

}
