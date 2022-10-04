using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return
{
    public class Vector3BlackBoard : GenericBlackBoard<Vector3>
    {
        public override VirtualValue ValueType => VirtualValue.Vector3;
    }
}