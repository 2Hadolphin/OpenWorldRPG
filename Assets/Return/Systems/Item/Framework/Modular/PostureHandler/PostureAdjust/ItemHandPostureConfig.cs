using System.Collections.Generic;
using System;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Linq;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Unity.Collections;
using Return.Animations;
using Return;

namespace Return.Items
{
    public interface IPlayableDisposable : IDisposable
    {
        Playable GetPlayable { get; }
    }

    public abstract class PlayableCache : NullCheck, IPlayableDisposable
    {
        public abstract Playable GetPlayable { get; }
        public abstract void Dispose();

        ~PlayableCache()
        {
            Dispose();
        }
    }

    public class ItemPostureJobCache : PlayableCache
    {
        public ItemPostureJobCache(Playable playable)
        {
            m_Playable = playable;
        }

        public override Playable GetPlayable => m_Playable;

        Playable m_Playable;

        public List<IDisposable> JobNativeCache;

        //public NativeArray<StreamAdditivePositionHandle> AdditivePositions;
        //public NativeArray<StreamAdditiveRotationHandle> AdditiveRotations;

        public override void Dispose()
        {
            foreach (var job in JobNativeCache)
            {
                job.Dispose();
            }

            //if (AdditivePositions.IsCreated)
            //    AdditivePositions.DisposePlayingPerformer();

            //if (AdditiveRotations.IsCreated)
            //    AdditiveRotations.DisposePlayingPerformer();
        }
    }

    [HideLabel]
    [Serializable]
    public class ItemHandPostureConfig: BasePoseAdjustData
    {
        public override IPlayableDisposable CreateAdjustJob(AbstractItem item, Animator animator, PlayableGraph graph,bool loadTwoBoneIK=false)
        {
            List<IAnimationStreamHandler> additiveJobs=new(3);

            var posHandles = new List<StreamAdditivePositionHandle>(3);
            var rotHandles = new List<StreamAdditiveRotationHandle>(3);

            if (item)
            {
                if (Item.AdjustMode)
                {
                    var handleSlot = animator.GetBoneTransform(HumanBodyBones.RightHand).Find("RightHandHandle");
                    //item.transform.SetParent(anim.transform.Find("ItemHandleSlot_R"));

                    if (animator.GetBoneTransform(HumanBodyBones.Chest).FindChild(HandleOption.ItemHandle_RightHand, out var itemSlot))
                        item.transform.SetParent(itemSlot);
                    else
                        Debug.LogError("Not found child name as " + HandleOption.ItemHandle_RightHand);

                    item.transform.SetLocalPR(Item);


                    Debug.Log(itemSlot);

                    var job = Item.GetJob(handleSlot, item, animator);
                    if(job!=null)
                    additiveJobs.Add(job);
                }
            }
      

            if (RightHand)
            {
                if (RightHand.AdjustMode)
                {
                    var job = RightHand.GetJob(animator.GetBoneTransform(HumanBodyBones.RightHand), item, animator);
                    if (job != null)
                        additiveJobs.Add(job);
                }
            }

            if (LeftHand)
            {
                if (LeftHand.AdjustMode)
                {
                    var job = RightHand.GetJob(animator.GetBoneTransform(HumanBodyBones.LeftHand), item, animator);
                    if (job != null)
                        additiveJobs.Add(job);
                }
            }

            if (m_FingersData != null)
            {
                foreach (var finger in m_FingersData)
                {
                    if (finger.TryGetJob(animator, out var job))
                        rotHandles.Add(job);
                }
            }

            var handles = new NativeArray<StreamAdditiveRotationHandle>(rotHandles.ToArray(), Allocator.Persistent);

            var postureJob = new ItemPoseAdjustJob()
            {
                HandleJobs=additiveJobs.ToArray(),
                AdditiveRotations = handles,
            };

            if (loadTwoBoneIK)
            {
                return default;
            }
            else
            {
                var handle = new ItemPostureJobCache(AnimationScriptPlayable.Create(graph, postureJob))
                {
                    JobNativeCache = new()
                    {
                        handles
                    },
                };

                return handle;
            }

   

            //var m_duringTransit = new ItemPoseAdjustJob()
            //{
            //    AdditivePositions=new NativeArray<StreamAdditivePositionHandle>(),
            //    AdditiveRotations=new NativeArray<StreamAdditiveRotationHandle>(),
            //};

            //return AnimationScriptPlayable.Create(graph, m_duringTransit);
        }

        [SerializeField]
        [HideLabel]
        [BoxGroup("m_items")]
        public HandleConfig Item = new();

