using UnityEngine;
using System;

namespace Return
{
    public class Coordinate : WrapTransform, ICoordinate
    {
        public Transform Transform { get => tf; }

        /// <summary>
        /// output value via global space or local space
        /// </summary>
        public Space OutputPositionSpace { get; set; }
        /// <summary>
        /// output value via global space or local space
        /// </summary>
        public Space OutputRotationSpace { get; set; }



        public virtual Vector3 position
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
        public virtual Quaternion rotation
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

        public Coordinate(Transform transform, Space space = Space.World) : base(transform)
        {
            OutputPositionSpace = space;
        }

        public Coordinate(ITransform other) : base(other)
        {

        }

        //public bool Equals(ICoordinate coordinate)
        //{
        //    return Transform == coordinate.Transform;
        //}


    }
}
