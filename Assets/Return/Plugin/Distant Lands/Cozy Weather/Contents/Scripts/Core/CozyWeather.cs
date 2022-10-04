// Distant Lands 2021.



using DistantLands.Cozy.Data;
using UnityEngine;

namespace DistantLands.Cozy
{

    [ExecuteAlways] 
    public class CozyWeather : MonoBehaviour
    {

        #region Variables
        private float m_MinCloudCover;
        private float m_MaxCloudCover;

        private float m_Cumulus;
        private float m_Cirrus;
        private float m_Altocumulus;
        private float m_Cirrostratus;
        private float m_Chemtrails;
        private float m_Nimbus;
        private float m_NimbusHeight;
        private float m_NimbusVariation;
        private float m_Border;
        private float m_BorderEffect;
        private float m_BorderVariation;


        [ColorUsage(true, true)]
        private Color m_HorizonColor;
        [ColorUsage(true, true)]
        private Color m_ZenithColor;
        [ColorUsage(true, true)]
        private Color m_CloudColor;
        [ColorUsage(true, true)]
        private Color m_CloudHighlight;
        [ColorUsage(true, true)]
        private Color m_CloudZenith;
        [HideInInspector]
        public FogColors m_FogColors;
        [HideInInspector]
        public float m_FogDensity;
        [ColorUsage(true, true)]
        private Color m_SunColor;
        [ColorUsage(true, true)]
        private Color m_FogFlareColor;
        [ColorUsage(true, true)]
        private Color m_StarColor;
        [ColorUsage(true, true)]
        private Color m_AmbientHorizon;
        [ColorUsage(true, true)]
        private Color m_AmbientZenith;


        private float m_GalaxyIntensity;
        private float m_RainbowIntensity;


        private float m_FilterSaturation;  
        private float m_FilterValue;
        private Color m_FilterColor;
        private Color m_SunFilter;
        private Color m_CloudFilter;



        private float m_AdjustedScale;


        [System.Serializable]
        public class FogColors
        {
            [ColorUsage(true, true)]
            public Color color1;
            [ColorUsage(true, true)]
            public Color color2;
            [ColorUsage(true, true)]
            public Color color3;
            [ColorUsage(true, true)]
            public Color color4;
            [ColorUsage(true, true)]
            public Color color5;
        }

        public AtmosphereProfile atmosphereProfile;
        public WeatherProfile weatherProfile;
        public PerennialProfile perennialProfile;




        public bool lockToCamera = true;
        public bool allowRuntimeChanges = true;
        public float sunDirection;
        public float sunPitch;
        public float weatherTransitionSpeed;




        private Light m_SunLight;
        private MeshRenderer m_Clouds;
        private MeshRenderer m_Skysphere;
        private MeshRenderer m_FogDome;
        [SerializeField]
        private ParticleSystem[] m_Stars;
        [SerializeField]
        private ParticleSystem[] m_CloudParticles;
        private Camera m_MainCamera;
        private AtmosphereProfile m_CheckAtmosChange;


        #endregion

        #region Modules

        [HideInInspector]
        public CozyAudio cozyAudio;
        [HideInInspector]
        public CozyMaterialManager cozyMaterials;
        [HideInInspector]
        public CozyClimate climate;
        [HideInInspector]
        public CozyCalendar calender;
        [HideInInspector]
        public CozyAmbienceManager ambience;
        [HideInInspector]
        public CozyForecast forecast;
        [HideInInspector]
        public CozyTriggerManager triggerManager;
        [HideInInspector]
        public CozyLightningManager lightningManager;
        [HideInInspector]
        public CozySaveLoad saveLoad;
        [HideInInspector]
        public CozyWindManager wind;

        #endregion






        public Transform GetChild(string name)
        {

            foreach (Transform i in transform.GetComponentsInChildren<Transform>())
                if (i.name == name)
                    return i;

            return null;

        }
        public Transform GetChild(string name, Transform parent)
        {

            foreach (Transform i in parent.GetComponentsInChildren<Transform>())
                if (i.name == name)
                    return i;

            return null;

        }


