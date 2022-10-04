#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Editor.Validation;
using Return;
using System.Reflection;
[assembly: RegisterValidator(typeof(VirtualBlackBoardValid))]
public class VirtualBlackBoardValid : AttributeValidator<VirtualStateAttribute, AbstractValue>
{
    protected override void Validate(ValidationResult result)
    {
        base.Validate(result);

        if (this.ValueEntry.SmartValue == null)
            return;

        var rquireType = this.Attribute.ValueType;
        var targetType = this.ValueEntry.SmartValue.ValueType;

        if (rquireType != targetType)
        {
            result.ResultType = ValidationResultType.Error;
            result.Message = "Error value state.";
        }

    }
}
#endif