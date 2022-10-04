using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Return
{
    public static class PhysicUtility//:IComparer<RaycastHit>
    {

        // cache raycast result
        private static RaycastHit[] ps_Hits = new RaycastHit[64];
        private static List<RaycastHit> ps_cacheHits=new(64);
        private static RayCastHandle ps_raycastHandle = new();


        public static void Order(this RaycastHit[] hits,int length=-1)
        {
            if (length < 0)
                length = hits.Length;

            if (length <= 0)
                return;

            ps_cacheHits.Clear();

            try
            {
                for (int i = 0; i < length; i++)
                {
                    ps_cacheHits.Add(hits[i]);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }

            {
                var orders = ps_cacheHits.OrderBy((x) => x.distance);

                int i = 0;
                foreach (var order in orders)
                {
                    hits[i++] = order;
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public static bool RaycastFiltered(Ray ray, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, Transform ignoreRoot = null)
        {
            int hitCount = Physics.RaycastNonAlloc(ray, ps_Hits, maxDistance, layerMask);
            if (hitCount > 0)
            {
                // Get the closest (not ignored)
                int closest = -1;
                for (int i = 0; i < hitCount; ++i)
                {
                    // CheckAdd if closer
                    if (closest == -1 || ps_Hits[i].distance < ps_Hits[closest].distance)
                    {
                        if (ignoreRoot != null)
                        {
                            // CheckAdd if transform or parents match ignore root
                            Transform t = ps_Hits[i].transform;
                            bool ignore = false;
                            while (t != null)
                            {
                                if (t == ignoreRoot)
                                {
                                    ignore = true;
                                    break;
                                }
                                t = t.parent;
                            }
                            // Not ignored. This is closest
                            if (!ignore)
                                closest = i;
                        }
                        else
                            closest = i;
                    }
                }
                // CheckAdd if all ignored
                if (closest == -1)
                    return false;
                else
                    return true;
            }
            else
                return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool RaycastNonAllocSingle(Ray ray, out RaycastHit hit, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, Transform ignoreRoot = null, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            int hitCount = Physics.RaycastNonAlloc(ray, ps_Hits, maxDistance, layerMask, queryTriggerInteraction);
            if (hitCount > 0)
            {
                // Get the closest (not ignored)
                int closest = -1;
                for (int i = 0; i < hitCount; ++i)
                {
                    // CheckAdd if closer
                    if (closest == -1 || ps_Hits[i].distance < ps_Hits[closest].distance)
                    {
                        if (ignoreRoot != null)
                        {
                            // CheckAdd if transform or parents match ignore root
                            Transform t = ps_Hits[i].transform;
                            bool ignore = false;
                            while (t != null)
                            {
                                if (t == ignoreRoot)
                                {
                                    ignore = true;
                                    break;
                                }
                                t = t.parent;
                            }
                            // Not ignored. This is closest
                            if (!ignore)
                                closest = i;
                        }
                        else
                            closest = i;
                    }
                }
                // CheckAdd if all ignored
                if (closest == -1)
                {
                    hit = new RaycastHit();
                    return false;
                }
                // Return the relevant hit
                hit = ps_Hits[closest];
                return true;
            }
            else
            {
                hit = new RaycastHit();
                return false;
            }
        }

        public static RayCastHandle RayCastNonAllocAll(Ray ray,float distance,int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            var num= Physics.RaycastNonAlloc(ray, ps_Hits, distance, layerMask, queryTriggerInteraction);
            ps_raycastHandle.hits = ps_Hits;
            ps_raycastHandle.hitCount = num;

            return ps_raycastHandle;
        }

        #region Sphere


        /// <summary>
        /// Cast sphere and get hit result.
        /// </summary>
        /// <param name="ignoreRoot">Colliders under this transform will be ignore.</param>
        /// <returns></returns>
        public static bool SphereCastFiltered(Ray ray, float radius, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, Transform ignoreRoot = null)
        {
            int hitCount = Physics.SphereCastNonAlloc(ray, radius, ps_Hits, maxDistance, layerMask);
            if (hitCount > 0)
            {
                // Get the closest (not ignored)
                int closest = -1;
                for (int i = 0; i < hitCount; ++i)
                {
                    // CheckAdd if closer
                    if (closest == -1 || ps_Hits[i].distance < ps_Hits[closest].distance)
                    {
                        if (ignoreRoot != null)
                        {
                            // CheckAdd if transform or parents match ignore root
                            Transform t = ps_Hits[i].transform;
                            bool ignore = false;
                            while (t != null)
                            {
                                if (t == ignoreRoot)
                                {
                                    ignore = true;
                                    break;
                                }
                                t = t.parent;
                            }
                            // Not ignored. This is closest
                            if (!ignore)
                                closest = i;
                        }
                        else
                            closest = i;
                    }
                }
                // CheckAdd if all ignored
                if (closest == -1)
                    return false;
                else
                    return true;
            }
            else
                return false;
        }

        public static bool SphereCastNonAllocSingle(Ray ray, float radius, out RaycastHit hit, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, Transform ignoreRoot = null, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            int hitCount = Physics.SphereCastNonAlloc(ray, radius, ps_Hits, maxDistance, layerMask, queryTriggerInteraction);
            if (hitCount > 0)
            {
                // Get the closest (not ignored)
                int closest = -1;
                for (int i = 0; i < hitCount; ++i)
                {
                    // CheckAdd if closer
                    if (closest == -1 || ps_Hits[i].distance < ps_Hits[closest].distance)
                    {
                        if (ignoreRoot != null)
                        {
                            // CheckAdd if transform or parents match ignore root
                            Transform t = ps_Hits[i].transform;
                            bool ignore = false;
                            while (t != null)
                            {
                                if (t == ignoreRoot)
                                {
                                    ignore = true;
                                    break;
                                }
                                t = t.parent;
                            }
                            // Not ignored. This is closest
                            if (!ignore)
                                closest = i;
                        }
                        else
                            closest = i;
                    }
                }
                // CheckAdd if all ignored
                if (closest == -1)
                {
                    hit = new RaycastHit();
                    return false;
                }
                // Return the relevant hit
                hit = ps_Hits[closest];
                return true;
            }
            else
            {
                hit = new RaycastHit();
                return false;
            }
        }

        #endregion

        public static bool CapsuleCastFiltered(Vector3 point1, Vector3 point2, float radius, Vector3 direction, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, Transform ignoreRoot = null)
        {
            int hitCount = Physics.CapsuleCastNonAlloc(point1, point2, radius, direction, ps_Hits, maxDistance, layerMask);
            if (hitCount > 0)
            {
                // Get the closest (not ignored)
                int closest = -1;
                for (int i = 0; i < hitCount; ++i)
                {
                    // CheckAdd if closer
                    if (closest == -1 || ps_Hits[i].distance < ps_Hits[closest].distance)
                    {
                        if (ignoreRoot != null)
                        {
                            // CheckAdd if transform or parents match ignore root
                            Transform t = ps_Hits[i].transform;
                            bool ignore = false;
                            while (t != null)
                            {
                                if (t == ignoreRoot)
                                {
                                    ignore = true;
                                    break;
                                }
                                t = t.parent;
                            }
                            // Not ignored. This is closest
                            if (!ignore)
                                closest = i;
                        }
                        else
                            closest = i;
                    }
                }
                // CheckAdd if all ignored
                if (closest == -1)
                    return false;
                else
                    return true;
            }
            else
                return false;
        }

        public static bool CapsuleCastNonAllocSingle(Vector3 point1, Vector3 point2, float radius, Vector3 direction, out RaycastHit hit, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, Transform ignoreRoot = null, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            int hitCount = Physics.CapsuleCastNonAlloc(point1, point2, radius, direction, ps_Hits, maxDistance, layerMask, queryTriggerInteraction);
            if (hitCount > 0)
            {
                // Get the closest (not ignored)
                int closest = -1;
                for (int i = 0; i < hitCount; ++i)
                {
                    // CheckAdd if closer
                    if (closest == -1 || ps_Hits[i].distance < ps_Hits[closest].distance)
                    {
                        if (ignoreRoot != null)
                        {
                            // CheckAdd if transform or parents match ignore root
                            Transform t = ps_Hits[i].transform;
                            bool ignore = false;
                            while (t != null)
                            {
                                if (t == ignoreRoot)
                                {
                                    ignore = true;
                                    break;
                                }
                                t = t.parent;
                            }
                            // Not ignored. This is closest
                            if (!ignore)
                                closest = i;
                        }
                        else
                            closest = i;
                    }
                }
                // CheckAdd if all ignored
                if (closest == -1)
                {
                    hit = new RaycastHit();
                    return false;
                }
                // Return the relevant hit
                hit = ps_Hits[closest];
                return true;
            }
            else
            {
                hit = new RaycastHit();
                return false;
            }
        }

        public static bool BoxCastFiltered(Vector3 center, Vector3 halfExtents, Vector3 direction, Quaternion orientation, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, Transform ignoreRoot = null)
        {
            int hitCount = Physics.BoxCastNonAlloc(center, halfExtents, direction, ps_Hits, orientation, maxDistance, layerMask);
            if (hitCount > 0)
            {
                // Get the closest (not ignored)
                int closest = -1;
                for (int i = 0; i < hitCount; ++i)
                {
                    // CheckAdd if closer
                    if (closest == -1 || ps_Hits[i].distance < ps_Hits[closest].distance)
                    {
                        if (ignoreRoot != null)
                        {
                            // CheckAdd if transform or parents match ignore root
                            Transform t = ps_Hits[i].transform;
                            bool ignore = false;
                            while (t != null)
                            {
                                if (t == ignoreRoot)
                                {
                                    ignore = true;
                                    break;
                                }
                                t = t.parent;
                            }
                            // Not ignored. This is closest
                            if (!ignore)
                                closest = i;
                        }
                        else
                            closest = i;
                    }
                }
                // CheckAdd if all ignored
                if (closest == -1)
                    return false;
                else
                    return true;
            }
            else
                return false;
        }

        public static bool BoxCastNonAllocSingle(Vector3 center, Vector3 halfExtents, Vector3 direction, out RaycastHit hit, Quaternion orientation, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, Transform ignoreRoot = null, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            int hitCount = Physics.BoxCastNonAlloc(center, halfExtents, direction, ps_Hits, orientation, maxDistance, layerMask, queryTriggerInteraction);
            if (hitCount > 0)
            {
                // Get the closest (not ignored)
                int closest = -1;
                for (int i = 0; i < hitCount; ++i)
                {
                    // CheckAdd if closer
                    if (closest == -1 || ps_Hits[i].distance < ps_Hits[closest].distance)
                    {
                        if (ignoreRoot != null)
                        {
                            // CheckAdd if transform or parents match ignore root
                            Transform t = ps_Hits[i].transform;
                            bool ignore = false;
                            while (t != null)
                            {
                                if (t == ignoreRoot)
                                {
                                    ignore = true;
                                    break;
                                }
                                t = t.parent;
                            }
                            // Not ignored. This is closest
                            if (!ignore)
                                closest = i;
                        }
                        else
                            closest = i;
                    }
                }
                // CheckAdd if all ignored
                if (closest == -1)
                {
                    hit = new RaycastHit();
                    return false;
                }
                // Return the relevant hit
                hit = ps_Hits[closest];
                return true;
            }
            else
            {
                hit = new RaycastHit();
                return false;
            }
        }

    }

    /// <summary>
    /// Do not asyn or cross frame, single thread only, mulitly job use clone extension.
    /// </summary>
    public class RayCastHandle : Cloneable,IEnumerable<RaycastHit>
    {
        public int hitCount;
        public RaycastHit[] hits;

        public IEnumerator<RaycastHit> GetEnumerator()
        {
            for (int i = 0; i < hitCount; i++)
            {
                yield return hits[i];
            }

            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public static partial class RaycastHitHandleExtension
    {
        public static RayCastHandle OrderByDistance(this RayCastHandle handle, bool fromClosest = true)
        {
            handle.hits.Order(handle.hitCount);
            return handle;
        }

        public static RayCastHandle Clone(this RayCastHandle handle)
        {
            var newHandle = handle.Clone();
            return newHandle as RayCastHandle;
        }

    }
}