        void Awake()
        {

            SetupReferences();
            SetupModules();
            SetStaticShaderVariables();

            if (Application.isPlaying)
                SetupWeatherFX();
            

        }

        // Start is called before the first frame update
        void Start()
        {

            


            ResetVariables();

            ResetQuality();



        }


        public void ResetQuality()
        {

            CloudDome.sharedMaterial.shader = atmosphereProfile.cloudShader.shader;
            SkyDome.sharedMaterial.shader = atmosphereProfile.skyShader.shader;
            FogDome.sharedMaterial.shader = atmosphereProfile.fogShader.shader;
            SetStaticShaderVariables();


        }

        // Update is called once per frame
        public void Update() 
        {

            if (!Application.isPlaying)
            {
                SetupModules();            
                if (m_CheckAtmosChange != atmosphereProfile)
                {
                    ResetQuality();
                    m_CheckAtmosChange = atmosphereProfile;

                }
            }

            if (weatherProfile == null || atmosphereProfile == null || perennialProfile == null)
            {

                Debug.LogWarning("Cozy Weather requires an active weather profile, an active perennial profile and an active atmosphere profile to function properly.\nPlease ensure that the active CozyWeather script contains all necessary profile references.");
                return;
            }


            if (Application.isPlaying)
            {
                if (allowRuntimeChanges)
                    SetProperties();
            }
            else
                ResetVariables();


            if (allowRuntimeChanges)
            {
                SetGlobalVariables();
                SetShaderVariables();
            }


            if (m_MainCamera == null)
                m_MainCamera = Camera.main;


            if (m_MainCamera && lockToCamera)
            {
                transform.position = m_MainCamera.transform.position;
                m_AdjustedScale = m_MainCamera.farClipPlane / 1000;

                transform.localScale = Vector3.one * m_AdjustedScale;

            }

        }


        public void SetupModules()
        {

            if (!cozyAudio)
                if (GetComponent<CozyAudio>())
                    cozyAudio = GetComponent<CozyAudio>();
            if (!climate)
                if (GetComponent<CozyClimate>())
                    climate = GetComponent<CozyClimate>();
            if (!cozyMaterials)
                if (GetComponent<CozyMaterialManager>())
                    cozyMaterials = GetComponent<CozyMaterialManager>();
            if (!calender)
                if (GetComponent<CozyCalendar>())
                    calender = GetComponent<CozyCalendar>();
            if (!forecast)
                if (GetComponent<CozyForecast>())
                    forecast = GetComponent<CozyForecast>();
            if (!ambience)
                if (GetComponent<CozyAmbienceManager>())
                    ambience = GetComponent<CozyAmbienceManager>();
            if (!triggerManager)
                if (GetComponent<CozyTriggerManager>())
                    triggerManager = GetComponent<CozyTriggerManager>();
            if (!lightningManager)
                if (GetComponent<CozyLightningManager>())
                    lightningManager = GetComponent<CozyLightningManager>();
            if (!saveLoad)
                if (GetComponent<CozySaveLoad>())
                    saveLoad = GetComponent<CozySaveLoad>();
            if (!wind)
                if (GetComponent<CozyWindManager>())
                    wind = GetComponent<CozyWindManager>();


        }


        public void SetupReferences()
        {

            m_SunLight = GetChild("Sun").GetComponent<Light>();
            m_Skysphere = GetChild("Skydome").GetComponent<MeshRenderer>();
            m_Clouds = GetChild("Foreground Clouds").GetComponent<MeshRenderer>();
            m_FogDome = GetChild("Fog").GetComponent<MeshRenderer>();

        }

        public void SetupWeatherFX()
        {

            if (!forecast || !forecast.enabled)
                if (weatherProfile.particleFX && weatherProfile.useVFX)
                {

                    foreach (CozyParticles cozyParticles in FindObjectsOfType<CozyParticles>())
                    {

                        if (cozyParticles.weatherProfile == weatherProfile)
                            return;

                    }

                    ParticleSystem system = Instantiate(weatherProfile.particleFX, transform.GetChild(2)).GetComponent<ParticleSystem>();

                    system.gameObject.name = weatherProfile.name;

                    if (system.GetComponent<CozyParticles>())
                        system.GetComponent<CozyParticles>().weatherProfile = weatherProfile;
                    else
                        system.gameObject.AddComponent<CozyParticles>().weatherProfile = weatherProfile;


                    if (triggerManager)
                    {
                        ParticleSystem.TriggerModule triggers = system.trigger;

                        triggers.enter = ParticleSystemOverlapAction.Kill;
                        triggers.inside = ParticleSystemOverlapAction.Kill;
                        for (int j = 0; j < triggerManager.cozyTriggers.Count; j++)
                            triggers.SetCollider(j, triggerManager.cozyTriggers[j]);
                    }
                }
        }


