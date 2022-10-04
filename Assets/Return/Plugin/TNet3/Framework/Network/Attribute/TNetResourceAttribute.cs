using UnityEngine;
using System;

//#if UNITY_EDITOR
//using Sirenix.OdinInspector.Editor.Validation;
//[assembly: RegisterValidator(typeof(TNetResourceDrawer))]
//#endif

//[assembly: RegisterValidator(typeof(TNetResourceValidator<>))]
[AttributeUsage(AttributeTargets.All, Inherited = true)]
public class TNetResourceAttribute : PropertyAttribute
{
    public string TargetPath;

    public TNetResourceAttribute(string pathField)
    {
        TargetPath = pathField;
    }
}

