using UnityEditor;
using UnityEngine;

namespace DistantLands.Cozy.EditorScripts
{
    [CustomEditor(typeof(CozyLightning))]
    [CanEditMultipleObjects]
    public class E_Lightning : Editor
    {

        SerializedProperty m_ThunderSounds;
        SerializedProperty m_LightIntensity;
        SerializedProperty m_ThunderDelayRange;
        CozyLightning cozyLightning;


        void OnEnable()
        {
            m_ThunderSounds = serializedObject.FindProperty("m_ThunderSounds");
            m_LightIntensity = serializedObject.FindProperty("m_LightIntensity");
            m_ThunderDelayRange = serializedObject.FindProperty("m_ThunderDelayRange");

            cozyLightning = (CozyLightning)target;


        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_ThunderSounds);
            EditorGUILayout.PropertyField(m_ThunderDelayRange);
            EditorGUILayout.PropertyField(m_LightIntensity);
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();

        }
    }
}