using UnityEditor;
using UnityEngine;

namespace DistantLands.Cozy.EditorScripts
{

    [CustomEditor(typeof(CozyClimate))]
    [CanEditMultipleObjects]
    public class E_CozyClimate : Editor
    {

        SerializedProperty climateProfile;
        SerializedProperty tempratureFilter;
        SerializedProperty precipitationFilter;

        SerializedProperty currentTemprature;
        SerializedProperty currentTempratureCelsius;
        SerializedProperty currentPrecipitation;
        CozyClimate cozyClimate;
        SerializedObject so;

        protected static bool profileSettings;
        protected static bool localSettings;
        protected static bool currentInfo;

        void OnEnable()
        {
            climateProfile = serializedObject.FindProperty("climateProfile");
            tempratureFilter = serializedObject.FindProperty("localTempratureFilter");
            precipitationFilter = serializedObject.FindProperty("localPrecipitationFilter");
            currentTemprature = serializedObject.FindProperty("currentTemprature");
            currentTempratureCelsius = serializedObject.FindProperty("currentTempratureCelsius");
            currentPrecipitation = serializedObject.FindProperty("currentPrecipitation");

            cozyClimate = (CozyClimate)target;
            so = new SerializedObject(cozyClimate.climateProfile);



        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();


            if (!cozyClimate.climateProfile)
                EditorGUILayout.HelpBox("Make sure that you have an active Climate Profile reference!", MessageType.Error);


            GUIStyle foldoutStyle = new GUIStyle(GUI.skin.GetStyle("toolbarPopup"));
            foldoutStyle.fontStyle = FontStyle.Bold;
            foldoutStyle.margin = new RectOffset(30, 10, 5, 5);

            profileSettings = EditorGUILayout.BeginFoldoutHeaderGroup(profileSettings, "    Profile Settings", foldoutStyle);
            EditorGUI.EndFoldoutHeaderGroup();
            if (profileSettings)
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(climateProfile);
                if (cozyClimate.climateProfile)
                {
                    EditorGUILayout.Space();
                    CreateEditor(cozyClimate.climateProfile).OnInspectorGUI();
                }
                so.ApplyModifiedProperties();
                EditorGUILayout.Space();
            } 




            localSettings = EditorGUILayout.BeginFoldoutHeaderGroup(localSettings, "    Local Filters", foldoutStyle);
            EditorGUI.EndFoldoutHeaderGroup();
            if (localSettings)
            {

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(tempratureFilter);
                EditorGUILayout.PropertyField(precipitationFilter);
                EditorGUILayout.Space();
            }


            currentInfo = EditorGUILayout.BeginFoldoutHeaderGroup(currentInfo, "    Current Information", foldoutStyle);
            EditorGUI.EndFoldoutHeaderGroup();
            if (currentInfo)
            {
                EditorGUILayout.HelpBox("Currently it is " + Mathf.Round(cozyClimate.currentTemprature) + "°F or " + Mathf.Round(cozyClimate.currentTempratureCelsius) + "°C with a precipitation chance of " + Mathf.Round(cozyClimate.currentPrecipitation) + "%.\n" +
                    "Tempratures will " + (cozyClimate.currentTemprature > cozyClimate.GetTemprature(false, cozyClimate.weatherSphere.perennialProfile.ticksPerDay) ? "drop" : "rise") + " tomorrow, bringing the temprature to " + Mathf.Round(cozyClimate.GetTemprature(false, cozyClimate.weatherSphere.perennialProfile.ticksPerDay)) + "°F", MessageType.None);
                EditorGUILayout.Space();
            }

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();


        }
    }
}