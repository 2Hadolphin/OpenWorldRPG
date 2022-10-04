using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System;
using UnityEngine.Assertions;

namespace Return
{

    public static class HierarchyExtension
    {

        public static Transform GetRoot<T>(this T component) where T : Component
        {
            var tf = component.transform;
            var parent = tf.parent;

            if (!parent)
                return tf;

            while (parent)
            {
                var p = parent.parent;

                if (p)
                    parent = p;
                else
                    break;
            }

            return parent;
        }

        public static Transform GetRoot<T>(this T component, string tag) where T : Component
        {
            var parent = component.transform;

            Transform root = null;

            while (parent)
            {
                if (parent && parent.CompareTag(tag))
                {
                    root = parent;
                    break;
                }
                else
                {
                    var p = parent.parent;

                    if (p)
                        parent = p;
                    else
                        break;
                }
            }

            return root;
        }

        /// <summary>
        /// Get closest parent with tag 
        /// </summary>
        /// <param name="tf"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static Transform GetRoot(this Transform tf, string tag)
        {
            var parent = tf.parent;

            Assert.IsFalse(string.IsNullOrEmpty(tag));

            if (!parent && tf.CompareTag(tag))
                return tf;

            while (parent)
            {
                var p = parent.parent;

                if (!p)
                    break;

                if (p.CompareTag(tag))
                    return p;

                parent = p;
            }

            if (!parent.CompareTag(tag))
                return null;

            return parent;
        }


        #region Tag

        public static bool GetChildNamedWithTag(this GameObject @object,string tag,string name,out GameObject target)
        {
            var tfs = @object.transform.Traverse();

            foreach (var tf in tfs)
            {
                if (tf.CompareTag(tag) && tf.name.Equals(name))
                {
                    target = tf.gameObject;
                    return true;
                }
            }

            target = default;
            return false;
        }

        public static IEnumerable<GameObject> GetAllTags(this GameObject parent,string tag) 
        {
            var tfs = parent.transform.Traverse();

            foreach (Transform tf in tfs)
            {
                if (tf.CompareTag(tag))
                    yield return tf.gameObject;
            }
        }

        public static T[] FindComponentsInChildWithTag<T>(this Component parent, string tag) where T : Component
        {
            Transform t = parent.transform;
            var list = new List<T>();

            foreach (Transform tf in t)
            {
                if (tf.CompareTag(tag))
                {
                    if (tf.TryGetComponent<T>(out var value))
                        list.Add(value);
                }
            }
            return list.ToArray();
        }

        public static T FindComponentInChildWithTag<T>(this GameObject parent, string tag) where T : Component
        {
            Transform t = parent.transform;
            foreach (Transform tf in t)
            {
                if (tf.CompareTag(tag))
                {
                    object o = tf.GetComponent<T>();
                    if (o != null) { return (T)o; }
                }
            }
            return null;
        }

        public static T FindComponentInRootWithTag<T>(this GameObject parent, string tag) where T : Component
        {
            Transform t = parent.transform.root;
            foreach (Transform tf in t)
            {
                if (tf.CompareTag(tag))
                {
                    object o=tf.GetComponent<T>();
                    if (o != null) { return tf.GetComponent<T>();  }
                }
            }
            return null;
        }

        public static T[] FindComponentsInRootWithTag<T>(this GameObject parent, string tag) where T : Component
        {
            Transform t = parent.transform.root;
            List<T> o = new List<T>();
            foreach (Transform tf in t)
            {
                if (tf.CompareTag(tag))
                {
                    object p = tf.GetComponent<T>();
                    if (p != null) 
                    {
                        o.Add((T)p);
                    }
                }
            }
            return o.ToArray();
        }

        public static T[] FindComponentsInRootWithTag<T>(this Transform parent, string tag) where T : Component
        {
            Transform t = parent.transform.root;
            List<T> o = new List<T>();
            foreach (Transform tf in t)
            {
                if (tf.CompareTag(tag))
                {
                    object p = tf.GetComponent<T>();
                    if (p != null)
                    {
                        o.Add((T)p);
                    }
                }
            }
            return o.ToArray();
        }

        public static T[] FindComponentsInRootWithTag<T>(this Collider parent, string tag) where T : Component
        {
            Transform t = parent.transform.root;
            List<T> o = new List<T>();
            foreach (Transform tf in t)
            {
                if (tf.CompareTag(tag))
                {
                    object p = tf.GetComponent<T>();
                    if (p != null)
                    {
                        o.Add((T)p);
                    }
                }
            }
            return o.ToArray();
        }

        public static T[] FindChildComponentsWithTag<T>(this Transform parent, string tag) where T : Component
        {
            T[] t = parent.GetComponentsInChildren<T>();
            List<T> o = new List<T>();
            int length = t.Length;
            for(int i = 0; i < length; i++)
            {
                if (t[i].CompareTag(tag))
                    o.Add(t[i]);
            }

            
            return o.ToArray();
        }

        public static T FindComponentInParentWithTag<T>(this GameObject parent, string tag) where T : Component
        {
            Transform t = parent.transform.parent;
            foreach (Transform tf in t)
            {
                if (tf.CompareTag(tag))
                {
                    object o = tf.GetComponent<T>();
                    if (o != null) { return tf.GetComponent<T>(); }
                }
            }
            return null;
        }

        #endregion

        public static bool TryGetCompontAtRoot<T>(this Transform tf ,string tag, out T o) where T : Component
        {
            Debug.Log(tag);
            int length = tf.hierarchyCount;
            for(int i = 0; i < length && tf != null; i++)
            {
                if (tf.CompareTag(tag))
                    if (tf.TryGetComponent(out o))
                        return true;

                tf = tf.parent;
            }

            o = null;
            return false;
        }

        public static bool TryGetComponentInChildren<T>(this GameObject parent, out T o) where T : Component
        {
            Transform t = parent.transform.parent;
            foreach (Transform tf in t)
            {
                o = tf.GetComponent<T>();
                if (o != null) { return true; }
            }
            o = null;
            return false;
        }

        public static bool TryGetComponentInChildren<T>(this Transform parent, out T o) where T : Component
        {
            foreach (Transform tf in parent)
            {
                o = tf.GetComponent<T>();
                if (o != null) { return true; }
            }
            o = null;
            return false;
        }

        public static int GetTagsInChild(Transform parent, string tag, out Transform[] o)
        {
            int length = parent.childCount;
            o = new Transform[length];
            Transform tf;
            int num = 0;

            for (int i = 0; i < length; i++)
            {
                tf = parent.GetChild(i);
                if (tf.CompareTag(tag))
                {
                    o[num] = tf;
                    num++;
                }
            }

            return num;
        }

        public static bool TryGetComponentInChildWithTag<T>(this Transform parent,string tag, out T o) where T : Component
        {
            int length = parent.childCount;
            Transform tf;
            for(int i = 0; i < length; i++)
            {
                tf = parent.GetChild(i);
                if (tf.CompareTag(tag))
                {
                    if(tf.TryGetComponent<T>(out o))
                    {
                        return true;
                    }
                }
            }

            if (TryGetComponentInChildren(parent, out o))
                return true;

            o = null;
            return false;
        }
    }
}