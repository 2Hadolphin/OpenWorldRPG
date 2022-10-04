using System;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Assertions;

namespace Return.Framework.Parameter
{

    /// <summary>
    /// Cache values in the same frame to prevent performance overlap. 
    /// </summary>
    [HideLabel]
    [Serializable]
    public abstract class Parameters<T> : NullCheck, IValue<T>
    {
        //protected override object value { get => m_Value; set => m_Value = (TSerializable)value; }

        [ShowInInspector]
        protected T m_Value;

        public virtual event Action<T> OnValueChange;

        public abstract T GetValue();


        public static implicit operator T(Parameters<T> parameters)
        {
            try
            {
                Assert.IsFalse(parameters==null);
                return parameters.GetValue();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return default;
            }
        }

        public static implicit operator Func<T>(Parameters<T> parameters)
        {
            try
            {
                Assert.IsFalse(parameters == null);
                return parameters.GetValue;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return ()=>default;
            }
        }

    }
}