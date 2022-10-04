using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using System.Linq;
using Return.Humanoid.IK;

namespace Return.Humanoid.Animation
{
    public class Behaviour_HandIK : HumanoidBehaviour<BehaviourBundle_Humanoid_HandIK>
    {
        public Behaviour_HandIK(BehaviourBundle_Humanoid_HandIK bundle):base(bundle)
        {

        }

        public AnimationScriptPlayable mplayable;

        protected Job_TwoBoneIKPair Job;

        protected Avatar avatar;
        public override float Weight { get => Job.Weight; set => Job.Weight = value; }

        public override Playable GetOutputPort => mplayable;

        public override Playable GetInputPort => mplayable;

        public Dictionary<string, PR> Poses { get; protected set; }

 
        public override void Init(IAdvanceAnimator IAnimator)
        {
            base.Init(IAnimator);



            #region Load Data

            avatar = IAnimator.GetAnimator.avatar;
            var binding = BehaviourBundle_Humanoid_HandIK.HandPose.BindingHandPoseBounce(IAnimator);
            Poses = Bundle.Poses.Select(x=>x.Binding(binding)).ToDictionary(x=>x.Key,x=>x.Value);

            if(Poses.TryGetValue("IdleState",out var pos))
            {

            }

            #endregion
        }

        public virtual void Init(ILimbIKGoalData rightHandData,ILimbIKGoalData leftHandData)
        {

            var graph = Animator.GetGraph;

            #region SetJob
            Job = Job_TwoBoneIKPair.Create();


            var rightHand = Job.Right;
            rightHand.Top = Animator.GetBindingBone(HumanBodyBones.RightUpperArm);
            rightHand.Middle = Animator.GetBindingBone(HumanBodyBones.RightLowerArm);
            rightHand.End = Animator.GetBindingBone(HumanBodyBones.RightHand);
            Job.Right = rightHand;
            Job.RightData = rightHandData;

            var leftHand = Job.Left;
            leftHand.Top = Animator.GetBindingBone(HumanBodyBones.LeftUpperArm);
            leftHand.Middle = Animator.GetBindingBone(HumanBodyBones.LeftLowerArm);
            leftHand.End = Animator.GetBindingBone(HumanBodyBones.LeftHand);
            Job.Left = leftHand;
            Job.LeftData = leftHandData;

            Job.Weight = 0;
            Job.Root = Animator.GetAnimator.BindStreamTransform(Animator.IKGoalRootBone);

            #endregion

            mplayable = AnimationScriptPlayable.Create(graph, Job);
        }

        protected virtual void SetJobSchedule(float p)
        {
            mplayable.SetJobData(Job);
  
        }

    
        /// <summary>
        /// Must invoke this to set m_duringTransit data after any parameter changed  space/ position/ rotation/ weight etc.
        /// </summary>
        public override void UpdateJobData()
        {
            mplayable.SetJobData(Job);
        }
        public override void AddSource(Playable playable)
        {
            mplayable.AddInput(playable, 0, 1);
        }

        public override void Unload(ITimer timer, float time = 0)
        {

        }

        public override void Dispose()
        {
            if (mplayable.CanDestroy())
                mplayable.Destroy();
        }
    }

}
