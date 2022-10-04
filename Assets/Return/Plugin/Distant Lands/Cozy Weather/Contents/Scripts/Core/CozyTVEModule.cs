using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if THE_VEGETATION_ENGINE
using TheVegetationEngine;
#endif

namespace DistantLands.Cozy {

    [ExecuteAlways]
    public class CozyTVEModule : CozyModule
    {

        public enum UpdateFrequency { everyFrame, onAwake, viaScripting }
        public UpdateFrequency updateFrequency;

#if THE_VEGETATION_ENGINE

        public TVEGlobalControl globalControl;
        public TVEGlobalMotion globalMotion;

#endif


        // Start is called before the first frame update
        void Awake()
        {

            SetupModule();

#if THE_VEGETATION_ENGINE
            if (updateFrequency == UpdateFrequency.onAwake)
                UpdateTVE();
#endif

        }

#if THE_VEGETATION_ENGINE
        public override void SetupModule()
        {

            if (!enabled)
                return;

            weatherSphere = CozyWeather.instance;

            if (!weatherSphere)
            {
                enabled = false;
                return;
            }

            if (!globalControl)
                globalControl = FindObjectOfType<TVEGlobalControl>();

            if (!globalControl)
            {
                enabled = false;
                return;
            }

            if (!globalMotion)
                globalMotion = FindObjectOfType<TVEGlobalMotion>();

            if (!globalMotion)
            {
                enabled = false;
                return;
            }

            calendarModule = weatherSphere.calender;
            climateModule = weatherSphere.climate;
            forecastModule = weatherSphere.forecast;
            materialManagerModule = weatherSphere.cozyMaterials;
            ambienceManagerModule = weatherSphere.ambience;
            audioModule = weatherSphere.cozyAudio;
            triggerManagerModule = weatherSphere.triggerManager;
            lightningManagerModule = weatherSphere.lightningManager;

            globalControl.mainLight = weatherSphere.Sun;


        }


        // Update is called once per frame
        void Update()
        {

            if (updateFrequency == UpdateFrequency.everyFrame)
                UpdateTVE();



        }

        public void UpdateTVE()
        {


            globalControl.globalWetness = materialManagerModule.m_Wetness;
            globalControl.globalOverlay = materialManagerModule.m_SnowAmount;
            globalControl.seasonControl = weatherSphere.perennialProfile.YearPercentage() * 4;
            globalMotion.windPower = weatherSphere.weatherProfile.windSpeed;
            globalMotion.transform.LookAt(globalMotion.transform.position + weatherSphere.wind.windDirection, Vector3.up);



        }

#endif
    }
}