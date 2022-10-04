using System;
using UnityEngine;
[Serializable]
public struct CamMatchData
{
    [SerializeField]
    public CameraConstraint Position;

    /// <summary>
    /// **X=>Pitch **Y=>Yaw
    /// </summary>
    [SerializeField]
    public CameraConstraint Rotation;
}

