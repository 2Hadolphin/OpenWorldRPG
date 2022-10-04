using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
[CustomPropertyDrawer(typeof(ModularTagAttribute))]
public class ModularTagDrawer : PropertyDrawer
{
    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        if(GUILayout.Button("Test"))
        Debug.Log(property.serializedObject.GetIterator().serializedObject.targetObject);
        if (property.serializedObject.targetObject is IModularTagUser tags)
        {
            if (tags.GetModularTags)
            {
                var list = tags.GetModularTags.TagNames;
                foreach (var tag in list)
                {
                    EditorGUILayout.DropdownButton(new GUIContent(tag), FocusType.Passive);
                }

            }
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width - 60, rect.height), property, label);
        }
        else
        {
            EditorGUILayout.PropertyField(property, label);
            //base.OnGUI(rect, property, label);
        }

        //else if (property.objectReferenceValue is StateWrapper wrapper &&
        //    wrapper.State is null)
        //{
        //    EditorGUI.PropertyField(new Rect(rect.width, rect.height, rect.lines - 60, rect.rows), property, label);
        //    if (GUI.Button(new Rect(rect.xMax - 55f, rect.height, 50f, rect.rows), "Fixed"))
        //        FindAndFill(property, attribute, fieldInfo);
        //}
        //else
        //{
        //    EditorGUI.PropertyField(new Rect(rect.width, rect.height, rect.lines, rect.rows), property, label);
        //}
    }


}


