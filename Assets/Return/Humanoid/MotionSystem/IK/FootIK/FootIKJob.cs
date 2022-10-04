using UnityEngine;
using UnityEngine.Animations;

namespace Return.Humanoid.IK
{
    public struct FootIKJob : IAnimationJob
    {
        /// <summary>
        /// Send anim pose back
        /// </summary>
        public IHumanStreamJob[] CatchJobs;

        public IHumanStreamJob[] SetGoalJobs;

        public GetBodyPositionAnimPose Body;

        public void ProcessAnimation(AnimationStream stream)
        {
            var hs = stream.AsHuman();

            foreach (var job in CatchJobs)
            {
                job.Slove(hs);
            }

            Body.Slove(hs);

            foreach (var job in SetGoalJobs)
            {
                job.Slove(hs);
            }


        }

        public void ProcessRootMotion(AnimationStream stream)
        {

        }
    }
}