using System;
using UnityEngine;
using Return;
using TNet;
using UnityEngine.Assertions;
using VContainer;
using Cysharp.Threading;
using Cysharp;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;

namespace Return.Humanoid.IK
{
    /// <summary>
    /// Handle body aiming IK, work on OnAnimatorIK phase.
    /// </summary>
    //[DefaultExecutionOrder(5)]
    public class ProceduralSpineAimMatcher : BaseComponent
    {
        //[SerializeField, Tooltip("The transform to use for the body heading")]
        //private Transform m_YawTransform = null;
        //[SerializeField, Tooltip("The transform to use for the aim heading")]
        //private Transform m_PitchTransform = null;

        //[SerializeField, Distance(0f, 1f), Tooltip("The strength of the rotation to apply to the upper chest (usually this should be the strongest and all bones should add up to 1 or less).")]
        //private float m_UpperChestStrength = 0.5f;
        //[SerializeField, Distance(0f, 1f), Tooltip("The strength of the rotation to apply to the middle chest bone (usually this should be medium strength, and all bones should add up to 1 or less).")]
        //private float m_ChestStrength = 0.3f;
        //[SerializeField, Distance(0f, 1f), Tooltip("The strength of the rotation to apply to the lower spine bone (usually this should be the weakest since it is anchored to the hips, and all bones should add up to 1 or less).")]
        //private float m_SpineStrength = 0.2f;

        public IAnimator animator;

        [Inject]
        private TNObject tno;

        private float m_AimStrength = 1f;
        private float m_TargetAimStrength = 0f;
        private float m_StrengthIncrement = 100000f;

        private bool IsMine;

        [Serializable]
        class BoneData
        {
#if UNITY_EDITOR
            [ShowInInspector]
            [Range(0, 1f)]
            [OnValueChanged(nameof(SetValue))]
            float Value;

            void SetValue()
            {
                x_Axis = Value;
                y_Axis = Value;
                z_Axis = Value;
            }
#endif

            [BoxGroup("Weight")]
            public float x_Axis;
            [BoxGroup("Weight")]
            public float y_Axis;
            [BoxGroup("Weight")]
            public float z_Axis;

            public Vector3 weight
            {
                get=> new(x_Axis, y_Axis, z_Axis);
                set
                {
                    x_Axis = value.x;
                    y_Axis = value.y;
                    z_Axis = value.z;
                }
            }

            public Transform Bone;

            public HumanBodyBones humanBone;

            public Vector3 Rotation;
        }


        [SerializeField]
        BoneData[] boneChain;

        //Transform[] BonesCache;

        Transform RootBone;

        [SerializeField]
        private Quaternion IKTargetRotation;

        /// <summary>
        /// Space ** firearms.forward.
        /// </summary>
        public ICoordinate AlignFrom;

        /// <summary>
        /// m_items handle ** view.forward
        /// </summary>
        public ICoordinate AlignTo;

        [SerializeField]
        public AnimationCurve XAxisCurve;

        /// <summary>
        /// Init animing module.
        /// </summary>
        /// <param name="characterAnimator">Character anim</param>
        /// <param name="tno">m_items tno</param>
        public virtual void Init(IAnimator system,TNObject tno)
        {
            Assert.IsNotNull(system);

            animator = system;

            var anim = system.Animator;

            Assert.IsNotNull(anim);


            RootBone = anim.GetBoneTransform(HumanBodyBones.Hips).parent;

            if (boneChain.IsNull() || boneChain.Length ==0)
            {
                boneChain = anim.GetBoneChain(HumanBodyBones.Spine, HumanBodyBones.UpperChest).
                    Select(x=>new BoneData() { Bone= x }).
                    ToArray();



                // remap weight

                var boneCount = boneChain.Length;

                Debug.Log(boneCount);

                for (int i = 0; i < boneCount; i++)
                {
                    var weight = 1f / boneCount;

                    boneChain[i].weight = Vector3.one.Multiply(weight);
                }

            }
            else
            {
                var invalidData = new Stack<BoneData>();

                foreach (var data in boneChain)
                {
                    if(data.Bone.IsNull())
                        data.Bone=anim.GetBoneTransform(data.humanBone);

                    if (data.Bone.IsNull())
                        invalidData.Push(data);
                }

                var invalidCount = invalidData.Count;

                if (invalidCount > 0)
                {
                    var dataCount = boneChain.Length;
                    var ratio = (float)dataCount/(dataCount - invalidCount);

                    // remap weight to total 1

                    boneChain = boneChain.
                        SkipWhile(x => invalidData.Contains(x)).
                        Select(x =>
                        {
                            x.weight = x.weight.Multiply(ratio);
                            return x;
                        }).
                        ToArray();

                }


                //var length = boneChain.Length;
                //BonesCache = new Transform[length];

                //for (int i = 0; i < length; i++)
                //BonesCache[i] = anim.GetBoneTransform(boneChain[i].humanBone);
            }

            Assert.IsNotNull(tno);
            this.tno = tno;
            IsMine = tno.isMine;

            //if (enabled)
            //    subscribeIKPhase(true);

            enabled = true;
            OnIK = true;
        }

