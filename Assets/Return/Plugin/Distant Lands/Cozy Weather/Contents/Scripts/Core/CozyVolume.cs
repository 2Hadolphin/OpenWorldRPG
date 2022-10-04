// Distant Lands 2021.



using DistantLands.Cozy.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DistantLands.Cozy {
    [RequireComponent(typeof(Collider))]
    public class CozyVolume : MonoBehaviour
    {


        public enum TriggerType { setWeather, setAmbience, setTicks, setDay, setAtmosphere }
        public enum TriggerState { onEnter, onStay, onExit }

        [SerializeField]
        private TriggerType m_TriggerType;
        [SerializeField]
        private TriggerState m_TriggerState;
        [SerializeField]
        private string m_Tag = "Untagged";
        private CozyWeather m_CozyWeather;
        private CozyAmbienceManager m_CozyAmbience;


        [SerializeField]
        private WeatherProfile m_WeatherProfile;
        [SerializeField]
        private AmbienceProfile m_AmbienceProfile;
        [SerializeField]
        private AtmosphereProfile m_AtmosphereProfile;
        [SerializeField]
        private float ticks;
        [SerializeField]
        private int day;


        private void Awake()
        {

            m_CozyWeather = CozyWeather.instance;

            if (m_CozyWeather.ambience)
                m_CozyAmbience = m_CozyWeather.ambience;

        }

        public void Run()
        {

            switch (m_TriggerType)
            {
                case (TriggerType.setWeather):
                    m_CozyWeather.weatherProfile = m_WeatherProfile;
                    break;
                case (TriggerType.setAmbience):
                    m_CozyAmbience.currentAmbienceProfile = m_AmbienceProfile;
                    break;
                case (TriggerType.setAtmosphere):
                    m_CozyWeather.atmosphereProfile = m_AtmosphereProfile;
                    m_CozyWeather.ResetQuality();
                    break;
                case (TriggerType.setDay):
                    m_CozyWeather.perennialProfile.currentDay = day;
                    break;
                case (TriggerType.setTicks):
                    m_CozyWeather.perennialProfile.currentTicks = ticks;
                    break;
            }

        }


        private void OnTriggerEnter(Collider other)
        {

            if (m_TriggerState != TriggerState.onEnter)
                return;

            if (other.gameObject.tag == m_Tag)
                Run();


        }

        private void OnTriggerStay(Collider other)
        {
            if (m_TriggerState != TriggerState.onStay)
                return;

            if (other.gameObject.tag == m_Tag)
                Run();


        }

        private void OnTriggerExit(Collider other)
        {
            if (m_TriggerState != TriggerState.onExit)
                return;

            if (other.gameObject.tag == m_Tag)
                Run();


        }


    }
}