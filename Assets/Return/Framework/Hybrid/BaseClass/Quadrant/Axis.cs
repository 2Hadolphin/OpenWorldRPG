using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return
{
    public enum Axis
    {
        X=0,
        Y=1,
        Z=2,
    }

    public static partial class mExtension
    {
        public static int Parse(this Axis axis)
        {
            return (int)axis;
        }
    }
}