        public void SetFilterColors ()
        {

            m_FilterColor = WeatherLerp(m_FilterColor, weatherProfile.weatherFilter.colorFilter);
            m_FilterSaturation = WeatherLerp(m_FilterSaturation, weatherProfile.weatherFilter.saturation);
            m_FilterValue = WeatherLerp(m_FilterValue, weatherProfile.weatherFilter.value);
            m_SunFilter = WeatherLerp(m_SunFilter, weatherProfile.sunFilter);
            m_CloudFilter = WeatherLerp(m_CloudFilter, weatherProfile.cloudFilter);



        }


        public void SetGlobalVariables()
        {

            Material i = FogDome.sharedMaterial;

            Shader.SetGlobalColor("CZY_FogColor1", m_FogColors.color1);
            Shader.SetGlobalColor("CZY_FogColor2", m_FogColors.color2);
            Shader.SetGlobalColor("CZY_FogColor3", m_FogColors.color3);
            Shader.SetGlobalColor("CZY_FogColor4", m_FogColors.color4);
            Shader.SetGlobalColor("CZY_FogColor5", m_FogColors.color5);

            Shader.SetGlobalFloat("CZY_FogColorStart1", atmosphereProfile.fogStart1);
            Shader.SetGlobalFloat("CZY_FogColorStart2", atmosphereProfile.fogStart2);
            Shader.SetGlobalFloat("CZY_FogColorStart3", atmosphereProfile.fogStart3);
            Shader.SetGlobalFloat("CZY_FogColorStart4", atmosphereProfile.fogStart4);


            Shader.SetGlobalFloat("CZY_FogIntensity", i.GetFloat("_FogIntensity"));
            Shader.SetGlobalFloat("CZY_FogOffset", atmosphereProfile.fogHeight);
            Shader.SetGlobalFloat("CZY_FogSmoothness", i.GetFloat("_FogSmoothness"));
            Shader.SetGlobalFloat("CZY_FogDepthMultiplier", m_FogDensity);

            Shader.SetGlobalColor("CZY_LightColor",  m_FogFlareColor);
            Shader.SetGlobalVector("CZY_SunDirection", -m_SunLight.transform.forward);
            Shader.SetGlobalFloat("CZY_LightFalloff", i.GetFloat("_LightFalloff"));
            Shader.SetGlobalFloat("CZY_LightIntensity", i.GetFloat("LightIntensity"));




        }

