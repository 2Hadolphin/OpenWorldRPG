// Distant Lands 2021.



using System.Collections.Generic;
#if UNITY_EDITOR 
using UnityEditor;
#endif
using UnityEngine;



namespace DistantLands.Cozy.Data
{

    [System.Serializable]
    [CreateAssetMenu(menuName = "Distant Lands/Cozy/Perennial Profile", order = 361)]
    public class PerennialProfile : ScriptableObject
    {

        [Header("Current Settings")]
        [Tooltip("Specifies the current ticks.")]
        public float currentTicks;
        [Tooltip("Specifies the current day.")]
        public int currentDay;
        [Tooltip("Specifies the current year.")]
        public int currentYear;
        [HideInInspector]
        public float dayAndTime;
        public bool pauseTime;
        public bool realisticYear;
        public bool useLeapYear;

        [Header("Day Settings")]
        [Tooltip("Specifies the maximum amount of ticks per day.")]
        public float ticksPerDay = 360;
        [Tooltip("Specifies the amount of ticks that passs in a second.")]
        public float tickSpeed = 1;
        [Tooltip("Changes tick speed based on the percentage of the day.")]
        public AnimationCurve tickSpeedMultiplier;

        [System.Serializable]
        public class Month
        {

            public string name;
            public int days;

        }

        public Month[] standardYear = new Month[12] { new Month() { days = 31, name = "January"}, new Month() { days = 28, name = "Febraury" },
        new Month() { days = 31, name = "March"}, new Month() { days = 30, name = "April"}, new Month() { days = 31, name = "May"},
        new Month() { days = 30, name = "June"}, new Month() { days = 31, name = "July"}, new Month() { days = 31, name = "August"},
        new Month() { days = 30, name = "September"}, new Month() { days = 31, name = "October"}, new Month() { days = 30, name = "Novemeber"},
        new Month() { days = 31, name = "December"}};
        public Month[] leapYear = new Month[12] { new Month() { days = 31, name = "January"}, new Month() { days = 29, name = "Febraury" },
        new Month() { days = 31, name = "March"}, new Month() { days = 30, name = "April"}, new Month() { days = 31, name = "May"},
        new Month() { days = 30, name = "June"}, new Month() { days = 31, name = "July"}, new Month() { days = 31, name = "August"},
        new Month() { days = 30, name = "September"}, new Month() { days = 31, name = "October"}, new Month() { days = 30, name = "Novemeber"},
        new Month() { days = 31, name = "December"}};

        [Header("Year Settings")]
        public int daysPerYear = 48;


        public float ModifiedTickSpeed()
        {

            return tickSpeed * tickSpeedMultiplier.Evaluate(currentTicks / ticksPerDay);

        }

        public float YearPercentage()
        {

            return dayAndTime / daysPerYear;


        }

        public void GetCurrentMonth( out string monthName, out int monthDay, out float monthPercentage)
        {

            int i = currentDay;
            int j = 0;

            while (i > ((useLeapYear && currentYear % 4 == 0) ? leapYear[j].days : standardYear[j].days))
            {

                i -= (useLeapYear && currentYear % 4 == 0) ? leapYear[j].days : standardYear[j].days;

                j++;

                if (j >= ((useLeapYear && currentYear % 4 == 0) ? leapYear.Length : standardYear.Length))
                    break;

            }

            Month k = (useLeapYear && currentYear % 4 == 0) ? leapYear[j] : standardYear[j];

            monthName = k.name;
            monthDay = i;
            monthPercentage = k.days;


        }

        public float DayPercentage()
        {

            return currentTicks / ticksPerDay;


        }

        public float YearPercentage(float inTicks) { return (dayAndTime + (inTicks / ticksPerDay)) / daysPerYear; }

        public int RealisticDaysPerYear()
        {

            int i = 0;
            foreach (Month j in (useLeapYear && currentYear % 4 == 0) ? leapYear : standardYear) i += j.days;
            return i;


        }

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(PerennialProfile))]
    [CanEditMultipleObjects]
    public class E_PerennialProfile : Editor
    {

        SerializedProperty tickSpeedMultiplier;
        SerializedProperty standardYear;
        SerializedProperty leapYear;
        PerennialProfile prof;

        void OnEnable()
        {

            tickSpeedMultiplier = serializedObject.FindProperty("tickSpeedMultiplier");
            standardYear = serializedObject.FindProperty("standardYear");
            leapYear = serializedObject.FindProperty("leapYear");
            prof = (PerennialProfile)target;

        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Undo.RecordObject(prof, prof.name + " Profile Changes");

            EditorGUILayout.LabelField("Current Settings", EditorStyles.boldLabel);
            prof.currentTicks = EditorGUILayout.Slider("Current Ticks", prof.currentTicks, 0, prof.ticksPerDay);
            prof.currentDay = EditorGUILayout.IntSlider("Current Day", prof.currentDay, 0, prof.daysPerYear);
            prof.currentYear = EditorGUILayout.IntField("Current Year", prof.currentYear);
            EditorGUILayout.Space(20);

            EditorGUILayout.LabelField("Time Movement", EditorStyles.boldLabel);
            prof.pauseTime = EditorGUILayout.Toggle("Pause Time", prof.pauseTime);
            if (!prof.pauseTime)
            {
                prof.tickSpeed = EditorGUILayout.FloatField("Tick Speed", prof.tickSpeed);
                EditorGUILayout.PropertyField(tickSpeedMultiplier);
            }
            EditorGUILayout.Space(20);

            EditorGUILayout.LabelField("Length Settings", EditorStyles.boldLabel);
            prof.ticksPerDay = EditorGUILayout.FloatField("Ticks Per Day", prof.ticksPerDay);
            EditorGUILayout.Space(10);
            prof.realisticYear = EditorGUILayout.Toggle("Realistic Year", prof.realisticYear);
            if (prof.realisticYear)
            {
                prof.useLeapYear = EditorGUILayout.Toggle("Use Leap Year", prof.useLeapYear);

                EditorGUILayout.Space(10);
                EditorGUILayout.PropertyField(standardYear);
                if (prof.useLeapYear)
                EditorGUILayout.PropertyField(leapYear);
            }
            else
            {

                prof.daysPerYear = EditorGUILayout.IntField("Days Per Year", prof.daysPerYear);

            }

            EditorGUILayout.Space(20);

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(prof);

        }

    }
#endif
}