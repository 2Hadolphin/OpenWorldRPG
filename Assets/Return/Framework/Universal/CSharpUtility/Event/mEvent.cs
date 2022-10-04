using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using System.Linq;

namespace Return
{
    /// <summary>
    /// <para>Only member of TMember can subscribe this event</para> 
    /// Action event with lock to unsubscribe and safe call event
    /// </summary>
    [Serializable]
    public class mEvent<TMember, TResult> :NullCheck,IEnumerable<TMember>
    {
        protected mEvent() { }
        public mEvent(object owner)
        {
            Owner = owner;
            m_Update += (x) => Result = x;
        }
        [ShowInInspector]
        protected readonly object Owner;
        [ShowInInspector]
        protected HashSet<object> m_EventLock = new HashSet<object>();
        protected event Action<TResult> m_Update;
        public TResult Result { get; protected set; }
        /// <summary>
        /// Event will be trigger after value update
        /// </summary>
        public virtual event Action<TResult> performed
        {
            add
            {
                if (value.Target.GetType().IsSubclassOf(typeof(TMember)))
                {
                    if (m_EventLock.Add(value))
                        m_Update += value;
                    else
                        Debug.LogError(string.Format("Event already subscribe by {0}.", value.Target));

                }
                else
                    Debug.LogError(string.Format( "type of {0} can't not subscribe this event.",value.Target.GetType()));
            }
            remove
            {
                if (m_EventLock.Remove(value)) 
                    m_Update -= value;
            }
        }

        public virtual void Invoke(object sender, TResult result)
        {
            if (Owner.Equals(sender))
            {
                m_Update?.Invoke(result);
            }
        }

        public virtual void Clear()
        {
            foreach (Action<TResult> o in m_EventLock)
            {
                if (o == null)
                    continue;

                m_Update -= o;
            }
            m_EventLock.Clear();
        }

        public IEnumerator GetEnumerator()
        {
            return m_EventLock.Select(x => (TMember)(((Action<TResult>)x)?.Target)).GetEnumerator();
        }

        IEnumerator<TMember> IEnumerable<TMember>.GetEnumerator()
        {
            return m_EventLock.Select(x => (TMember)(((Action<TResult>)x)?.Target)).GetEnumerator();
        }
    }


    /// <summary>
    /// <para>Any object can be as member to subscribe this event</para> 
    /// Action event with lock to unsubscribe and safe call event
    /// </summary>
    public class UEvent<TResult>:mEvent<object, TResult>
    {
        public UEvent(object owner) : base(owner) { }
    }


 
}

