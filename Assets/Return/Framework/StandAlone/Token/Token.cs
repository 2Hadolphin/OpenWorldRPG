using System;
using UnityEngine;

namespace Return
{

    public class Token: BaseToken,IComparable<Token>
    {
        [SerializeField]
        Category m_Category;
        public Category Category { get => m_Category; set => m_Category = value; }


        [SerializeField]
        byte m_Priority;
        public int Priority { get => m_Priority; set => m_Priority = (byte)value; }

        public static implicit operator int (Token token)
        {
            return token.GetHashCode();
        }

        public override int CompareTo(object other)
        {
            if (other is Token token)
                return CompareTo(token);
            else
                return 0;
        }

        public int CompareTo(Token other)
        {
            return Priority - other.Priority;
        }
    }
}