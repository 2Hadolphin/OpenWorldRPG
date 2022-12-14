using UnityEngine;
using Return;

namespace Return.Cameras
{
    public abstract class bl_CameraMotionBase : BaseComponent
    {
        /// <summary>
        /// Add an extra camera rotation to the camera motion
        /// </summary>
        /// <param name="rotation"></param>
        public abstract void AddCameraRotation(Quaternion rotation);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        /// <param name="amplitude"></param>
        public abstract void SetActiveBreathing(bool active, float amplitude = 0);

        public Transform CachedTransform;

        protected virtual void Awake()
        {
            CachedTransform = transform;
        }
    }
}