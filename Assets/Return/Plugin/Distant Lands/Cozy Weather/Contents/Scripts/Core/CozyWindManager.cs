using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DistantLands.Cozy
{
    [ExecuteAlways]
    public class CozyWindManager : CozyModule
    {

        private WindZone m_WindZone;

        private float m_WindSpeed;
        private float m_WindAmount;
        private Vector3 m_WindDirection;
        private float m_Seed;
        public bool useWindzone = true;
        public bool useShaderWind = true;

        public Vector3 windDirection
        {
            get { return m_WindDirection; }

        }


        // Start is called before the first frame update
        void Awake()
        {


            base.SetupModule();
            m_Seed = Random.value * 1000;

            if (!m_WindZone && useWindzone)
                if (GetComponentInChildren<WindZone>())
                {
                    m_WindZone = GetComponentInChildren<WindZone>();
                }
                else
                if (FindObjectOfType<WindZone>()) {

                    m_WindZone = FindObjectOfType<WindZone>();


                }
                else
                {
                    Transform wind = new GameObject().transform;
                    wind.localPosition = Vector3.zero;
                    wind.localEulerAngles = Vector3.zero;
                    wind.localScale = Vector3.one;
                    wind.name = "COZY Wind Zone";
                    m_WindZone = wind.gameObject.AddComponent<WindZone>();

                }
        }

        // Update is called once per frame
        void Update()
        {


            if (useWindzone)
            {
                if (m_WindZone == null)
                {
                    Transform wind = new GameObject().transform;
                    wind.localPosition = Vector3.zero;
                    wind.localEulerAngles = Vector3.zero;
                    wind.localScale = Vector3.one;
                    wind.name = "COZY Wind Zone";
                    m_WindZone = wind.gameObject.AddComponent<WindZone>();

                    Debug.Log("If you don't want a windzone in your scene, disable the use windzone toggle on the Cozy Wind Manager Module!");

                }

            }


            ManageWind();



            if (useShaderWind)
            {
                Shader.SetGlobalFloat("CZY_WindSpeed", m_WindSpeed);
                Shader.SetGlobalVector("CZY_WindDirection", m_WindDirection * m_WindAmount);
            }
        }

        public void ManageWind()
        {

            m_WindAmount = weatherSphere.WeatherLerp(m_WindAmount, weatherSphere.weatherProfile.windAmount);
            m_WindSpeed = weatherSphere.WeatherLerp(m_WindSpeed, weatherSphere.weatherProfile.windSpeed);

            float i = 360 * Mathf.PerlinNoise(m_Seed, Time.time / 100000);

            m_WindDirection = new Vector3(Mathf.Sin(i), 0, Mathf.Cos(i));


            if (useWindzone)
            {
                m_WindZone.transform.LookAt(transform.position + m_WindDirection, Vector3.up);
                m_WindZone.windMain = m_WindAmount;
            }



        }

        public override void DisableModule()
        {

            if (m_WindZone)
                DestroyImmediate(m_WindZone.gameObject);

            DestroyImmediate(this);

        }

    }
}