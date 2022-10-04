using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Return
{
    public abstract class TokenT<T> : BaseToken, IComparable<TokenT<T>>
    {
        /// <summary>
        /// ???
        /// </summary>
        protected virtual bool Loop=>false;

        [Tooltip("Which token has higher priority than this ")]
        [ShowInInspector]
        [ListDrawerSettings(Expanded = true)]
        public HashSet<TokenT<T>> HigherPriority=new();

        [Tooltip("Which token has lower priority than this ")]
        [ShowInInspector]
        [ListDrawerSettings(Expanded = true)]
        public HashSet<TokenT<T>> LowerPriority = new();

        /// <summary>
        /// Whether this token takes precedence over than other
        /// </summary>
        /// <returns>
        /// <para>(-1)=>Priority higher than other </para>
        /// <para>( 0)=>Priority equals to other </para> 
        /// <para>( 1)=>Priority lower than other </para>
        /// </returns>
        public int CompareTo(TokenT<T> other)
        {
            if (HigherPriority != null)
                if (HigherPriority.Contains(other))
                    return 1;

            if (LowerPriority != null)
                if (LowerPriority.Contains(other))
                    return -1;


            if (other.HigherPriority != null)
                if (other.HigherPriority.Contains(this))
                    return -1;

            if (other.LowerPriority != null)
                if (other.LowerPriority.Contains(this))
                    return 1;

            return DefaultPriority;
        }

        /// <summary>
        /// Default retun unbreak while doesn't match the tyoe.
        /// </summary>
        public override int CompareTo(object other)
        {
            if (other is IComparable<TokenT<T>> token)
                return CompareTo(token);
            else
                return DefaultPriority; 
        }
    }
}