        private void Awake()
        {
            enabled = false;
            //InstanceIfNull(ref anim);
            //InstanceIfNull(ref tno);
        }

        protected virtual void OnEnable()
        {

            subscribeIKPhase(true);

            //Routine.OnLateUpdateEarly.Subscribe(SolveIK);
            //Timers.Timer.SubscribeUpdate(SyncIKTarget,0.3f);

            SyncIK();
        }

        protected virtual void OnDisable()
        {
            subscribeIKPhase(false);

            //Routine.OnLateUpdateEarly.Unsubscribe(SolveIK);
            //Timers.Timer.UnsubscribeUpdate(SyncIKTarget);
        }

        async void SyncIK()
        {
            do
            {
                SyncIK(IKTargetRotation);

                await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
            }
            while (this.NotNull()&&enabled);
        }



        void subscribeIKPhase(bool activate)
        {
            if (animator.IsNull())
                return;

            animator.OnAnimatorIKPhase -= SolveIK;

            if (activate)
                animator.OnAnimatorIKPhase += SolveIK;
        }

        //void SyncIKTarget(float deltaTime)
        //{
        //    SyncIK(IKTargetRotation);
        //}

        public bool OnIK;
        //public Quaternion offsetRaw;

        public int interation = 5;

#if UNITY_EDITOR

        Ray gizmosRay;

        [SerializeField]
        float GizmosDistance=5;

        Vector3 targetPoint;

        private void OnDrawGizmos()
        {
            if (!UnityEditor.EditorApplication.isPlaying)
                return;

            var dst = GizmosDistance;

            if (Physics.Raycast(gizmosRay, out var hit))
                dst = hit.distance.Max(dst);    

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(gizmosRay.CastPoint(dst),0.1f);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(targetPoint,0.1f);

            //if (AlignTo.NotNull())
            //{
            //    Gizmos.color = Color.red;
            //    Gizmos.DrawSphere(AlignTo.position + AlignTo.Transform.forward.Multiply(1.5f),0.1f);

            //    Gizmos.color = Color.white;
            //    Gizmos.DrawRay(AlignTo.position, AlignTo.rotation * Vector3.forward * 4f);            
            //}

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, transform.forward.Multiply(dst));
        }

#endif


        private void LateUpdate()
        {
            SolveIK();
        }

