using UnityEngine;
using UnityEngine.Animations;

namespace Return.Animations
{
    public struct StreamAdditiveAlignHandle: IAnimationStreamHandler
    {
        public TransformStreamHandle AlignHandle;
        public TransformStreamHandle ItemHandle;
        public Vector3 Position;
        public Quaternion Rotation;

        TransformStreamHandle IAnimationStreamHandler.Handle { get => ItemHandle; set => ItemHandle = value; }

        public void ProcessStream(ref AnimationStream stream)
        {
#if UNITY_EDITOR
            if (!AlignHandle.IsValid(stream))
            {
                Debug.LogError(AlignHandle + " is not valid.");
                return;
            }

            if (!ItemHandle.IsValid(stream))
            {
                Debug.LogError(ItemHandle + " is not valid.");
                return;
            }
#endif

            var rot = AlignHandle.GetRotation(stream);
            var pos = AlignHandle.GetPosition(stream);

            ItemHandle.SetGlobalTR(stream, pos + rot * Position, Rotation * rot,false);

            //Debug.Log(this);
            //mGizmos.DrawCoordinate(pos + rot * Position, Rotation * rot);
        }
    }
}