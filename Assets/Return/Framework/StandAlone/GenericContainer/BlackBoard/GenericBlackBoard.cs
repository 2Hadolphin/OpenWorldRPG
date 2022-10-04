using UnityEngine;
using Sirenix.OdinInspector;
using Newtonsoft.Json;
namespace Return
{
    /// <summary>
    /// Blackboard with default value
    /// </summary>
    /// <typeparam name="TValue">IValue Type</typeparam>
    public abstract class GenericBlackBoard<TValue> : AbstractValue
    {
        [ShowInInspector][JsonIgnore]
        protected TValue m_Value;
        public virtual TValue Value { get => m_Value; set => m_Value = value; }


        public static implicit operator TValue(GenericBlackBoard<TValue> blackboard )
        {
            return blackboard.m_Value;
        }
    }

}