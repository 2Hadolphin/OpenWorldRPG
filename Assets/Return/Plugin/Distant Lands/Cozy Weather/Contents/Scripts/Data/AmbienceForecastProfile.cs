// Distant Lands 2021.



using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace DistantLands.Cozy.Data
{

    [System.Serializable]
    [CreateAssetMenu(menuName = "Distant Lands/Cozy/Ambience Forecast Profile", order = 361)]
    public class AmbienceForecastProfile : ScriptableObject
    {

        public List<AmbienceProfile> ambienceProfiles;

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(AmbienceForecastProfile))]
    [CanEditMultipleObjects]
    public class E_AmbienceForecast : Editor
    {
        SerializedProperty ambienceProfiles;
        Vector2 scrollPos;
        AmbienceForecastProfile prof;



        void OnEnable()
        {

            ambienceProfiles = serializedObject.FindProperty("ambienceProfiles");
            prof = (AmbienceForecastProfile)target;


        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();


            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUILayout.PropertyField(ambienceProfiles, true);
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(prof);

            EditorGUILayout.EndScrollView();

        }
    }
#endif
}