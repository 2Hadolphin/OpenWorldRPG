using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace Return.Humanoid.Motion
{
    /// <summary>
    /// Motion module token
    /// </summary>
    [Serializable]
    public class MotionOrder : TokenT<MotionOrder>
    {
        [ShowInInspector]
        [ListDrawerSettings(Expanded = true)]
        public HashSet<MotionModulePreset> User=new ();

        public int MaxQueueNumber;
    }
}