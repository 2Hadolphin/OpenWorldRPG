using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using UnityEngine.Animations;
using System;
using System.Linq;
using Return.Humanoid.IK;
using Return.Animations.IK;

namespace Return.Humanoid.Animation
{
    public struct Job_TwoBoneIKPair : IAnimationJob
    {
        public float Weight;
        public TwoBoneIKJob Right;
        public TwoBoneIKJob Left;
        public ILimbIKGoalData RightData;
        public ILimbIKGoalData LeftData;
        public TransformStreamHandle Root;
        public PropertyStreamHandle PropertyHandle;


        public static Job_TwoBoneIKPair Create()
        {
            var job = new Job_TwoBoneIKPair();
            job.Right = new TwoBoneIKJob();
            job.Left = new TwoBoneIKJob();
            return job;
        }

        public void ProcessAnimation(AnimationStream stream)
        {

            if (Weight.Equals(0))
                return;

            var humanStream = stream.AsHuman();





            if (RightData.HasTarget)
                RightData.Solve(ref humanStream);
            else 
                RightData.Release(ref humanStream);


            if (LeftData.HasTarget)
                LeftData.Solve(ref humanStream);
            else
                LeftData.Release(ref humanStream);


            humanStream.SolveIK();
            return;

            var rootPos = Root.GetPosition(stream);
            var rootRot = Root.GetRotation(stream);

            #region IKGoal
            if (RightData.GoalWeight != 0)
                Right.Solve(stream, RightData.GoalPosition, RightData.GoalRotation, RightData.GoalWeight * Weight);

            if (LeftData.GoalWeight != 0)
                Left.Solve(stream, LeftData.GoalPosition, LeftData.GoalRotation, LeftData.GoalWeight * Weight);

            #endregion
        }


        public void ProcessRootMotion(AnimationStream stream)
        {

        }
    }


   

}