using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using NeoFPS.CharacterMotion;
using System;
using Return;
using Return.Cameras;

namespace NeoFPS
{
    [RequireComponent(typeof(Animator))]
    public class FirstPersonBody : BaseComponent
    {
        [SerializeField, Tooltip("The distance to cast for ground detection")]
        private float m_GroundingCastDistance = 1f;
        [SerializeField, Range(0f, 1f), Tooltip("A smoothing value for foot offsets to prevent popping on stepped or faceted ground")]
        private float m_IkOffsetDamping = 0.25f;
        [SerializeField, Tooltip("The maximum distance downwards that the body can be shifted to prevent legs overextending on slopes")]
        private float m_MaxBodyOffset = 0.25f;

        [Header("Aim")]

        [SerializeField, Tooltip("The name of a float parameter on the character's anim that should be set with the pitch value. Pitch will be normalised to 90 degrees, so looking straight up would be +1 and down would be -1.")]
        private string m_AimPitchKey = string.Empty;
        [SerializeField, Range(0f, 1f), Tooltip("A multiplier applied to the pitch value to reduce any animation effect.")]
        private float m_AimPitchMultiplier = 1f;
        [SerializeField, Tooltip("The name of a float parameter on the character's anim that should be set with the aim yaw value (relative to the body heading). Pitch will be normalised to 90 degrees, so looking right would be +1 and left would be -1.")]
        private string m_AimYawKey = string.Empty;
        [SerializeField, Range(0f, 1f), Tooltip("A multiplier applied to the yaw value to reduce any animation effect.")]
        private float m_AimYawMultiplier = 1f;

        private class FootInfo
        {
            [NonSerialized] public Vector3 footContactOffset;
            [NonSerialized] public Vector3 toesContactOffset;
            [NonSerialized] public HumanBodyBones footBone;
            [NonSerialized] public HumanBodyBones toesBone;
            [NonSerialized] public HumanBodyBones kneeBone;
            [NonSerialized] public AvatarIKGoal goal;
            [NonSerialized] public AvatarIKHint kneeHint;
            [NonSerialized] public Vector3 groundNormal;
            [NonSerialized] public Vector3 groundNormalTarget;
            [NonSerialized] public Vector3 groundNormalVelocity;
            [NonSerialized] public float heightOffset;
            [NonSerialized] public float heightOffsetTarget;
            [NonSerialized] public float heightOffsetVelocity;

            public FootInfo(Animator animator, int index)
            {
                if (index == 0)
                {
                    footBone = HumanBodyBones.LeftFoot;
                    toesBone = HumanBodyBones.LeftToes;
                    kneeBone = HumanBodyBones.LeftLowerLeg;
                    goal = AvatarIKGoal.LeftFoot;
                    kneeHint = AvatarIKHint.LeftKnee;
                }
                else
                {
                    footBone = HumanBodyBones.RightFoot;
                    toesBone = HumanBodyBones.RightToes;
                    kneeBone = HumanBodyBones.RightLowerLeg;
                    goal = AvatarIKGoal.RightFoot;
                    kneeHint = AvatarIKHint.RightKnee;
                }

                Transform rootTransform = animator.transform;
                Transform footTransform = animator.GetBoneTransform(footBone);
                Transform toesTransform = animator.GetBoneTransform(toesBone);

                if (footTransform != null)
                    footContactOffset = GetOffset(rootTransform, footTransform);
                if (toesTransform != null)
                    toesContactOffset = GetOffset(rootTransform, toesTransform);
            }

            Vector3 GetOffset(Transform root, Transform bone)
            {
                var groundPlane = new Plane(root.up, root.position);
                var groundPoint = groundPlane.ClosestPointOnPlane(bone.position);
                return bone.InverseTransformPoint(groundPoint);
            }
        }

        public bool ikEnabled
        {
            get { return m_IkBlendTarget != 0f; }
        }

        private FootInfo[] m_Feet = null;
        private Transform m_LocalTransform = null;
        private Transform m_RootTransform = null;
        private Animator m_Animator = null;
        private IAimController m_AimController = null;
        private float m_IkBlend = 0f;
        private float m_IkBlendTarget = 1f;
        private float m_IkBlendInvDuration = 1f;
        private int m_AimPitchHash = 0;
        private int m_AimYawHash = 0;
        private float m_CurrentBodyOffset = 0f;

