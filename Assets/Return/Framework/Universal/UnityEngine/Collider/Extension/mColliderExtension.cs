using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return
{
    public static class mColliderExtension
    {
        #region Collider

        public static void ClosestPoint(this Collider[] colliders, Vector3 Point, float maxDistance, out int sn, out Collider col, out float minDistance, out Vector3 ClosestPoint)
        {
            var length = colliders.Length;
            sn = 0;
            for (int i = 0; i < length; i++)
            {
                if (colliders[i])
                {
                    var d = colliders[i].bounds.ClosestPoint(Point).GetLength_Pow();
                    if (d < maxDistance)
                    {
                        maxDistance = d;
                        sn = i;
                    }
                }
            }
            col = colliders[sn];
            ClosestPoint = col.bounds.ClosestPoint(Point);
            minDistance = maxDistance;
        }

        #endregion

        public static float GetColliderTransLength(this Collider collider, Ray ray)
        {
            Vector3 origin = ray.origin;
            RaycastHit hit;
            ray.origin = ray.GetPoint(1000);
            ray.direction = -ray.direction;
            if (collider.Raycast(ray, out hit, 2000))
            {
                return Vector3.Distance(origin, hit.point);
            }
            else
            {
                return 1;
            }
        }
    }
}
