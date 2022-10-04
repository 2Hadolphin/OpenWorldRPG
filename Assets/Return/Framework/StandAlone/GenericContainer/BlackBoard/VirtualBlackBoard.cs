using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Return
{
    /// <summary>
    /// Contains value type and event id.
    /// </summary>
    [Serializable]
    public class VirtualBlackBoard : AbstractValue
    {
        [SerializeField]
        VirtualValue m_ValueType;

        public override VirtualValue ValueType { get => m_ValueType;  }
        public virtual VirtualValue SetValueType { set => m_ValueType = value; }
    }

    /// <summary>
    /// Provide value identify to white borad.
    /// </summary>
    public abstract class AbstractValue : BlackBoard
    {
        public virtual VirtualValue ValueType { get; set; }
    }


}