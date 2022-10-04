using NeoFPS.CharacterMotion;
using NeoFPS.ModularFirearms;
using NeoSaveGames;
using NeoSaveGames.Serialization;
using UnityEngine;

namespace Return.Cameras
{
    public class RotationBob :BaseRotationBob
    {
        private IAdditiveTransformHandler m_Handler = null;


        protected void Awake()
        {
            // Get relevant components
            m_Handler = GetComponent<IAdditiveTransformHandler>();
        }

        protected void OnEnable()
        {
            m_Handler.ApplyAdditiveEffect(this);
            m_CurrentStrength = 0f;

            if (velocityGetter.IsNull())
                velocityGetter = () => Time.time;
        }

        protected void OnDisable()
        {
            m_Handler.RemoveAdditiveEffect(this);
        }
    }


}