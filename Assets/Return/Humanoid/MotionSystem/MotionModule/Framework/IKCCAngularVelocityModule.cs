using UnityEngine;

namespace Return.Motions
{
    public interface IKCCAngularVelocityModule
    {
        /// <summary>
        /// This is called when the motor wants to know what its rotation should be right now
        /// </summary>
        void UpdateRotation(ref Quaternion currentRotation, float deltaTime);
    }

}

