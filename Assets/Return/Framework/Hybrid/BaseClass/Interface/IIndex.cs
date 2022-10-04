using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IIndex<T>
{
    public T Current { get; set; }
    public T Next { get; set; }
    public T Last { get; set; }
}
