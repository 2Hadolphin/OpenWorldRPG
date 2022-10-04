using UnityEngine;
using UnityEngine.Assertions;

namespace Return
{
    public class WrapTransform : ITransform, ReadOnlyTransform
    {
        public WrapTransform(Transform transform)
        {
            Assert.IsNotNull(transform);
            tf = transform;
        }

        public WrapTransform(ITransform Itransform)
        {
            tf = (Itransform as WrapTransform).tf;
        }
        protected readonly Transform tf;

        public virtual Vector3 position { get => tf.position; set => tf.position = value; }
        public virtual Vector3 localPosition { get => tf.localPosition; set => tf.localPosition = value; }
        public virtual Quaternion rotation { get => tf.rotation; set => tf.rotation = value; }
        public virtual Quaternion localRotation { get => tf.localRotation; set => tf.localRotation = value; }
        public virtual Vector3 localScale { get => tf.localScale; set => tf.localScale = value; }
        public virtual Vector3 lossyScale { get => tf.lossyScale; }

        public virtual Vector3 up { get => tf.up; set => tf.up = value; }
        public virtual Vector3 forward { get => tf.forward; set => tf.forward = value; }
        public virtual Vector3 right { get => tf.right; set => tf.right = value; }


        public Vector3 TransformVector(Vector3 vector) => tf.TransformVector(vector);
        public Vector3 InverseTransformVector(Vector3 vector) => tf.InverseTransformVector(vector);

        public Vector3 TransformPoint(Vector3 vector) => tf.TransformPoint(vector);
        public Vector3 InverseTransformPoint(Vector3 vector) => tf.InverseTransformPoint(vector);

        public Vector3 TransformDirection(Vector3 vector) => tf.TransformDirection(vector);
        public Vector3 InverseTransformDirection(Vector3 vector) => tf.InverseTransformDirection(vector);

        public Quaternion TransformRotation(Quaternion quaternion) => tf.TransformRotation(quaternion);
        public Quaternion InverseTransformRotation(Quaternion quaternion) => tf.InverseTransformRotation(quaternion);
    }
}
