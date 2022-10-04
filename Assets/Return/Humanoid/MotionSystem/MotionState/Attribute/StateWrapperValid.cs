# if UNITY_EDITOR

using Sirenix.OdinInspector.Editor.Validation;
using Return;
using System.Reflection;
using UnityEditor;
using UnityEngine;


[assembly: RegisterValidator(typeof(StateWrapperValid))]
public class StateWrapperValid : AttributeValidator<VirtualStateAttribute, StateWrapper>
{
    

    protected override void Validate(ValidationResult result)
    {
        if (this.ValueEntry.SmartValue == null)
        {
            result.ResultType = ValidationResultType.Error;
            result.Message = string.Format("The state inside wrapper is null, please insert {0} state.", Attribute.ValueType);
            return;
        }
         
        var requireType = this.Attribute.ValueType;

        if (this.ValueEntry.SmartValue.State is null)
        {
            result.ResultType = ValidationResultType.Error;
            result.Message = string.Format("The state inside wrapper is null, please insert {0} state.", requireType);
            return;
        }


        var targetType = this.ValueEntry.SmartValue.State.ValueType;

        if (requireType != targetType)
        {
            result.ResultType = ValidationResultType.Error;
            result.Message = string.Format("Error value state. This wrapper field require {0} state.", requireType);

            GUILayout.Button("???");

            result.AddWarning("Target state doesn't match require type.").
                WithButton("Fixed Type", () => this.Value.State.ValueType = requireType);

            //.WithFix(() => this.Value.State.ValueType=requireType);
        }



        if (Attribute.Writable)
        {
            if (ValueEntry.SmartValue.Token is null)
            {
                result.ResultType = ValidationResultType.Error;
                result.Message = "State token require.";
            }
        }

    }

    

    //protected override void Validate(object parentInstance, StateWrapper memberValue, MemberInfo member, ValidationResult result)
    //{
    //    if (this.ValueEntry.SmartValue == null)
    //        return;

    //    var rquireType = this.Attribute.ValueType;
    //    var targetType = this.ValueEntry.SmartValue.State.m_ValueType;

    //    if (rquireType != targetType)
    //    {
    //        result.ResultType = ValidationResultType.Error;
    //        result.Message = "Error value state.";
    //    }
    //}
}


#endif