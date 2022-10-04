using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Return
{
    public static class TransformExtension
    {
        #region Coordinate

        /// <summary>
        /// Attach coordinate to target transform.
        /// </summary>
        public static T AttachTo<T, U>(this T source, U target) where T : Component where U : Component
        {
            Assert.IsFalse(source == null || target == null);
            source.transform.Copy(target.transform);
            return source;
        }



        /// <summary>
        /// Zero coordinate at local space
        /// </summary>
        public static void Zero(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// Copy transform info(PR) from target
        /// </summary>
        public static void Copy(this Transform transform, Transform target)
        {
            transform.SetPositionAndRotation(target.position, target.rotation);
        }

        /// <summary>
        /// Transforms rotation from local space to world space.
        /// </summary>
        public static Quaternion TransformRotation(this Transform transform, Quaternion quaternion)
        {
            return transform.rotation * quaternion;
        }

        /// <summary>
        /// Transforms rotation from world space to local space.
        /// </summary>
        public static Quaternion InverseTransformRotation(this Transform transform, Quaternion quaternion)
        {
            return Quaternion.Inverse(transform.rotation) * quaternion;
        }



        #endregion

        #region Hierarchy


        /// <summary>
        /// Set parent with local offset
        /// </summary>
        public static T ParentOffset<T>(this T child, Component parent, Vector3 offset = default, Quaternion rot = default) where T : Component
        {
            var tf = child.transform;
            tf.SetParent(parent.transform);

            tf.localPosition = offset;

            if (rot == default)
                rot = Quaternion.identity;

            tf.localRotation = rot;

            return child;
        }

        /// <summary>
        /// Set child transform parent as target.
        /// </summary>
        public static void SetParent(this GameObject child, GameObject parent)
        {
            child.transform.SetParent(parent.transform);
        }



        /// <summary>
        /// Return transforms of first child level.
        /// </summary>
        public static IEnumerable<Transform> GetChilds(this Transform parent)
        {
            var nums = parent.childCount;

            for (int i = 0; i < nums; i++)
            {
                var child = parent.GetChild(i);

                if (child.NotNull())
                    yield return child;
            }

            yield break;


            //var nums = parent.childCount;
            //var childs = new Transform[nums];

            //for (int i = 0; i < nums; i++)
            //{
            //    childs[i] = parent.GetChild(i);
            //}

            //return childs;
        }


        public static IEnumerable<Transform> TraverseNoRoot(this Transform parent)
        {
            foreach (Transform child in parent)
            {
                var traverse = child.Traverse();
                foreach (Transform descendant in traverse)
                {
                    yield return descendant;
                }
            }
        }


        /// <summary>
        /// Return all transforms under this transform (include root).
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static IEnumerable<Transform> Traverse(this Transform parent)
        {
            yield return parent;

            foreach (Transform child in parent)
            {
                var traverse = child.Traverse();

                foreach (Transform descendant in traverse)
                {
                    yield return descendant;
                }
            }
        }

        public static IEnumerable<Transform> Traverse_NoRoot(this Transform parent)
        {
            foreach (Transform child in parent)
            {
                var traverse = child.Traverse_NoRoot();
                foreach (Transform descendant in traverse)
                {
                    yield return descendant;
                }
            }
        }


        public static bool FindChild(this Transform tf, string name, out Transform value)
        {
            value = tf.Traverse().FirstOrDefault(x => x.name.Equals(name, System.StringComparison.CurrentCultureIgnoreCase));

            return value.NotNull();

            value = tf.Find(name);

            if (value == null)
            {
                var traverse = tf.Traverse_NoRoot();

                foreach (var child in traverse)
                    if (child.FindChild(name, out value))
                        break;
            }

            return value;
        }

        public static Transform GetChildOfIndex(this Transform tf, int siblingIndex, bool passRoot = true)
        {
            foreach (var child in tf.Traverse())
            {
                if (passRoot && child == tf)
                {
                    passRoot = false;
                    continue;
                }

                if (child.GetSiblingIndex() == siblingIndex)
                {
                    Debug.Log(child.name + " " + siblingIndex);
                    return child;
                }
            }

            return null;
        }


        public static Transform GetTagUpward(this Transform tf, string Tag)
        {

            if (tf.CompareTag(Tag))
                return tf;

            int lev = tf.hierarchyCount - 1;

            for (int i = 0; i < lev; i++)
            {
                tf = tf.parent;
                if (tf.CompareTag(Tag))
                    return tf;
            }

            Debug.LogError(" Can not find any Tag as " + Tag + " in parents from " + tf);
            return tf;
        }

        #endregion


        #region Logic ??

        public static float TakeDir(this Transform origin, Transform Target)
        {
            return Vector3.Dot(origin.right, (Target.position - origin.position).normalized);//dot1>0==right <0==left
        }

        #endregion


    }
}