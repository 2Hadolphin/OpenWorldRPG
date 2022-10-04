using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return
{
    public interface INode<T> : IChilds<T>, IParents<T>
    {

    }
    public interface IParents<T>
    {
        IEnumerator<T> Parents { get; }
    }
    public interface IChilds<T>
    {
        IEnumerator<T> Childs { get; }
    }
}