        public void SetShaderVariables()
        {

            Material skyMat = m_Skysphere.sharedMaterial;
            Material cloudsMat = m_Clouds.sharedMaterial;
            Material fogMat = m_FogDome.sharedMaterial;

            if (skyMat.HasProperty("_ZenithColor"))
                skyMat.SetColor("_ZenithColor", m_ZenithColor);
            if (skyMat.HasProperty("_HorizonColor"))
                skyMat.SetColor("_HorizonColor", m_HorizonColor);
            if (skyMat.HasProperty("_StarColor"))
                skyMat.SetColor("_StarColor", m_StarColor);
            if (skyMat.HasProperty("_GalaxyMultiplier"))
                skyMat.SetFloat("_GalaxyMultiplier", m_GalaxyIntensity);
            if (skyMat.HasProperty("_RainbowIntensity"))
                skyMat.SetFloat("_RainbowIntensity", m_RainbowIntensity);

            if (cloudsMat.HasProperty("_MinCloudCover"))
                cloudsMat.SetFloat("_MinCloudCover", m_MinCloudCover);
            if (cloudsMat.HasProperty("_MaxCloudCover"))
                cloudsMat.SetFloat("_MaxCloudCover", m_MaxCloudCover);
            if (cloudsMat.HasProperty("_CloudColor"))
                cloudsMat.SetColor("_CloudColor", m_CloudColor);
            if (cloudsMat.HasProperty("_CloudHighlightColor"))
                cloudsMat.SetColor("_CloudHighlightColor", m_CloudHighlight);
            if (cloudsMat.HasProperty("_AltoCloudColor"))
                cloudsMat.SetColor("_AltoCloudColor", m_CloudZenith);
            if (cloudsMat.HasProperty("_SunDirection"))
                cloudsMat.SetVector("_SunDirection", -m_SunLight.transform.forward);
            if (wind)
                if (cloudsMat.HasProperty("_StormDirection"))
                    cloudsMat.SetVector("_StormDirection", -wind.windDirection);
            if (cloudsMat.HasProperty("_CumulusCoverageMultiplier"))
                cloudsMat.SetFloat("_CumulusCoverageMultiplier", m_Cumulus);
            if (cloudsMat.HasProperty("_NimbusMultiplier"))
                cloudsMat.SetFloat("_NimbusMultiplier", m_Nimbus);
            if (cloudsMat.HasProperty("_NimbusHeight"))
                cloudsMat.SetFloat("_NimbusHeight", m_NimbusHeight);
            if (cloudsMat.HasProperty("_NimbusVariation"))
                cloudsMat.SetFloat("_NimbusVariation", m_NimbusVariation);
            if (cloudsMat.HasProperty("_BorderHeight"))
                cloudsMat.SetFloat("_BorderHeight", m_Border);
            if (cloudsMat.HasProperty("_BorderEffect"))
                cloudsMat.SetFloat("_BorderEffect", m_BorderEffect);
            if (cloudsMat.HasProperty("_BorderVariation"))
                cloudsMat.SetFloat("_BorderVariation", m_BorderVariation);
            if (cloudsMat.HasProperty("_AltocumulusMultiplier"))
                cloudsMat.SetFloat("_AltocumulusMultiplier", m_Altocumulus);
            if (cloudsMat.HasProperty("_CirrostratusMultiplier"))
                cloudsMat.SetFloat("_CirrostratusMultiplier", m_Cirrostratus);
            if (cloudsMat.HasProperty("_ChemtrailsMultiplier"))
                cloudsMat.SetFloat("_ChemtrailsMultiplier", m_Chemtrails);
            if (cloudsMat.HasProperty("_CirrusMultiplier"))
                cloudsMat.SetFloat("_CirrusMultiplier", m_Cirrus);


            m_SunLight.transform.parent.eulerAngles = new Vector3(0, sunDirection, sunPitch);
            m_SunLight.transform.localEulerAngles = new Vector3(((perennialProfile.currentTicks / perennialProfile.ticksPerDay) * 360) - 90, 0, 0);
            m_SunLight.color = m_SunColor;
            SetStarColors(m_StarColor);
            SetCloudColors(m_CloudColor);


            RenderSettings.ambientSkyColor = m_AmbientZenith;
            RenderSettings.ambientEquatorColor = m_AmbientHorizon * (1 - (m_MaxCloudCover / 2));
            RenderSettings.ambientGroundColor = m_AmbientHorizon * Color.gray * (1 - (m_MaxCloudCover / 2));




            if (lockToCamera)
                if (fogMat.HasProperty("_FogSmoothness"))
                    fogMat.SetFloat("_FogSmoothness", 100 * m_AdjustedScale);


            if (fogMat.HasProperty("_FogDepthMultiplier"))
                fogMat.SetFloat("_FogDepthMultiplier", m_FogDensity);
            if (fogMat.HasProperty("_LightColor"))
                fogMat.SetColor("_LightColor", m_FogFlareColor);
            if (fogMat.HasProperty("_FogColor1"))
                fogMat.SetColor("_FogColor1", m_FogColors.color1);
            if (fogMat.HasProperty("_FogColor2"))
                fogMat.SetColor("_FogColor2", m_FogColors.color2);
            if (fogMat.HasProperty("_FogColor3"))
                fogMat.SetColor("_FogColor3", m_FogColors.color3);
            if (fogMat.HasProperty("_FogColor4"))
                fogMat.SetColor("_FogColor4", m_FogColors.color4);
            if (fogMat.HasProperty("_FogColor5"))
                fogMat.SetColor("_FogColor5", m_FogColors.color5);
            if (fogMat.HasProperty("_SunDirection"))
                fogMat.SetVector("_SunDirection", -m_SunLight.transform.forward);


        }

