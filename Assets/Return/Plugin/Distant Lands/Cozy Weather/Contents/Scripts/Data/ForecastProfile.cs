// Distant Lands 2022.



using DistantLands.Cozy.Data;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;



namespace DistantLands.Cozy.Data
{

    [System.Serializable]
    [CreateAssetMenu(menuName = "Distant Lands/Cozy/Forecast Profile", order = 361)]
    public class ForecastProfile : ScriptableObject
    {


        [Tooltip("The weather profiles that this profile will forecast.")]
        public List<WeatherProfile> profilesToForecast;

        public WeatherProfile startWeatherWith;
        public bool startWithRandomWeather = true;

        [Tooltip("The amount of weather profiles to forecast ahead.")]
        public int forecastLength;

    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ForecastProfile))]
[CanEditMultipleObjects]
public class E_ForecastProfile : Editor
{

    SerializedProperty profilesToForecast;
    SerializedProperty forecastLength;
    SerializedProperty startWeatherWith;
    SerializedProperty startWithRandomWeather;
    ForecastProfile prof;
    Vector2 scrollPos;

    void OnEnable()
    {
        profilesToForecast = serializedObject.FindProperty("profilesToForecast");
        forecastLength = serializedObject.FindProperty("forecastLength");
        startWeatherWith = serializedObject.FindProperty("startWeatherWith");
        startWithRandomWeather = serializedObject.FindProperty("startWithRandomWeather");
        prof = (ForecastProfile)target;

    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();


        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        EditorGUILayout.PropertyField(profilesToForecast);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(startWithRandomWeather);
        if (startWithRandomWeather.boolValue == false)
            EditorGUILayout.PropertyField(startWeatherWith);

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(forecastLength);
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(prof);

        EditorGUILayout.EndScrollView();

    }

}
#endif