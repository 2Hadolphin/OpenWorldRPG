using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

[AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
public class ModularTagAttribute : PropertyAttribute
{
    public ModularTagAttribute()
    {

    }

}

