using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return
{
    public abstract class CastAttribute: Attribute
    {
        public InjectOption InjectOption = InjectOption.Handler;
        public object ID;

        public bool Optional = false;
    }
}