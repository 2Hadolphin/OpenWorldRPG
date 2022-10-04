using UnityEngine;
using UnityEngine.Animations;

namespace Return.Animations
{
    public struct StreamAdditiveRotationHandle: IAnimationStreamHandler
    {
        public TransformStreamHandle Handle;
        public Quaternion Rotation;

        TransformStreamHandle IAnimationStreamHandler.Handle { get => Handle; set =>Handle=value; }

        public void ProcessStream(ref AnimationStream stream)
        {
#if UNITY_EDITOR
            if (!Handle.IsValid(stream))
            {
                Debug.LogError(Handle + " is not valid.");
                return;
            }
#endif
            var rot = Handle.GetLocalRotation(stream);
            Handle.SetLocalRotation(stream, Rotation * rot);
        }
    }
}