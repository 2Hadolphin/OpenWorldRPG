using UnityEngine;

namespace Return.Motions
{
    /// <summary>
    /// Interface to prvide controller motor velocity.  **Speed  **Inertia
    /// </summary>
    public interface IKCCVelocityModule
    {
        //public delegate void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime);
        //UpdateVelocity Update { get; }

        /// <summary>
        /// This will be invoke before velocity update.
        /// </summary>
        void UpdateInertiaVelocity(ref Vector3 currentInertia, float deltaTime);


        /// <summary>
        /// This is called when the motor wants to know what its velocity should be right now.
        /// </summary>
        /// <param name="currentVelocity">Inertia velocity.</param>
        /// <param name="deltaTime">??</param>
        void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime);
    }

}

