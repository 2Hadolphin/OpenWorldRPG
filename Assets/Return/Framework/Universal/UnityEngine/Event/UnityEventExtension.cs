using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

namespace Return
{
    public static class UnityEventExtension
    {
        public static UnityAction AddEvent(this UnityEvent @event, Action action, bool exclusive = false)
        {
            if (exclusive)
                @event.RemoveAllListeners();

            var unityAction = new UnityAction(action);
            @event.AddListener(unityAction);
            return unityAction;
        }

        public static UnityAction<T> AddEvent<T>(this UnityEvent<T> @event, Action<T> action, bool exclusive = false)
        {
            if (exclusive)
                @event.RemoveAllListeners();

            var unityAction = new UnityAction<T>(action);
            @event.AddListener(unityAction);
            return unityAction;
        }


        public static UnityAction<T,U> AddEvent<T,U>(this UnityEvent<T,U> @event, Action<T,U> action, bool exclusive = false)
        {
            if (exclusive)
                @event.RemoveAllListeners();

            var unityAction = new UnityAction<T,U>(action);
            @event.AddListener(unityAction);
            return unityAction;
        }

        [Obsolete]
        public static UnityAction<T, U> RemoveEvent<T, U>(this UnityEvent<T, U> uEvent, Action<T, U> action)
        {
            var unityAction = new UnityAction<T, U>(action);
            uEvent.RemoveListener(unityAction);
            return unityAction;
        }


    }
}