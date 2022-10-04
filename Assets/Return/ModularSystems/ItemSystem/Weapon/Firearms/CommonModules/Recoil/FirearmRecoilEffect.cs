using Return;
using Return.Cameras;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    public class FirearmRecoilEffect : MonoBehaviour, IAdditiveTransform
    {
        private AnimationCurve m_RotationCurve = null;
        private AnimationCurve m_JiggleCurve = null;
        private AnimationCurve m_PushBackCurve = null;
        private Vector2 m_StartRotation = Vector2.zero;
        private Vector2 m_RecoilRotation = Vector2.zero;
        private float m_StartJiggle = 0f;
        private float m_RecoilJiggle = 0f;
        private float m_StartPushBack = 0f;
        private float m_RecoilPushBack = 0f;
        private float m_InverseDuration = 1f;
        private float m_Lerp = 1f;

        public IAdditiveTransformHandler transformHandler
        {
            get;
            private set;
        }

        public Quaternion rotation
        {
            get
            {
                if (m_Lerp < 1f)
                    return Quaternion.Euler(currentRecoilAngle.x, currentRecoilAngle.y, currentJiggle);
                else
                    return Quaternion.identity;
            }
        }

        public Vector3 position
        {
            get
            {
                if (m_Lerp < 1f)
                    return new Vector3(0f, 0f, -currentPushBack);
                else
                    return Vector3.zero;
            }
        }

        public bool bypassPositionMultiplier
        {
            get { return true; }
        }

        public bool bypassRotationMultiplier
        {
            get { return true; }
        }

        public Vector2 currentRecoilAngle
        {
            get;
            private set;
        }

        public float currentJiggle
        {
            get;
            private set;
        }

        public float currentPushBack
        {
            get;
            private set;
        }

        protected void Awake()
        {
            transformHandler = GetComponent<IAdditiveTransformHandler>();
        }

        protected void OnEnable()
        {
            transformHandler.ApplyAdditiveEffect(this);
        }

        protected void OnDisable()
        {
            transformHandler.RemoveAdditiveEffect(this);
        }

        public void UpdateTransform()
        {
            if (m_Lerp < 1f)
            {
                m_Lerp += Time.deltaTime * m_InverseDuration;
                if (m_Lerp >= 1f)
                {
                    m_Lerp = 1f;
                    currentRecoilAngle = Vector2.zero;
                    currentJiggle = 0f;
                    currentPushBack = 0f;
                }
                else
                {
                    // Get the "spring from" values
                    float eased = 1f - EasingFunctions.EaseInOutQuadratic(m_Lerp);
                    Vector2 rotationFrom = m_StartRotation * eased;
                    float jiggleFrom = m_StartJiggle * eased;
                    float pushFrom = m_StartPushBack * eased;

                    // Evaluate the spring curves
                    currentRecoilAngle = Vector2.LerpUnclamped(rotationFrom, m_RecoilRotation, m_RotationCurve.Evaluate(m_Lerp));
                    currentJiggle = Mathf.LerpUnclamped(jiggleFrom, m_RecoilJiggle, m_JiggleCurve.Evaluate(m_Lerp));
                    currentPushBack = Mathf.LerpUnclamped(pushFrom, m_RecoilPushBack, m_PushBackCurve.Evaluate(m_Lerp));
                }
            }
        }

        public void AddRecoil(Vector2 recoil, AnimationCurve angleCurve, float pushBack, float maxPushDistance, AnimationCurve pushCurve, float jiggle, AnimationCurve jiggleCurve , float duration)
        {
            // Set the animation curves
            m_RotationCurve = angleCurve;
            m_JiggleCurve = jiggleCurve;
            m_PushBackCurve = pushCurve;

            // Get the starting values
            m_StartRotation = currentRecoilAngle;
            m_StartJiggle = currentJiggle;
            m_StartPushBack = currentPushBack;

            // Calculate the new recoil rotation
            m_RecoilRotation = m_StartRotation + new Vector2(-recoil.y, recoil.x);

            // Calculate the new jiggle & pushback
            m_RecoilJiggle = jiggle;
            m_RecoilPushBack = m_StartPushBack + pushBack;
            if (m_RecoilPushBack > maxPushDistance)
                m_RecoilPushBack = maxPushDistance;

            // Sort timing
            m_InverseDuration = 1f / Mathf.Max(duration, 0.001f);
            m_Lerp = 0f;
        }
    }
}