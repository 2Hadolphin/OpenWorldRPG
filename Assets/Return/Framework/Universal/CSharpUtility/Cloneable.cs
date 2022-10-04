using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Return
{
    [Serializable]
    public class Cloneable: NullCheck
    {
        /// <summary>
        /// Clone data and inject into a new instance
        /// </summary>
        public T Copy<T>() where T : Cloneable
        {
            return MemberwiseClone() as T;
        }
    }
}