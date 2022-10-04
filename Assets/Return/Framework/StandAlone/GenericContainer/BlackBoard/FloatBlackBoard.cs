using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Return;
using System;
namespace Return
{
    public class FloatBlackBoard : GenericBlackBoard<float>
    {
        public override VirtualValue ValueType { get => VirtualValue.Float; }
        Dictionary<int, int> Subscribes = new Dictionary<int, int>();

        [Button("Build")]
        void Test()
        {
            var values = (int[])Enum.GetValues(typeof(VirtualValue));
            foreach (var value in values)
                Subscribes.Add(value, 0);
        }

        [Button("Add")]
        void Add()
        {
            Subscribes[(int)ValueType]++;
            Debug.Log(Subscribes[(int)ValueType]);
        }
    }
}

