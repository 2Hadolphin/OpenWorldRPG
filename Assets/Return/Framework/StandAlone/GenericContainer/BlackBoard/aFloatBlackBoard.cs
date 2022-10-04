using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return
{
    [Obsolete]
    public class aFloatBlackBoard : BlackBoard
    {
        public float Value;

        public static implicit operator float(aFloatBlackBoard @float)
        {
            return @float.Value;
        }
    }
}

