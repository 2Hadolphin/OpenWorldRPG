using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return.Cameras
{
    public interface ICameraVolumeProvider
    {
        void AddEffect(Camera camera);
        void RemoveEffect(Camera camera);
    }
}