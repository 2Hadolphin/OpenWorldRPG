using UnityEditor;
using UnityEngine;

namespace DistantLands.Cozy.EditorScripts
{
    [CustomEditor(typeof(CozyCalendar))]
    [CanEditMultipleObjects]
    public class E_CozyCalendar : Editor
    {

        SerializedProperty resetTicksOnStart;
        SerializedProperty startTicks;
        SerializedProperty profile;
        SerializedProperty formattedTime;
        SerializedObject prof;
        SerializedObject so;
        CozyCalendar calendar;

        protected static bool settings;
        protected static bool currentInfo;
        protected static bool perennialSettings;



        void OnEnable()
        {


            calendar = (CozyCalendar)target;
            resetTicksOnStart = serializedObject.FindProperty("resetTicksOnStart");
            startTicks = serializedObject.FindProperty("startTicks");
            so = new SerializedObject(calendar.weatherSphere);
            prof = new SerializedObject(calendar.weatherSphere.perennialProfile);
            profile = so.FindProperty("perennialProfile");

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUIStyle foldoutStyle = new GUIStyle(GUI.skin.GetStyle("toolbarPopup"));
            foldoutStyle.fontStyle = FontStyle.Bold;
            foldoutStyle.margin = new RectOffset(30, 10, 5, 5);

            

            perennialSettings = EditorGUILayout.BeginFoldoutHeaderGroup(perennialSettings, "    Profile Settings", foldoutStyle);
            EditorGUI.EndFoldoutHeaderGroup();
            if (perennialSettings)
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(profile);
                so.ApplyModifiedProperties();
                EditorGUILayout.Space();
                if (profile.objectReferenceValue)
                    CreateEditor(calendar.weatherSphere.perennialProfile).OnInspectorGUI();

                prof.ApplyModifiedProperties();
            }

            settings = EditorGUILayout.BeginFoldoutHeaderGroup(settings, "    Options", foldoutStyle);
            EditorGUI.EndFoldoutHeaderGroup();
            if (settings)
            {
                EditorGUILayout.PropertyField(resetTicksOnStart);
                if (resetTicksOnStart.boolValue == true)
                    EditorGUILayout.PropertyField(startTicks);
                calendar.continuousUpdate = EditorGUILayout.Toggle("Continuous Update", calendar.continuousUpdate);
                EditorGUILayout.Space();
            }

            currentInfo = EditorGUILayout.BeginFoldoutHeaderGroup(currentInfo, "    Current Information", foldoutStyle);
            EditorGUI.EndFoldoutHeaderGroup();
            if (currentInfo)
            {
                if (calendar.weatherSphere.perennialProfile.realisticYear)
                    EditorGUILayout.HelpBox("Currently it is " + calendar.formattedTime + " on " + calendar.monthName + ".", MessageType.None, true);
                else
                    EditorGUILayout.HelpBox("Currently it is " + calendar.formattedTime + " in " + calendar.monthName + ".", MessageType.None, true);

            }

            EditorGUILayout.Space();


            serializedObject.ApplyModifiedProperties();


        }


    }
}