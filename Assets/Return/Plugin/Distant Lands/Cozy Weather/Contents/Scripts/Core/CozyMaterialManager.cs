// Distant Lands 2021.




using DistantLands.Cozy.Data;
using UnityEngine;

namespace DistantLands.Cozy
{

    [ExecuteAlways]
    public class CozyMaterialManager : CozyModule
    {

        [SerializeField]
        [Range(0, 1)]
        public float m_SnowAmount;
        [SerializeField]
        public float m_SnowMeltSpeed = 0.35f;
        [SerializeField]
        [Range(0, 1)]
        public float m_Wetness;
        [SerializeField]
        public float m_DryingSpeed = 0.5f;
        public bool useRainbow = true;



        public MaterialManagerProfile profile;



        // Start is called before the first frame update
        void Awake()
        {

            if (!enabled)
                return;
            base.SetupModule();

            SetupStaticGlobalVariables();



        }

        // Update is called once per frame                              
        void Update()
        {

            if (weatherSphere == null)
                base.SetupModule();


            m_SnowAmount += Time.deltaTime * weatherSphere.weatherProfile.snowAccumulationSpeed;

            if (weatherSphere.weatherProfile.snowAccumulationSpeed == 0)
                if (weatherSphere.climate)
                    if (weatherSphere.climate.currentTemprature > 32)
                        m_SnowAmount -= Time.deltaTime * m_SnowMeltSpeed * 0.03f;
                    else
                        m_SnowAmount -= Time.deltaTime * m_SnowMeltSpeed * 0.001f;
                else
                    m_SnowAmount -= Time.deltaTime * m_SnowMeltSpeed * 0.001f;



            m_Wetness += (Time.deltaTime * weatherSphere.weatherProfile.wetnessSpeed) + (-1 * m_DryingSpeed * 0.001f);


            m_SnowAmount = Mathf.Clamp01(m_SnowAmount);
            m_Wetness = Mathf.Clamp01(m_Wetness);


            SetupStaticGlobalVariables();

            Shader.SetGlobalFloat("CZY_SnowAmount", m_SnowAmount);
            Shader.SetGlobalFloat("CZY_WetnessAmount", m_Wetness);

            if (weatherSphere.calender)
            {

                foreach (MaterialManagerProfile.TerrainLayerProfile i in profile.terrainLayers)
                {

                    i.layer.specular = i.color.Evaluate(weatherSphere.calender.YearPercentage());

                }

                foreach (MaterialManagerProfile.SeasonalColorMaterialProfile i in profile.seasonalMaterials)
                {

                    i.material.SetColor(i.propertyToChange, i.color.Evaluate(weatherSphere.calender.YearPercentage()));


                }
                foreach (MaterialManagerProfile.SeasonalValueMaterialProfile i in profile.seasonalValueMaterials)
                {                                       

                    i.material.SetFloat(i.propertyToChange, i.value.Evaluate(weatherSphere.calender.YearPercentage()));


                }
            }

        }

        public void SetupStaticGlobalVariables()
        {

            Shader.SetGlobalFloat("CZY_SnowScale", profile.snowNoiseSize);
            Shader.SetGlobalTexture("CZY_SnowTexture", profile.snowTexture);
            Shader.SetGlobalColor("CZY_SnowColor", profile.snowColor);
            Shader.SetGlobalFloat("CZY_PuddleScale", profile.puddleScale);


        }

    }
}