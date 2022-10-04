using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Return;

public struct Job_DebugDraw : IAnimationJob//IAnimationWindowPreview
{
    public TransformStreamHandle handle;
    public Vector3 Rotation;
    public void ProcessAnimation(AnimationStream stream)
    {

        var humanStream = stream.AsHuman();

        handle.SetLocalRotation(stream, Quaternion.Euler(Rotation));

        //        humanStream.SetGoalLocalRotation(AvatarIKGoal.RightLeg, Quaternion.identity);
        //      humanStream.SetGoalLocalRotation(AvatarIKGoal.LeftLeg, Quaternion.identity);

        HumanoidAnimationUtility.DrawCoordinate(humanStream, AvatarIKGoal.RightFoot);
        HumanoidAnimationUtility.DrawCoordinate(humanStream, AvatarIKGoal.LeftFoot);
    }

    public void ProcessRootMotion(AnimationStream stream)
    {

    }
}
