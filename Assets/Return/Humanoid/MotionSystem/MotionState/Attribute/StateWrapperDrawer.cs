#if UNITY_EDITOR

using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Return;
using System.Collections.Generic;
using Return.Humanoid.Modular;
using Return.Humanoid;
using Return.Motions;

/// <summary>
/// Fill element
/// </summary>
[CustomPropertyDrawer(typeof(VirtualStateAttribute))]
public class StateFillDrawer : PropertyDrawer
{
    /// <summary>
    /// <see cref="MotionStateBundle"/>
    /// </summary>
    const string BundlePath= "Assets/Return/Humanoid/MotionSystem/ObsoleteMotionState/MotionStateBundle.asset";

    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        if (property.objectReferenceValue == null)
        {
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width - 60, rect.height), property, label);
            if (GUI.Button(new Rect(rect.xMax - 55f, rect.y, 50f, rect.height), "Fill"))
                FindAndFill(property, attribute, fieldInfo);
        }
        else if (property.objectReferenceValue is StateWrapper wrapper &&
            wrapper.State is null)
        {
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width - 60, rect.height), property, label);
            if (GUI.Button(new Rect(rect.xMax - 55f, rect.y, 50f, rect.height), "Fixed"))
                FindAndFill(property, attribute, fieldInfo);
        }
        else
        {
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, rect.height), property, label);
        }
    }

    public static void FindAndFill(SerializedProperty property, Attribute attribute, FieldInfo fieldInfo)
    {
        var so = property.serializedObject;

        Debug.Log(so.targetObject +"-"+property.name);

        if (so.targetObject is not ScriptableObject) 
            return;

        if(attribute is not VirtualStateAttribute stateAttribute)
            return;

        var isModule = so.targetObject.GetType().IsSubclassOf(typeof(MotionModulePreset));

        var build = AssetDatabase.LoadAssetAtPath<MotionStateBundle>(BundlePath);

        StateWrapper GetWrapper()
        {
            var drawerTarget = property.serializedObject.targetObject;
            var wrapper = ScriptableObject.CreateInstance<StateWrapper>();
            wrapper.name = property.name;
            wrapper.Title = property.name;
            AssetDatabase.AddObjectToAsset(wrapper, drawerTarget);
            AssetDatabase.SaveAssets();

            property.objectReferenceValue = wrapper;

            return wrapper;
        }

        if (build == null)
        {
            Debug.LogError($"Failure to find motion state bundle {BundlePath}.");
            var wrapper = GetWrapper();


            var statid = property.name;

            var state = ScriptableObject.CreateInstance<MotionState>();
            wrapper.State = state;

            state.Title = statid;
            state.EventID = statid;
            state.SetValueType = stateAttribute.ValueType;
            state.name = statid;
        }
        else
            build.BindWrapper(property, stateAttribute, isModule,GetWrapper);

        so.ApplyModifiedProperties();
    }
}

#endif