        [SerializeField]
        bool Clamp=true;
        // early late update phase
        void SolveIK(int layer=0)
        {
            if (layer > 0)
                Debug.LogError(layer);

            //if (m_AimStrength != m_TargetAimStrength)
            //{
            //    m_AimStrength += ConstCache.deltaTime * m_StrengthIncrement;

            //    if (m_AimStrength > 1f)
            //    {
            //        m_AimStrength = 1f;
            //        m_StrengthIncrement = 0f;
            //    }

            //    if (m_AimStrength < 0f)
            //    {
            //        m_AimStrength = 0f;
            //        m_StrengthIncrement = 0f;
            //    }
            //}

            //if (m_AimStrength > 0f)
            {
                //if (isMine)
                {
                    //  var offsetYaw = AlignFrom.rotation * Quaternion.Inverse(m_YawTransform.rotation);
                    //offsetYaw = Quaternion.Inverse(offsetYaw);

                    //offsetRaw = offsetYaw;


                    //if (m_PitchTransform && m_YawTransform)
                    //IKTargetRotation = m_PitchTransform.rotation * Quaternion.Inverse(m_YawTransform.rotation);
                    {

                    }
                    //else
                    //    IKTargetRotation = Quaternion.identity;

                    if (OnIK)
                    {

                        if (AlignFrom.NotNull() && AlignTo.NotNull())
                        {

                            var rootRotation = RootBone.rotation;

                            var defaultRot = AlignFrom.rotation*Quaternion.Inverse(rootRotation);
                            
                            // clamp with root
                            var clamp = new Vector3(83, 85,5);

                            targetPoint = GetTargetPos(10);

                            for (int i = 0; i < interation; i++)
                            {
      

                                //var bone = anim.GetBoneTransform(HumanBodyBones.Spine);

                                //targetRotation = aimTowards * bone.rotation;

                                //targetRotation = bone.InverseTransformRotation(targetRotation);

                                var length = boneChain.Length;

                                var aimDir = AlignFrom.forward;

                                var targetDir = GetTargetPos(10) - AlignFrom.position;
                                //targetDir = targetDir.ClampRotation(rootRotation,-clamp,clamp);

                                #region Clamp

                                if (Clamp)
                                {
                                    var targetRot = Quaternion.LookRotation(targetDir, RootBone.up);

                                    targetRot = RootBone.InverseTransformRotation(targetRot);

                                    targetRot =
                                        targetRot.eulerAngles.Clamp(-clamp, clamp).
                                        ToEuler();

                                    targetRot = RootBone.TransformRotation(targetRot);

                                    targetDir = targetRot * Vector3.forward;
                                }

                                #endregion

                                var aimTowards = Quaternion.FromToRotation(aimDir, targetDir);

                                for (int k = 0; k < length; k++)
                                {
                                    SetBoneRotation(boneChain[k], aimTowards);
                                }

                                //SetBonesRotation(targetPos);
                            }

                            //var rot = AlignFrom.rotation * Quaternion.Inverse(AlignTo.rotation);
                            //var rot = Quaternion.RotateTowards(AlignFrom.rotation, AlignTo.rotation,179);


                            //var rot = AlignFrom.ReadOnlyTransform.InverseTransformRotation(AlignTo.rotation);
                            //rot = AlignFrom.ReadOnlyTransform.TransformRotation(rot);

                            //var rot = AlignTo.rotation*Quaternion.Inverse(AlignFrom.rotation);
                            //IKTargetRotation = rot;
                            //rot = AlignFrom.ReadOnlyTransform.TransformRotation(rot);

                            //var fromFwd = AlignFrom.rotation * Vector3.forward;
                            //var toFwd = AlignTo.rotation * Vector3.forward;

                            //var angleA = Mathf.Atan2(fromFwd.width, fromFwd.z) * Mathf.Rad2Deg;
                            //var angleB = Mathf.Atan2(toFwd.width, toFwd.z) * Mathf.Rad2Deg;

                            //// get the signed difference in these angles
                            //var y_axis = Mathf.DeltaAngle(angleA, angleB);




                            //targetRotation = m_YawTransform.InverseTransformRotation(rot);
                        }
                        else
                        {
                            Debug.Log(AlignFrom + " ** " + AlignTo);
                            return;
                        }
                    }
                    else
                    {
                        //targetRotation = AlignFrom.ReadOnlyTransform.TransformRotation(IKTargetRotation);
                        //targetRotation = m_YawTransform.InverseTransformRotation(targetRotation);

                        //SetBonesRotation(targetRotation);

                        foreach (var data in boneChain)
                        {
                            SetBoneRotation(data);
                        }
                    }


                    gizmosRay = new(transform.position, transform.forward);
                    //Debug.Log(IKTargetRotation.eulerAngles);
                }
            }
        }

        public float AngleLimit = 45f;

        /// <summary>
        /// Get focus point position.
        /// </summary>
        private Vector3 GetTargetPos(float distance=3f)
        {
            var alignFwd = AlignTo.forward;

            // get cast point
            var pos = AlignTo.position + alignFwd.Multiply(distance);

            var rootUp = RootBone.up;

            // wrap angle
            var angle = Vector3.Angle(RootBone.forward, Vector3.ProjectOnPlane(alignFwd, rootUp));


            //var localPos = RootBone.InverseTransformPoint(pos);
            //var angle = Vector3.Angle(Vector3.forward,Vector3.ProjectOnPlane(localPos,Vector3.up));

            if (angle.Abs() > AngleLimit)
            {
                float gap = 0;

                if (angle > AngleLimit)
                    gap = angle.Difference(AngleLimit);
                else if (angle < -AngleLimit)
                    gap = -angle.Difference(AngleLimit);


                if (gap.Abs() > 0)
                {
                    //localPos = Quaternion.AngleAxis(-gap, Vector3.up) * localPos;
                    //pos = RootBone.TransformPoint(localPos);
                    var rot=Quaternion.AngleAxis(-gap, rootUp) * pos;

                    //Debug.Log(angle+" ** "+gap);
                }
            }

            return pos;

            //var targetDir = pos - AlignFrom.position;
            //var aimDir = AlignFrom.Transform.forward;

            //var blendOut = 0f;

            //var angle = Vector3.Angle(targetDir, aimDir);

            //if (angle > AngleLimit)
            //    blendOut += (angle - AngleLimit) / 50f;

            //var dir = Vector3.Slerp(aimDir, targetDir, blendOut);

            //return AlignFrom.position+dir;
        }

