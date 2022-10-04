// Distant Lands 2021.





using DistantLands.Cozy.Data;
using UnityEngine;


namespace DistantLands.Cozy
{

    [ExecuteAlways]
    public class CozyClimate : CozyModule
    {



        public ClimateProfile climateProfile;


        [Tooltip("Adds an offset to the local temprature. Useful for adding biomes or climate change by location or elevation")]
        public float localTempratureFilter;
        [Tooltip("Adds an offset to the local precipitation. Useful for adding biomes or climate change by location or elevation")]
        public float localPrecipitationFilter;


        public float currentTemprature;
        public float currentTempratureCelsius;
        public float currentPrecipitation;





        void Awake()
        {

            if (!enabled)
                return;

            base.SetupModule();

            currentTemprature = climateProfile.GetTemprature(false, weatherSphere.perennialProfile);
            currentPrecipitation = climateProfile.GetHumidity(weatherSphere.perennialProfile);


        }

        // Update is called once per frame
        public void Update()
        {

            if (weatherSphere == null)
                if (CozyWeather.instance)
                    base.SetupModule();
                else
                {
                    Debug.LogError("Could not find an instance of COZY. Disabling " + name + ".");
                    return;
                }


            currentTemprature = climateProfile.GetTemprature(false, weatherSphere.perennialProfile) + localTempratureFilter;
            currentTempratureCelsius = climateProfile.GetTemprature(true, weatherSphere.perennialProfile) + localTempratureFilter;
            currentPrecipitation = climateProfile.GetHumidity(weatherSphere.perennialProfile) + localPrecipitationFilter;



        }


        public float GetTemprature(bool celsius)
        {

            return climateProfile.GetTemprature(celsius, weatherSphere.perennialProfile) + localTempratureFilter;

        }

        public float GetTemprature(bool celsius, float inTicks)
        {

            return climateProfile.GetTemprature(celsius, weatherSphere.perennialProfile, inTicks) + localTempratureFilter;

        }

        public float GetHumidity()
        {

            return climateProfile.GetHumidity(weatherSphere.perennialProfile) + localPrecipitationFilter;

        }

        public float GetHumidity(float inTicks)
        {

            return climateProfile.GetHumidity(weatherSphere.perennialProfile, inTicks) + localPrecipitationFilter;
        }



    }
}