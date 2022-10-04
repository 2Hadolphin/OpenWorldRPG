#if UNITY_EDITOR

using NeoFPS.ModularFirearms;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NeoFPSEditor.ModularFirearms
{
    //[CustomEditor(typeof(BaseRecoilHandlerBehaviour), true)]
    public class BaseRecoilHandlerBehaviourEditor : Editor
    {
        private static GUIContent m_PresetGUIContent = null;
        public static GUIContent presetGUIContent
        {
            get
            {
                if (m_PresetGUIContent == null)
                    m_PresetGUIContent = new GUIContent("Select A Preset");
                return m_PresetGUIContent;
            }
        }

        protected void OnInspectorGUIInternal()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnRecoil"));

            InspectAccuracy(serializedObject);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Recoil", EditorStyles.boldLabel);

            InspectRecoilModule();
        }

        private static SerializedObject s_AccuracyObject = null;

        public static void InspectAccuracy(SerializedObject serializedObject)
        {
            EditorGUILayout.LabelField("Accuracy", EditorStyles.boldLabel);

            // Presets
            if (EditorGUILayout.DropdownButton(new GUIContent("Copy From Preset"), FocusType.Passive))
            {
                s_AccuracyObject = serializedObject;
                var menu = new GenericMenu();
                AddPresets(menu);
                menu.ShowAsContext();
            }
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_HipAccuracyKick"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_HipAccuracyRecover"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SightedAccuracyKick"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SightedAccuracyRecover"));
        }

        static void AddPresets(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("100% Accurate"), false, OnAccuracyPresetSelect, new Vector4(0f, 1f, 0f, 1f));
            menu.AddItem(new GUIContent("Semi-Auto Light"), false, OnAccuracyPresetSelect, new Vector4(0.75f, 4f, 0.05f, 1f));
            menu.AddItem(new GUIContent("Semi-Auto Heavy"), false, OnAccuracyPresetSelect, new Vector4(0.175f, 0.75f, 0.1f, 0.75f));
            menu.AddItem(new GUIContent("Full-Auto Light"), false, OnAccuracyPresetSelect, new Vector4(0.1f, 0.85f, 0.05f, 0.75f));
            menu.AddItem(new GUIContent("Full-Auto Heavy"), false, OnAccuracyPresetSelect, new Vector4(0.1f, 0.75f, 0.075f, 0.8f));
        }

        static void OnAccuracyPresetSelect(object o)
        {
            var preset = (Vector4)o;
            s_AccuracyObject.FindProperty("m_HipAccuracyKick").floatValue = preset.x;
            s_AccuracyObject.FindProperty("m_HipAccuracyRecover").floatValue = preset.y;
            s_AccuracyObject.FindProperty("m_SightedAccuracyKick").floatValue = preset.z;
            s_AccuracyObject.FindProperty("m_SightedAccuracyRecover").floatValue = preset.w;
            s_AccuracyObject.ApplyModifiedProperties();
            s_AccuracyObject = null;
        }

        protected virtual void InspectRecoilModule()
        {
            var itr = serializedObject.FindProperty("m_SightedAccuracyRecover");
            while (itr.NextVisible(true))
                EditorGUILayout.PropertyField(itr);
        }
    }
}

#endif