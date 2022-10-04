using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return
{
    public class SwitchPool<T>
    {
        protected SwitchPool() { }

        public SwitchPool(int capacity)
        {
            Values = new T[capacity];
            Cookies = new bool[capacity];
            Init();
        }

        public int Capacity { get; protected set; }

        /// <summary>
        /// Storage collection.
        /// </summary>
        public T[] Values { get; protected set; }

        /// <summary>
        /// State of value.
        /// </summary>
        public bool [] Cookies { get; protected set; }

        public virtual void Init()
        {
            for (int i = 0; i < Capacity; i++)
                Cookies[i] = true;
        }

        public virtual T Get()
        {
            for (int i = 0; i < Capacity; i++)
            {
                if (Cookies[i])
                    return Values[i];
            }

            return default;
        }

        public (bool,T) TryGet()
        {
            for (int i = 0; i < Capacity; i++)
                if (Cookies[i])
                    return (true, Values[i]);

            return (false, default);
        }

        public bool TryGet(out T value)
        {
            for (int i = 0; i < Capacity; i++)
                if (Cookies[i])
                {
                    value = Values[i];
                    return true;
                }

            value = default;
            return false;
        }

        public virtual void Return(T value)
        {
            for (int i = 0; i < Capacity; i++)
            {
                if (Values[i].Equals(value))
                   Reset(i);
            }
        }

        public virtual void Reset(int index)
        {
            Cookies[index] = true;
        }

    }
}