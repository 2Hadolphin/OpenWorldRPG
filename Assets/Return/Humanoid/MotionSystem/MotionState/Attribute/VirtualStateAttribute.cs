
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return;
using Sirenix.OdinInspector;
using System;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor.Validation;
[assembly: RegisterValidator(typeof(StateWrapperValid))]
#endif

[AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
public class VirtualStateAttribute : PropertyAttribute
{
    public readonly VirtualValue ValueType;
    public readonly bool Writable;


    public VirtualStateAttribute(VirtualValue valueType,bool writable=false)
    {
        this.ValueType = valueType;
        Writable = writable;
    }

    
}

