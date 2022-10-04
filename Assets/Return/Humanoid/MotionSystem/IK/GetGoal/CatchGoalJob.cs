using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using System.Linq;

public class GetBodyPositionAnimPose : IHumanStreamJob
{
    public Vector3 BodyPosition;

    public Vector3 NewBodyPosition;


    public void Slove(AnimationHumanStream hs)
    {
        BodyPosition = hs.bodyPosition;

        hs.bodyPosition = NewBodyPosition;
    }
}

public class GetGoalAnimPose: IHumanStreamJob
{
    public AvatarIKGoal Goal;

    public Vector3 GoalPosition;
    public Quaternion GoalRotation;

    public void Slove(AnimationHumanStream hs)
    {
        GoalPosition = hs.GetGoalPositionFromPose(Goal);
        GoalRotation = hs.GetGoalRotationFromPose(Goal);
    }
}

public class SetGoal : IHumanStreamJob
{
    public AvatarIKGoal Goal;

    public float GoalPositionWeight;
    public float GoalRotationWeight;

    public Vector3 GoalPosition;
    public Quaternion GoalRotation;

    public void Slove(AnimationHumanStream hs)
    {
        hs.SetGoalWeightPosition(Goal, GoalPositionWeight);

        hs.SetGoalPosition(Goal, GoalPosition);

        hs.SetGoalWeightRotation(Goal, GoalRotationWeight);

        hs.SetGoalRotation(Goal, GoalRotation);
    }
}



public interface IHumanStreamJob
{
    public void Slove(AnimationHumanStream hs);
}