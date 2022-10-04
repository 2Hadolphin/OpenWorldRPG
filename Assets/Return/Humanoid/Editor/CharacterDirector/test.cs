using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

//[CustomEditor(typeof(CharacterDirector))]
public class ExampleBehaviourEditor : Editor
{
    private static readonly string k_baseClassData = "Selector";
    private static GUIContent c_element = new GUIContent("Target Concrete Class");
    private SerializedProperty p_baseClassData;

    private void OnEnable()
    {
        p_baseClassData = serializedObject.FindProperty(k_baseClassData);
    }

    public override void OnInspectorGUI()
    {
        if (p_baseClassData == null) return;

        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(p_baseClassData, true);

        if (p_baseClassData.CountInProperty() > 1)
        {
            for (int i = 0; i < p_baseClassData.arraySize; i++)
            {
                SerializedProperty p_element = p_baseClassData.GetArrayElementAtIndex(i);
                if (p_element == null) continue;

                p_element.AbstractPropertyDrawer();
            }
        }
        else
        {
            SerializedProperty p_element = p_baseClassData.GetArrayElementAtIndex(0);
            p_element.AbstractPropertyDrawer();
        }


        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}

public static class ext
{
    public static void AbstractPropertyDrawer(this SerializedProperty property)
    {
        if (property == null)
            throw new ArgumentNullException(property.ToString());

        if (property.propertyType == SerializedPropertyType.ObjectReference)
        {
            if (property.objectReferenceValue == null)
            {
                //field is null, provide object field for user to insert instance to draw
                EditorGUILayout.PropertyField(property);
                return;
            }
            System.Type concreteType = property.objectReferenceValue.GetType();
            UnityEngine.Object castedObject = (UnityEngine.Object)System.Convert.ChangeType(property.objectReferenceValue, concreteType);

            UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(castedObject);

            editor.OnInspectorGUI();
        }
        else
        {
            //otherwise fallback to normal property field
            EditorGUILayout.PropertyField(property);
        }
    }
}