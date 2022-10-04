using UnityEngine;

namespace Return.Cameras
{
    public interface ICamWrapper
    {
        Quaternion GetCamCorrectedRotation { get; }
    }
}