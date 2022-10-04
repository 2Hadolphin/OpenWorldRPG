#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AutoRefreshConfig
{
    //This is called when you click on the 'Tools/Auto Refresh' and toggles its value
    [MenuItem("Project/Auto Refresh")]
    static void AutoRefreshToggle()
    {
        var status = EditorPrefs.GetInt("kAutoRefresh");
        if (status == 1)
            EditorPrefs.SetInt("kAutoRefresh", 0);
        else
            EditorPrefs.SetInt("kAutoRefresh", 1);
    }


    //This is called before 'Tools/Auto Refresh' is shown to check the current value
    //of kAutoRefresh and update the checkmark
    [MenuItem("Project/Auto Refresh", true)]
    static bool AutoRefreshToggleValidation()
    {
        var status = EditorPrefs.GetInt("kAutoRefresh");
        if (status == 1)
            Menu.SetChecked("Project/Auto Refresh", true);
        else
            Menu.SetChecked("Project/Auto Refresh", false);
        return true;
    }
}
#endif