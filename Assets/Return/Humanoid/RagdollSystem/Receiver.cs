using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;


namespace Return
{
    public abstract class Receiver:IEquatable<Receiver>
    {
        public abstract void Invoke<T>(object owner, T arg);
        public abstract Type mType { get; }
        public override int GetHashCode()
        {
            return mType.GetHashCode();
        }

        public bool Equals(Receiver other)
        {
            return mType == other.mType;
        }

        public static implicit operator Type (Receiver receiver)
        {
            return receiver.mType;
        }
    }

    /// <summary>
    /// Event arg listener
    /// </summary>
    /// <typeparam name="T">type of event arg</typeparam>
    public class Receiver<T> : Receiver
    {
        public Receiver(object owner)
        {
            Owner = owner;
            Type = typeof(T);
        }

        protected readonly object Owner;


        public override Type mType => Type;
        public Type Type;

        public event Action<Receiver> Dispose;

        protected event Action<T> mPost;
        protected HashSet<object> Lock=new HashSet<object>();


        public event Action<T> Post 
        { 
            add
            {
                if (Lock.Add(value))
                    mPost += value;
            }
            remove
            {
                if (Lock.Remove(value))
                {
                    mPost -= value;
                    if (Lock.Count == default)
                        Dispose?.Invoke(this);
                }
            }
        }
        
        public override void Invoke<TArg>(object owner,TArg arg)
        {
            Assert.IsTrue(owner == Owner);

            if (arg.GetType() is T value)
            {
                mPost?.Invoke(value);
            }
        }
    }
}
