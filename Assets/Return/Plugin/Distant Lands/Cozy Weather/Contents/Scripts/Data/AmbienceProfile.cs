// Distant Lands 2021.



using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace DistantLands.Cozy.Data
{

    [System.Serializable]
    [CreateAssetMenu(menuName = "Distant Lands/Cozy/Ambience Profile", order = 361)]
    public class AmbienceProfile : ScriptableObject
    {

        [Tooltip("Specifies the minimum (x) and maximum (y) length for this ambience profile.")]
        public Vector2 playTime = new Vector2(120, 480);
        [Tooltip("Multiplier for the computational chance that this ambience profile will play; 0 being never, and 2 being twice as likely as the average.")]
        [Range(0, 2)]
        public float likelihood = 1;
        public WeatherProfile[] dontPlayDuring;
        public List<ChanceEffector> chances;


        public GameObject particleFX;
        public AudioClip soundFX;
        [Range(0, 1)]
        public float FXVolume = 1;
        public bool useVFX;


        [System.Serializable]
        public class ChanceEffector
        {

            public enum LimitType { Temprature, Precipitation, YearPercentage, Time };
            public LimitType limitType;
            public AnimationCurve curve;


            public float GetChance(float temp, float precip, float yearPercent, float timePercent)
            {

                switch (limitType)
                {
                    case LimitType.Temprature:
                        return curve.Evaluate(temp / 100);
                    case (LimitType.Precipitation):
                        return curve.Evaluate(precip / 100);
                    case (LimitType.YearPercentage):
                        return curve.Evaluate(yearPercent);
                    case (LimitType.Time):
                        return curve.Evaluate(timePercent);
                    default:
                        return 1;
                }

            }
        }


        public float GetChance (float temp, float precip, float yearPercent, float time)
        {

            float i = likelihood;

            foreach (ChanceEffector j in chances)
            {
                i *= j.GetChance(temp, precip, yearPercent, time);
            }

            return Mathf.Clamp(i, 0, 1000000);

        }

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(AmbienceProfile))]
    [CanEditMultipleObjects]
    public class E_AmbienceProfile : Editor
    {

        SerializedProperty dontPlayDuring;
        SerializedProperty chances;
        SerializedProperty particleFX;
        SerializedProperty soundFX;
        SerializedProperty likelihood;
        float minPlayTime = 30;
        float maxPlayTime = 90;
        Vector2 scrollPos;
        AmbienceProfile prof;



        void OnEnable()
        {

            prof = (AmbienceProfile)target;

            dontPlayDuring = serializedObject.FindProperty("dontPlayDuring");
            chances = serializedObject.FindProperty("chances");
            particleFX = serializedObject.FindProperty("particleFX");
            soundFX = serializedObject.FindProperty("soundFX");
            likelihood = serializedObject.FindProperty("likelihood");

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Undo.RecordObject(prof, prof.name + " Profile Changes");

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUILayout.LabelField("Forecasting Behaviours", EditorStyles.boldLabel);

            minPlayTime = prof.playTime.x;
            maxPlayTime = prof.playTime.y;
            EditorGUILayout.MinMaxSlider("Ambience Length", ref minPlayTime, ref maxPlayTime, 0, 150);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Min Ticks: " + Mathf.Round(minPlayTime).ToString(), EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Max Ticks: " + Mathf.Round(maxPlayTime).ToString(), EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
            prof.playTime = new Vector2(minPlayTime, maxPlayTime);
            EditorGUILayout.PropertyField(likelihood);

            EditorGUILayout.Space(10);

            EditorGUILayout.PropertyField(dontPlayDuring, true);
            EditorGUILayout.PropertyField(chances, true);
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Ambient Effects", EditorStyles.boldLabel);
            prof.useVFX = EditorGUILayout.Toggle("Use FX", prof.useVFX);
            if (prof.useVFX)
            {
                EditorGUILayout.PropertyField(particleFX);
                EditorGUILayout.PropertyField(soundFX);
                if (soundFX.objectReferenceValue != null)
                    prof.FXVolume = EditorGUILayout.FloatField("FX Volume", prof.FXVolume);
            }
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(prof);

            EditorGUILayout.EndScrollView();

        }
    }
#endif
}