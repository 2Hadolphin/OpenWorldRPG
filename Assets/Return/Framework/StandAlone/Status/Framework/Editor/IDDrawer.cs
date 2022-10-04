using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Return.Framework.Stats
{

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(IDs), true)]
    public class IDDrawer : PropertyDrawer
    {
        /// <summary> Cached style to use to draw the popup button. </summary>
        private GUIStyle popupStyle;

        List<IDs> Instances;
        List<string> popupOptions;

        // private readonly Color Require= new Color(1, 0.4f, 0.4f, 1);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (popupStyle == null)
            {
                popupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions"));
                popupStyle.imagePosition = ImagePosition.ImageOnly;
            }

            label = EditorGUI.BeginProperty(position, label, property);

            if (property.objectReferenceValue)
            {
                label.tooltip += $"\n ID Value: [{(property.objectReferenceValue as IDs).ID}]";
                if (label.text.Contains("Element")) label.text = property.objectReferenceValue.GetType().Name;
            }

            position = EditorGUI.PrefixLabel(position, label);

            EditorGUI.BeginChangeCheck();

            float height = EditorGUIUtility.singleLineHeight;


            // Calculate rect for configuration button
            Rect buttonRect = new Rect(position);
            buttonRect.yMin += popupStyle.margin.top;
            buttonRect.width = popupStyle.fixedWidth + popupStyle.margin.right;
            buttonRect.x -= 20;
            buttonRect.height = height;

            //position.xMin = buttonRect.xMax;

            // Store old indent level and set it to 0, the PrefixLabel takes care of it
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;


            if (Instances == null || Instances.Count == 0)
            {
                var NameOfType = GetPropertyType(property);
                string[] guValueIdentify = AssetDatabase.FindAssets("t:" + NameOfType);  //FindAssets uses tags check documentation for more info

                Instances = new List<IDs>();
                popupOptions = new List<string>();
                popupOptions.Add("None");

                for (int i = 0; i < guValueIdentify.Length; i++)         //probably could get optimized 
                {
                    string path = AssetDatabase.GUIDToAssetPath(guValueIdentify[i]);
                    var inst = AssetDatabase.LoadAssetAtPath<IDs>(path);
                    Instances.Add(inst);
                }

                Instances = Instances.OrderBy(x => x.ID).ToList(); //Order by ID

                for (int i = 0; i < Instances.Count; i++)         //probably could get optimized 
                {
                    var inst = Instances[i];
                    var displayname = inst.name;
                    var ValueIdentifytring = "[" + Instances[i].ID.ToString() + "] ";

                    //if (Instances[i] is Tag) 
                    //    ValueIdentifytring = ""; //Do not show On tag 

                    if (!string.IsNullOrEmpty(inst.DisplayName))
                    {
                        displayname = inst.DisplayName;
                        int pos = displayname.LastIndexOf("/") + 1;
                        displayname = displayname.Insert(pos, ValueIdentifytring);
                    }
                    else
                    {
                        displayname = ValueIdentifytring + displayname;
                    }

                    popupOptions.Add(displayname);
                }
            }
            var PropertyValue = property.objectReferenceValue;

            //  Debug.Log(PropertyValue);
            int result = 0;

            if (PropertyValue != null && Instances.Count > 0)
            {
                result = Instances.FindIndex(i => i.name == PropertyValue.name) + 1; //Plus 1 because 0 is None
            }



            result = EditorGUI.Popup(buttonRect, result, popupOptions.ToArray(), popupStyle);

            if (result == 0)
            {
                property.objectReferenceValue = null;
            }
            else
            {
                var NewSelection = Instances[result - 1];
                property.objectReferenceValue = NewSelection;

            }

            position.height = EditorGUIUtility.singleLineHeight;


            EditorGUI.PropertyField(position, property, GUIContent.none, false);

            if (EditorGUI.EndChangeCheck())
                property.serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public static string GetPropertyType(SerializedProperty property)
        {
            var type = property.type;
            var match = Regex.Match(type, @"PPtr<\$(.*?)>");
            if (match.Success)
                type = match.Groups[1].Value;
            return type;
        }
    }
#endif
}