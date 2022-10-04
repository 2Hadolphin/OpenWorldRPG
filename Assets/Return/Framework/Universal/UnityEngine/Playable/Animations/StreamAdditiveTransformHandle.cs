using UnityEngine;
using UnityEngine.Animations;

namespace Return.Animations
{
    public struct StreamAdditiveTransformHandle: IAnimationStreamHandler
    {
        public TransformStreamHandle Handle;

        TransformStreamHandle IAnimationStreamHandler.Handle { get => Handle; set => Handle = value; }

        public Vector3 Position;
        public Quaternion Rotation;

        public void ProcessStream(ref AnimationStream stream)
        {
#if UNITY_EDITOR
            if (!Handle.IsValid(stream))
            {
                Debug.LogError(Handle + " is not valid.");
                return;
            }
#endif
            var pos = Handle.GetLocalPosition(stream);
            Handle.SetLocalPosition(stream, pos + Position);

            var rot = Handle.GetLocalRotation(stream);
            Handle.SetLocalRotation(stream, Rotation * rot);
        }
    }
}