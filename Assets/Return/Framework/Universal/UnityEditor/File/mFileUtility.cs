using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using Object = UnityEngine.Object;
public  class mFileUtility : EditorWindow
{
    

    mFileUtility()
    {
        this.titleContent = new GUIContent("File Converter");
    }

    [MenuItem("Tools/Archive", false, 0)]
    static void Init()
    {
        var window = (mFileUtility)EditorWindow.GetWindow(typeof(mFileUtility));
        window.Show();
    }

    Object @object;
    private void OnGUI()
    {
        GUILayout.Label("Archive Source", EditorStyles.boldLabel);


        if (@object)
        {
            @object = EditorGUILayout.ObjectField("File", @object, @object.GetType(), false);
        }
        else
        {
            @object = EditorGUILayout.ObjectField("File", @object, typeof(Object), false);
            return;
        }


        var style = new GUIStyle() { alignment = TextAnchor.MiddleRight, fixedWidth = 30 };




        if (GUILayout.Button("SaveFile"))
        {
           EditorHelper.WriteFile(@object, @object.GetType());
        }
    }

}
