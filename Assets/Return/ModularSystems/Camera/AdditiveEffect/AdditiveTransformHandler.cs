using System;
using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using Return;
using Return.Items;
using Return.Agents;
using DG.Tweening;

namespace Return.Cameras
{


    public class AdditiveTransformHandler : BaseAdditiveTransformHandler
    {
        [SerializeField, Tooltip("The transform to move (if null, it will move the transform of the CharacterRoot the behaviour is attached to).")]
        Transform m_TargetTransform = null;
        public Transform targetTransform
        {
            get => m_TargetTransform;
            set => m_TargetTransform = value;
        }

        public event Action<PR> OnAdditiveChanged;

        [SerializeField, Tooltip("When to update the transform.")]// FixedAndLerp will update in fixed, but lerp between results in Update for a smooth result.")]
        protected AdjustUpdateType UpdateMode = AdjustUpdateType.LateUpdate;


        protected virtual void OnEnable()
        {
            if (targetTransform.IsNull())
            {
                enabled = false;
                return;
            }
            //targetTransform = transform;


            m_PositionLerpFrom = m_PositionLerpTo = targetTransform.localPosition;
            m_RotationLerpFrom = m_RotationLerpTo = targetTransform.localRotation;
        }

        protected virtual void Update()
        {
            if (UpdateMode == AdjustUpdateType.Update)
                UpdateTransforms(false);
            else
            {
                if (UpdateMode == AdjustUpdateType.FixedAndLerp)
                    Interpolate();
            }
        }

        protected virtual void LateUpdate()
        {
            if (UpdateMode == AdjustUpdateType.LateUpdate)
                UpdateTransforms(false);
            else
            {
                if (UpdateMode == AdjustUpdateType.FixedAndLateLerp)
                    Interpolate();
            }
        }

        protected virtual void Interpolate()
        {
            // Interpolate position and rotation based on start & end calculated in fixed update
            float elapsed = Time.unscaledTime - Time.fixedUnscaledTime;

            PR pr;

            if (elapsed > Time.fixedUnscaledDeltaTime)
            {
                pr = new(m_PositionLerpTo, m_RotationLerpTo);
            }
            else
            {
                // Would store an inverse, but it seems to cause crazy jitter
                float lerp = Mathf.Clamp01(elapsed / Time.fixedUnscaledDeltaTime);

                pr = new(
                    Vector3.LerpUnclamped(m_PositionLerpFrom, m_PositionLerpTo, lerp),
                    Quaternion.LerpUnclamped(m_RotationLerpFrom, m_RotationLerpTo, lerp));
            }

            ApplyAdditive(pr);
        }

        protected virtual void FixedUpdate()
        {
            if (UpdateMode == AdjustUpdateType.FixedUpdate)
            {
                UpdateTransforms(false);
            }
            if (UpdateMode == AdjustUpdateType.FixedAndLerp ||
                UpdateMode == AdjustUpdateType.FixedAndLateLerp)
            {
                UpdateTransforms(true);
            }
        }


        protected override void ApplyAdditive(PR pr)
        {
            PostAdditiveChange(pr);

            if (targetTransform)
                targetTransform.SetLocalPR(pr);
        }


        protected virtual void PostAdditiveChange(PR pr)
        {
            OnAdditiveChanged?.Invoke(pr);
        }

    }
}