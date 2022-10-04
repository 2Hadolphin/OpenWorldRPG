using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetsPresetValidAttribute : Attribute
{
    public string GUIDGetter;
    public string SourceTypeGetter;
    public AssetsPresetValidAttribute(string guidGetter,string sourceTypeGetter)
    {
        GUIDGetter = guidGetter;
        SourceTypeGetter = sourceTypeGetter;
    }
}
