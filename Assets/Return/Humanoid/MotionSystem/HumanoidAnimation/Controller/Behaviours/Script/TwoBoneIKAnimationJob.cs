using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace Return.Animations.IK
{
    public struct TwoBoneIKAnimationJob:IAnimationJob
    {
        public static TwoBoneIKAnimationJob Create(Animator animator, Transform handBone)
        {
            var job = new TwoBoneIKAnimationJob();

            if (handBone)
            {
                var End = handBone;
                job.End = animator.BindStreamTransform(End);

                if (End)
                {
                    var Middle = End.parent;
                    job.Middle = animator.BindStreamTransform(Middle);

                    if (Middle)
                    {
                        var Top = Middle.parent;
                        job.Top = animator.BindStreamTransform(Top);
                    }
                }
            }

            return job;
        }

        public bool isAdditive;

        public Vector3 Position;
        public Quaternion Rotation;

        public TransformStreamHandle Top;
        public TransformStreamHandle Middle;
        public TransformStreamHandle End;

        public void Solve(AnimationStream stream, Vector3 ePosition, Quaternion rotation, float weight)
        {
            Quaternion aRotation = Top.GetRotation(stream);
            Quaternion bRotation = Middle.GetRotation(stream);
            Quaternion cRotation = End.GetRotation(stream);
            rotation = Quaternion.Lerp(cRotation, rotation, weight);

            Vector3 aPosition = Top.GetPosition(stream);
            Vector3 bPosition = Middle.GetPosition(stream);
            Vector3 cPosition = End.GetPosition(stream);
            ePosition = Vector3.Lerp(cPosition, ePosition, weight);

            Vector3 ab = bPosition - aPosition;
            Vector3 bc = cPosition - bPosition;
            Vector3 ac = cPosition - aPosition;
            Vector3 ae = ePosition - aPosition;

            float abcAngle = TriangleAngle(ac.magnitude, ab, bc);
            float abeAngle = TriangleAngle(ae.magnitude, ab, bc);
            float angle = (abcAngle - abeAngle) * Mathf.Rad2Deg;
            Vector3 axis = Vector3.Cross(ab, bc).normalized;

            Quaternion fromToRotation = Quaternion.AngleAxis(angle, axis);

            Quaternion worldQ = fromToRotation * bRotation;
            Middle.SetRotation(stream, worldQ);

            cPosition = End.GetPosition(stream);
            ac = cPosition - aPosition;
            Quaternion fromTo = Quaternion.FromToRotation(ac, ae);
            Top.SetRotation(stream, fromTo * aRotation);
            End.SetRotation(stream, rotation);
        }

        /// <summary>
        /// Returns the angle needed between v1 and v2 so that their extremities are
        /// spaced with a specific length.
        /// </summary>
        /// <returns>The angle between v1 and v2.</returns>
        /// <param name="aLen">The desired length between the extremities of v1 and v2.</param>
        /// <param name="v1">First triangle edge.</param>
        /// <param name="v2">Second triangle edge.</param>
        private static float TriangleAngle(float aLen, Vector3 v1, Vector3 v2)
        {
            float aLen1 = v1.magnitude;
            float aLen2 = v2.magnitude;
            float c = Mathf.Clamp((aLen1 * aLen1 + aLen2 * aLen2 - aLen * aLen) / (aLen1 * aLen2) / 2.0f, -1.0f, 1.0f);
            return Mathf.Acos(c);
        }

        public void ProcessAnimation(AnimationStream stream)
        {
            if (!End.IsValid(stream))
                return;

            End.GetGlobalTR(stream, out var pos, out var rot);
            pos += rot * Position;
            rot = (Rotation * rot);// * rot;

            Solve(stream, pos, rot, 1);
        }

        public void ProcessRootMotion(AnimationStream stream)
        {

        }
    }
}