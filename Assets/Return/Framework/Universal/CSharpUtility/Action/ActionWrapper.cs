using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using UnityEngine.Assertions;

namespace Return
{
    public abstract class ActionWrapper:NullCheck
    {
        public abstract void OnRecive(object obj);
    }



    public class ActionWrapper<T> : ActionWrapper,IEquatable<ActionWrapper<T>>
    {
        public ActionWrapper(Action<T> callback)
        {
            Assert.IsNotNull(callback);
            Callback = callback;
        }

        public Action<T> Callback;

        public override void OnRecive(object obj)
        {
            if (obj is T value)
            {
                Callback?.Invoke(value);
                //Callback = null;
            }
        }

        public override int GetHashCode()
        {
            return Callback.GetHashCode();
        }

        public bool Equals(ActionWrapper<T> other)
        {
            return other.Callback == Callback;
        }


        public static implicit operator Action<object>(ActionWrapper<T> wrapper)
        {
            return wrapper.OnRecive;
        }
    }
}
