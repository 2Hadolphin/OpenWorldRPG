using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Return
{
    public interface IEntity<T>
    {
        T GetId();
    }
}
