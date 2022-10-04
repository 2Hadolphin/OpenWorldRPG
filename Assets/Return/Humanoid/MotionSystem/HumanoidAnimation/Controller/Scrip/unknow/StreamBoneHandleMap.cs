using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
namespace Return.Humanoid.Animation
{
    public class StreamBoneHandleMap
    {
        public StreamBoneHandleMap(Animator animator)
        {
            Handles = HumanoidAnimationUtility.GetStreamBones(animator);
        }
        protected readonly TransformStreamHandle[] Handles;


        public TransformStreamHandle GetHandle(HumanBodyBones humanBodyBones)
        {
            return Handles[(int)humanBodyBones];
        }
    }
}