        //void SetBonesRotation(Vector3 targetPosition)
        //{
        //    //SetBoneRotation(HumanBodyBones.Spine, targetPosition, m_AimStrength);

        //    SetBoneRotation(HumanBodyBones.Spine, targetPosition, m_SpineStrength * m_AimStrength);
        //    SetBoneRotation(HumanBodyBones.Chest,  targetPosition, m_ChestStrength * m_AimStrength);
        //    SetBoneRotation(HumanBodyBones.UpperChest, targetPosition, m_UpperChestStrength * m_AimStrength);
        //}

        //class IKBoneData
        //{
        //    public Transform bone;
        //    public float boneWeight;

        //}



        [RFC]
        protected virtual void SyncIK(Quaternion rot)
        {
            if (IsMine)
                tno.SendQuickly(nameof(SyncIK), Target.Others, rot);
            else
                IKTargetRotation = rot;
        }

        //void SetBoneRotation (HumanBodyBones bone, Vector3 targetPos,float weight)
        //{
        //    var boneTransform = anim.GetBoneTransform(bone);

        //    SetBoneRotation(boneTransform, targetPos, weight);
        //}

        void SetBoneRotation(BoneData data)
        {
            data.Bone.localEulerAngles = data.Rotation;
            Debug.Log(data.Bone.localEulerAngles);
        }

        void SetBoneRotation(BoneData data,Quaternion aimTowards)
        {
            aimTowards = Quaternion.Slerp(Quaternion.identity, aimTowards, data.weight.x);
            data.Bone.rotation = aimTowards * data.Bone.rotation;
            data.Rotation = data.Bone.localEulerAngles.WrapAngle();
        }

        void SetBoneRotation(Transform bone, Quaternion aimTowards, Vector3 weight)
        {
            if (bone != null)
            {

                //var localTowards = RootBone.InverseTransformRotation(aimTowards);

                //localTowards = Vector3.one.SlerpTo(localTowards, weight);

                //localTowards = Vector3.Lerp(Vector3.zero, localTowards, weight.width);

                //localTowards = Quaternion.Euler(
                //    Quaternion.Slerp(Quaternion.identity, aimTowards, weight.width).eulerAngles.width,
                //    Quaternion.Slerp(Quaternion.identity, aimTowards, weight.height).eulerAngles.height,
                //    Quaternion.Slerp(Quaternion.identity, aimTowards, weight.z).eulerAngles.z
                //    );

                //bone.rotation = RootBone.TransformRotation(localTowards) * bone.rotation;

                aimTowards = Quaternion.Slerp(Quaternion.identity, aimTowards, weight.x);

                bone.rotation = aimTowards * bone.rotation;
            }
            else
                Debug.LogError(bone + " is null");
        }

        //void SetBoneRotation(Transform bone, Vector3 targetPos, float weight)
        //{
        //    if (bone != null)
        //    {
        //        var aimDir = AlignFrom.forward;

        //        var targetDir = targetPos - AlignFrom.position;

        //        var aimTowards = Quaternion.FromToRotation(aimDir, targetDir);

        //        //var localTowards = RootBone.InverseTransformRotation(aimTowards).eulerAngles;

        //        //localTowards = Vector3.one.SlerpTo(localTowards, weight);

        //        //bone.localEulerAngles += localTowards;

        //        aimTowards = Quaternion.Slerp(Quaternion.identity, aimTowards, weight);

        //        bone.rotation = aimTowards * bone.rotation;
        //    }
        //    else
        //        Debug.LogError(bone + " is null");
        //}


        public void EnableAimMatching(float blendTime)
        {
            m_TargetAimStrength = 1f;
            if (m_AimStrength != 1f)
            {
                if (blendTime > 0f)
                    m_StrengthIncrement = 1f / blendTime;
                else
                    m_StrengthIncrement = 100000f;
            }
        }

        public void DisableAimAdapting(float blendTime)
        {
            m_TargetAimStrength = 0f;
            if (m_AimStrength != 0f)
            {
                if (blendTime > 0f)
                    m_StrengthIncrement = -1f / blendTime;
                else
                    m_StrengthIncrement = -100000f;
            }
        }
    }
}