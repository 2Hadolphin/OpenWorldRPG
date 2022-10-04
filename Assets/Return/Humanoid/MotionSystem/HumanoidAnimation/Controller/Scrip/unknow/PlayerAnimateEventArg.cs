
using System;
using UnityEngine;

namespace Return.Definition
{
    public class AnimConfigArg : EventArgs
    {
        public enum Option { Null=0,HandSecond=1,ActionAll}
        public readonly Option option;
        public readonly float weight;
        public AnimConfigArg(Option _option,float _weight)
        {
            option = _option;
            weight = _weight;
        }
    }

    public enum TargetedType { Null = 0, Item = 1, Slot = 2 }
    public class AnimTargetedPost : EventArgs
    {

        public TargetedType type;
        public enum Order { Prime=1,Second=2}
        public Order side;
        public AnimTargetedPost(TargetedType _type,Order _side)
        {
            type = _type;
            side = _side;
        }
    }
    public class FootStepArg : EventArgs
    {
        public readonly Side GroundedSide;
        public readonly bool enable;
        public FootStepArg(Side step, bool Enable)
        {
            GroundedSide = step;
            enable = Enable;
        }

    }

}