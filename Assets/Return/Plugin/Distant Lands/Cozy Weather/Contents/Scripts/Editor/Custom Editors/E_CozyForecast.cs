using UnityEditor;
using UnityEngine;

namespace DistantLands.Cozy.EditorScripts
{
    [CustomEditor(typeof(CozyForecast))]
    [CanEditMultipleObjects]
    public class E_CozyForecast : Editor
    {


        SerializedProperty forecast;
        SerializedProperty forecastProfile;
        SerializedObject so;

        protected static bool profileSettings;
        protected static bool currentInfo;
        CozyForecast cozyForecast;


        void OnEnable()
        {
            forecast = serializedObject.FindProperty("forecast");
            forecastProfile = serializedObject.FindProperty("forecastProfile");
            cozyForecast = (CozyForecast)target;
            so = new SerializedObject (cozyForecast.forecastProfile);

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUIStyle foldoutStyle = new GUIStyle(GUI.skin.GetStyle("toolbarPopup"));
            foldoutStyle.fontStyle = FontStyle.Bold;
            foldoutStyle.margin = new RectOffset(30, 10, 5, 5);

            if (forecastProfile.objectReferenceValue == null)
            {                                                                               

                EditorGUILayout.HelpBox("Make sure that you have all of the necessary profile references!", MessageType.Error);

            }

            profileSettings = EditorGUILayout.BeginFoldoutHeaderGroup(profileSettings, "    Profile Settings", foldoutStyle);
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (profileSettings)
            {

                EditorGUILayout.PropertyField(forecastProfile);
                if (cozyForecast.forecastProfile)
                    CreateEditor(cozyForecast.forecastProfile).OnInspectorGUI();
                EditorGUILayout.Space();
                so.ApplyModifiedProperties();
            }
            currentInfo = EditorGUILayout.BeginFoldoutHeaderGroup(currentInfo, "    Current Information", foldoutStyle);
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (currentInfo)
            {
                if (cozyForecast.forecast.Count == 0)
                {
                    EditorGUILayout.HelpBox("No information yet!", MessageType.None);

                }
                else
                {
                    EditorGUILayout.HelpBox("Currently it is " + cozyForecast.weatherSphere.weatherProfile.name, MessageType.None);


                    for (int i = 0; i < cozyForecast.forecast.Count; i++)
                    {

                        EditorGUILayout.HelpBox("Starting at " + cozyForecast.weatherSphere.calender.FormatTime(false, cozyForecast.forecast[i].startTicks) + " the weather will change to " +
                            cozyForecast.forecast[i].profile.name + " for " + Mathf.Round(cozyForecast.forecast[i].weatherProfileDuration) +
                            " ticks or unitl " + cozyForecast.weatherSphere.calender.FormatTime(false, cozyForecast.forecast[i].endTicks) + ".", MessageType.None, true);
                        
                        EditorGUILayout.Space(2);

                    }                                   
                }

                EditorGUILayout.Space();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}