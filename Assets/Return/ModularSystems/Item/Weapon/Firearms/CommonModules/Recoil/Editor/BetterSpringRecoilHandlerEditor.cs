#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;

namespace NeoFPSEditor.ModularFirearms
{
    //[CustomEditor(typeof(BetterSpringRecoilHandler))]
    public class BetterSpringRecoilHandlerEditor : BaseRecoilHandlerBehaviourEditor
    {
        private static GUIStyle m_FoldoutStyle = null;
        public static GUIStyle foldoutStyle
        {
            get
            {
                if (m_FoldoutStyle == null)
                {
                    m_FoldoutStyle = new GUIStyle(EditorStyles.foldout);
                    m_FoldoutStyle.fontStyle = FontStyle.Bold;
                }
                return m_FoldoutStyle;
            }
        }

        protected override void InspectRecoilModule()
        {
            InspectRecoilProfile(serializedObject.FindProperty("m_HipFireRecoil"), "Hip-Fire Recoil", true);
            InspectRecoilProfile(serializedObject.FindProperty("m_AimedRecoil"), "Aim Down Sights Recoil", true);
        }

        public static void InspectRecoilProfile(SerializedProperty profile, string title, bool showCurves)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Space(12);
            EditorGUILayout.BeginVertical();

            // Show foldout with title
            var expanded = profile.FindPropertyRelative("expanded");
            expanded.boolValue = EditorGUILayout.Foldout(expanded.boolValue, title, true, foldoutStyle);
            if (expanded.boolValue)
            {
                EditorGUILayout.PropertyField(profile.FindPropertyRelative("recoilAngle"));
                EditorGUILayout.PropertyField(profile.FindPropertyRelative("wander"));
                EditorGUILayout.PropertyField(profile.FindPropertyRelative("horizontalMultiplier"));
                EditorGUILayout.PropertyField(profile.FindPropertyRelative("verticalDivergence"));
                EditorGUILayout.PropertyField(profile.FindPropertyRelative("horizontalDivergence"));
                EditorGUILayout.PropertyField(profile.FindPropertyRelative("pushBack"));
                EditorGUILayout.PropertyField(profile.FindPropertyRelative("maxPushBack"));
                EditorGUILayout.PropertyField(profile.FindPropertyRelative("jiggle"));
                EditorGUILayout.PropertyField(profile.FindPropertyRelative("duration"));
                if (showCurves)
                {
                    EditorGUILayout.PropertyField(profile.FindPropertyRelative("recoilSpringCurve"));
                    EditorGUILayout.PropertyField(profile.FindPropertyRelative("weaponJiggleCurve"));
                    EditorGUILayout.PropertyField(profile.FindPropertyRelative("weaponPushCurve"));
                }
                GUILayout.Space(2);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }
}

#endif