using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Return.Framework.Stats
{
    ///<summary>  Bool Scriptable Variable. Based on the Talk - Game Architecture with Scriptable Objects by Ryan Hipple </summary>
    public class BoolVar : ScriptableVar
    {
        [SerializeField] private bool value;

        /// <summary>Invoked when the value changes </summary>
        public Action<bool> OnValueChanged = delegate { };

        /// <summary> Value of the Bool variable</summary>
        public virtual bool Value
        {
            get => value;
            set
            {
                //if (this.value != value)                  //If the value is diferent change it
                {
                    this.value = value;
                    OnValueChanged(value);         //If we are using OnChange event Invoked
//#if UNITY_EDITOR
//                    if (debug) Debug.Log($"<B>{name} -> [<color=blue> {value} </color>] </B>", this);
//#endif
                }
            }
        }

        public virtual void SetValue(BoolVar var) => SetValue(var.Value);

        public virtual void SetValue(bool var) => Value = var;
        public virtual void SetValueInverted(bool var) => Value = !var;
        public virtual void Toggle() => Value ^= true;
        public virtual void UpdateValue() => OnValueChanged?.Invoke(value);

        public static implicit operator bool(BoolVar reference) => reference.Value;
    }

    [Serializable]
    public class BoolReference : ReferenceVar
    {
        public bool ConstantValue;
        [Required] 
        BoolVar m_Variable;

        public BoolReference() => Value = false;

        public BoolReference(bool value) => Value = value;

        public BoolReference(BoolVar value) => Value = value.Value;

        public bool Value
        {
            get => UseConstant || Variable == null ? ConstantValue : Variable.Value;
            set
            {
                // Debug.Log(value);
                if (UseConstant || Variable == null)
                    ConstantValue = value;
                else
                    Variable.Value = value;
            }
        }

        public BoolVar Variable { get => m_Variable; set => m_Variable = value; }

        #region Operators
        public static implicit operator bool(BoolReference reference) => reference.Value;
        #endregion
    }

}
