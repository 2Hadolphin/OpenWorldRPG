using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Return;

[System.Serializable]
public struct CollectionExtractor<T>
{
    public CollectionExtractor(IList<T> list,int defaultIndex=0)
    {
        Assert.IsNotNull(list);
        List = list;
        Index = defaultIndex;
    }

    readonly IList<T> List;
 
    public int Index;

    public int GetIndex
    {
        get => Index;
        set
        {
            Index = value;
        }
    }

    public T Next()
    {
        return List.Next(ref Index);
    }

    public T Last()
    {
        return List.Last(ref Index);
    }

}
