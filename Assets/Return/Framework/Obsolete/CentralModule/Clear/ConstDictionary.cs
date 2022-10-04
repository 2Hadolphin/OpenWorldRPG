using System;
using System.Collections;
using UnityEngine;


public struct Enumerator_YieldFlags : IEnumerator
{
    public Enumerator_YieldFlags(int[] @enum,int Main)
    {
        value = Main;

        _enum = @enum;
        index = _enum.Length;
    }
    int value;
    int[] _enum;
    int index;


    public object Current
    {
        get
        {
            return index;
        }
    }


    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public bool MoveNext()
    {
        index -= 1;
        while (index >= 0)
        {
            if (_enum[index]>0)
                return true;
            else
                index -= 1;
        }
        return false;
    }

    public void Reset()
    {
        throw new NotImplementedException();
    }
}
