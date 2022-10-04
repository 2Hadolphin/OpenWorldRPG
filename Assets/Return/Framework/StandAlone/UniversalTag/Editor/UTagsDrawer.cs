using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace Return.Editors
{
    [CustomPropertyDrawer(typeof(UTags))]
    public class UTagsDrawer : PropertyDrawer
    {
        private SerializedProperty m_tagDefinition;
        private SerializedProperty m_tag;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            m_tagDefinition = property.FindPropertyRelative(nameof(UTagPicker.Definition));
            m_tag = property.FindPropertyRelative(nameof(UTagPicker.Tag));
            var hide = property.FindPropertyRelative(nameof(UTagPicker.HideDefinition));

            GUILayout.Label(property.name);

            GUILayout.BeginHorizontal();

            if (!hide.boolValue)
                EditorGUILayout.ObjectField(m_tagDefinition, label);

            if (m_tagDefinition.objectReferenceValue is UniversalTagDefinition definition)
                DrawTagField(m_tag, definition);

            GUILayout.EndHorizontal();
        }



        public static void DrawTagField(SerializedProperty tag, UniversalTagDefinition definition, params GUILayoutOption[] options)
        {
            string[] a_customNames = definition.TagNames.ToArray();

            bool multiSelect = definition.IsFlag;

            UTag enumValue = (UTag)tag.intValue;

            string enumName = "Mixed";

            if (enumValue == 0)
            {
                enumName = "None";
            }
            else if ((enumValue & (enumValue - 1)) == 0)
            {
                int enumIndex = Array.IndexOf(Enum.GetValues(typeof(UTag)), (UTag)tag.intValue);
                enumName = a_customNames[enumIndex - 1];
            }
            else if (!multiSelect)
            {
                enumName = "Error";
            }
            else if (enumValue + 1 == 0)
            {
                enumName = "Everything";
            }
            else
            {
                var flags = (int[])Enum.GetValues(typeof(UTag));
                var tag_sn = tag.intValue;
                var m_tag = (UTag)tag_sn;
                enumName = string.Join(',', flags.
                    Skip(1).
                    Where(x => m_tag.HasFlag((UTag)x)).
                    Select(x => (int)MathF.Log(x, 2)).
                    Where(x => x >= 0).
                    Select(x => a_customNames[x]));
            }

            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            if (options == null || options.Length == 0)
                options = new[] { /*GUILayout.ExpandWidth(true),*/GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.5f) /*GUILayout.Width(EditorGUIUtility.labelWidth)*/ };

            var style = new GUIStyle(EditorStyles.popup)
            {
                alignment = TextAnchor.MiddleCenter
            };


            if (GUILayout.Button(enumName, style, options))
            {
                var menu = new GenericMenu();

                menu.AddItem(new GUIContent("None"), enumValue == UTag.None ? true : false, OnNoneSelected, tag);

                if (multiSelect)
                    menu.AddItem(new GUIContent("Everything"), false, OnEverythingSelected, tag);

                int currentTags = tag.intValue;

                for (int i = 0; i < a_customNames.Length; ++i)
                {
                    bool isSelected = ((int)currentTags & (1 << i)) == (1 << i);
                    EnumFlagIndexPair enumFlagIndexPair = new(i, tag, isSelected) { IsFlags = multiSelect };

                    menu.AddItem(new GUIContent(a_customNames[i]), isSelected, OnFlagSelected, enumFlagIndexPair);
                }

                menu.ShowAsContext();
            }

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }


        public static void OnNoneSelected(object a_context)
        {
            var spTags = a_context as SerializedProperty;
            spTags.intValue = 0;

            spTags.serializedObject.ApplyModifiedProperties();
        }


        public static void OnEverythingSelected(object a_context)
        {
            var spTags = a_context as SerializedProperty;
            spTags.intValue = -1;

            spTags.serializedObject.ApplyModifiedProperties();
        }


        public static void OnFlagSelected(object a_context)
        {
            var flagIndexPair = a_context as EnumFlagIndexPair;

            if (flagIndexPair == null)
                return;

            UTag currentTags = (UTag)flagIndexPair.SpTags.intValue;
            UTag chosenTag = (UTag)(1 << flagIndexPair.EnumIndex);

            if ((currentTags & chosenTag) == chosenTag)
            {//Its already tagged, untag it
                currentTags &= (~chosenTag);
            }
            else
            { //It's not already tagged, tag it
                if (flagIndexPair.IsFlags)
                    currentTags |= chosenTag;
                else
                    currentTags = chosenTag;
            }

            flagIndexPair.SpTags.intValue = (int)currentTags;

            flagIndexPair.SpTags.serializedObject.ApplyModifiedProperties();
        }


        private class EnumFlagIndexPair
        {
            public int EnumIndex;
            public SerializedProperty SpTags;
            public bool IsSelected;
            public bool IsFlags;

            public EnumFlagIndexPair(int a_enumIndex, SerializedProperty a_spTags, bool a_isSelected)
            {
                EnumIndex = a_enumIndex;
                SpTags = a_spTags;
                IsSelected = a_isSelected;
            }
        }

    }

}