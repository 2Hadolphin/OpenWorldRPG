using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Return.Database;
using Cleverous.VaultDashboard;


namespace Return.Drawer
{
    [CustomPropertyDrawer(typeof(AssetDropdownAttribute))]
    public class AssetDropdownDrawer : PropertyDrawer
    {
        public static List<DataEntity> CurrentContent = new List<DataEntity>();
        public static SerializedProperty CurrentProperty;
        public static Type CurrentFilterType;

        private static AssetDropdownAttribute m_attribute;
        private static string[] m_contentNames;
        private static bool m_isClean;

        private static void RefreshContent(SerializedProperty property, DataEntity currentTarget)
        {
            CurrentProperty = property;
            if (!m_isClean) CurrentContent = AllDataEntity(m_attribute);
            m_isClean = true;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property == null) return;

            CurrentProperty = property;

            m_attribute = (AssetDropdownAttribute)attribute;
            CurrentFilterType = m_attribute.SourceType;

            // find the target object of the property field in the list of items in the project
            string pathToCurrentObj = AssetDatabase.GetAssetPath(property.objectReferenceValue);
            DataEntity currentTarget = AssetDatabase.LoadAssetAtPath<DataEntity>(pathToCurrentObj);

            float leftPadding = 14 * EditorGUI.indentLevel;
            const int buttonSizeX = 40;

            Rect left = new Rect(
                new Vector2(
                    position.x + leftPadding,
                    position.y),
                new Vector2(
                    EditorGUIUtility.labelWidth - leftPadding,
                    position.size.y));

            Rect mid = new Rect(
                new Vector2(
                    position.x + left.size.x,
                    position.y),
                new Vector2(
                    position.size.x - left.size.x - buttonSizeX * 2,
                    position.size.y));

            Rect right1new = new Rect(
                new Vector2(
                    left.x + left.size.x + mid.size.x - leftPadding,
                    position.y),
                new Vector2(
                    buttonSizeX,
                    left.size.y));

            Rect right2edit = new Rect(
                new Vector2(
                    left.x + left.size.x + mid.size.x + buttonSizeX - leftPadding,
                    position.y),
                new Vector2(
                    buttonSizeX,
                    left.size.y));

            // put content titles into an array
            m_contentNames = new string[CurrentContent.Count];
            for (int i = 0; i < CurrentContent.Count; i++)
            {
                m_contentNames[i] = CurrentContent[i].Title;
            }

            // build the field label on the left.
            GUI.Label(left, property.displayName);

            // insert the fancy dropdown
            DataEntity obj = (DataEntity)CurrentProperty.objectReferenceValue;
            string frontlabel = obj == null ? "(None)" : obj.Title;
            if (GUI.Button(mid, new GUIContent(frontlabel), EditorStyles.popup))
            {
                RefreshContent(property, currentTarget);
                VaultAdvancedDropdown dropdown = new VaultAdvancedDropdown(new AdvancedDropdownState())
                {
                    TargetProperty = property
                };
                dropdown.Show(mid);
            }
            else m_isClean = false;

            if (CurrentFilterType.IsAbstract) GUI.enabled = false;
            //if (GUI.Button(right1new, "New"))
            //{
            //    MakeNew(VaultDashboard.CreateNewAsset(CurrentFilterType));
            //}
            //GUI.enabled = true;

            //if (GUI.Button(right2edit, "Edit"))
            //{
            //    VaultDashboard.InspectAssetRemote(currentTarget, CurrentFilterType);
            //}
        }

        // Callback for the Dropdown being used to change the Property.
        public static void ItemSelected(SerializedProperty targetObject, DataEntity newValue)
        {
            PushUpdate(targetObject, newValue);
        }

        private static void PushUpdate(SerializedProperty targetProperty, DataEntity newValue)
        {
            if (newValue == null || newValue.Title == "(None)") targetProperty.objectReferenceValue = null;
            else targetProperty.objectReferenceValue = newValue;

            CurrentProperty.serializedObject.ApplyModifiedProperties();
        }
        private static void MakeNew(DataEntity value)
        {
            CurrentProperty.objectReferenceValue = value == null || value.Title == "(None)"
                ? null
                : value;
            CurrentProperty.serializedObject.ApplyModifiedProperties();
        }

        private static List<DataEntity> AllDataEntity(AssetDropdownAttribute att)
        {
            List<DataEntity> list = new List<DataEntity>
            {
                ScriptableObject.CreateInstance<None>()
            };
            string[] guids = AssetDatabase.FindAssets($"t:{att.SourceType}");
            list.AddRange(guids.Select(guid => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), att.SourceType) as DataEntity));
            return list.OrderBy(x => x.Title).ToList();
        }

        /// <summary>
        /// This is strictly only used to represent the "None" or Null state of the dropdowns. It's instantiated in the background and never used in the game.
        /// The PopupField can't handle Null stuff, so this is easiest workaround.
        /// </summary>
        // ReSharper disable once ClassNeverInstantiated.Local
        private class None : DataEntity
        {
            public None()
            {
                Title = "(None)";
                Description = "Null";
            }
            void Reset()
            {
                Title = "(None)";
                Description = "Null";
            }
        }
    }
}