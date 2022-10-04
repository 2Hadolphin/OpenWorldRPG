using UnityEditor;
using UnityEngine;

namespace DistantLands.Cozy.EditorScripts
{

    [CustomEditor(typeof(CozyWindManager))]
    [CanEditMultipleObjects]
    public class E_WindManager : Editor
    {

        SerializedProperty useWindzone;
        SerializedProperty useShaderWind;


        void OnEnable()
        {

            useShaderWind = serializedObject.FindProperty("useShaderWind");
            useWindzone = serializedObject.FindProperty("useWindzone");


        }

        public override void OnInspectorGUI()
        {

            serializedObject.Update();

            EditorGUILayout.PropertyField(useWindzone);
            EditorGUILayout.PropertyField(useShaderWind);

            EditorGUILayout.Space(20);
            EditorGUILayout.HelpBox("Edit wind speed properties in the invdividual weather profiles!", MessageType.Info);

            serializedObject.ApplyModifiedProperties();


        }
    }
}