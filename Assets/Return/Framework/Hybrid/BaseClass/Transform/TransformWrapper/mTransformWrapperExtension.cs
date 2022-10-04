using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return
{
    public static class mTransformWrapperExtension
    {

        public static ITransform ToInterface(this Transform tf)
        {
            return new WrapTransform(tf);
        }
    }
}
