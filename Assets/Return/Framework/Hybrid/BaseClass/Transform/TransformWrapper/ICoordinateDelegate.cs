using UnityEngine;
using System;

namespace Return
{
    public interface ICoordinateDelegate : IDisposable
    {
        Space OutputPositionSpace { get; }
        Space OutputRotationSpace { get; }
        float Weight { get; }
        float PositionWeight { get; }
        float RotationWeight { get; }
        float InterlockingWieght { get; }
        Vector3 position { get; }
        Quaternion rotation { get; }
    }
}
