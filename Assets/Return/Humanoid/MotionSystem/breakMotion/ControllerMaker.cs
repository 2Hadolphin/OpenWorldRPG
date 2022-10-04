using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return.Humanoid.Animation;
namespace Return.Humanoid
{
    public class ControllerMaker
    {
        public static CharacterController Generate(IAdvanceAnimator animator)
        {
            var map = animator.SkeletonMap;
            var controller = animator.RootBone.gameObject.AddComponent<CharacterController>();
            var rRadius=animator.GetSkeketonPathDistance(HumanBodyBones.UpperChest, HumanBodyBones.RightUpperArm, map);
            var lRadius = animator.GetSkeketonPathDistance(HumanBodyBones.UpperChest, HumanBodyBones.LeftUpperArm, map);

            controller.radius = (rRadius + lRadius)*0.5f;

            var rLegLength = animator.GetSkeketonPathDistance(HumanBodyBones.RightUpperLeg, HumanBodyBones.RightFoot, map);
            var lLegLength = animator.GetSkeketonPathDistance(HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftFoot, map);
            var bodyHeight = animator.GetSkeketonPathDistance(HumanBodyBones.Hips, HumanBodyBones.Head, map);

            controller.height = Mathf.Min(rLegLength, lLegLength) + bodyHeight;

            controller.center = new Vector3(0, controller.height * 0.5f);

            return controller;
        }
    }
}