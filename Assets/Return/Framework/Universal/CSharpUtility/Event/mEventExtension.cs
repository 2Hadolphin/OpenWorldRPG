using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Assertions;

namespace Return
{
    public static class mEventExtension
    {
        public static void Subscribe<T>(this EventHandler<T> action, EventHandler<T> callback)
        {
            Assert.IsNotNull(action);
            Assert.IsNotNull(callback);

            action -= callback;
            action += callback;
        }
    }
}