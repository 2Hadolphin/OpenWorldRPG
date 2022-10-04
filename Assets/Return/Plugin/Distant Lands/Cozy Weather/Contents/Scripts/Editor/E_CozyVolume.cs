// Distant Lands 2021.



using UnityEditor;

namespace DistantLands.Cozy.EditorScripts
{
    [CustomEditor(typeof(CozyVolume))]
    public class E_CozyVolume : Editor
    {

        SerializedProperty triggerState;
        SerializedProperty triggerType;
        SerializedProperty tag;

        SerializedProperty weatherProfile;
        SerializedProperty atmosphereProfile;
        SerializedProperty ambienceProfile;
        SerializedProperty ticks;
        SerializedProperty day;



        void OnEnable()
        {
            triggerState = serializedObject.FindProperty("m_TriggerState");
            triggerType = serializedObject.FindProperty("m_TriggerType");
            tag = serializedObject.FindProperty("m_Tag");

            ambienceProfile = serializedObject.FindProperty("m_AmbienceProfile");
            weatherProfile = serializedObject.FindProperty("m_WeatherProfile");
            atmosphereProfile = serializedObject.FindProperty("m_AtmosphereProfile");
            ticks = serializedObject.FindProperty("ticks");
            day = serializedObject.FindProperty("day");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(triggerState);
            tag.stringValue = EditorGUILayout.TagField("Collides With", tag.stringValue);
            EditorGUILayout.Space(20);
            EditorGUILayout.PropertyField(triggerType);


            switch (triggerType.enumValueIndex)
            {

                case (int)CozyVolume.TriggerType.setWeather:
                    EditorGUILayout.PropertyField(weatherProfile);
                    break;
                case (int)CozyVolume.TriggerType.setAmbience:
                    EditorGUILayout.PropertyField(ambienceProfile);
                    break;
                case (int)CozyVolume.TriggerType.setAtmosphere:
                    EditorGUILayout.PropertyField(atmosphereProfile);
                    break;
                case (int)CozyVolume.TriggerType.setTicks:
                    EditorGUILayout.PropertyField(ticks);
                    break;
                case (int)CozyVolume.TriggerType.setDay:
                    EditorGUILayout.PropertyField(day);
                    break;
            }



            serializedObject.ApplyModifiedProperties();


        }
    }

}