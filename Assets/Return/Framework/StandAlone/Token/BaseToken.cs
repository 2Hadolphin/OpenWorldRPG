using System;
using UnityEngine;

namespace Return
{
    public abstract class BaseToken : PresetDatabase, IToken
    {
        [SerializeField]
        int m_defaultPriority;
        public int DefaultPriority { get => m_defaultPriority; set => m_defaultPriority = value; }

        public abstract int CompareTo(object other);
    }
}