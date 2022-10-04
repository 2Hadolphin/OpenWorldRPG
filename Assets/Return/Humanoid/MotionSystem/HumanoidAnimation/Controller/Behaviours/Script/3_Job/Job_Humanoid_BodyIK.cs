using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using Unity.Collections;
using Return.Humanoid.IK;

namespace Return.Humanoid.Animation
{
    public struct Job_Humanoid_BodyIK : IAnimationJob
    {
        public static Job_Humanoid_BodyIK Create(IAdvanceAnimator animator)
        {
            var job = new Job_Humanoid_BodyIK();
            job.maxPullIteration = 5;
            job.stiffness = 1;

            return job;
        }
        [Range(1, 50)]

        public int maxPullIteration;
        [Range(0.0f, 1.5f)]
        public float stiffness;
        public IList<ILimbIKGoalData> ActivateGoalData;
        public float Weight;

  

        public void ProcessAnimation(AnimationStream stream)
        {
            if (Weight == 0)
                return;

            Solve(stream);
        }

        public void ProcessRootMotion(AnimationStream stream)
        {

        }

        private void Solve(AnimationStream stream)
        {
            AnimationHumanStream humanStream = stream.AsHuman();

            var bodyPosition = humanStream.bodyPosition;
            Vector3 bodyPositionDelta = SolvePull(stream);

            if(!bodyPositionDelta.Equals(Vector3.zero))
            {
                var mag = bodyPositionDelta.magnitude;
                if (mag > 0.15f)
                {
                    var p = 0.15f / mag;
                    Debug.Log(mag + "-" + p);
                    bodyPositionDelta = bodyPositionDelta.Multiply(p);
                    Debug.Log(bodyPositionDelta);
                }
            }

            bodyPosition += bodyPositionDelta;
            humanStream.bodyPosition = bodyPosition;

            //humanStream.SolveIK();
        }

        /// <summary>
        /// return bodyPositionDelta
        /// </summary>
        private Vector3 SolvePull(AnimationStream stream)
        {
            AnimationHumanStream humanStream = stream.AsHuman();

            Vector3 originalBodyPosition = humanStream.bodyPosition;
            Vector3 bodyPosition = originalBodyPosition;

            var ikNums = ActivateGoalData.Count;

            NativeArray<LimbPart> limbParts = new NativeArray<LimbPart>(ikNums, Allocator.Temp);
            PrepareSolvePull(stream, limbParts);

            for (int iter = 0; iter < maxPullIteration; iter++)
            {
                Vector3 deltaPosition = Vector3.zero;
                for (int goalIter = 0; goalIter < ikNums; goalIter++)
                {
                    Vector3 top = bodyPosition + limbParts[goalIter].localPosition;
                    Vector3 localForce = limbParts[goalIter].goalPosition - top;
                    float restLenght = limbParts[goalIter].maximumExtension;
                    float currentLenght = localForce.magnitude;

                    localForce.Normalize();

                    var force = Mathf.Max(limbParts[goalIter].stiffness * (currentLenght - restLenght), 0.0f);

                    deltaPosition += localForce * force * limbParts[goalIter].goalPullWeight * limbParts[goalIter].goalWeight;
                }

                deltaPosition /= maxPullIteration - iter;
                bodyPosition += deltaPosition;
            }

            limbParts.Dispose();

            return bodyPosition - originalBodyPosition;
        }

        private void PrepareSolvePull(AnimationStream stream, NativeArray<LimbPart> limbParts)
        {
            AnimationHumanStream humanStream = stream.AsHuman();

            Vector3 bodyPosition = humanStream.bodyPosition;
            var length = ActivateGoalData.Count;

            for (int goalSN = 0; goalSN < length; goalSN++)
            {
                var data = ActivateGoalData[goalSN];

                limbParts[goalSN] = new LimbPart
                {
                    localPosition = data.LimbRootHandle.GetPosition(stream) - bodyPosition,
                    goalPosition = data.GoalPosition,
                    goalWeight = data.GoalWeight,
                    goalPullWeight = data.PullWeight,
                    maximumExtension = data.MaximumExtension,
                    stiffness = stiffness
                };
            }
        }



    }
}