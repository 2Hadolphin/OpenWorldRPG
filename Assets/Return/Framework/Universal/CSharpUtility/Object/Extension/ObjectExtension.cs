using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
namespace Return
{
    public static class ObjectExtension
    {
        static readonly Type engine=typeof(UnityEngine.Object); 

        /// <summary>
        /// CheckAdd obj is not null.
        /// </summary>
        public static bool NotNull<T>(this T obj) where T : class
        {
            if (typeof(T).IsSubclassOf(engine) && obj is UnityEngine.Object uObj)
                return uObj != null;
            else
                return obj != null;
        }

        public static bool NotNull<T>(this T obj,out T cast) where T : class
        {
            var valid = NotNull(obj);

            cast = valid ? obj : default;

            return valid;
        }


        /// <summary>
        /// CheckAdd obj is null.
        /// </summary>
        public static bool IsNull<T>(this T obj) where T : class
        {
            if(typeof(T).IsSubclassOf(engine) && obj is UnityEngine.Object uObj)
                return uObj == null;
            else
                return obj == null;
        }


        public static T CreateIfNull<T>(this T obj) where T : class, new()
        {
#if UNITY_EDITOR
            if (obj is UnityEngine.Object uObj)
                throw new InvalidOperationException($"{uObj.name} is not a valid type to cast, use InstanceIfNull instead.");
#endif

            if (obj.IsNull())
                obj = new T();

            return obj;
        }
    }
}
