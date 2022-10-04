using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Method|AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
public class DropZoneAttribute : Attribute
{
    public Type Type;
    //public readonly string Method;
    public string Text;

    public bool AlwaysDrawZone;

    public bool AllowSceneObject;

    #region 

    public Color TextColor= Color.green;


    public int FontSize = 30;

    /// <summary>
    /// Height of drop zone.
    /// </summary>
    public float Height = 97;


    #endregion

    public DropZoneAttribute() { }

    public DropZoneAttribute(Type type =null, string text="Drop asset here", bool alwaysDrawZone=true,bool allowSceneObj=false)
    {
        this.Type = type;
        //this.Method = method;
        this.Text = text;

        this.AlwaysDrawZone = alwaysDrawZone;

        this.AllowSceneObject = allowSceneObj;
    }
}
