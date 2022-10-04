using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
public class CoroutineLock :MonoBehaviour, IDisposable, IEnumerator
{

    private static Dictionary<int, CoroutineLock> Lock_Loop = new Dictionary<int, CoroutineLock>(100);
    readonly int hash;
    protected bool waitDispose = false;
    private bool init = false;
    public CoroutineLock(int _hash)
    {
        if (Lock_Loop.TryGetValue(hash, out var old))
        {
            Lock_Loop[_hash] = this;
            old.Reset();
        }
        else
            Register();

        hash = _hash;
    }
    //private static Dictionary<int, Coroutine> Lock_Direct = new Dictionary<int, Coroutine>(50);
    //public CoroutineLock(int _hash, Coroutine coroutine)
    //{
    //    if (Lock_Direct.TryGetValue(hash, out var old))
    //    {
    //        Lock_Direct[_hash] = coroutine;
    //        StopCoroutine(old);//?
    //    }
    //    else
    //        Register();

    //    hash = _hash;
    //}

    public object Current => hash;


    /// <summary>
    /// Must been call by "Using"-Dispose
    /// </summary>
    public void Dispose()
    {
        if (Lock_Loop.ContainsKey(hash))
            Lock_Loop.Remove(hash);
    }

    /// <summary>
    /// Is routine avaliable to go,call while in queue or runtime check
    /// </summary>
    public bool MoveNext()
    {
        if (init) // next round
        {
            if (waitDispose)
                return false;
        }
        else //in queue
        {
            if (Lock_Loop.ContainsKey(hash))
                return false;
            else
                Register();
        }

        return true;
    }

    private void Register()
    {
        Lock_Loop.Add(hash, this);
        init = true;
    }

    /// <summary>
    /// Only call while breaking last coroutine 
    /// </summary>
    public void Reset() => waitDispose = true;
}
