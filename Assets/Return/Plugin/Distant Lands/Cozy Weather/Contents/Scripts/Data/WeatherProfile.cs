// Distant Lands 2021.



using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif



namespace DistantLands.Cozy.Data
{

    [System.Serializable]
    [CreateAssetMenu(menuName = "Distant Lands/Cozy/Weather Profile", order = 361)]
    public class WeatherProfile : ScriptableObject
    {

        [Tooltip("Specifies the minimum (x) and maximum (y) length for this weather profile.")]
        public Vector2 weatherTime = new Vector2(120, 480);
        [Tooltip("Multiplier for the computational chance that this weather profile will play; 0 being never, and 2 being twice as likely as the average.")]
        [Range(0, 2)]
        public float likelihood = 1;              
        [Tooltip("Allow only these weather profiles to immediately follow this weather profile in a forecast.")]
        public WeatherProfile[] forecastNext;
        [Tooltip("Animation curves that increase or decrease weather chance based on time, temprature, etc.")]
        public List<ChanceEffector> chances;


        public CloudSettings cloudSettings;


        [Tooltip("The density of fog for this weather profile.")]
        public float fogDensity = 1;


        [Tooltip("The average wind speed for this weather profile.")]
        public float windSpeed = 0.1f;
        [Tooltip("The average wind amount for this weather profile.")]
        public float windAmount = 1;

        [Tooltip("How much snow should accumulate per second.")]
        public float snowAccumulationSpeed = 0;
        [Tooltip("How much should puddles accumulate per second.")]
        public float wetnessSpeed = 0;
        public GameObject particleFX;
        public bool useThunder;
        [Tooltip("Specifies the minimum (x) and maximum (y) time between lightning strikes.")]
        public Vector2 thunderTime = new Vector2(7, 13);

        public bool useVFX;
        public AudioClip soundFX;
        public float FXVolume = 1;


        public WeatherFilter weatherFilter;

        [System.Serializable]
        public class WeatherFilter
        {
            [Range(-1, 1)]
            public float saturation;
            [Range(-1, 1)]
            public float value;
            public Color colorFilter = Color.white;

        }


        [System.Serializable]
        public class CloudSettings
        {
            [Tooltip("Specifies the minimum (x) and maximum (y) cloud coverage for this weather profile.")]
            public Vector2 cloudCoverage = new Vector2(0.2f, 0.3f);
            [Tooltip("Multiplier for cumulus clouds.")]
            [Range(0, 2)]
            public float cumulusCoverage = 1;
            [Tooltip("Multiplier for altocumulus clouds.")]
            [Range(0, 2)]
            public float altocumulusCoverage = 0;
            [Tooltip("Multiplier for chemtrails.")]
            [Range(0, 2)]
            public float chemtrailCoverage = 0;
            [Tooltip("Multiplier for cirrostratus clouds.")]
            [Range(0, 2)]
            public float cirrostratusCoverage = 0;
            [Tooltip("Multiplier for cirrus clouds.")]
            [Range(0, 2)]
            public float cirrusCoverage = 0;
            [Tooltip("Multiplier for nimbus clouds.")]
            [Range(0, 2)]
            public float nimbusCoverage = 0;
            [Tooltip("Variation for nimbus clouds.")]
            [Range(0, 1)]
            public float nimbusVariation = 0.9f;
            [Tooltip("Height mask effect for nimbus clouds.")]
            [Range(0, 1)]
            public float nimbusHeightEffect = 1;

            [Tooltip("Starting height for cloud border.")]
            [Range(0, 1)]
            public float borderHeight = 0.5f;
            [Tooltip("Variation for cloud border.")]
            [Range(0, 1)]
            public float borderVariation = 0.9f;
            [Tooltip("Multiplier for the border. Values below zero clip the clouds whereas values above zero add clouds.")]
            [Range(-1, 1)]
            public float borderEffect = 1;

        }

        public Color sunFilter = Color.white;
        public Color cloudFilter = Color.white;





        [System.Serializable]
        public class ChanceEffector
        {

            public enum LimitType { Temprature, Precipitation, YearPercentage, Time };
            public LimitType limitType;
            public AnimationCurve curve;


            public float GetChance(float temp, float precip, float yearPercent, float timePercent)
            {

                switch (limitType)
                {
                    case LimitType.Temprature:
                        return curve.Evaluate(temp / 100);
                    case (LimitType.Precipitation):
                        return curve.Evaluate(precip / 100);
                    case (LimitType.YearPercentage):
                        return curve.Evaluate(yearPercent);
                    case (LimitType.Time):
                        return curve.Evaluate(timePercent);
                    default:
                        return 1;
                }

            }
        }


