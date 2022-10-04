#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Return;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.OdinValidator;
using System.Reflection;
using System;
using JetBrains.Annotations;
using Sirenix.OdinInspector.Editor.Validation;
using UnityEditor;


[assembly: RegisterValidator(typeof(TNetResourceValidator<>))]
public class TNetResourceValidator<T> : AttributeValidator<TNetResourceAttribute, T> where T : UnityEngine.Object
{
    const string PathTag = "Resources/";

    protected override void Validate(ValidationResult result)
    {
        if (this.ValueEntry.SmartValue == null)
            return;

        var targetSource = this.ValueEntry.SmartValue;

        if (!targetSource)
            return;

        var path = AssetDatabase.GetAssetPath(targetSource);
        var index = path.LastIndexOf(PathTag);

        if (index < 0)
        {
            result.ResultType = ValidationResultType.Error;
            result.Message = "Error value source. \n Require asset must under resources folder.";
            return;
        }
        else
            path = path.Substring(index).Remove(0,PathTag.Length);

        index = path.LastIndexOf('.');

        if (index > 0)
            path = path.Substring(0, index);
        
        var pathProperty = Property.Parent.FindChild(prop=> prop.Name == this.Attribute.TargetPath, false);

        if (pathProperty != null)
        {
            pathProperty.ValueEntry.WeakSmartValue = path;
            pathProperty.ValueEntry.ApplyChanges();
        }
        else Debug.Log(pathProperty);

    }

    public override RevalidationCriteria RevalidationCriteria => RevalidationCriteria.OnValueChange;

}


#endif