using Return.Creature;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return.Motions
{
    public class IdleMotion : MotionModule
    {
        public override void SetHandler(IMotionSystem module)
        {

        }

        protected override void FinishModuleMotion()
        {
            enabled = false;
        }

        public override int CompareTo(IMotionModule other)
        {
            return 2;
        }
    }
}