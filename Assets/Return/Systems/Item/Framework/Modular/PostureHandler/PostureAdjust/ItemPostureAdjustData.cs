using System.Collections;
using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Return.Creature;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Unity.Collections;
using Return.Animations;
using UnityEngine.Assertions;
using Return.Humanoid.Motion;
using Return.Humanoid;

namespace Return.Items
{


    /// <summary>
    /// Storage item space config(offset) information **Hands **Rect **others
    /// </summary>
    [Serializable]
    public class ItemPostureAdjustData : BaseItemPostureAdjustData
    {
        /// <summary>
        /// 
        /// </summary>
        public virtual bool GetHumanoidConfig(Limb offsetType, out PR offset)
        {
            var key = offsetType.ToString();
            return GetOffset(key, out offset);
        }

        public override bool GetOffset(string key, out PR offset)
        {
            if(AdjustData is ItemHandPostureConfig handConfig)
            {
                switch (key)
                {
                    case nameof(ItemHandPostureConfig.Item):

                        break;

                    case nameof(ItemHandPostureConfig.LeftHand):

                        break;

                    case nameof(ItemHandPostureConfig.RightHand):

                        break;

                }
            }
            else
            {

            }


            offset = PR.Default;
            return false;
        }


        public bool CreateStreamAdjustJob(AbstractItem item,PlayableGraph graph,out IPlayableDisposable playableDisposable)
        {
            Debug.Log("Creating stream adjust m_duringTransit "+this);

            //var m_duringTransit= Playable.Create(graph);

            //var data = new ItemPoseAdjustJob() { };

            //var m_duringTransit = AnimationScriptPlayable.Create(graph, data);

    
            //if (item.Agent.Resolver.TryGetModule<IAnimator>(out var IAnimator))
            {
                //m_duringTransit = Playable.Null;

                //var anim=IAnimator.GetAnimator;

                var animator=item.Agent.GameObject.GetComponentInParent<Animator>();

                playableDisposable = m_AdjustData.CreateAdjustJob(item, animator, graph);

                if (playableDisposable != null)
                {

                    playableDisposable.GetPlayable.SetTraversalMode(PlayableTraversalMode.Passthrough);
                    playableDisposable.GetPlayable.SetPropagateSetTime(false);

                    return true;
                }
            }


            Debug.LogError("Failure to create stream adjust m_duringTransit " + this);

            playableDisposable = null;
            return false;
        }

        [PropertyOrder(20)]
        [SerializeField]
        private ItemHandPostureConfig m_AdjustData;

        public ItemHandPostureConfig AdjustData
        {
            get => m_AdjustData;
            set => m_AdjustData = value;
        }

#if UNITY_EDITOR
        [PropertyOrder(10)]
        [Button(ButtonSizes.Large, ButtonStyle.CompactBox)]
        void Edit()
        {
            var editor = Return.Editors.ItemPostureAdjustWizer.Open();
            editor.LoadData(this);
        }

#endif
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