        [ToggleLeft]
        [BoxGroup("LeftHand")]
        public bool UseLeftHand;
        [ShowIf(nameof(UseLeftHand))]
        [BoxGroup("LeftHand")]
        public HandleConfig LeftHand = new();
        [BoxGroup("LeftHand")]
        public bool isLeftFingerAdditive = true;

        [ToggleLeft]
        [BoxGroup("RightHand")]
        public bool UseRightHand;
        [ShowIf(nameof(UseRightHand))]
        [BoxGroup("RightHand")]
        public HandleConfig RightHand = new();
        [BoxGroup("RightHand")]
        public bool isRightFingerAdditive = true;


        [SerializeField]
        [ListDrawerSettings(NumberOfItemsPerPage =10)]
        private List<PostureRotationData> m_FingersData;

        public List<PostureRotationData> FingersData
        {
            get
            {
                if (m_FingersData == null)
                    m_FingersData = new();

                return m_FingersData;
            }
            set => m_FingersData = value;
        }


        [SerializeField, HideInInspector]
        private Vector3 m_LeftHandPositionOffset = Vector3.zero;
        [SerializeField, HideInInspector]
        private Vector3 m_LeftHandRotationOffset = Vector3.zero;

        [SerializeField, HideInInspector]
        private Vector3 m_RightHandPositionOffset = Vector3.zero;
        [SerializeField, HideInInspector]
        private Vector3 m_RightHandRotationOffset = Vector3.zero;


#if UNITY_EDITOR

        /// <summary>
        /// Get SO data for init
        /// </summary>
        public Dictionary<HumanBodyBones, Vector3> GetFingerOffsetCatch
        {
            get
            {
                var m_OffsetRotationCatch = new Dictionary<HumanBodyBones, Vector3>(30);

                m_OffsetRotationCatch = m_FingersData == null ? new() : m_FingersData.ToDictionary(x => x.Bone, y => y.Rotation);
                var handBones = HumanBodyBonesUtility.AllFingers();

                foreach (var bone in handBones)
                    if (!m_OffsetRotationCatch.ContainsKey(bone))
                        m_OffsetRotationCatch.Add(bone, default);

                return m_OffsetRotationCatch;
            }
        }

        /// <summary>
        /// Get SO data for init
        /// </summary>
        public Dictionary<HumanBodyBones, Vector3> GetHandleOffsetCatch
        {
            get
            {
                var m_OffsetRotationCatch = new Dictionary<HumanBodyBones, Vector3>(30);

                m_OffsetRotationCatch = m_FingersData == null ? new() : m_FingersData.ToDictionary(x => x.Bone, y => y.Rotation);
                var handBones = HumanBodyBonesUtility.AllFingers();

                foreach (var bone in handBones)
                    if (!m_OffsetRotationCatch.ContainsKey(bone))
                        m_OffsetRotationCatch.Add(bone, default);

                return m_OffsetRotationCatch;
            }
        }
#endif

        public Vector3 leftHandPositionOffset
        {
            get { return m_LeftHandPositionOffset; }
            set { m_LeftHandPositionOffset = value; }
        }

        public Vector3 rightHandPositionOffset
        {
            get { return m_RightHandPositionOffset; }
            set { m_RightHandPositionOffset = value; }
        }


//#if UNITY_EDITOR


        public Vector3 leftHandEulerAngleOffset
        {
            get => m_LeftHandRotationOffset;
            set => m_LeftHandRotationOffset = value;
        }

        public Vector3 rightHandEulerAngleOffset
        {
            get => m_RightHandRotationOffset;
            set => m_RightHandRotationOffset = value;
        }

        public Quaternion leftHandRotationOffset
        {
            get { return Quaternion.Euler(m_LeftHandRotationOffset); }
        }

        public Quaternion rightHandRotationOffset
        {
            get { return Quaternion.Euler(m_RightHandRotationOffset); }
        }

//#else

//        Quaternion[] m_FingerRotations = new Quaternion[30];

//        public Quaternion leftHandRotationOffset
//        {
//            get;
//            private set;
//        }

//        public Quaternion rightHandRotationOffset
//        {
//            get;
//            private set;
//        }
        
//        public Quaternion GetFingerRotation(HumanBodyBones bone)
//        {
//            return m_FingerRotations[(int)bone - 24];
//        }

//        void Awake()
//        {
//            // Get hand rotations
//            leftHandRotationOffset = Quaternion.Euler(m_LeftHandRotationOffset);
//            rightHandRotationOffset = Quaternion.Euler(m_RightHandRotationOffset);

//            // Get finger rotations
//            for (int i = 0; i < 30; ++i)
//                m_FingerRotations[i] = Quaternion.Euler(m_FingerOffsets[i]);
//            m_FingerOffsets = null;
//        }

//#endif
    }
}

