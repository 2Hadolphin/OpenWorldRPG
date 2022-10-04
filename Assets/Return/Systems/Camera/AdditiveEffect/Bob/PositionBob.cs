using NeoFPS.CharacterMotion;
using NeoSaveGames;
using NeoSaveGames.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return.Preference.GamePlay;
using Sirenix.OdinInspector;
using DG.Tweening;

namespace Return.Cameras
{
    public class PositionBob : Bob, IAdditiveTransform
    {
        private IAdditiveTransformHandler m_Handler = null;

        /// <summary>
        /// ???
        /// </summary>
        private bool m_Attached = false;

        protected void Awake()
        {          
            if(m_Handler.IsNull())
                m_Handler = GetComponent<IAdditiveTransformHandler>();
        }

        [Button]
        void OnHeadBobSettingsChanged(float headWeight)
        {
            // Get the weight
            if (m_BobType == AdditiveType.Item)
                weight = 1f - headWeight;
            else
                weight = headWeight;

            // Attach / detach if required
            if (m_Attached)
            {
                if (weight == 0f)
                {
                    m_Handler.RemoveAdditiveEffect(this);
                    m_Attached = false;
                }
            }
            else
            {
                if (weight > 0f && m_Handler != null)
                {
                    m_Handler.ApplyAdditiveEffect(this);
                    m_Attached = true;
                }
            }
        }

        protected void OnEnable()
        {
            // Attach to settings && get the head vs item weighting
            //FpsSettings.gameplay.onHeadBobChanged += OnHeadBobSettingsChanged;

            //GamePlaySetting.GetInstance();

            //OnHeadBobSettingsChanged(FpsSettings.gameplay.headBob);

            // Create default data if none is set
            if (m_BobData == null)
                m_BobData = ScriptableObject.CreateInstance<PositionBobData>();

            OnHeadBobSettingsChanged(1);

            if(velocityGetter.IsNull())
                velocityGetter=()=>Time.time;
        }

        protected void OnDisable()
        {
            if (m_Attached)
            {
                m_Handler.RemoveAdditiveEffect(this);
                m_Attached = false;
            }

            // Detach from settings
            //FpsSettings.gameplay.onHeadBobChanged -= OnHeadBobSettingsChanged;
        }

        



        //[Button]
        //void Tween(Vector3 pos, float time = 0.5f)
        //{
        //    DOTween.Punch(() => transform.localPosition, (width) => transform.localPosition = width, pos, time);
        //    //transform.DOPunchPosition(pos, time);
        //}
    }

}