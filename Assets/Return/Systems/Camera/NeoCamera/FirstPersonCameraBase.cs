using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using System;
using Return.Cameras;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/fpcamref-mb-firstpersoncamera.html")]
    public abstract class FirstPersonCameraBase : MonoBehaviour//, INeoSerializableComponent
    {
        [Header("Aim Properties")]

        [SerializeField, Tooltip("The transform to use for accurate shooting. If you add extra spring effects to the camera that don't affect the gun, you might want to set this to something higher up the hierarchy.")]
        private Transform m_AimTransform = null;


        private const float k_DefaultFov = 50.625f;

        private IAimController m_Aimer = null;
        private float m_BaseInputMultiplier = 1f;
        private float m_BaseFoV = 50.625f;


        public abstract Transform cameraTransform
        {
            get;
        }

        public Transform aimTransform
        {
            get { return m_AimTransform; }
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (m_AimTransform == null)
                m_AimTransform = transform;
            if (m_OffsetTransform == null)
                m_OffsetTransform = transform;
        }
#endif

        protected virtual void Awake()
        {
            m_Aimer = GetComponentInParent<IAimController>();

            // Get default FoV and subscribe to event
            //FpsSettings.graphics.onVerticalFoVChanged += OnVerticalFoVChanged;
            //OnVerticalFoVChanged(FpsSettings.graphics.verticalFoV);
        }

        private void OnVerticalFoVChanged(float fov)
        {
            m_BaseFoV = fov;
            m_BaseInputMultiplier = m_BaseFoV / k_DefaultFov;
            ApplyFoVMultipliers();
        }

        protected virtual void Start()
        {
            ApplyFoVMultipliers();
        }

        protected virtual void Update()
        {
            LerpOffset();
            LerpFov();
        }

        protected virtual void OnDestroy()
        {
            if (current == this)
                current = null;

            //FpsSettings.graphics.onVerticalFoVChanged -= OnVerticalFoVChanged;
        }

        protected virtual void OnDisable()
        {

        }

        public static event UnityAction<FirstPersonCameraBase> onCurrentCameraChanged;

        private static FirstPersonCameraBase s_Current = null;
        public static FirstPersonCameraBase current
        {
            get { return s_Current; }
            protected set
            {
                s_Current = value;
                // Fire a static camera changed event
                if (onCurrentCameraChanged != null)
                    onCurrentCameraChanged(s_Current);
            }
        }


        public Ray GetAimRay()
        {
            return new Ray(aimTransform.position, aimTransform.forward);
        }

        #region AIMING

        [Header("Aiming")]

        [SerializeField, Tooltip("The offset from standard upright position for moving the head. Used for aiming down sights, etc")]
        private Transform m_OffsetTransform = null;
        [SerializeField, Range(0f, 1f), Tooltip("The multiplier applied to additive spring effects while aiming.")]
        private float m_AimPositionEffectMultiplier = 0.25f;
        [SerializeField, Range(0f, 1f), Tooltip("The multiplier applied to additive spring effects while aiming.")]
        private float m_AimRotationEffectMultiplier = 0.25f;

        private Vector3 m_FromPosition = Vector3.zero;
        private Quaternion m_FromRotation = Quaternion.identity;
        private Vector3 m_ToPosition = Vector3.zero;
        private Quaternion m_ToRotation = Quaternion.identity;
        private float m_OffsetSpeed = 1f;
        private float m_OffsetLerp = 1f;

        private float m_TargetFovMultiplier = 1f;
        private float m_FromFov = 1f;
        private float m_TargetInputMultiplier = 1f;
        private float m_FromInput = 1f;
        private float m_FovSpeed = 1f;
        private float m_FovLerp = 1f;
        private float m_CurrentFieldOfViewMultiplier = 1f;
        private float m_CurrentInputMultiplier = 1f;
        private AnimationCurve m_PulseCurve = null;
        private float m_PulseMultiplier = 1f;
        private float m_PulseTarget = 1f;
        private float m_PulseProgress = 1f;
        private float m_PulseInverseTime = 1f;

        protected float baseFoV
        {
            get { return m_BaseFoV; }
        }

        public float fovMultiplier
        {
            get { return m_CurrentFieldOfViewMultiplier * m_PulseMultiplier; }
        }

        public float inputMultiplier
        {
            get { return m_BaseInputMultiplier * m_CurrentInputMultiplier; }
        }

        protected float currentAimPositionEffectMultiplier
        {
            get { return Mathf.Lerp(1f, m_AimPositionEffectMultiplier, m_OffsetLerp); }
        }

        protected float currentAimRotationEffectMultiplier
        {
            get { return Mathf.Lerp(1f, m_AimRotationEffectMultiplier, m_OffsetLerp); }
        }

        public void SetOffset(Vector3 posOffset, Quaternion rotOffset, float aimTime)
        {
            // CheckAdd if instant or not
            if (aimTime <= 0f)
            {
                // Set lerp complete (prevents actual lerping)
                m_OffsetLerp = 1f;
                m_OffsetSpeed = 100f;
                // Set position and rotation directly
                m_OffsetTransform.localPosition = posOffset;
                m_OffsetTransform.localRotation = rotOffset;
            }
            else
            {
                // Set lerp values
                m_FromPosition = m_OffsetTransform.localPosition;
                m_FromRotation = m_OffsetTransform.localRotation;
                m_ToPosition = posOffset;
                m_ToRotation = rotOffset;
                m_OffsetSpeed = 1f / aimTime;
                m_OffsetLerp = 0f;
            }
        }

        public void ResetOffset(float aimTime)
        {
            // CheckAdd if instant or not
            if (aimTime <= 0f)
            {
                // Set lerp complete (prevents actual lerping)
                m_OffsetLerp = 1f;
                m_OffsetSpeed = 100f;
                // Set position and rotation directly
                m_OffsetTransform.localPosition = Vector3.zero;
                m_OffsetTransform.localRotation = Quaternion.identity;
            }
            else
            {
                // Set lerp values
                m_FromPosition = m_OffsetTransform.localPosition;
                m_FromRotation = m_OffsetTransform.localRotation;
                m_ToPosition = Vector3.zero;
                m_ToRotation = Quaternion.identity;
                m_OffsetSpeed = 1f / aimTime;
                m_OffsetLerp = 0f;
            }
        }

        void LerpOffset()
        {
            if (m_OffsetLerp < 1f)
            {
                m_OffsetLerp += Time.deltaTime * m_OffsetSpeed;
                if (m_OffsetLerp > 1f)
                {
                    m_OffsetLerp = 1f;
                    m_OffsetTransform.localPosition = m_ToPosition;
                    m_OffsetTransform.localRotation = m_ToRotation;
                }
                else
                {
                    m_OffsetTransform.localPosition = Vector3.Lerp(m_FromPosition, m_ToPosition, m_OffsetLerp);
                    m_OffsetTransform.localRotation = Quaternion.Lerp(m_FromRotation, m_ToRotation, m_OffsetLerp);
                }
            }
        }

        public void SetFov(float targetFovMult, float aimTime)
        {
            SetFov(targetFovMult, targetFovMult, aimTime);
        }

        public void SetFov(float targetFovMult, float targetInputMult, float aimTime)
        {
            targetFovMult = Mathf.Clamp(targetFovMult, 0.05f, 2f);
            targetInputMult = Mathf.Clamp(targetInputMult, 0.05f, 2f);

            // Set proxy and target
            m_FromFov = fovMultiplier;
            m_TargetFovMultiplier = targetFovMult;
            m_FromInput = inputMultiplier;
            m_TargetInputMultiplier = targetInputMult;

            // CheckAdd if instant or not
            if (aimTime <= 0f)
            {
                // Set lerp complete (prevents actual lerping)
                m_FovLerp = 1f;
                m_FovSpeed = 100f;
                // Set position and fov
                m_CurrentFieldOfViewMultiplier = m_TargetFovMultiplier;
                m_CurrentInputMultiplier = m_TargetInputMultiplier;
                // Apply
                ApplyFoVMultipliers();
            }
            else
            {
                m_FovLerp = 0f;
                m_FovSpeed = 1f / aimTime;
            }
        }

        public void ResetFov(float aimTime)
        {
            // Set from and target
            m_FromFov = fovMultiplier;
            m_TargetFovMultiplier = 1f;
            m_FromInput = inputMultiplier;
            m_TargetInputMultiplier = 1f;

            // CheckAdd if instant or not
            if (aimTime <= 0f)
            {
                // Set lerp complete (prevents actual lerping)
                m_FovLerp = 1f;
                m_FovSpeed = 100f;
                // Set position and fov
                m_CurrentFieldOfViewMultiplier = 1f;
                m_CurrentInputMultiplier = 1f;
                // Apply
                ApplyFoVMultipliers();
            }
            else
            {
                m_FovLerp = 0f;
                m_FovSpeed = 1f / aimTime;
            }
        }

        void LerpFov()
        {
            bool changed = false;

            if (m_FovLerp < 1f)
            {
                changed = true;

                m_FovLerp += Time.deltaTime * m_FovSpeed;
                if (m_FovLerp > 1f)
                {
                    m_FovLerp = 1f;
                    m_CurrentFieldOfViewMultiplier = m_TargetFovMultiplier;
                    m_CurrentInputMultiplier = m_TargetInputMultiplier;
                }
                else
                {
                    m_CurrentFieldOfViewMultiplier = Mathf.Lerp(m_FromFov, m_TargetFovMultiplier, EasingFunctions.EaseInOutQuadratic(m_FovLerp));
                    m_CurrentInputMultiplier = Mathf.Lerp(m_FromInput, m_TargetInputMultiplier, EasingFunctions.EaseInOutQuadratic(m_FovLerp));
                }
            }

            if (m_PulseCurve != null)
            {
                changed = true;

                m_PulseProgress += Time.deltaTime * m_PulseInverseTime;
                if (m_PulseProgress >= 1f)
                {
                    m_PulseProgress = 1f;
                    m_PulseMultiplier = 1f;
                    m_PulseCurve = null;
                }
                else
                {
                    m_PulseMultiplier = Mathf.LerpUnclamped(1f, m_PulseTarget, m_PulseCurve.Evaluate(m_PulseProgress));
                }
            }

            if (changed)
                ApplyFoVMultipliers();

        }

        public void PulseFoV(AnimationCurve pulseCurve, float fovMultiplier, float duration)
        {
            if (duration < 0.1f)
                duration = 0.1f;
            fovMultiplier = Mathf.Clamp(fovMultiplier, 0.1f, 2f);

            m_PulseCurve = pulseCurve;
            m_PulseTarget = fovMultiplier;
            m_PulseInverseTime = 1f / duration;
            m_PulseProgress = 0f;
        }

        protected virtual void ApplyFoVMultipliers()
        {
            if (m_Aimer != null)
                m_Aimer.turnRateMultiplier = inputMultiplier;
        }

        #endregion

        #region SAVE GAMES

        private static readonly NeoSerializationKey k_FovKey = new NeoSerializationKey("fov");
        private static readonly NeoSerializationKey k_InputMultiplierKey = new NeoSerializationKey("inputMult");
        private static readonly NeoSerializationKey k_OffsetLerpKey = new NeoSerializationKey("offsetLerp");
        private static readonly NeoSerializationKey k_OffsetSpeedKey = new NeoSerializationKey("offsetSpeed");
        private static readonly NeoSerializationKey k_FromPosKey = new NeoSerializationKey("fromPos");
        private static readonly NeoSerializationKey k_FromRotKey = new NeoSerializationKey("fromRot");
        private static readonly NeoSerializationKey k_ToPosKey = new NeoSerializationKey("toPos");
        private static readonly NeoSerializationKey k_ToRotKey = new NeoSerializationKey("toRot");
        private static readonly NeoSerializationKey k_OffsetPosKey = new NeoSerializationKey("offsetPos");
        private static readonly NeoSerializationKey k_OffsetRotKey = new NeoSerializationKey("offsetRot");
        private static readonly NeoSerializationKey k_TargetFovKey = new NeoSerializationKey("targetFov");
        private static readonly NeoSerializationKey k_FromFovKey = new NeoSerializationKey("fromFov");
        private static readonly NeoSerializationKey k_FovSpeedKey = new NeoSerializationKey("fovSpeed");
        private static readonly NeoSerializationKey k_FovLerpKey = new NeoSerializationKey("fovLerp");
        private static readonly NeoSerializationKey k_PulseCurveKey = new NeoSerializationKey("pulseCurve");
        private static readonly NeoSerializationKey k_PulseTargetKey = new NeoSerializationKey("pulseTarget");
        private static readonly NeoSerializationKey k_PulseMultiplierKey = new NeoSerializationKey("pulseMult");
        private static readonly NeoSerializationKey k_PulseProgressKey = new NeoSerializationKey("pulseProgress");
        private static readonly NeoSerializationKey k_PulseInvTimeKey = new NeoSerializationKey("pulseInvTime");

        public virtual void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            if (m_OffsetLerp < 1f)
            {
                writer.WriteValue(k_OffsetLerpKey, m_OffsetLerp);
                writer.WriteValue(k_OffsetSpeedKey, m_OffsetSpeed);
                writer.WriteValue(k_FromPosKey, m_FromPosition);
                writer.WriteValue(k_FromRotKey, m_FromRotation);
                writer.WriteValue(k_ToPosKey, m_ToPosition);
                writer.WriteValue(k_ToRotKey, m_ToRotation);
            }
            else
            {
                writer.WriteValue(k_OffsetPosKey, m_OffsetTransform.localPosition);
                writer.WriteValue(k_OffsetRotKey, m_OffsetTransform.localRotation);
            }

            if (m_PulseCurve != null)
            {
                writer.WriteSerializable(k_PulseCurveKey, m_PulseCurve);
                writer.WriteValue(k_PulseTargetKey, m_PulseTarget);
                writer.WriteValue(k_PulseMultiplierKey, m_PulseMultiplier);
                writer.WriteValue(k_PulseProgressKey, m_PulseProgress);
                writer.WriteValue(k_PulseInvTimeKey, m_PulseInverseTime);
            }

            writer.WriteValue(k_TargetFovKey, m_TargetFovMultiplier);
            writer.WriteValue(k_FromFovKey, m_FromFov);
            writer.WriteValue(k_FovSpeedKey, m_FovSpeed);
            writer.WriteValue(k_FovLerpKey, m_FovLerp);
            writer.WriteValue(k_FovKey, m_CurrentFieldOfViewMultiplier);
            writer.WriteValue(k_InputMultiplierKey, m_CurrentInputMultiplier);
        }

        public virtual void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            if (reader.TryReadValue(k_OffsetLerpKey, out m_OffsetLerp, m_OffsetLerp))
            {
                reader.TryReadValue(k_OffsetSpeedKey, out m_OffsetSpeed, m_OffsetSpeed);
                reader.TryReadValue(k_FromPosKey, out m_FromPosition, m_FromPosition);
                reader.TryReadValue(k_FromRotKey, out m_FromRotation, m_FromRotation);
                reader.TryReadValue(k_ToPosKey, out m_ToPosition, m_ToPosition);
                reader.TryReadValue(k_ToRotKey, out m_ToRotation, m_ToRotation);
            }
            else
            {
                Vector3 pos = Vector3.zero;
                if (reader.TryReadValue(k_OffsetPosKey, out pos, pos))
                    m_OffsetTransform.localPosition = pos;
                Quaternion rot = Quaternion.identity;
                if (reader.TryReadValue(k_OffsetRotKey, out rot, rot))
                    m_OffsetTransform.localRotation = rot;
            }

            reader.TryReadValue(k_TargetFovKey, out m_TargetFovMultiplier, m_TargetFovMultiplier);
            reader.TryReadValue(k_FromFovKey, out m_FromFov, m_FromFov);
            reader.TryReadValue(k_FovSpeedKey, out m_FovSpeed, m_FovSpeed);
            reader.TryReadValue(k_FovLerpKey, out m_FovLerp, m_FovLerp);

            // Read FoV
            bool applyFoV = reader.TryReadValue(k_FovKey, out m_CurrentFieldOfViewMultiplier, m_CurrentFieldOfViewMultiplier) || reader.TryReadValue(k_InputMultiplierKey, out m_CurrentInputMultiplier, m_CurrentInputMultiplier);

            // Read FoV Pulse
            if (reader.TryReadSerializable(k_PulseCurveKey, out m_PulseCurve, null))
            {
                applyFoV = true;

                reader.TryReadValue(k_PulseTargetKey, out m_PulseTarget, m_PulseTarget);
                reader.TryReadValue(k_PulseMultiplierKey, out m_PulseMultiplier, m_PulseMultiplier);
                reader.TryReadValue(k_PulseProgressKey, out m_PulseProgress, m_PulseProgress);
                reader.TryReadValue(k_PulseInvTimeKey, out m_PulseInverseTime, m_PulseInverseTime);
            }

            if (applyFoV)
                ApplyFoVMultipliers();
        }

        #endregion
    }
}