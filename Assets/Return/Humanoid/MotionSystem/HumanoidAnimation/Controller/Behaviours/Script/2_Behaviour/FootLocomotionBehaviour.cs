using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using System;
namespace Return.Humanoid.Animation
{
    public class FootLocomotionBehaviour : HumanoidBehaviour<BehaviourBundle_Humanoid_Foot_Locomotion>
    {
        public FootLocomotionBehaviour(AnimationBehaviourBundle bundleSource) : base(bundleSource)
        {
 
        }

        //public NativeArray<TransformStreamHandle> UnityHandles;
        public AnimationMixerPlayable MixerPlayable;
        public Vector2 MotionDirection;
        public AnimationLibrary_Vector3 library;
        protected FreeFormBlendNode BlendTree;

        
        public override void Init(IAdvanceAnimator IAnimator)
        {
            base.Init(IAnimator);

            var animator = IAnimator.GetAnimator;
            var graph = IAnimator.GetGraph;
            MixerPlayable = AnimationMixerPlayable.Create(graph);



            // subscript animtion paramater

            /*
            #region BindBone
            var bones = GetLegBones;
            var num = bones.Length;

            UnityHandles = new NativeArray<TransformStreamHandle>(num, Allocator.Persistent, NativeArrayOptions.ClearMemory);

            for (int i = 0; i < num; i++)
            {
                try
                {
                    UnityHandles[i] = anim.BindStreamTransform(anim.GetBoneTransform(bones[i]));
                }
                catch (System.Exception e)
                {
                    Debug.LogError("anim binding can't find transform of" + bones[i].ToString() + e);
                    //slove new or find
                    throw;
                }
            }
            #endregion
            */



            #region SetJob

            BlendTree = IAnimator.CreateBlendNode();
    
            var playables = BlendTree.Init(library, IAnimator);

            foreach (var playable in playables)
            {
                MixerPlayable.AddInput(playable, 0, 0f);
            }
            
  


            MixerPlayable.SetOutputCount(1);

            #endregion

            #region Subscript
            BlendTree.WeightPost += UpdateClipWeight;
            IAnimator.MovementVelocity += OverSpeed;
            IAnimator.MotionChannel.Update += UpdateBlendTreeParameter;
            #endregion

            
        }


        protected void OverSpeed(float velocity)
        {
            var blendSpeed=BlendTree.GetNodeVector().magnitude;

            if (velocity > blendSpeed)
                velocity /= blendSpeed;

            MixerPlayable.SetSpeed(velocity);
        }
        protected void UpdateBlendTreeParameter()
        {
            var motionData=Animator.MotionChannel.ChannelPoints;
            var frameData=motionData[Animator.MotionChannel.Key_Current];
            var parameter= Animator.ToLoacl(frameData.Velocity).Z2Y();
            BlendTree.InputParameter = parameter;
        }
        protected void UpdateClipWeight(float[] weights)//cal overSpeed
        {
            var length = weights.Length;
            for (int i = 0; i < length; i++)
            {
                 MixerPlayable.SetInputWeight(i, weights[i]);
            }
        }


        public override void AddSource(Playable playable)
        {
            Debug.LogError(this + " should be first priority behaviour  ");
            return;
        }

        public override void Unload(ITimer timer, float time = 0)
        {
            throw new System.NotImplementedException();
        }
        public override void UpdateJobData()
        {

        }
        public override void Dispose()
        {

        }

        public static HumanBodyBones[] GetLegBones = new HumanBodyBones[]
        {
        HumanBodyBones.Hips,
        HumanBodyBones.LeftUpperLeg,
        HumanBodyBones.LeftLowerLeg,
        HumanBodyBones.LeftFoot,
        HumanBodyBones.LeftToes,
        HumanBodyBones.RightUpperLeg,
        HumanBodyBones.RightLowerLeg,
        HumanBodyBones.RightFoot,
        HumanBodyBones.RightToes,
        };


        public override float Weight { get; set; }

        public override Playable GetOutputPort => MixerPlayable;

        public override Playable GetInputPort => MixerPlayable;
    }


}
