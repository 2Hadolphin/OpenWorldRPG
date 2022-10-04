using Sirenix.OdinInspector.Editor.Validation;
//using Return;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using Return.Database;

[assembly: RegisterValidator(typeof(AssetsPresetValidator<>))]
public class AssetsPresetValidator<T> : AttributeValidator<AssetsPresetValidAttribute, T> where T : UnityEngine.Object
{
    public override RevalidationCriteria RevalidationCriteria => RevalidationCriteria.OnValueChange;

    ValueResolver<string> vr_GUID;
    ValueResolver<AssetsSourceType> vr_SourceType;

    protected override void Initialize()
    {
        vr_GUID = ValueResolver.Get<string>(this.Property, this.Attribute.GUIDGetter);
        vr_SourceType = ValueResolver.Get<AssetsSourceType>(this.Property, this.Attribute.SourceTypeGetter);
    }

    protected override void Validate(ValidationResult result)
    {
        string path = string.Empty;
        // valid source reference
        if (this.ValueEntry.SmartValue == null)
        {
            // search via guid 
            var guid = vr_GUID.GetValue();

            path = AssetDatabase.GUIDToAssetPath(guid);

            if (string.IsNullOrEmpty(path))
            {
                result.Message = "Asset source required.";
                result.ResultType = ValidationResultType.Error;
                return;
            }
            else
            {
                this.ValueEntry.SmartValue = AssetDatabase.LoadAssetAtPath<T>(path);
                //Property.ValueEntry.WeakSmartValue = AssetDatabase.LoadAssetAtPath<TSerializable>(path);
                Property.ValueEntry.ApplyChanges();
            }
        }

        // valid source path
        switch (vr_SourceType.GetValue())
        {
            case AssetsSourceType.Internal:
                break;
            case AssetsSourceType.Resources:
                if(!path.Contains("Resources"))
                {
                    result.Message = "Asset source require resource loader.";
                    result.ResultType = ValidationResultType.Error;
                    return;
                }
                break;
            case AssetsSourceType.AssetsBundle:
                break;
            case AssetsSourceType.Serialize:
                break;
        }


        //var targetSource = this.ValueEntry.SmartValue;

        //if (!targetSource)
        //    return;

        //var path = AssetDatabase.GetAssetPath(targetSource);
        //var index = path.LastIndexOf(PathTag);

        //if (index < 0)
        //{
        //    result.ResultType = ValidationResultType.Error;
        //    result.Message = "Error value source. \n Require asset must under resources folder.";
        //    return;
        //}
        //else
        //    path = path.Substring(index).Remove(0, PathTag.Length);

        //index = path.LastIndexOf('.');

        //if (index > 0)
        //    path = path.Substring(0, index);

        //var pathProperty = Property.Parent.FindChild(prop => prop.Name == this.Attribute.TargetPath, false);

        //if (pathProperty != null)
        //{
        //    pathProperty.ValueEntry.WeakSmartValue = path;
        //    pathProperty.ValueEntry.ApplyChanges();
        //}
        //else Debug.Log(pathProperty);

    }


}
