using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Return
{
    [Obsolete]
    public static class mBaseClassExtension
    {
        [Obsolete]
        public static PR WrapAngle(this PR pr)
        {
            var rot = pr.eulerAngles;

            pr.Rotation = Quaternion.Euler(
                MathfUtility.WrapAngle(rot.x),
                            MathfUtility.WrapAngle(rot.y),
                                        MathfUtility.WrapAngle(rot.z)
                );

            return pr;
        }



    }
}