        protected void Awake()
        {
            m_LocalTransform = transform;


                m_RootTransform = m_LocalTransform.root;

            m_Animator = GetComponent<Animator>();
            if (m_Animator == null)
                enabled = false;
            else
            {
                m_Feet = new FootInfo[2];
                m_Feet[0] = new FootInfo(m_Animator, 0);
                m_Feet[1] = new FootInfo(m_Animator, 1);

                // Aim pitch / yaw
                if (!string.IsNullOrWhiteSpace(m_AimPitchKey))
                    m_AimPitchHash = Animator.StringToHash(m_AimPitchKey);
                if (!string.IsNullOrWhiteSpace(m_AimYawKey))
                    m_AimYawHash = Animator.StringToHash(m_AimYawKey);
                if (m_AimPitchHash != 0 || m_AimYawHash != 0)
                {
                    m_AimController = GetComponentInParent<IAimController>();
                    if (m_AimController == null)
                    {
                        m_AimPitchHash = 0;
                        m_AimYawHash = 0;
                    }
                }
            }
        }

        protected void Update()
        {

            /*
            // Sort IK blending
            if (!Mathf.Approximately(m_IkBlend, m_IkBlendTarget))
            {
                if (m_IkBlendTarget > m_IkBlend)
                {
                    m_IkBlend += Time.deltaTime * m_IkBlendInvDuration;
                    if (m_IkBlend > m_IkBlendTarget)
                        m_IkBlend = m_IkBlendTarget;
                }
                else
                {
                    m_IkBlend -= Time.deltaTime * m_IkBlendInvDuration;
                    if (m_IkBlend < m_IkBlendTarget)
                    {
                        m_IkBlend = m_IkBlendTarget;

                        // Reset offsets if blending disabled
                        if (m_IkBlend == 0f)
                        {
                            m_Feet[0].groundNormal = Vector3.up;
                            m_Feet[0].groundNormalTarget = Vector3.up;
                            m_Feet[0].groundNormalVelocity = Vector3.zero;
                            m_Feet[0].heightOffset = 0f;
                            m_Feet[0].heightOffsetTarget = 0f;
                            m_Feet[0].heightOffsetVelocity = 0f;
                            m_Feet[1].groundNormal = Vector3.up;
                            m_Feet[1].groundNormalTarget = Vector3.up;
                            m_Feet[1].groundNormalVelocity = Vector3.zero;
                            m_Feet[1].heightOffset = 0f;
                            m_Feet[1].heightOffsetTarget = 0f;
                            m_Feet[1].heightOffsetVelocity = 0f;
                        }
                    }
                }
            }
            */
            // Update aim parameters
            if (m_AimPitchHash != 0)
                m_Animator.SetFloat(m_AimPitchHash, m_AimController.pitch * m_AimPitchMultiplier / 90f);

            //if (m_AimYawHash != 0)
            //    m_Animator.SetFloat(m_AimYawHash, -Mathf.DeltaAngle(m_AimController.aimYawDiff, 0f) * m_AimYawMultiplier / 90f);
        }

        private void OnAnimatorIK(int layerIndex)
        {
            //if (m_IkBlend > 0f)
            //{
            //    var up = m_RootTransform.up;
            //    float minHeight = HandleFootIK(0, up);
            //    minHeight = Mathf.Min(HandleFootIK(1, up), minHeight);

            //    m_CurrentBodyOffset = Mathf.Lerp(m_CurrentBodyOffset, Mathf.Clamp(m_CurrentBodyOffset + minHeight, -m_MaxBodyOffset, 0f), Time.deltaTime * 2.5f);

            //    m_LocalTransform.localPosition = new Vector3(0f, m_CurrentBodyOffset, 0f);
            //}
        }

        public void SetIkState(bool ikEnabled, float blendTime = 0f)
        {
            m_IkBlendTarget = ikEnabled ? 1f : 0f;
            if (blendTime > 0.001f)
                m_IkBlendInvDuration = 1f / blendTime;
            else
                m_IkBlend = m_IkBlendTarget;
        }


