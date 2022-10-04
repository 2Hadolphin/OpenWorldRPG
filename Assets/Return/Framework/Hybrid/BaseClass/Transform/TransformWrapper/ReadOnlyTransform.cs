using UnityEngine;

namespace Return
{
    public interface ReadOnlyTransform : ITransformConvert
    {
        public Vector3 position { get; }
        public Vector3 localPosition { get; }

        public Quaternion rotation { get; }
        public Quaternion localRotation { get; }

        public Vector3 up { get; }
        public Vector3 forward { get; }
        public Vector3 right { get; }

        public Vector3 lossyScale { get; }
        public Vector3 localScale { get; }
    }
}
