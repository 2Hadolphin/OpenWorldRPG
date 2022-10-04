using System;

namespace Return.Items
{
    [Serializable]
    public abstract class BaseItemPostureAdjustData : PresetDatabase
    {
        public abstract bool GetOffset(string key, out PR offset);
    }

    #region Obsolete
    //[Obsolete]
    //public class PoseAdjust_Position : PoseAdjust
    //{
    //    public Vector3 GUIDs;

    //    public override void ProcessAnimationStream(AnimationStream stream)
    //    {
    //        if (Valid(stream))
    //            StreamHandle.SetPosition(stream, GUIDs);
    //    }
    //}

    //[Obsolete]
    //public class PoseAdjust_LocalPosition : PoseAdjust_Position
    //{
    //    public override void ProcessAnimationStream(AnimationStream stream)
    //    {
    //        if (Valid(stream))
    //            StreamHandle.SetLocalPosition(stream, GUIDs);
    //    }
    //}

    //[Obsolete]
    //public class PoseAdjust_Rotation: PoseAdjust
    //{
    //    public Quaternion Rotaion=Quaternion.identity;

    //    public override void ProcessAnimationStream(AnimationStream stream)
    //    {
    //        if(Valid(stream))
    //            StreamHandle.SetRotation(stream, Rotaion);
    //    }
    //}

    //[Obsolete]
    //public class PoseAdjust_LocalRotation : PoseAdjust_Rotation
    //{
    //    public override void ProcessAnimationStream(AnimationStream stream)
    //    {
    //        if (Valid(stream))
    //            StreamHandle.SetLocalRotation(stream, Rotaion);
    //    }
    //}

    //    [Obsolete]
    //    [HideLabel]
    //    [Serializable]
    //    public class OffsetData: BasePoseAdjustData
    //    {
    //        public override IPlayableDisposable CreateAdjustJob(m_items item, Animator anim, PlayableGraph graph)
    //        {
    //            return default;
    //            //var m_duringTransit = new ItemPoseAdjustJob()
    //            //{

    //            //};

    //            //return AnimationScriptPlayable.Create(graph, m_duringTransit);
    //        }


    //#if UNITY_EDITOR
    //        [TabGroup("HumanBodyBone")]
    //        [OnValueChanged(nameof(SetBoneID))]
    //        public HumanBodyBones Bone;
    //        void SetBoneID()
    //        {
    //            ID = Bone.Equals(HumanBodyBones.LastBone) ? null : Bone.ToString();
    //        }

    //        [TabGroup("HumanoidHandle")]
    //        [OnValueChanged(nameof(SetHandleID))]
    //        public Symmetry SetType;

    //        void SetHandleID()
    //        {
    //            ID = SetType.Equals(Symmetry.Deny) ? null : SetType.ToHandle().ToString();
    //        }

    //        [Title("Offset Config")]

    //        [DropZone(typeof(ReadOnlyTransform), "Release m_items Transfrom")]
    //        [OnValueChanged(nameof(SetOffset))]
    //        //[NonSerialized]
    //        [ShowInInspector]
    //        UnityEngine.Object SetTarget;

    //        void SetOffset()
    //        {
    //            if (SetTarget is ReadOnlyTransform tf)
    //                Offset = tf.GetLocalPR();
    //        }

    //#endif
    //        public string ID;
    //        [HideLabel]
    //        public PR Offset;
    //    }

    #endregion
}

