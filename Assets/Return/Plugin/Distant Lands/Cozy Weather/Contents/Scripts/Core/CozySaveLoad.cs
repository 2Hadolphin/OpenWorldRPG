using DistantLands.Cozy.Data;
using System.Collections.Generic;
using UnityEngine;

namespace DistantLands.Cozy
{
    public class CozySaveLoad : CozyModule
    {



        // Start is called before the first frame update
        void Awake()
        {

            if (!enabled)
                return;

            SetupModule();

        }


        public void Save ()
        {

            if (weatherSphere == null)
                SetupModule();

            if (forecastModule)
            {

                List<CozyForecast.Weather> modifiedForecast = new List<CozyForecast.Weather>();

                CozyForecast.Weather currentWeather = new CozyForecast.Weather();
                currentWeather.profile = weatherSphere.weatherProfile;
                currentWeather.weatherProfileDuration = forecastModule.weatherTimer;

                modifiedForecast.Add(currentWeather);
                modifiedForecast.AddRange(forecastModule.forecast);

                string forecastJSON = JsonUtility.ToJson(modifiedForecast);
                PlayerPrefs.SetString("CZY_FORECASTJSON", forecastJSON);

            }

            if (materialManagerModule)
            {

                PlayerPrefs.SetFloat("CZY_MATS_SNOWAMOUNT", materialManagerModule.m_SnowAmount);
                PlayerPrefs.SetFloat("CZY_MATS_WETNESSAMOUNT", materialManagerModule.m_Wetness);

            }


            string profileJSON = JsonUtility.ToJson(weatherSphere.perennialProfile);
            PlayerPrefs.SetString("CZY_PERENNIALJSON", profileJSON);



        }

        public void Load ()
        {


            if (weatherSphere == null)
                SetupModule();

            if (forecastModule == null)
                Debug.Log("Hey");

            if (PlayerPrefs.HasKey("CZY_FORECASTJSON") && forecastModule)
                forecastModule.forecast = JsonUtility.FromJson<List<CozyForecast.Weather>>(PlayerPrefs.GetString("CZY_FORECASTJSON"));

            if (PlayerPrefs.HasKey("CZY_PERENNIALJSON")) {
                JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString("CZY_PERENNIALJSON"), weatherSphere.perennialProfile);
            }

            if (materialManagerModule)
            {
                if (PlayerPrefs.HasKey("CZY_MATS_SNOWAMOUNT"))
                    weatherSphere.cozyMaterials.m_SnowAmount = PlayerPrefs.GetFloat("CZY_MATS_SNOWAMOUNT");
                if (PlayerPrefs.HasKey("CZY_MATS_WETNESSAMOUNT"))
                    weatherSphere.cozyMaterials.m_Wetness = PlayerPrefs.GetFloat("CZY_MATS_WETNESSAMOUNT");
            }

            weatherSphere.Update();

        }
    }
}