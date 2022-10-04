using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEngine.Animations.Rigging;
using UnityEngine.Animations;

namespace Return.Editors
{
    public partial class CreateRig
    {
        [MenuItem("Tools/Animation/Rigs/Hand")]
        static void CreateHandRigs()
        {
            var builder = BuildHandRigs().GetComponentInParent<RigBuilder>();
            builder.gameObject.InstanceIfNull<EditorRiggingPlayer>();
        }

        public static Rig BuildHandRigs(Animator animator = null, bool destory = false)
        {
            if (!animator)
                if (!mEditorAnimationUtility.SelectedAnimator(out animator))
                    return null;


            var builder = animator.gameObject.InstanceIfNull<RigBuilder>();

            var rig = new GameObject("Rig_Hand").AddComponent<Rig>();
            builder.layers.Add(new RigLayer(rig, true));


            var right = BuildIK(
                animator,
                rig,
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.RightLowerArm,
                HumanBodyBones.RightHand
                );


            var left = BuildIK(
                animator,
                rig,
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.LeftLowerArm,
                HumanBodyBones.LeftHand
                );

            if (destory)
            {
                rig.gameObject.hideFlags = HideFlags.DontSave;
            }

            return rig;
        }
        public static TwoBoneIKConstraint BuildIK(Animator animator, Rig rig, params HumanBodyBones[] bones)
        {
            rig.transform.SetParent(animator.transform, false);

            var ikRig = new GameObject(bones[2].ToString()).AddComponent<TwoBoneIKConstraint>();
            ikRig.transform.SetParent(rig.transform, false);
            ikRig.data.maintainTargetPositionOffset = true;
            ikRig.data.maintainTargetRotationOffset = true;

            var arm = animator.GetBoneTransform(bones[0]);
            var elbow = animator.GetBoneTransform(bones[1]);
            var hand = animator.GetBoneTransform(bones[2]);


            ikRig.data.root = arm;
            ikRig.data.mid = elbow;
            ikRig.data.tip = hand;

            var target = new GameObject("target").transform;
            target.SetParent(ikRig.transform, false);
            target.Copy(hand);


            ikRig.data.target = target;

            var hint = new GameObject("hint").transform;
            hint.SetParent(ikRig.transform, false);
            hint.Copy(elbow);
            ikRig.data.hint = hint;

            return ikRig;
        }

    }
}