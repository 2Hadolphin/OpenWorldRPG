// Distant Lands 2021.



using UnityEngine;


namespace DistantLands.Cozy
{
    [ExecuteAlways]
    public class CozySatellite : MonoBehaviour
    {

        public float orbitOffset;
        public float satelliteRotateSpeed;
        public float satelliteDirection;
        private Vector3 offset;
        private Transform m_Satellite;
        private CozyWeather m_WeatherManager;




        // Start is called before the first frame update
        void Awake()
        {

            m_Satellite = transform.GetChild(0);
            m_WeatherManager = CozyWeather.instance;


        }

        // Update is called once per frame
        void Update()
        {

            m_Satellite.localEulerAngles = m_Satellite.localEulerAngles + Vector3.up * Time.deltaTime * satelliteRotateSpeed;
            transform.localEulerAngles = new Vector3(-((m_WeatherManager.perennialProfile.currentTicks / m_WeatherManager.perennialProfile.ticksPerDay * 360) - 90 + orbitOffset), satelliteDirection, 0);



        }
    }
}