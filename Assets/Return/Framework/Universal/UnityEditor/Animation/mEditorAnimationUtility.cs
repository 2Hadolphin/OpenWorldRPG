using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using Object = UnityEngine.Object;
using System.Linq;
using UnityEngine.Animations;
using Return;
using System.Collections.Generic;
using Newtonsoft.Json;

public class mEditorAnimationUtility
{
    #region Animation

#if UNITY_EDITOR
    [MenuItem("Tools/Animation/ForceT-Pose")]
    public static void TPose()
    {
        if (EditorUtilityTools.GetSelectedComponent(out Animator animator))
        {
            Undo.RegisterFullObjectHierarchyUndo(animator.gameObject, nameof(AnimatorExtension.ForceTPose));
            AnimatorExtension.ForceTPose(animator);
        }
    }
#endif


    public static bool SelectedAnimator(out Animator animator)
    {
        animator = null;

        var selected = Selection.activeGameObject;
        if (!selected)
            return false;

        if (!selected.TryGetComponent(out animator))
            return false;

        return true;
    }

    [MenuItem("Tools/Animation/CapturePose")]
    public static void Capture()
    {
        if (!SelectedAnimator(out var mAnimator))
            return;

        var time = 1;

        var poseHandler = new HumanPoseHandler(mAnimator.avatar, mAnimator.transform);
        var pose = new HumanPose();

        poseHandler.GetHumanPose(ref pose);

        var clip = new AnimationClip();

        #region Muscle
        var muscles = pose.muscles;
        var length = MuscleHandle.muscleHandleCount;
        var handles = new MuscleHandle[length];
        MuscleHandle.GetMuscleHandles(handles);
        var type = typeof(Animator);
        var bindings = handles.Select(x => new EditorCurveBinding() { propertyName = x.name, type = type }).ToArray();
        var curves = muscles.Select(x => new AnimationCurve(new Keyframe(0, x), new Keyframe(time, x))).ToArray();

        AnimationUtility.SetEditorCurves(clip, bindings, curves);
        #endregion

        var root = mAnimator.transform;
        var mroot = root;
        var avatar = mAnimator.avatar;
        var dic = avatar.humanDescription.skeleton.ToDictionary(x => x.name);
        var scale = mAnimator.humanScale;

        #region Root(Hips)
        var bone = mAnimator.GetBoneTransform(HumanBodyBones.Hips);
        BindEditorCurve("CharacterTransform",dic, root,mroot, bone, out bindings, out var values,true,scale);
        curves = values.Select(x => new AnimationCurve(new Keyframe(0,x),new Keyframe(time,x))).ToArray(); 

        AnimationUtility.SetEditorCurves(clip, bindings, curves);
        #endregion

        mroot = bone;
        
        #region RightFoot
        bone = mAnimator.GetBoneTransform(HumanBodyBones.RightFoot);

        BindEditorCurve("RightFoot", dic, root,mroot, bone, out bindings, out values);
        curves = values.Select(x => new AnimationCurve(new Keyframe(0, x), new Keyframe(time, x))).ToArray();

        AnimationUtility.SetEditorCurves(clip, bindings, curves);
        #endregion

        #region LeftFoot
        bone = mAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);

        BindEditorCurve("LeftFoot", dic, root,mroot, bone, out bindings, out values);
        curves = values.Select(x => new AnimationCurve(new Keyframe(0, x), new Keyframe(time, x))).ToArray();

        AnimationUtility.SetEditorCurves(clip, bindings, curves);
        #endregion


        //#region RightHand
        //bone = mAnimator.GetBoneTransform(HumanBodyBones.RightHand);

        //BindEditorCurve("RightHand", root, bone, out bindings, out values);
        //curves = values.Select(x => new AnimationCurve(new Keyframe(0, x), new Keyframe(time, x))).ToArray();

        //AnimationUtility.SetEditorCurves(clip, bindings, curves);
        //#endregion

        //#region LeftHand
        //bone = mAnimator.GetBoneTransform(HumanBodyBones.LeftHand);

        //BindEditorCurve("LeftHand", root, bone, out bindings, out values);
        //curves = values.Select(x => new AnimationCurve(new Keyframe(0, x), new Keyframe(time, x))).ToArray();

        //AnimationUtility.SetEditorCurves(clip, bindings, curves);
        //#endregion

        var path =EditorHelper.SaveFilePanel("Save Clip", string.Empty, mAnimator.gameObject.name + ".asset", "asset");

