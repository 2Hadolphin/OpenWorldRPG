using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Runtime.CompilerServices;
using System;
using System.Threading.Tasks;

namespace Return
{
    public static class AwaitExtension
    {
        public static TaskAwaiter GetAwaiter(this TimeSpan timeSpan)
        {
            return Task.Delay(timeSpan).GetAwaiter();
        }
    }
}
