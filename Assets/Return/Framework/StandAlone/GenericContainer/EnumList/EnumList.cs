using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Fake flag enum
/// </summary>
[System.Serializable]
[HideLabel]
public struct EnumList<T>
{
    public EnumList(T value)
    {
        mList = new List<T>() { value };
    }

#if UNITY_EDITOR
    [BoxGroup()]
    [ShowInInspector]
    public T AddItem
    {
        get => default;
        set => List.Add(value);
    }

#endif
    [BoxGroup()]
    [ShowInInspector]
    [ListDrawerSettings(Expanded =true)]
    [HideLabel]
    List<T> mList;

    public List<T> List
    {
        get
        {
            if (mList == null)
                mList = new List<T>();

            return mList;
        }
    }    

    public static implicit operator T[] (EnumList<T> list)
    {
        return list.List.ToArray();
    }
}
