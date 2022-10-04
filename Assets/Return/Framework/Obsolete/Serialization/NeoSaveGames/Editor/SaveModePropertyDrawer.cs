#if UNITY_EDITOR



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace NeoSaveGames
{
	[CustomPropertyDrawer (typeof (SaveMode))]
	public class SaveModePropertyDrawer : PropertyDrawer
	{
	    // Draw the property inside the given rect
	    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
	    {
	        // Using BeginProperty / EndProperty on the parent property means that
	        // prefab override logic works on the entire property.
	        EditorGUI.BeginProperty (position, label, property);

	        // Draw label
	        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

	        // Don't make child fields be indented
	        var indent = EditorGUI.indentLevel;
	        EditorGUI.indentLevel = 0;

			string[] names = SaveMode.names;
			int oldSelection = Mathf.Clamp (property.FindPropertyRelative ("m_Value").intValue, 0, names.Length - 1);
			int newSelection = Mathf.Clamp (EditorGUI.Popup (position, oldSelection, names), 0, names.Length - 1);
			if (oldSelection != newSelection)
				property.FindPropertyRelative ("m_Value").intValue = newSelection;

	        // Set indent back to what it was
	        EditorGUI.indentLevel = indent;

	        EditorGUI.EndProperty();
	    }
	}
}

#endif