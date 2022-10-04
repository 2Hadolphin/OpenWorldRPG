using UnityEngine;

namespace Return
{
    public interface ITransformConvert
    {
        public Vector3 TransformVector(Vector3 vector);
        /// <summary>
        /// World to local
        /// </summary>
        public Vector3 InverseTransformVector(Vector3 vector);

        public Vector3 TransformPoint(Vector3 vector);
        /// <summary>
        /// World to local
        /// </summary>
        public Vector3 InverseTransformPoint(Vector3 vector);

        public Vector3 TransformDirection(Vector3 vector);
        /// <summary>
        /// World to local
        /// </summary>
        public Vector3 InverseTransformDirection(Vector3 vector);

        public Quaternion TransformRotation(Quaternion quaternion);
        /// <summary>
        /// World to local
        /// </summary>
        public Quaternion InverseTransformRotation(Quaternion quaternion);

    }
}
