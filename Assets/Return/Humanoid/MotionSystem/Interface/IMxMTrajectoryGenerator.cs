using UnityEngine;

namespace Return.Motions
{
    public interface IMxMTrajectoryGenerator
    {
        /// <summary>
        /// 
        /// </summary>
        Vector3 InputVector { get; set; }

        /// <summary>
        /// The desired orienation relative to the camera.
        /// </summary>
        Vector3 StrafeDirection { get; set; }

        void ExtractGoal();
    }
}
