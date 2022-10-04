using UnityEngine;
using System;

public static class mEditorGUIExtension
{

    public static T EnumToButton<T>(this T @enum, GUILayoutOption GUILayoutOption) where T : Enum
    {
        var type = @enum.GetType();
        var enums = Enum.GetValues(type);
        foreach (var value in enums)
        {
            if (@enum.Equals(value))
            {
                var style = new GUIStyle(GUI.skin.button);
                //style.normal.textColor = Color.black;
                style.fontStyle = FontStyle.Bold;
                GUILayout.Button(value.ToString(), style, GUILayoutOption);
            }
            else
            {
                if (GUILayout.Button(value.ToString(), GUILayoutOption))
                {
                    return (T)value;
                }
            }

        }
        return @enum;
    }
}