        public void SetStaticShaderVariables()
        {

            Material skybox = m_Skysphere.sharedMaterial;
            Material clouds = m_Clouds.sharedMaterial;
            Material fog = m_FogDome.sharedMaterial;

            skybox.SetFloat("_Power", atmosphereProfile.gradientExponent);
            skybox.SetFloat("_PatchworkHeight", atmosphereProfile.atmosphereVariation.y);
            skybox.SetFloat("_PatchworkVariation", atmosphereProfile.atmosphereVariation.x);
            skybox.SetFloat("_PatchworkBias", atmosphereProfile.atmosphereBias);
            skybox.SetFloat("_SunSize", atmosphereProfile.sunSize);
            skybox.SetColor("_SunColor", atmosphereProfile.sunColor);
            skybox.SetFloat("_SunFlareFalloff", atmosphereProfile.sunFalloff);
            skybox.SetColor("_SunFlareColor", atmosphereProfile.sunFlareColor);
            skybox.SetColor("_MoonFlareColor", atmosphereProfile.moonFlareColor);
            skybox.SetFloat("_MoonFlareFalloff", atmosphereProfile.moonFalloff);
            skybox.SetColor("_GalaxyColor1", atmosphereProfile.galaxy1Color);
            skybox.SetColor("_GalaxyColor2", atmosphereProfile.galaxy2Color);
            skybox.SetColor("_GalaxyColor3", atmosphereProfile.galaxy3Color);
            skybox.SetColor("_LightColumnColor", atmosphereProfile.lightScatteringColor);
            skybox.SetFloat("_RainbowSize", atmosphereProfile.rainbowPosition);
            skybox.SetFloat("_RainbowWidth", atmosphereProfile.rainbowWidth);

            clouds.SetColor("_CloudTextureColor", atmosphereProfile.cloudTextureColor);
            clouds.SetColor("_MoonColor", atmosphereProfile.cloudMoonColor);
            clouds.SetFloat("_SunFlareFalloff", atmosphereProfile.cloudSunHighlightFalloff);
            clouds.SetFloat("_MoonFlareFalloff", atmosphereProfile.cloudMoonHighlightFalloff);
            clouds.SetFloat("_WindSpeed", atmosphereProfile.cloudWindSpeed);
            clouds.SetFloat("_CloudCohesion", atmosphereProfile.cloudCohesion);
            clouds.SetFloat("_Spherize", atmosphereProfile.spherize);
            clouds.SetFloat("_ClippingThreshold", atmosphereProfile.clippingThreshold);
            clouds.SetFloat("_CloudThickness", atmosphereProfile.cloudThickness);
            clouds.SetFloat("_MainCloudScale", atmosphereProfile.cloudMainScale);
            clouds.SetFloat("_DetailScale", atmosphereProfile.cloudDetailScale);
            clouds.SetFloat("_DetailAmount", atmosphereProfile.cloudDetailAmount);
            clouds.SetFloat("_TextureAmount", atmosphereProfile.textureAmount);
            clouds.SetFloat("_AltocumulusScale", atmosphereProfile.acScale);
            clouds.SetFloat("_CirrostratusMoveSpeed", atmosphereProfile.cirroMoveSpeed);
            clouds.SetFloat("_CirrusMoveSpeed", atmosphereProfile.cirrusMoveSpeed);
            clouds.SetFloat("_ChemtrailsMoveSpeed", atmosphereProfile.chemtrailsMoveSpeed);

            fog.SetFloat("LightIntensity", atmosphereProfile.fogLightFlareIntensity);
            fog.SetFloat("_LightFalloff", atmosphereProfile.fogLightFlareFalloff);
            fog.SetFloat("_FogOffset", atmosphereProfile.fogHeight);
            fog.SetFloat("_FogColorStart1", atmosphereProfile.fogStart1);
            fog.SetFloat("_FogColorStart2", atmosphereProfile.fogStart2);
            fog.SetFloat("_FogColorStart3", atmosphereProfile.fogStart3);
            fog.SetFloat("_FogColorStart4", atmosphereProfile.fogStart4);


        }

