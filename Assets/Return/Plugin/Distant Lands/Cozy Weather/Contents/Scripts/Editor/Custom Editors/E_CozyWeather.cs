using UnityEditor;
using UnityEngine;

namespace DistantLands.Cozy.EditorScripts
{
    [CustomEditor(typeof(CozyWeather))]
    [CanEditMultipleObjects]
    public class E_CozyWeather : Editor
    {

        SerializedProperty atmo;
        SerializedProperty weather;
        SerializedProperty annual;


        SerializedProperty lockToCamera;
        SerializedProperty allowRuntimeChanges;
        SerializedProperty sunDirection;
        SerializedProperty sunPitch;
        SerializedProperty weatherTransitionSpeed;

        SerializedProperty stars;
        SerializedProperty clouds;
        SerializedObject weatherSO;
        SerializedObject timeSO;
        SerializedObject atmosphereSO;
        CozyWeather cozyWeather;


        protected static bool atmosphereSettings;
        protected static bool perennialSettings;
        protected static bool weatherSettings;
        protected static bool optionSettings;
        protected static bool referenceSettings;



        void OnEnable()
        {
            atmo = serializedObject.FindProperty("atmosphereProfile");
            weather = serializedObject.FindProperty("weatherProfile");
            annual = serializedObject.FindProperty("perennialProfile");

            lockToCamera = serializedObject.FindProperty("lockToCamera");
            allowRuntimeChanges = serializedObject.FindProperty("allowRuntimeChanges");
            sunDirection = serializedObject.FindProperty("sunDirection");
            sunPitch = serializedObject.FindProperty("sunPitch");
            weatherTransitionSpeed = serializedObject.FindProperty("weatherTransitionSpeed");

            stars = serializedObject.FindProperty("m_Stars");
            clouds = serializedObject.FindProperty("m_CloudParticles");

            cozyWeather = (CozyWeather)target;

            weatherSO = new SerializedObject(cozyWeather.weatherProfile);
            timeSO = new SerializedObject(cozyWeather.perennialProfile);
            atmosphereSO = new SerializedObject(cozyWeather.atmosphereProfile);

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();


            GUIStyle foldoutStyle = new GUIStyle(GUI.skin.GetStyle("toolbarPopup"));
            foldoutStyle.fontStyle = FontStyle.Bold;
            foldoutStyle.margin = new RectOffset(30, 10, 5, 5);





            if (atmo.objectReferenceValue == null || weather.objectReferenceValue == null || annual.objectReferenceValue == null)
            {

                EditorGUILayout.HelpBox("Make sure that you have all of the necessary profile references!", MessageType.Error);

            }

            atmosphereSettings = EditorGUILayout.BeginFoldoutHeaderGroup(atmosphereSettings, "    Atmosphere Settings", foldoutStyle);
            EditorGUI.EndFoldoutHeaderGroup();
            if (atmosphereSettings)
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(atmo);
                EditorGUILayout.Space();
                if (atmo.objectReferenceValue)
                    CreateEditor(cozyWeather.atmosphereProfile).OnInspectorGUI();
                atmosphereSO.ApplyModifiedProperties();
            }

            if (!cozyWeather.calender)
            {
                perennialSettings = EditorGUILayout.BeginFoldoutHeaderGroup(perennialSettings, "    Perennial Settings", foldoutStyle);
                EditorGUI.EndFoldoutHeaderGroup();
                if (perennialSettings)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(annual);
                    EditorGUILayout.Space();
                    if (annual.objectReferenceValue)
                        CreateEditor(cozyWeather.perennialProfile).OnInspectorGUI();
                    timeSO.ApplyModifiedProperties();
                }
            }

            weatherSettings = EditorGUILayout.BeginFoldoutHeaderGroup(weatherSettings, "    Current Weather", foldoutStyle);
            EditorGUI.EndFoldoutHeaderGroup();
            if (weatherSettings)
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(weather);
                EditorGUILayout.Space();
                if (weather.objectReferenceValue)
                    CreateEditor(cozyWeather.weatherProfile).OnInspectorGUI();
                weatherSO.ApplyModifiedProperties();
            }



            optionSettings = EditorGUILayout.BeginFoldoutHeaderGroup(optionSettings, "    Options", foldoutStyle);
            EditorGUI.EndFoldoutHeaderGroup();
            if (optionSettings)
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(lockToCamera);
                EditorGUILayout.PropertyField(allowRuntimeChanges);
                EditorGUILayout.PropertyField(sunDirection);
                EditorGUILayout.PropertyField(sunPitch);
                EditorGUILayout.PropertyField(weatherTransitionSpeed);
                EditorGUILayout.Space();
            }


            referenceSettings = EditorGUILayout.BeginFoldoutHeaderGroup(referenceSettings, "    References", foldoutStyle);
            EditorGUI.EndFoldoutHeaderGroup();
            if (referenceSettings)
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(stars);
                EditorGUILayout.PropertyField(clouds);
                EditorGUILayout.Space();
            }

            serializedObject.ApplyModifiedProperties();
            cozyWeather.Update();

        }



    }
}