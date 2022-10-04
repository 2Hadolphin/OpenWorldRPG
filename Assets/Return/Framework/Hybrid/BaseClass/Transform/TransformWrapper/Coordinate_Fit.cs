using UnityEngine;
using System;

namespace Return
{
    /// <summary>
    /// handle with bounds
    /// </summary>
    public class Coordinate_Fit : Coordinate
    {
        public readonly Bounds Bounds;
        public override Vector3 position
        {
            get
            {
                var pos = base.position;
                return OutputPositionSpace switch
                {
                    Space.World => pos,
                    Space.Self => pos,
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
                    Space.World => Transform.rotation,
                    Space.Self => Transform.localRotation,
                    _ => throw new InvalidProgramException(),
                };
            }
        }
        public Coordinate_Fit(Transform transform, bool inputViaGlobal = true, Space space = Space.World) : base(transform, space)
        {

        }
    }
}
