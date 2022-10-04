using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return
{
    public static class VectorExtension
    {
        public static Vector2 TopDown(this Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }

        public static Vector2 TopDownDirection(this Vector3 v)
        {
            Vector2 result = new(v.x, v.z);
            result.Normalize();
            return result;
        }

        public static Vector3 SlerpTo(this Vector3 a,Vector3 b,Vector3 weight)
        {
            return new (
                Mathf.SmoothStep(a.x, b.x, weight.x),
                Mathf.SmoothStep(a.y, b.y, weight.y),
                Mathf.SmoothStep(a.z, b.z, weight.z)
            );
        }

     

    }
}