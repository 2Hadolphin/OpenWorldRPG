using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Return
{
    public interface IDisposeEvent<T> : IDisposable
    {
        event Action<T> OnDispose;
    }
}