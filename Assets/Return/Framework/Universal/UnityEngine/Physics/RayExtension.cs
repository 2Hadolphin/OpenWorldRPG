using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return
{
    public static class RayExtension
    {

        public static Vector3 CastPoint(this Ray ray, float distance)
        {
            return ray.origin + ray.direction.Multiply(distance);
        }

        /// <summary>
        /// Get ray from transform position and set local direction.
        /// </summary>
        /// <param name="tf">Origing point of ray.</param>
        /// <param name="dir">Direction of ray, input as local space then turn to global space.</param>
        public static Ray GetRay(this Transform tf, Vector3 dir = default)
        {
            if (dir.Equals(default))
                dir = Vector3.forward;

            return new Ray(tf.position, tf.rotation * dir);
        }

    }
}