        public float GetChance (float temp, float precip, float yearPercent, float time)
        {

            float i = likelihood;

            foreach (ChanceEffector j in chances)
            {
                i *= j.GetChance(temp, precip, yearPercent, time);
            }

            return Mathf.Clamp(i, 0, 1000000);

        }

    }


#if UNITY_EDITOR
    [CustomEditor(typeof(WeatherProfile))]
    [CanEditMultipleObjects]
    public class E_WeatherProfile : Editor
    {

        SerializedProperty forecastNext;
        SerializedProperty chances;
        SerializedProperty cloudSettings;
        SerializedProperty particleFX;
        SerializedProperty soundFX;
        SerializedProperty weatherFilter;
        SerializedProperty sunFilter;
        SerializedProperty cloudFilter;
        float minPlayTime = 30;
        float maxPlayTime = 90;
        Vector2 scrollPos;
        WeatherProfile prof;



        void OnEnable()
        {

            prof = (WeatherProfile)target;

            forecastNext = serializedObject.FindProperty("forecastNext");
            chances = serializedObject.FindProperty("chances");
            cloudSettings = serializedObject.FindProperty("cloudSettings");
            particleFX = serializedObject.FindProperty("particleFX");
            soundFX = serializedObject.FindProperty("soundFX");
            weatherFilter = serializedObject.FindProperty("weatherFilter");
            sunFilter = serializedObject.FindProperty("sunFilter");
            cloudFilter = serializedObject.FindProperty("cloudFilter");

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Undo.RecordObject(prof, prof.name + " Profile Changes");

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUILayout.LabelField("Forecasting Behaviours", EditorStyles.boldLabel);

            minPlayTime = prof.weatherTime.x;
            maxPlayTime = prof.weatherTime.y;
            EditorGUILayout.MinMaxSlider("Weather Play Length", ref minPlayTime, ref maxPlayTime, 0, 150);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Min Ticks: " + Mathf.Round(minPlayTime).ToString(), EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Max Ticks: " + Mathf.Round(maxPlayTime).ToString(), EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
            prof.weatherTime = new Vector2(minPlayTime, maxPlayTime);
            prof.likelihood = EditorGUILayout.Slider("Likelihood", prof.likelihood, 0, 2);

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(forecastNext, true);
            EditorGUILayout.PropertyField(chances, true);


            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("Cloud Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(cloudSettings);
            prof.fogDensity = EditorGUILayout.Slider("Fog Density", prof.fogDensity, 0, 5);


            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("Wind Settings", EditorStyles.boldLabel);
            prof.windSpeed = EditorGUILayout.Slider("Wind Speed", prof.windSpeed, 0, 2);
            prof.windAmount = EditorGUILayout.Slider("Wind Amount", prof.windAmount, 0, 2);

            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("Precipitation", EditorStyles.boldLabel);
            prof.snowAccumulationSpeed = EditorGUILayout.Slider("Snow Accumulation Speed", prof.snowAccumulationSpeed, 0, 0.05f);
            prof.wetnessSpeed = EditorGUILayout.Slider("Puddle Accumulation Speed", prof.wetnessSpeed, 0, 0.05f);

            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("Weather Effects", EditorStyles.boldLabel);
            prof.useVFX = EditorGUILayout.Toggle("Use FX", prof.useVFX);
            if (prof.useVFX)
            {
                EditorGUILayout.PropertyField(particleFX);
                EditorGUILayout.PropertyField(soundFX);
                if (soundFX.objectReferenceValue != null)
                    prof.FXVolume = EditorGUILayout.FloatField("FX Volume", prof.FXVolume);

                prof.useThunder = EditorGUILayout.Toggle("Use Thunder", prof.useThunder);
                if (prof.useThunder)
                {
                    float minThunderTime = prof.thunderTime.x;
                    float maxThunderTime = prof.thunderTime.y;
                    EditorGUILayout.MinMaxSlider("Lightning Time", ref minThunderTime, ref maxThunderTime, 0, 25);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Min Time Between Lightning: " + Mathf.Round(minThunderTime).ToString(), EditorStyles.miniLabel);
                    EditorGUILayout.LabelField("Max Time Between Lightning: " + Mathf.Round(maxThunderTime).ToString(), EditorStyles.miniLabel);
                    EditorGUILayout.EndHorizontal();
                    prof.thunderTime = new Vector2(minThunderTime, maxThunderTime);


                }

            }


            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("Color Correction", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(weatherFilter);
            EditorGUILayout.PropertyField(sunFilter);
            EditorGUILayout.PropertyField(cloudFilter);


            EditorGUILayout.Space(20);

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(prof);

            EditorGUILayout.EndScrollView();

        }
    }
#endif

}