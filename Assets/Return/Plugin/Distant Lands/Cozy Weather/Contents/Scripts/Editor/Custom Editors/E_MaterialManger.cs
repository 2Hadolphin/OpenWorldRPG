using UnityEditor;
using UnityEngine;

namespace DistantLands.Cozy.EditorScripts
{

    [CustomEditor(typeof(CozyMaterialManager))]
    [CanEditMultipleObjects]
    public class E_MaterialManger : Editor
    {
        SerializedProperty m_SnowAmount;
        SerializedProperty m_SnowMeltSpeed;
        SerializedProperty m_Wetness;

        SerializedProperty m_DryingSpeed;
        SerializedProperty useRainbow;
        SerializedProperty profile;
        CozyMaterialManager materialManager;
        protected static bool profileSettings;
        protected static bool settings;
        SerializedObject so;


        void OnEnable()
        {
            m_SnowAmount = serializedObject.FindProperty("m_SnowAmount");
            m_SnowMeltSpeed = serializedObject.FindProperty("m_SnowMeltSpeed");
            m_Wetness = serializedObject.FindProperty("m_Wetness");
            m_DryingSpeed = serializedObject.FindProperty("m_DryingSpeed");
            useRainbow = serializedObject.FindProperty("useRainbow");
            profile = serializedObject.FindProperty("profile");

            materialManager = (CozyMaterialManager)target;
            so = new SerializedObject(materialManager.profile);


        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUIStyle foldoutStyle = new GUIStyle(GUI.skin.GetStyle("toolbarPopup"));
            foldoutStyle.fontStyle = FontStyle.Bold;
            foldoutStyle.margin = new RectOffset(30, 10, 5, 5);


            if (profile.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Make sure that you have all of the necessary profile references!", MessageType.Error);
            }

            profileSettings = EditorGUILayout.BeginFoldoutHeaderGroup(profileSettings, "    Profile Settings", foldoutStyle);
            EditorGUI.EndFoldoutHeaderGroup();
            if (profileSettings)
            {
                EditorGUILayout.PropertyField(profile);
                EditorGUILayout.Space();

                if (materialManager.profile)
                    CreateEditor(materialManager.profile).OnInspectorGUI();
                EditorGUILayout.Space();
                so.ApplyModifiedProperties();

            }

            settings = EditorGUILayout.BeginFoldoutHeaderGroup(settings, "    Options", foldoutStyle);
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (settings)
            {
                EditorGUILayout.PropertyField(m_SnowAmount);
                EditorGUILayout.PropertyField(m_SnowMeltSpeed);
                EditorGUILayout.PropertyField(m_Wetness);
                EditorGUILayout.PropertyField(m_DryingSpeed);
                EditorGUILayout.PropertyField(useRainbow);
                EditorGUILayout.Space();
            }



            serializedObject.ApplyModifiedProperties();


        }
    }
}