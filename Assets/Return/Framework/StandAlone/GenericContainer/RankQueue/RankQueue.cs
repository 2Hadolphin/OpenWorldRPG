using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Assertions;

public class RankQueue<T>
{
    protected RankQueue(int capacity)
    {
        Assert.IsTrue(capacity > 0);
        List = new(capacity);
    }

    public RankQueue(int capacity,Func<T,bool> selectFunc,bool hotValid=false)
    {
        Assert.IsTrue(capacity > 0);
        List = new(capacity);

        Assert.IsNotNull(selectFunc);
        SelectFunc = selectFunc;

        HotValid=hotValid;
    }

    protected virtual Func<T, bool> SelectFunc { get; set; }
    protected readonly bool HotValid;

    public T Current
    {
        get
        {
            if (!HotValid)
                return _Current;

            var length = List.Count;
            for (int i = 0; i < length; i++)
            {
                var value = List[i];
                if (SelectFunc(value))
                {
                    _Current = value;
                    return value;
                }
            }

            Debug.LogError(new KeyNotFoundException(typeof(T).ToString()));
            return default;
        }
    }



    protected T _Current;

    protected List<T> List;


    public virtual void Add(T value)
    {
        List.Add(value);
    }

    public virtual void Remove(T value)
    {
        List.Remove(value);
    }

#if Return
    

#endif
}
