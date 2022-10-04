using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using System.Linq;

namespace Return.Humanoid.Animation
{
    public class Behaviour_Jump : HumanoidBehaviour<BehaviourBundle_Humanoid_Jump>
    {
        public Behaviour_Jump(AnimationBehaviourBundle bundle):base(bundle)
        {

        }

        public AnimationScriptPlayable mplayable;

        protected Job_Humanoid_Jump Job;
        protected Avatar avatar;
        public override float Weight { get => Job.Weight; set => Job.Weight = value; }

        public override Playable GetOutputPort => mplayable;

        public override Playable GetInputPort => mplayable;

        protected float UpLegHeight;
        protected float LowLegHeight;
        public override void Init(IAdvanceAnimator IAnimator)
        {
            base.Init(IAnimator);

            var graph = IAnimator.GetGraph;

            Job = new Job_Humanoid_Jump(IAnimator);

            mplayable = AnimationScriptPlayable.Create(graph, Job);

            IAnimator.JumpSchedule += SetJobSchedule;
            IAnimator.RootScale += UpdateScale;
            #region Load Data

            avatar = IAnimator.GetAnimator.avatar;
          // UpLegHeight=avatar.humanDescription.human.Select(width=> {if(width.limit.axisLength.Equals() }))

            #endregion

        }

        public override void Listener(object sender, AnimationArg e)
        {
            
        }
        protected virtual void UpdateScale(Vector3 scale)
        {

        }

        protected virtual void SetJobSchedule(float p)
        {
            Job.Schedule = p;
            mplayable.SetJobData(Job);
        }

        public override void UpdateJobData()
        {

        }


        public override void AddSource(Playable playable)
        {
            mplayable.AddInput(playable, 0, 1);
            //Parent.GetSource.SetSpeed(0.1f);
        }




        public override void Unload(ITimer timer, float time = 0)
        {
            throw new System.NotImplementedException();
        }

        public override void Dispose()
        {
            if (mplayable.CanDestroy())
                mplayable.Destroy();
        }
    }

}
