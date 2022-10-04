using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Return;
using Unity.Collections;
using UnityEngine.Assertions;
public static partial class CastExtension
{

    #region Gernic

    /// <summary>
    /// Fill target<T2> field with value<T1>
    /// </summary>
    [Obsolete]
    public static void Fill<T1,T2>(this T1 value,ref T2 target) where T2:T1
    {
        target = (T2)value;
    }



    /// <summary>
    /// Cast T1 value to T2 and invoke action if result is valid. 
    /// </summary>
    public static bool Parse<T1,T2>(this T1 value,Action<T2> action)
    {
        if (value is not T2 cast)
            return false;

        action.Invoke(cast);
        return true;
    }

    /// <summary>
    /// Cast T1 value to T2 and return if result is valid. 
    /// </summary>
    public static bool Parse<T1, T2>(this T1 value, out T2 result)
    {
        var valid = false;

        if (value is UnityEngine.Object obj)
        {
            if (obj is T2 t2)
            {
                result = t2;
                valid= obj!=null;
            }
            else
            {
                result = default;
                valid= false;
            }

            //if (obj is T2 t2)
            //    result = t2;
            //else
            //    result = default;

            //if (result is UnityEngine.Object t2obj)
            //    return t2obj != null;
            //else
            //    return result != null;
        }
        else
        {
            if (value is not null && value is T2 t2)
            {
                result = t2;
                valid = true;
            }
            else
            {
                result = default;
                valid= false;
            }
        }

#if UNITY_EDITOR
        if (!valid)
            Debug.LogError($"\"{value}\" is {typeof(T1).Name} failure cast to {typeof(T2).Name}");
#endif


        return valid;
    }

    //public static T2 Parse<T1,T2>(this T1 value)
    //{
    //    if (value is T2 t2)
    //        return t2;
    //    else
    //        return default;
    //}



    //public static bool ParseC<T1,T2>(this T1 value,out T2 result) where T1:class where T2 :class
    //{
    //    var equal = value is not null && value is T2;

    //    if(equal)
    //        result = value as T2;
    //    else
    //        result = default;

    //    return equal;
    //}

    #endregion






    #region Camera delete

    public static void LookAt(this Camera cam,Vector3 position,Quaternion rotation,float distance)
    {
        //var tf = cam.transform;
        //tf.position = position+rotation*Vector3.back.Multiply(distance);
        //tf.rotation = rotation;
    }

    #endregion

    #region Layer
    public static int ToLayer(this LayerMask layerMask)
    {
        int layerNumber = 0;
        int layer = layerMask.value;
        while (layer > 0)
        {
            layer = layer >> 1;
            layerNumber++;
        }
        return layerNumber - 1;
    }


    #endregion

    #region NativeArray
    public static void Clean<T>(this NativeArray<T> array) where T : unmanaged
    {
        if (array.IsCreated)
            array.Dispose();
    }
    #endregion

 


    #region Text

    public static string LastSection(this string text,char c)
    {
        var index = text.LastIndexOf(c);

        if (index > 0)
            return text.Substring(index+1);
        else
            return text;
    }

    #endregion


}





