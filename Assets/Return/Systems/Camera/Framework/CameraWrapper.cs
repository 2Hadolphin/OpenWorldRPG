using UnityEngine;

namespace Return.Cameras
{
    public class CameraWrapper : CustomCamera
    {
        public override Transform HostObject => Target;
        protected Transform Target;

        private void Start()
        {
            Target = transform;
        }

        public override void SetTarget(Transform tf)
        {
            Target = tf;
        }

    }
}