using System.Collections;
using System.Collections.Generic;
using NeoSaveGames;
using NeoSaveGames.Serialization;
using Return.Items;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-springrecoilhandler.html")]
    public class SpringRecoilHandler :  BaseRecoilHandlerBehaviour
    {

        [Header("Weapon Spring Recoil")]

        [SerializeField/*, NeoObjectInHierarchyField(true, required = true)*/, Tooltip("The additive kicker behaviour on the object.")]
        private AdditiveKicker m_WeaponKicker = null;
        [SerializeField, Range(0f, 1f), Tooltip("How much of the weapon recoil rotation is directed sideways instead of up.")]
        private float m_WeaponWander = 0.1f;
        [SerializeField, Tooltip("The rotation angle of the weapon recoil. At 0 wander, this is how many degrees the weapon pitches up.")]
        private float m_WeaponRotation = 1f;
        [SerializeField, Tooltip("The rotation angle of the weapon recoil when aimed. At 0 wander, this is how many degrees the weapon pitches up.")]
        private float m_WeaponRotationAimed = 1f;
        [SerializeField, Tooltip("Knock-back is movement backwards, towards the camera.")]
        private float m_WeaponKnockBack = 0.01f;

        [Header("View Spring Recoil")]

        [SerializeField, Range(0f, 1f), Tooltip("How much of the head recoil rotation is directed sideways instead of up. At negative values this will be the opposite of the guns sideways rotation.")]
        private float m_HeadWander = 0f;
        [SerializeField, Tooltip("The head rotation angle of the weapon recoil when firing from the hip. At 0 wander, this is how many degrees the weapon pitches up.")]
        private float m_HeadRotation = 0.5f;
        [SerializeField, Tooltip("The head rotation angle of the weapon recoil when aimed. At 0 wander, this is how many degrees the weapon pitches up.")]
        private float m_HeadRotationAimed = 0.5f;

        [Header("Shared")]

        //[SerializeField, Tooltip("Should the recoil be applied at the end of the frame (after tracers have been placed, etc)")]
        //private bool m_WaitForEndOfFrame = false;
        [Tooltip("Should the movement recoil effect be affected by the weapon and head spring multipliers? These affect everything, from bob to impacts to shake. If the multiplier is at zero then the recoil will be disabled either way.")]
        [SerializeField] private bool m_BypassMoveMultiplier = false;
        [Tooltip("Should the rotation recoil effect be affected by the weapon and head spring multipliers? These affect everything, from bob to impacts to shake. If the multiplier is at zero then the recoil will be disabled either way.")]
        [SerializeField] private bool m_BypassRotateMultiplier = true;
        [Tooltip("The amount of time the angle recoil effect takes to return to zero.")]
        [SerializeField] private float m_RotationDuration = 0.5f;
        [Tooltip("The amount of time the knockback effect takes to return to zero.")]
        [SerializeField] private float m_KnockBackDuration = 0.75f;

        private WaitForEndOfFrame m_Yield = new WaitForEndOfFrame();
        private AdditiveKicker m_HeadKicker = null;
        private float m_MoveMultiplier = 1f;
        private float m_RotateMultiplier = 1f;

        protected override void OnValidate()
        {
            base.OnValidate();

            if (m_WeaponKicker == null)
                m_WeaponKicker = GetComponentInChildren<AdditiveKicker>();

            m_WeaponRotation = Mathf.Clamp(m_WeaponRotation, 0f, 45f);
            m_WeaponRotationAimed = Mathf.Clamp(m_WeaponRotationAimed, 0f, 45f);
            m_WeaponKnockBack = Mathf.Clamp(m_WeaponKnockBack, 0f, 0.5f);
            m_HeadRotation = Mathf.Clamp(m_HeadRotation, -45f, 45f);
            m_HeadRotationAimed = Mathf.Clamp(m_HeadRotationAimed, -45f, 45f);
            m_RotationDuration = Mathf.Clamp(m_RotationDuration, 0.01f, 5f);
            m_KnockBackDuration = Mathf.Clamp(m_KnockBackDuration, 0.01f, 5f);
        }

        public override bool isModuleValid
        {
            get { return m_WeaponKicker != null; }
        }

        protected override void Awake()
        {
            base.Awake();
            if (m_WeaponKicker == null)
                Debug.Log("Firearm uses spring recoil but has no additive kicker component attached.", gameObject);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            //if (firearm.wielder != null && firearm.wielder.headTransformHandler != null)
            //    m_HeadKicker = firearm.wielder.headTransformHandler.GetAdditiveTransform<AdditiveKicker>();
            //else
            //    m_HeadKicker = null;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            m_HeadKicker = null;
        }

        public override void Recoil()
        {
            // Get the unscaled wander amount (used for both head an gun)
            float unscaledWander = 0f;
            if (!Mathf.Approximately(m_WeaponWander, 0f) || !Mathf.Approximately(m_HeadWander, 0f))
                unscaledWander = UnityEngine.Random.Range(-90f, 90f);

            bool aiming = false;//(firearm.aimer != null && firearm.aimer.isAiming);

            if (m_WeaponKicker != null)
            {
                // Kick the weapon back
                if (m_MoveMultiplier > 0f)
                {
                    StartCoroutine(DelayedKickWeaponPosition(Vector3.back * (m_WeaponKnockBack * m_MoveMultiplier)));
                    //if (m_WaitForEndOfFrame)
                    //    StartCoroutine(DelayedKickWeaponPosition(Vector3.back * (m_WeaponKnockBack * m_MoveMultiplier)));
                    //else
                    //    m_WeaponKicker.KickPosition(Vector3.back * (m_WeaponKnockBack * m_MoveMultiplier), m_KnockBackDuration);
                }

                if (m_RotateMultiplier > 0f)
                {
                    // Get the roll axis
                    Vector3 rollAxis = Vector3.left;
                    if (unscaledWander != 0f && m_WeaponWander != 0f)
                        rollAxis = Quaternion.AngleAxis(unscaledWander * m_WeaponWander, Vector3.forward) * rollAxis;

                    // Apply the weapon rotation
                    float rotation = (aiming) ? m_WeaponRotationAimed + m_HeadRotationAimed : m_WeaponRotation + m_HeadRotation;
                    rotation *= m_RotateMultiplier;
                    Quaternion weaponRecoil = Quaternion.AngleAxis(rotation, rollAxis);

                    StartCoroutine(DelayedKickWeaponRotation(weaponRecoil));
                    //if (m_WaitForEndOfFrame)
                    //    StartCoroutine(DelayedKickWeaponRotation(weaponRecoil));
                    //else
                    //    m_WeaponKicker.KickRotation(weaponRecoil, m_RotationDuration);
                }
            }

            // Kick the body if required
            if (m_RotateMultiplier > 0f && m_HeadRotation > 0f && m_HeadKicker != null)
            {
                // Get the roll axis
                Vector3 rollAxis = Vector3.left;
                if (unscaledWander != 0f && m_HeadWander != 0f)
                    rollAxis = Quaternion.AngleAxis(unscaledWander * m_HeadWander, Vector3.forward) * rollAxis;

                // Apply the weapon rotation
                float rotation = (aiming) ? m_HeadRotationAimed : m_HeadRotation;
                rotation *= m_RotateMultiplier;
                Quaternion headRecoil = Quaternion.AngleAxis(rotation, rollAxis);

                StartCoroutine(DelayedKickHeadRotation(headRecoil));
                //if (m_WaitForEndOfFrame)
                //    StartCoroutine(DelayedKickHeadRotation(headRecoil));
                //else
                //    m_HeadKicker.KickRotation(headRecoil, m_RotationDuration);
            }

            base.Recoil();
        }

        IEnumerator DelayedKickWeaponRotation(Quaternion rotation)
        {
            yield return m_Yield;
            m_WeaponKicker.KickRotation(rotation, m_RotationDuration);
        }

        IEnumerator DelayedKickWeaponPosition(Vector3 knockBack)
        {
            yield return m_Yield;
            m_WeaponKicker.KickPosition(knockBack, m_KnockBackDuration);
        }

        IEnumerator DelayedKickHeadRotation(Quaternion rotation)
        {
            yield return m_Yield;
            m_HeadKicker.KickRotation(rotation, m_RotationDuration);
        }

        public override void SetRecoilMultiplier(float move, float rotation)
        {
            if (m_BypassMoveMultiplier)
            {
                if (Mathf.Abs(move) > 0.00001f)
                    m_MoveMultiplier = 1f / move;
                else
                    m_MoveMultiplier = 0f;
            }
            if (m_BypassRotateMultiplier)
            {
                if (Mathf.Abs(rotation) > 0.00001f)
                    m_RotateMultiplier = 1f / rotation;
                else
                    m_RotateMultiplier = 0f;
            }

            //if (firearm.wielder != null)
            //{
            //    var t = firearm.wielder.headTransformHandler;
            //    if (t != null)
            //    {
            //        t.springPositionMultiplier = move;
            //        t.springRotationMultiplier = rotation;
            //    }
            //}

            if (m_WeaponKicker != null && m_WeaponKicker.transformHandler != null)
            {
                m_WeaponKicker.transformHandler.springPositionMultiplier = move;
                m_WeaponKicker.transformHandler.springRotationMultiplier = rotation;
            }
        }
    }
}