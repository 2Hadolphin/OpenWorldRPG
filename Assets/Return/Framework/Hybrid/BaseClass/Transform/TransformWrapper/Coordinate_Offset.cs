using UnityEngine;
using System;

namespace Return
{
    public class Coordinate_Offset : Coordinate
    {
        public Vector3 OffsetPosition;
        public Quaternion OffsetRotation;
        public Space OffsetSpace;


        public virtual Vector3 OriginPosition
        {
            get
            {
                return OutputPositionSpace switch
                {
                    Space.World => Transform.position,
                    Space.Self => Transform.localPosition,
                    _ => throw new InvalidProgramException(),
                };
            }
        }
        public override Vector3 position
        {
            get
            {
                var pos = base.position;
                return OutputPositionSpace switch
                {
                    Space.World => pos + (OffsetSpace == Space.World ? OffsetPosition : Transform.TransformVector(OffsetPosition)),
                    Space.Self => pos + (OffsetSpace == Space.Self ? OffsetPosition : Transform.InverseTransformVector(OffsetPosition)),
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
                    Space.World => Transform.rotation * OffsetRotation,
                    Space.Self => Transform.localRotation * OffsetRotation,
                    _ => throw new InvalidProgramException(),
                };
            }
        }
        public Coordinate_Offset(ITransform transform) : base(transform)
        {

        }
        public Coordinate_Offset(Transform transform, Vector3 offsetPosition = default, Quaternion offsetRotation = default, bool inputViaGlobal = true, Space space = Space.World, Space offsetSpace = Space.Self) : base(transform, space)
        {
            OffsetSpace = offsetSpace;

            if (inputViaGlobal && offsetSpace.HasFlag(Space.Self))
            {
                offsetPosition = transform.InverseTransformPoint(offsetPosition);
                offsetRotation = transform.rotation * Quaternion.Inverse(offsetRotation);
            }

            OffsetPosition = offsetPosition;
            OffsetRotation = offsetRotation;
        }
    }
}
