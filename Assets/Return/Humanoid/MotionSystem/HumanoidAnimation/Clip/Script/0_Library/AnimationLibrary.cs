using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using Sirenix.OdinInspector;

[Serializable]
public abstract class AnimationLibrary<T>: ScriptableObject
{
    [Searchable]
    [PropertyOrder(0)]
    [SerializeField]
    public List<ClipCollection> Collections=new List<ClipCollection>();
    public T[] GetParameters
    {
        get
        {
            if (Collections == null)
                return null;

            var length = Collections.Count;

            List<T> value = new List<T>(length);

            for (int i = 0; i < length; i++)
            {
                value.Add(Collections[i].Parameter);
            }

            return value.ToArray();
        }
    }


    [Serializable]
    public struct ClipCollection
    {
        public AnimationClip Clip;
        public T Parameter;
        public bool Mirror;
    }

}



[Serializable]
public class AniamtionLibrary_Vector2 : AnimationLibrary<Vector2>
{

}


[Serializable]
public class AniamtionLibrary_Vector4 : AnimationLibrary<Vector4>
{

}