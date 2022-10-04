using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Assertions;

namespace Return.Cameras
{
    public abstract class BaseAdditiveTransformHandler : BaseComponent, IAdditiveTransformHandler 
    {
        #region Config

        protected Vector3 m_OffsetPosition;
        protected Quaternion m_OffsetRotation;

        /// <summary>
        /// ???? pivot?
        /// </summary>
        [SerializeField, Tooltip("The pivot point to rotate and move around")]
        protected Vector3 m_PivotOffset = Vector3.zero;

        private float m_SpringPositionMultiplier = 1f;
        public float springPositionMultiplier
        {
            get { return m_SpringPositionMultiplier; }
            set { m_SpringPositionMultiplier = value; }
        }

        private float m_SpringRotationMultiplier = 1f;
        public float springRotationMultiplier
        {
            get { return m_SpringRotationMultiplier; }
            set { m_SpringRotationMultiplier = value; }
        }


        protected Vector3 m_PositionLerpFrom = Vector3.zero;
        protected Vector3 m_PositionLerpTo = Vector3.zero;
        protected Quaternion m_RotationLerpFrom = Quaternion.identity;
        protected Quaternion m_RotationLerpTo = Quaternion.identity;

        #endregion

        #region Effect Cache

        [ShowInInspector]
        protected List<IAdditiveTransform> AdditiveEffects = new(4);

        protected Dictionary<Type, List<IAdditiveTransform>> m_AdditiveTransformDictionary = new();

        public virtual void ApplyAdditiveEffect(IAdditiveTransform add)
        {
            Assert.IsNotNull(add);

            if (AdditiveEffects.CheckAdd(add))
            {
                // Add to the type dictionary
                Type t = add.GetType();
                List<IAdditiveTransform> list;


                if (m_AdditiveTransformDictionary.TryGetValue(t, out list))
                {
                    list.Add(add);
                }
                else
                {
                    list = new(1)
                    {
                        add
                    };
                    m_AdditiveTransformDictionary.Add(t, list);
                }
            }
        }

        public virtual void RemoveAdditiveEffect(IAdditiveTransform add)
        {
            int index = AdditiveEffects.IndexOf(add);
            if (index != -1)
            {
                // Remove the effect
                AdditiveEffects.RemoveAt(index);
                // Remove from the type dictionary
                Type t = add.GetType();
                List<IAdditiveTransform> list;
                if (m_AdditiveTransformDictionary.TryGetValue(t, out list))
                    list.Remove(add);
            }
        }

        public T GetAdditiveTransform<T>() where T : class, IAdditiveTransform
        {
            // Get type
            Type t = typeof(T);
            // Get the list
            List<IAdditiveTransform> list;
            if (m_AdditiveTransformDictionary.TryGetValue(t, out list))
            {
                if (list.Count > 0)
                    return list[0] as T;
                else
                    return null;
            }
            else
            {
                return null;
            }
        }

        public T[] GetAdditiveTransforms<T>() where T : class, IAdditiveTransform
        {
            // Get type
            Type t = typeof(T);
            // Get the list
            List<IAdditiveTransform> list;
            if (m_AdditiveTransformDictionary.TryGetValue(t, out list))
            {
                if (list.Count > 0)
                {
                    T[] result = new T[list.Count];
                    for (int i = 0; i < list.Count; ++i)
                        result[i] = list[i] as T;
                    return result;
                }
                else
                    return new T[0];
            }
            else
            {
                return new T[0];
            }
        }

        #endregion


        public virtual void UpdateTransforms(bool lerp=false)
        {
            if (lerp)
            {
                m_PositionLerpFrom = m_PositionLerpTo;
                m_RotationLerpFrom = m_RotationLerpTo;
            }

            Quaternion rotation = Quaternion.identity;
            Vector3 position = m_PivotOffset;

            // Accumulate effects
            for (int i = 0; i < AdditiveEffects.Count; ++i)
            {
                var effect = AdditiveEffects[i];
                effect.UpdateTransform();

                // Apply position
                if (effect.bypassPositionMultiplier || Mathf.Approximately(springPositionMultiplier, 1f))
                    position += /*rotation * */effect.position;
                else
                    position += /*rotation **/ effect.position * springPositionMultiplier;

                // Apply rotation
                if (effect.bypassRotationMultiplier || Mathf.Approximately(springRotationMultiplier, 1f))
                    rotation *= effect.rotation;
                else
                    rotation *= effect.rotation.ScaleRotation(springRotationMultiplier);
            }

            // Pivot offset
            //position -= rotation * m_PivotOffset;

            if (lerp)
            {
                m_PositionLerpTo = position;
                m_RotationLerpTo = rotation;
            }
            else
            {
                ApplyAdditive(new(position, rotation));
            }
        }

        /// <summary>
        /// Apply local additive offset to transform.
        /// </summary>
        /// <param name="pr"></param>
        protected abstract void ApplyAdditive(PR pr);
        

    }
}