using UnityEngine;
using Return;
using Return.Items.Weapons;

namespace Return.Cameras
{
    public abstract class bl_CameraShakerBase : BaseComponent
    {

        /// <summary>
        /// Add or start shake movement
        /// </summary>
        /// <param name="present"></param>
        public abstract void SetShake(ShakerPresent present, string key = "global");

        /// <summary>
        /// Remove/stop a specific shake
        /// </summary>
        /// <param name="key"></param>
        public abstract void RemoveShake(string key);

        /// <summary>
        /// Stop all the shakes
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// Set the current camera position/rotation as the origin position
        /// </summary>
        public abstract void SetCurrentAsOriginPosition();
    }
}