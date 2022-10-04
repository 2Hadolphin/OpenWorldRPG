using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace Return.Cameras
{
    /// <summary>
    /// Interface for custom camera works with cinemachine.
    /// </summary>
    public interface IVirtualCamera
    {
        void SetBrain(CinemachineBrain brain);
        void SetCamera(Camera camera);

    }
}