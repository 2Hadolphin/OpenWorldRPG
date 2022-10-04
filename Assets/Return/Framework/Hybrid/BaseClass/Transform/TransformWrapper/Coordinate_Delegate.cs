using UnityEngine;
using System;

namespace Return
{
    public class Coordinate_Delegate : Coordinate_Offset, ICoordinateDelegate
    {

        public Coordinate_Delegate(Transform transform) : base(transform) { }


        public Vector3 SetPosition;
        public Vector3 SetRotation;
        public override Vector3 position
        {
            get
            {
                return OutputPositionSpace switch
                {
                    Space.World => SetPosition,
                    Space.Self => Transform.localPosition + SetPosition,
                    _ => throw new InvalidProgramException(),
                };
            }
        }
        public override Quaternion rotation
        {
            get
            {
                return OutputPositionSpace switch
                {
                    Space.World => Quaternion.Euler(SetRotation),
                    Space.Self => Transform.localRotation * Quaternion.Euler(SetRotation),
                    _ => throw new InvalidProgramException(),
                };
            }
        }

        public float Weight { get; set; }

        public float InterlockingWieght { get; set; }

        public float PositionWeight { get; set; }

        public float RotationWeight { get; set; }


        public void Dispose()
        {

        }
    }
}
