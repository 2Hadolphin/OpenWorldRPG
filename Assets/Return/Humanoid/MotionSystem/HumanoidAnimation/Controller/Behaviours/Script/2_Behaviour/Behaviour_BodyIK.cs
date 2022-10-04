using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace Return.Humanoid.Animation
{
    public class Behaviour_BodyIK : HumanoidBehaviour<BehaviourBundle_Humanoid_BodyIK>
    {
        public Behaviour_BodyIK(BehaviourBundle_Humanoid_BodyIK bundle):base(bundle)
        {

        }

        protected Job_Humanoid_BodyIK Job;
        protected AnimationScriptPlayable mplayable;

        public override float Weight { get => Job.Weight; set => Job.Weight=value; }

        public override Playable GetOutputPort => mplayable;

        public override Playable GetInputPort => mplayable;

        public override void Init(IAdvanceAnimator IAnimator)
        {
            base.Init(IAnimator);
        }

        public override void UpdateJobData()
        {
            mplayable.SetJobData(Job);
        }

        public virtual void Init(IList<IK.ILimbIKGoalData> data)
        {
            Job = Job_Humanoid_BodyIK.Create(Animator);
            var graph = Animator.GetGraph;
            Job.ActivateGoalData = data;
            Job.Weight = 0;
            mplayable = AnimationScriptPlayable.Create(graph, Job);
        }

        public override void AddSource(Playable playable)
        {
            
        }

        public override void Dispose()
        {
        }

        public override void Unload(ITimer timer, float time = 0)
        {

        }
    }
}