using UnityEngine;

namespace Return.Cameras
{
    public interface IAimer
    {
        #region Yaw
        /// <summary>
        /// target pitch value(axis-width).
        /// </summary>
        float yaw { get; }

        Quaternion yawLocalRotation { get; }


        #endregion

        #region Pitch
        /// <summary>
        /// target pitch value(axis-width).
        /// </summary>
        float pitch { get; }

        Quaternion pitchLocalRotation { get; }

        #endregion


    }

    /// <summary>
    /// ????? rename as ICameraController **Viewport?
    /// </summary>
	public interface IAimController: IAimer
    {
        #region Coordinate

        /// <summary>
        /// Yaw upwrad.
        /// </summary>
        Vector3 yawUp { get; }

        /// <summary>
        /// Yaw forward.
        /// </summary>
        Vector3 heading { get; }

        /// <summary>
        /// AimYaw forward.
        /// </summary>
        Vector3 aimHeading { get; }

        /// <summary>
        /// Pitch forward.
        /// </summary>
        Vector3 forward { get; }



        #endregion


        /// <summary>
        /// Used for slowing turn when zooming, etc
        /// </summary>
        float turnRateMultiplier { get; set; }

        /// <summary>
        /// ????
        /// </summary>
        float steeringRate { get; set; }
        /// <summary>
        /// ????
        /// </summary>
        float aimYawDiff { get; }

        void AddYaw(float rotation);
        void ResetYawLocal();
        void ResetYawLocal(float offset);

        void AddPitch(float rotation);
        void ResetPitchLocal();

        void AddRotation(float y, float p);

        void SetYawConstraints(Vector3 center, float range);
        void SetPitchConstraints(float min, float max);
        void SetHeadingConstraints(Vector3 center, float range);
        void ResetYawConstraints();
        void ResetPitchConstraints();
        void ResetHeadingConstraints();

        Transform transform { get; }
    }
}