        private void SetStarColors(Color color)
        {
            foreach (ParticleSystem i in m_Stars)
            {
                ParticleSystem.MainModule j = i.main;
                j.startColor = color;

            }
        }

        private void SetCloudColors(Color color)
        {
            foreach (ParticleSystem i in m_CloudParticles)
            {
                ParticleSystem.MainModule j = i.main;
                j.startColor = color; 
                
                ParticleSystem.TrailModule k = i.trails;
                k.colorOverLifetime = color;

            }
        }

        public void SetProperties()
        {
            
            SetFilterColors();

            m_MinCloudCover = WeatherLerp(m_MinCloudCover, weatherProfile.cloudSettings.cloudCoverage.x);
            m_MaxCloudCover = WeatherLerp(m_MaxCloudCover, weatherProfile.cloudSettings.cloudCoverage.y);
            m_ZenithColor = FilterColor(atmosphereProfile.skyZenithColor);
            m_HorizonColor = FilterColor(atmosphereProfile.skyHorizonColor);
            m_CloudColor = FilterColor(atmosphereProfile.cloudColor) * m_CloudFilter;
            m_CloudHighlight = FilterColor(atmosphereProfile.cloudHighlightColor) * m_SunFilter;
            m_CloudZenith = FilterColor(atmosphereProfile.highAltitudeCloudColor) * m_CloudFilter;
            m_SunColor = FilterColor(atmosphereProfile.sunlightColor) * m_SunFilter;
            m_StarColor = FilterColor(atmosphereProfile.starColor);
            m_GalaxyIntensity = atmosphereProfile.galaxyIntensity.Evaluate(perennialProfile.currentTicks / perennialProfile.ticksPerDay);
            if (cozyMaterials)
                m_RainbowIntensity = cozyMaterials.useRainbow ? cozyMaterials.m_Wetness * (1 - m_StarColor.a) : 0;
            else
                m_RainbowIntensity = 0;

            m_Cumulus = WeatherLerp(m_Cumulus, weatherProfile.cloudSettings.cumulusCoverage);
            m_Altocumulus = WeatherLerp(m_Altocumulus, weatherProfile.cloudSettings.altocumulusCoverage);
            m_Cirrus = WeatherLerp(m_Cirrus, weatherProfile.cloudSettings.cirrusCoverage);
            m_Cirrostratus = WeatherLerp(m_Cirrostratus, weatherProfile.cloudSettings.cirrostratusCoverage);
            m_Chemtrails = WeatherLerp(m_Chemtrails, weatherProfile.cloudSettings.chemtrailCoverage);
            m_Nimbus = WeatherLerp(m_Nimbus, weatherProfile.cloudSettings.nimbusCoverage);
            m_NimbusHeight = WeatherLerp(m_NimbusHeight, weatherProfile.cloudSettings.nimbusHeightEffect);
            m_NimbusVariation = WeatherLerp(m_NimbusVariation, weatherProfile.cloudSettings.nimbusVariation);
            m_Border = WeatherLerp(m_Border, weatherProfile.cloudSettings.borderHeight, 3);
            m_BorderEffect = WeatherLerp(m_BorderEffect, weatherProfile.cloudSettings.borderEffect);
            m_BorderVariation = WeatherLerp(m_BorderVariation, weatherProfile.cloudSettings.borderVariation, 3);



            m_AmbientHorizon = FilterColor(atmosphereProfile.ambientLightHorizonColor);
            m_AmbientZenith = FilterColor(atmosphereProfile.ambientLightZenithColor);
                                                                                                         

            m_FogDensity = WeatherLerp(m_FogDensity, weatherProfile.fogDensity);
            m_FogFlareColor = FilterColor(atmosphereProfile.fogFlareColor);
            m_FogColors.color1 = FilterColor(atmosphereProfile.fogColor1);
            m_FogColors.color2 = FilterColor(atmosphereProfile.fogColor2);
            m_FogColors.color3 = FilterColor(atmosphereProfile.fogColor3);
            m_FogColors.color4 = FilterColor(atmosphereProfile.fogColor4);
            m_FogColors.color5 = FilterColor(atmosphereProfile.fogColor5);



        }

