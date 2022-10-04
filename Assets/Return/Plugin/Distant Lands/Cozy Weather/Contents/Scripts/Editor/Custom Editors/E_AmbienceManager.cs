using UnityEditor;
using UnityEngine;

namespace DistantLands.Cozy.EditorScripts
{

    [CustomEditor(typeof(CozyAmbienceManager))]
    [CanEditMultipleObjects]
    public class E_AmbienceManager : Editor
    {
        SerializedProperty m_Ambiences;
        SerializedProperty currentAmbienceProfile;

        protected static bool profileSettings;
        protected static bool currentInfo;
        CozyAmbienceManager ambienceManager;
        SerializedObject so;


        void OnEnable()
        {
            m_Ambiences = serializedObject.FindProperty("m_Ambiences");
            currentAmbienceProfile = serializedObject.FindProperty("currentAmbienceProfile");
            ambienceManager = (CozyAmbienceManager)target;
            so = new SerializedObject(ambienceManager.m_Ambiences);


        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (!ambienceManager.m_Ambiences)
                EditorGUILayout.HelpBox("Make sure that you have an active Climate Profile reference!", MessageType.Error);


            GUIStyle foldoutStyle = new GUIStyle(GUI.skin.GetStyle("toolbarPopup"));
            foldoutStyle.fontStyle = FontStyle.Bold;
            foldoutStyle.margin = new RectOffset(30, 10, 5, 5);


            profileSettings = EditorGUILayout.BeginFoldoutHeaderGroup(profileSettings, "    Profile Settings", foldoutStyle);
            EditorGUI.EndFoldoutHeaderGroup();
            if (profileSettings)
            {
                EditorGUILayout.PropertyField(m_Ambiences);
                if (ambienceManager.m_Ambiences)
                    CreateEditor(ambienceManager.m_Ambiences).OnInspectorGUI();
                EditorGUILayout.Space();
                so.ApplyModifiedProperties();

            }


            currentInfo = EditorGUILayout.BeginFoldoutHeaderGroup(currentInfo, "    Current Information", foldoutStyle);
            EditorGUI.EndFoldoutHeaderGroup();
            if (currentInfo)
            {
                EditorGUILayout.PropertyField(currentAmbienceProfile);
                if (Application.isPlaying)
                EditorGUILayout.HelpBox(ambienceManager.currentAmbienceProfile.name + " will be playing for the next " + Mathf.Round(ambienceManager.TimeTillNextAmbience()) + " ticks.", MessageType.None, true);

            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}