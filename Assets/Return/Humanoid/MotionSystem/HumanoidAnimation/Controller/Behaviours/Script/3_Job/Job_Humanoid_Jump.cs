using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Animations;
using Return.Humanoid.Animation;
using System.Linq;


namespace Return.Humanoid.Animation
{
    public struct Job_Humanoid_Jump : IAnimationJob
    {
        [Range(0, 1f)]
        public float Weight;
        /// <summary>
        /// 0-1 Accumulate->Jump<0.5>-Balance
        /// </summary>
        [Range(0, 1f)]
        public float Schedule;

        public TransformStreamHandle Hips;
        public StreamHandle_Leg Right;
        public StreamHandle_Leg Left;

        public float RightLegLength;
        public float LeftLegLength;

        /// <summary>
        /// 0-1 Jump rows between little jump and full jump
        /// </summary>
        [Range(0, 1f)]
        public float Strength;

        //need set hint 
        public void ProcessAnimation(AnimationStream stream)
        {
            var weight = Mathf.Abs(Schedule - 0.5f).Remap_Clamp(0f, 0.5f, 1f, 0f);

            if (weight == 0)
                return;

            var humanStream = stream.AsHuman();


            Hips.GetGlobalTR(stream, out var rootP, out var rootR);

            //RightLeg
            {
                var rp = humanStream.GetGoalPositionFromPose(AvatarIKGoal.RightFoot);
                var rr = humanStream.GetGoalRotationFromPose(AvatarIKGoal.RightFoot);

                var blindDistance =RightLegLength * 2 / 5;

                var hipdownDir = rootR * Vector3.down;
                var maxProject = hipdownDir.Multiply(blindDistance);

                // project foot on body rows
                var footProject = Vector3.Project(rp - rootP, hipdownDir);
                // lerp point between current pose and max pose
                var currentOverwrite = Vector3.Lerp(footProject, maxProject, weight);

                rp += currentOverwrite - footProject;
                humanStream.SetGoalPosition(AvatarIKGoal.RightFoot, rp);

                DebugDrawUtil.DrawCoordinate(rp, rr);
            }

            //LeftLeg
            {
                var lp = humanStream.GetGoalPositionFromPose(AvatarIKGoal.LeftFoot);
                var lr = humanStream.GetGoalRotationFromPose(AvatarIKGoal.LeftFoot);

                var blindDistance = LeftLegLength* 2 / 5;

                var hipdownDir = rootR * Vector3.down;
                var maxProject = hipdownDir.Multiply(blindDistance);

                // project foot on body rows
                var footProject = Vector3.Project(lp - rootP, hipdownDir);
                // lerp point between current pose and max pose
                var currentOverwrite = Vector3.Lerp(footProject, maxProject, weight);

                lp += currentOverwrite - footProject;
                humanStream.SetGoalPosition(AvatarIKGoal.LeftFoot, lp);
                
                DebugDrawUtil.DrawCoordinate(lp, lr);
            }

        }

        public void ProcessRootMotion(AnimationStream stream)
        {
            // cal root position => mass center or feet position

        }


        public Job_Humanoid_Jump(IAdvanceAnimator animator)
        {
            Weight = 0;
            Schedule = 0;
            Strength = 0;
            RightLegLength = animator.GetHumanBone(HumanBodyBones.RightUpperLeg).limit.axisLength + animator.GetHumanBone(HumanBodyBones.RightLowerLeg).limit.axisLength; 
            LeftLegLength = animator.GetHumanBone(HumanBodyBones.LeftUpperLeg).limit.axisLength + animator.GetHumanBone(HumanBodyBones.LeftUpperLeg).limit.axisLength;



            Hips = animator.GetBindingBone(HumanBodyBones.Hips);
            Right = new StreamHandle_Leg(animator, HumanPartDof.RightLeg);
            Left = new StreamHandle_Leg(animator, HumanPartDof.LeftLeg);
        }

    }

}
