using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Return
{

    public class SafeEvent<T>
    {
        public static SafeEvent<T> Create(out Action<T> invokeHandle)
        {
            var e = new SafeEvent<T>();
            invokeHandle = e.Post;
            return e;
        }

        protected Action<T> Post;
        public event Action<T> Perform
        {
            add
            {
                Post -= value;
                Post += value;
            }

            remove => Post -= value;
        }

        public virtual void Clear()
        {
            Post = null;
        }
    }

    public class mSafeEvent<T> : SafeEvent<T>
    {
        public virtual void Invoke(T arg)
        {
            Post?.Invoke(arg);
        }
    }

}
