using UnityEngine;
using UnityEngine.Animations;
//using Return.Animations.IK;
using UnityEngine.Assertions;
using Return;

namespace Return.Animations
{

    public struct StreamAdditivePositionHandle: IAnimationStreamHandler
    {
        public TransformStreamHandle Handle;
        public Vector3 Position;

        TransformStreamHandle IAnimationStreamHandler.Handle { get => Handle; set => Handle = value; }

        public void ProcessStream(ref AnimationStream stream)
        {
#if UNITY_EDITOR
            if(!Handle.IsValid(stream))
            {
                Debug.LogError(Handle + " is not valid.");
                return;
            }    
#endif
            var pos = Handle.GetLocalPosition(stream);
            Handle.SetLocalPosition(stream, pos + Position);
        }
    }
}