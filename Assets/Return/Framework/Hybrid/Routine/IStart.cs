using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return
{

    public interface ITickable
    {
        void Invoke();
    }

    public interface IStart//: ITickable
    {
        //void ITickable.Invoke() => Initialize();
        void Initialize();
    }
}