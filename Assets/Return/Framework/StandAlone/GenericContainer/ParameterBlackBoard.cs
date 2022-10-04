using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine;

namespace Return
{
    public abstract class ParameterBlackBoard<TObject, TResult>:IEnumerable<KeyValuePair<TObject,TResult>>
    {
        public ParameterBlackBoard()
        {
            EventPost = new UEvent<TResult>(this);
        }

        protected Dictionary<TObject, TResult> m_blackboard = new Dictionary<TObject, TResult>();

        public readonly UEvent<TResult> EventPost;

        protected TResult m_Value;

        /// <summary>
        /// Return current value on black board
        /// </summary>
        public virtual TResult GetValue => m_Value;

        /// <summary>
        /// Return last value on black board
        /// </summary>
        public virtual TResult LastValue { get; protected set; }


        /// <summary>
        /// Use this method to add value or update value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="value">new value to update</param>
        /// <returns> Current IValue </returns>
        public virtual void Inject(TObject sender, TResult value, bool postEvent = true)
        {
            LastValue = m_Value;

            if (m_blackboard.TryGetValue(sender, out var old))
            {
                m_blackboard[sender] = value;
                m_Value = NewResult(value, old);
            }
            else
            {
                m_blackboard.Add(sender, value);
                m_Value = NewResult(value, default);
            }

            if (postEvent)
                EventPost.Invoke(this,m_Value);
        }

        public virtual void Unregister(TObject sender, bool postEvent = true)
        {
            LastValue = m_Value;

            if (m_blackboard.TryGetValue(sender, out var old))
            {
                m_blackboard.Remove(sender);
                m_Value = NewResult(default, old);
            }
            else
                return;

            if (postEvent)
                EventPost.Invoke(this,m_Value);
        }

        /// <summary>
        /// Process new result of this black board
        /// </summary>
        /// <returns>New black board result</returns>
        protected abstract TResult NewResult(TResult newValue, TResult oldvalue);

        public IEnumerator<KeyValuePair<TObject, TResult>> GetEnumerator()
        {
            return m_blackboard.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual void Clear(bool cleanListener = false)
        {
            if (cleanListener)
                EventPost.Clear();

            m_blackboard.Clear();
        }
    }


    /// <summary>
    /// key of TSerializable , data type of float
    /// </summary>
    public class BlackBoard_Summary<T> : ParameterBlackBoard<T, float>
    {
        protected override float NewResult(float newValue, float oldvalue)
        {
            return LastValue + newValue - oldvalue;
        }

        public static implicit operator float(BlackBoard_Summary<T> blackBoard)
        {
            return blackBoard.m_Value;
        }
    }

    
    
}

