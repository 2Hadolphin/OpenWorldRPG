using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Assertions;
using System.Reflection;
using Object = UnityEngine.Object;

namespace Return
{
    public static class UnityObjectExtension
    {
        public static bool IsInstance<T>(this T obj) where T :Object 
        {
            Assert.IsNotNull(obj);

            return obj.GetInstanceID() < 0;
        }

        public static bool IsPrefab(this GameObject go) 
        {
            return !go.scene.IsValid();
        }

        /// <summary>
        /// Safe check destroy object.
        /// </summary>
        public static void Destroy(this Object obj2destroy)
        {

            if (obj2destroy == null)
            {
                Debug.LogWarning($"Trying to destory null object, this operation will be ignore.", obj2destroy);
                return;
            }

            if (!obj2destroy.IsInstance())
            {
                Debug.LogError($"Trying to destory native asset [{obj2destroy.name}], this operation will be ignore.", obj2destroy);
                return;
            }

#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying)
                Object.Destroy(obj2destroy);
            else
            {
                Object.DestroyImmediate(obj2destroy);
                Debug.LogWarning($"Trying to destory {obj2destroy.name} immediately.", obj2destroy);
            }
#else
            Object.Destroy(obj2destroy);
#endif
        }


        /// <summary>
        /// ??????
        /// </summary>
        //[Obsolete]
        //public static void Reflect<T>(this T obj,Func<string,object> search)
        //{
        //    var prop = typeof(T).GetProperties();
        //    Debug.Log(typeof(T)+" : "+prop.Length);
        //    foreach (PropertyInfo x in prop)
        //    {
        //        Debug.Log("Found "+x.Name);
        //        var ans = search(x.Name);
        //        Debug.Log(ans);
        //        if (x.CanWrite)
        //            x.SetValue(obj, ans);
        //    }
        //}
    }
}