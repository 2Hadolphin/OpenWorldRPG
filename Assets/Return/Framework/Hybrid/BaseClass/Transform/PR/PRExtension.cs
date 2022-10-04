using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return
{
    public static class PRExtension
    {
        /// <summary>
        /// Transforms rotation from world space to local space.
        /// </summary>
        public static PR InverseTransformPR(this Transform transform, PR pr)
        {
            pr.Position = transform.InverseTransformPoint(pr);
            pr.Rotation = transform.InverseTransformRotation(pr);

            return pr;
        }


        /// <summary>
        /// Transforms rotation from local space to world space.
        /// </summary>
        public static PR TransformPR(this Transform transform,PR pr)
        {
            pr.Position = transform.TransformPoint(pr);
            pr.Rotation = transform.TransformRotation(pr);

            return pr;
        }


        /// <summary>
        /// **outdated use implicit transform to instead
        /// </summary>
        public static PR GetWorldPR(this Transform transform)
        {
            return new PR(transform.position,transform.rotation);
        }
        public static PR GetLocalPR(this Transform transform)
        {
            return new PR(transform.localPosition, transform.localRotation);
        }

        public static void SetWorldPR(this Transform transform, PR value)
        {
            transform.SetPositionAndRotation(value, value);
        }
        public static void SetLocalPR(this Transform transform, PR value)
        {
            transform.localPosition = value;
            transform.localRotation = value;
        }

        /// <summary>
        /// Add the variable to origin transform, useful to IK phase.
        /// </summary>
        /// <param name="transform">Transform to edit</param>
        /// <param name="value">Local transform variable</param>
        /// <param name="useSelfCoordinate">Use self transform direction when true, otherwise use parent space.</param>
        public static void SetAdditiveLocalPR(this Transform transform, PR value,bool useSelfCoordinate=true)
        {
            transform.localRotation *= value.Rotation;

            if (useSelfCoordinate)
                transform.position += transform.rotation * value.Position;
            else
                transform.localPosition += value;
        }

        /// <summary>
        /// Copy transform info(PR) with local offset from target
        /// </summary>
        public static void CopyViaOffset(this Transform transform, Transform target, PR offset)
        {
            transform.SetPositionAndRotation(
                target.TransformPoint(offset),
                target.TransformRotation(offset)
                );
        }
    }
}
