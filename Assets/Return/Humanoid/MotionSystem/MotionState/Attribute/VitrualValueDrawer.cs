#if UNITY_EDITOR

using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using System;

//[CustomPropertyDrawer(typeof(VirtualStateAttribute))]
public class WrapperValueDrawer : PropertyDrawer
{


	private GUIContent guiContent;

	private Type[] methodArgumentTypes;
	private object[] methodArguments;

	public WrapperValueDrawer(string tooltip)
	{
		guiContent = new GUIContent(string.Empty, tooltip);

		//methodArgumentTypes = new[] { typeof(Rect), typeof(MaterialProperty), typeof(GUIContent) };
		//methodArguments = new object[3];

		//internalMethod = typeof(MaterialEditor)
		//	.GetMethod("DefaultShaderPropertyInternal", BindingFlags.Instance | BindingFlags.NonPublic,
		//	null,
		//	methodArgumentTypes,
		//	null);
	}
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
		guiContent = label;
	}

}


#endif