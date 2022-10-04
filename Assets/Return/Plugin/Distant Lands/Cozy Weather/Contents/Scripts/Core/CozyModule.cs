//Distant Lands 2022.

//Empty COZY: Weather Module that contains all necessary references and is used as a base class for all subsequent modules.



using UnityEngine;



namespace DistantLands.Cozy
{

    [RequireComponent(typeof(CozyWeather))]
    public class CozyModule : MonoBehaviour
    {


        [HideInInspector]
        public CozyWeather weatherSphere;
        [HideInInspector]
        public CozyCalendar calendarModule;
        [HideInInspector]
        public CozyClimate climateModule;
        [HideInInspector]
        public CozyForecast forecastModule;
        [HideInInspector]
        public CozyMaterialManager materialManagerModule;
        [HideInInspector]
        public CozyAmbienceManager ambienceManagerModule;
        [HideInInspector]
        public CozyAudio audioModule;
        [HideInInspector]
        public CozyTriggerManager triggerManagerModule;
        [HideInInspector]
        public CozyLightningManager lightningManagerModule;

        public virtual void SetupModule()
        {

            if (!enabled)
                return;
            weatherSphere = CozyWeather.instance;

            calendarModule = weatherSphere.calender;
            climateModule = weatherSphere.climate;
            forecastModule = weatherSphere.forecast;
            materialManagerModule = weatherSphere.cozyMaterials;
            ambienceManagerModule = weatherSphere.ambience;
            audioModule = weatherSphere.cozyAudio;
            triggerManagerModule = weatherSphere.triggerManager;
            lightningManagerModule = weatherSphere.lightningManager;



        }

        public virtual void DisableModule()
        {


            DestroyImmediate(this);

        }

    }
}