        float HandleFootIK(int footIndex, Vector3 up)
        {
            var foot = m_Feet[footIndex];
            var footTransform = m_Animator.GetBoneTransform(foot.footBone);
            if (footTransform == null)
                return 0f;

            var toesTransform = m_Animator.GetBoneTransform(foot.toesBone);
            if (toesTransform == null)
                toesTransform = footTransform; // HHHHAAACCCCKKKKK

            // Get contact points
            var footPosition = footTransform.position;
            var toesPosition = toesTransform.position;
            var footContact = footPosition + footTransform.rotation * foot.footContactOffset;
            var toesContact = toesPosition + toesTransform.rotation * foot.toesContactOffset;

            // Character virtual ground plane
            var characterPlane = new Plane(up, m_LocalTransform.position);

            // Get positions on plane and distances
            var footPoP = characterPlane.ClosestPointOnPlane(footContact);
            var toesPoP = characterPlane.ClosestPointOnPlane(toesContact);
            var footDistance = characterPlane.GetDistanceToPoint(footContact);

            // Ground normals and offset
            var footNormal = Vector3.zero;
            var toesNormal = Vector3.zero;
            var footHitOffset = 0f;
            var toesHitOffset = 0f;
            bool doesHit = false;

            // Do raycasts
            RaycastHit hit;
            if (PhysicUtility.RaycastNonAllocSingle(
                new Ray(footPoP + up * m_GroundingCastDistance * 0.5f, -up),
                out hit,
                m_GroundingCastDistance,
                PhysicsFilter.Masks.CharacterBlockers,
                m_LocalTransform,
                QueryTriggerInteraction.Ignore))
            {
                footNormal = hit.normal;
                footHitOffset = m_GroundingCastDistance * 0.5f - hit.distance;
                doesHit = true;
            }

            // Do raycasts
            if (PhysicUtility.RaycastNonAllocSingle(
                new Ray(toesPoP + up * m_GroundingCastDistance * 0.5f, -up),
                out hit,
                m_GroundingCastDistance,
                PhysicsFilter.Masks.CharacterBlockers,
                m_LocalTransform,
                QueryTriggerInteraction.Ignore))
            {
                toesNormal = hit.normal;
                toesHitOffset = m_GroundingCastDistance * 0.5f - hit.distance;
                doesHit = true;
            }

            // Assign correct normal and offset based on foot vs toe
            if (doesHit) // Should this have per foot/toe?
            {
                if (footHitOffset > toesHitOffset)
                {
                    foot.groundNormalTarget = footNormal;
                    foot.heightOffsetTarget = footHitOffset;
                }
                else
                {
                    foot.groundNormalTarget = toesNormal;
                    foot.heightOffsetTarget = toesHitOffset;
                }
            }
            else
            {
                foot.groundNormalTarget = m_RootTransform.up;
                foot.heightOffsetTarget = 0f;
            }

            // Apply damping
            float footPositionTime = Mathf.Lerp(0.01f, 0.15f, m_IkOffsetDamping);
            foot.heightOffset = Mathf.SmoothDamp(foot.heightOffset, foot.heightOffsetTarget, ref foot.heightOffsetVelocity, footPositionTime);
            foot.groundNormal = Vector3.SmoothDamp(foot.groundNormal, foot.groundNormalTarget, ref foot.groundNormalVelocity, footPositionTime).normalized;

            // Apply offsets
            Quaternion rotationOffset = Quaternion.FromToRotation(up, foot.groundNormal);
            m_Animator.SetIKPosition(foot.goal, footPoP + up * (foot.heightOffset + footDistance) + rotationOffset * footTransform.rotation * -foot.footContactOffset);
            m_Animator.SetIKRotation(foot.goal, rotationOffset * m_Animator.GetIKRotation(foot.goal));

            // Set IK weight
            m_Animator.SetIKPositionWeight(foot.goal, m_IkBlend); // IK strength based on distance from ground plane
            m_Animator.SetIKRotationWeight(foot.goal, m_IkBlend); // IK strength based on distance from ground plane

            return foot.heightOffset;// + footDistance;
        }

        #region DEBUG GIZMOS
#if UNITY_EDITOR

        Vector3 GetGroundedPosition(Transform bone)
        {
            var position = bone.position;
            var groundPlane = new Plane(transform.up, transform.position);
            return groundPlane.ClosestPointOnPlane(position);
        }

        private void OnDrawGizmos()
        {
            var animator = GetComponent<Animator>();

            var c = Gizmos.color;
            Gizmos.color = Color.red;

            if (Application.isPlaying && m_Feet != null)
            {
                var t = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                if (t != null)
                    Gizmos.DrawSphere(t.position + (t.rotation * m_Feet[0].footContactOffset), 0.02f);

                t = animator.GetBoneTransform(HumanBodyBones.LeftToes);
                if (t != null)
                    Gizmos.DrawSphere(t.position + (t.rotation * m_Feet[0].toesContactOffset), 0.02f);

                t = animator.GetBoneTransform(HumanBodyBones.RightFoot);
                if (t != null)
                    Gizmos.DrawSphere(t.position + (t.rotation * m_Feet[1].footContactOffset), 0.02f);

                t = animator.GetBoneTransform(HumanBodyBones.RightToes);
                if (t != null)
                    Gizmos.DrawSphere(t.position + (t.rotation * m_Feet[1].toesContactOffset), 0.02f);
            }
            else
            {
                var t = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                if (t != null)
                    Gizmos.DrawSphere(GetGroundedPosition(t), 0.02f);

                t = animator.GetBoneTransform(HumanBodyBones.LeftToes);
                if (t != null)
                    Gizmos.DrawSphere(GetGroundedPosition(t), 0.02f);

                t = animator.GetBoneTransform(HumanBodyBones.RightFoot);
                if (t != null)
                    Gizmos.DrawSphere(GetGroundedPosition(t), 0.02f);

                t = animator.GetBoneTransform(HumanBodyBones.RightToes);
                if (t != null)
                    Gizmos.DrawSphere(GetGroundedPosition(t), 0.02f);
            }

            Gizmos.color = c;
        }
#endif
        #endregion
    }
}