        AssetDatabase.CreateAsset(clip, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Animation/Debug")]
    public static void m_Debug()
    {
        if (!SelectedAnimator(out var mAnimator))
            return;

        var avatar=mAnimator.avatar;
        var posehandler = new HumanPoseHandler(avatar, mAnimator.transform);

        var humanpose = new HumanPose();

        //posehandler.GetHumanPose(ref humanpose);

        //var avatarPose = new NativeArray<float>();
        //posehandler.GetInternalAvatarPose(avatarPose);


        posehandler.GetInternalHumanPose(ref humanpose);


        posehandler.SetHumanPose(ref humanpose);

    }


    public static void BindEditorCurve(string name,Dictionary<string,SkeletonBone> dic,Transform animator,Transform root, Transform goal,out EditorCurveBinding[] bindings,out float[] values,bool isRoot=false,float humanScle=1f)
    {
        var type = typeof(Animator);

        bindings = new[]
        {
            new EditorCurveBinding(){propertyName=string.Format("{0}T.x",name),type=type},
            new EditorCurveBinding(){propertyName=string.Format("{0}T.y",name),type=type},
            new EditorCurveBinding(){propertyName=string.Format("{0}T.z",name),type=type},
            new EditorCurveBinding(){propertyName=string.Format("{0}Q.x",name),type=type},
            new EditorCurveBinding(){propertyName=string.Format("{0}Q.y",name),type=type},
            new EditorCurveBinding(){propertyName=string.Format("{0}Q.z",name),type=type},
            new EditorCurveBinding(){propertyName=string.Format("{0}Q.w",name),type=type},
        };



        Vector3 pos=default;
        Quaternion rot=Quaternion.identity;
        if (isRoot)
        {

            if (dic!=null&& dic.TryGetValue(goal.name, out var skeleton))
            {
                var inverse = Quaternion.Inverse(skeleton.rotation);

                rot = goal.localRotation * inverse;
                if (isRoot)
                {

                }
            }
            else
                rot = goal.localRotation;//rotation*Quaternion.Inverse(root.rotation);

        }
        else
        {

        }

        pos = animator.InverseTransformVector(goal.position - root.position);


        values = new[]
        {
            pos.x,
            pos.y,
            pos.z,
            rot.x,
            rot.y,
            rot.z,
            rot.w,
        };

    }


    [MenuItem("Tools/Animation/HumanPose/CopyPose")]
    static void CopyPose()
    {
        if (!SelectedAnimator(out var animator))
            return;

        var handle = new HumanPoseHandler(animator.avatar, animator.transform);

        var pose = new HumanPose();

        handle.GetHumanPose(ref pose);


        var json = JsonConvert.SerializeObject(pose,new JsonSerializerSettings() { ReferenceLoopHandling=ReferenceLoopHandling.Ignore});
        Debug.Log(json);

        EditorGUIUtility.systemCopyBuffer = json;
        handle.Dispose();
    }

    [MenuItem("Tools/Animation/HumanPose/PastePose")]
    static void PastePose()
    {
        if (!SelectedAnimator(out var animator))
            return;

        var handle = new HumanPoseHandler(animator.avatar, animator.transform);
        var json = EditorGUIUtility.systemCopyBuffer;
        Debug.Log(json);
        var pose = JsonConvert.DeserializeObject<HumanPose>(json);

        handle.GetHumanPose(ref pose);
        handle.Dispose();
    }

    public static AnimationClip TPoseClip(Animator animator=null)
    {
        var clip = new AnimationClip();
        var type = typeof(Animator);
        var time = 1f;
        var handles = new MuscleHandle[MuscleHandle.muscleHandleCount];
        MuscleHandle.GetMuscleHandles(handles);
        var curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(time, 0));
        foreach (var handle in handles)
        {
            var binding = EditorCurveBinding.DiscreteCurve(string.Empty, type, handle.name);
            AnimationUtility.SetEditorCurve(clip, binding, curve);
        }

        if (animator)
        {
            var root = animator.transform;
            var bone = animator.GetBoneTransform(HumanBodyBones.Hips);
            BindEditorCurve("CharacterTransform", null, root, root, bone, out var bindings, out var values, true, animator.humanScale);
            var curves = values.Select(x => new AnimationCurve(new Keyframe(0, x), new Keyframe(time, x))).ToArray();
            AnimationUtility.SetEditorCurves(clip, bindings, curves);

            var hips = bone;

            #region RightFoot
            bone = animator.GetBoneTransform(HumanBodyBones.RightFoot);

            BindEditorCurve("RightFoot", null, root, hips, bone, out bindings, out values);
            curves = values.Select(x => new AnimationCurve(new Keyframe(0, x), new Keyframe(time, x))).ToArray();

            AnimationUtility.SetEditorCurves(clip, bindings, curves);
            #endregion

            #region LeftFoot
            bone = animator.GetBoneTransform(HumanBodyBones.LeftFoot);

            BindEditorCurve("LeftFoot", null, root, hips, bone, out bindings, out values);
            curves = values.Select(x => new AnimationCurve(new Keyframe(0, x), new Keyframe(time, x))).ToArray();

            AnimationUtility.SetEditorCurves(clip, bindings, curves);
            #endregion
        }

        return clip;
    }


    #endregion
}