        public void ResetVariables()
        {


            m_FilterColor = weatherProfile.weatherFilter.colorFilter;
            m_FilterSaturation = weatherProfile.weatherFilter.saturation;
            m_FilterValue = weatherProfile.weatherFilter.value;
            m_SunFilter = weatherProfile.sunFilter;
            m_CloudFilter = weatherProfile.cloudFilter;
            m_GalaxyIntensity = atmosphereProfile.galaxyIntensity.Evaluate(perennialProfile.currentTicks / perennialProfile.ticksPerDay);
            if (cozyMaterials)
                m_RainbowIntensity = cozyMaterials.useRainbow ? cozyMaterials.m_Wetness * (1 - m_StarColor.a) : 0;
            else
                m_RainbowIntensity = 0;

            m_MinCloudCover = weatherProfile.cloudSettings.cloudCoverage.x;
            m_MaxCloudCover = weatherProfile.cloudSettings.cloudCoverage.y;


            m_CloudColor = FilterColor(atmosphereProfile.cloudColor.Evaluate(perennialProfile.currentTicks / perennialProfile.ticksPerDay)) * m_CloudFilter;
            m_CloudZenith = FilterColor(atmosphereProfile.highAltitudeCloudColor.Evaluate(perennialProfile.currentTicks / perennialProfile.ticksPerDay)) * m_CloudFilter;
            m_CloudHighlight = FilterColor(atmosphereProfile.cloudHighlightColor.Evaluate(perennialProfile.currentTicks / perennialProfile.ticksPerDay)) * m_SunFilter;
            m_HorizonColor = FilterColor(atmosphereProfile.skyHorizonColor.Evaluate(perennialProfile.currentTicks / perennialProfile.ticksPerDay));
            m_ZenithColor = FilterColor(atmosphereProfile.skyZenithColor.Evaluate(perennialProfile.currentTicks / perennialProfile.ticksPerDay));
            m_StarColor = FilterColor(atmosphereProfile.starColor);


            m_Cumulus = weatherProfile.cloudSettings.cumulusCoverage;
            m_Altocumulus = weatherProfile.cloudSettings.altocumulusCoverage;
            m_Cirrus = weatherProfile.cloudSettings.cirrusCoverage;
            m_Cirrostratus = weatherProfile.cloudSettings.cirrostratusCoverage;
            m_Chemtrails = weatherProfile.cloudSettings.chemtrailCoverage;
            m_Nimbus = weatherProfile.cloudSettings.nimbusCoverage;
            m_NimbusHeight = weatherProfile.cloudSettings.nimbusHeightEffect;
            m_NimbusVariation = weatherProfile.cloudSettings.nimbusVariation;
            m_Border = weatherProfile.cloudSettings.borderHeight;
            m_BorderEffect = weatherProfile.cloudSettings.borderEffect;
            m_BorderVariation = weatherProfile.cloudSettings.borderVariation;


            m_SunColor = atmosphereProfile.sunlightColor.Evaluate(perennialProfile.currentTicks / perennialProfile.ticksPerDay) * m_SunFilter;
            m_AmbientHorizon = FilterColor(atmosphereProfile.ambientLightHorizonColor.Evaluate(perennialProfile.currentTicks / perennialProfile.ticksPerDay));
            m_AmbientZenith = FilterColor(atmosphereProfile.ambientLightZenithColor.Evaluate(perennialProfile.currentTicks / perennialProfile.ticksPerDay));

            m_FogDensity = weatherProfile.fogDensity;
            m_FogFlareColor = FilterColor(atmosphereProfile.fogFlareColor.Evaluate(perennialProfile.currentTicks / perennialProfile.ticksPerDay));
            m_FogColors.color1 = FilterColor(atmosphereProfile.fogColor1.Evaluate(perennialProfile.currentTicks / perennialProfile.ticksPerDay));
            m_FogColors.color2 = FilterColor(atmosphereProfile.fogColor2.Evaluate(perennialProfile.currentTicks / perennialProfile.ticksPerDay));
            m_FogColors.color3 = FilterColor(atmosphereProfile.fogColor3.Evaluate(perennialProfile.currentTicks / perennialProfile.ticksPerDay));
            m_FogColors.color4 = FilterColor(atmosphereProfile.fogColor4.Evaluate(perennialProfile.currentTicks / perennialProfile.ticksPerDay));
            m_FogColors.color5 = FilterColor(atmosphereProfile.fogColor5.Evaluate(perennialProfile.currentTicks / perennialProfile.ticksPerDay));



        }

        public float WeatherLerp(float start, float target) { return Mathf.Lerp(start, target, Time.deltaTime * weatherTransitionSpeed * perennialProfile.ModifiedTickSpeed()); }
        public float WeatherLerp(float start, float target, float speedMultiplier) { return Mathf.Lerp(start, target, Time.deltaTime * weatherTransitionSpeed * speedMultiplier * perennialProfile.ModifiedTickSpeed()); }

        public Color WeatherLerp(Color start, Color target) { return Color.Lerp(start, target, Time.deltaTime * weatherTransitionSpeed * perennialProfile.ModifiedTickSpeed()); }
        public Color WeatherLerp(Color start, Gradient target) { return Color.Lerp(start, target.Evaluate(perennialProfile.currentTicks / perennialProfile.ticksPerDay), Time.deltaTime * weatherTransitionSpeed * perennialProfile.ModifiedTickSpeed()); }
        public Color AtmosGradient(Gradient target) { return Color.Lerp(target.Evaluate(perennialProfile.currentTicks / perennialProfile.ticksPerDay), FilterColor(target.Evaluate(perennialProfile.currentTicks / perennialProfile.ticksPerDay)), Time.deltaTime * weatherTransitionSpeed * perennialProfile.ModifiedTickSpeed()); }

        public Color FilterColor(Color color)
        {

            float h;
            float s;
            float v;
            float a = color.a;
            Color j;

            Color.RGBToHSV(color, out h, out s, out v);

            s = Mathf.Clamp(s + m_FilterSaturation, 0, 10);
            v = Mathf.Clamp(v + m_FilterValue, 0, 10);

            j = Color.HSVToRGB(h, s, v);

            j *= m_FilterColor;
            j.a = a;

            return j;

        }

        public Color FilterColor(Gradient color)
        {

            float h;
            float s;
            float v;
            float a = color.Evaluate(perennialProfile.currentTicks / perennialProfile.ticksPerDay).a;
            Color j;

            Color.RGBToHSV(color.Evaluate(perennialProfile.currentTicks / perennialProfile.ticksPerDay), out h, out s, out v);


            s = Mathf.Clamp(s + m_FilterSaturation, 0, 10);
            v = Mathf.Clamp(v + m_FilterValue, 0, 10);

            j = Color.HSVToRGB(h, s, v, true);

            j *= m_FilterColor;
            j.a = a;

            return j;

        }

        public void ChangeProfile(WeatherProfile profile)
        {

            weatherProfile = profile;


            if (!forecast)
                SetupWeatherFX();

            if (cozyAudio)
                cozyAudio.ChangeSound(profile);
            

        }

        public MeshRenderer CloudDome
        {
            get { return m_Clouds; }
        }
        public Light Sun
        {
            get { return m_SunLight; }
        }
        public MeshRenderer FogDome
        {
            get { return m_FogDome; }
        }
        public MeshRenderer SkyDome
        {
            get { return m_Skysphere; }
        }

        public void SkipTicks(float ticksToSkip)
        {

            perennialProfile.currentTicks += ticksToSkip;
            GetComponent<CozyForecast>().SkipTicks(ticksToSkip);
            GetComponent<CozyAmbienceManager>().SkipTicks(ticksToSkip);
            ResetVariables();

        }

        static public CozyWeather instance
        {

            get { if (FindObjectOfType<CozyWeather>()) return FindObjectOfType<CozyWeather>(); else return null; }

        }

    }
}