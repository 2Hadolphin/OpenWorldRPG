using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;

public class SceneSelector : OdinEditorWindow
{
    [MenuItem("Tools/Scene/Selector")]
    private static void OpenWindow()
    {
        GetWindow<SceneSelector>().position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
    }

    public Transform SearchTarget;
    public string Filter;
    public string Defilter;

    public Transform[] MatchTargets;

    [Button("Search")]
    public void Search()
    {
        string value;
        if (string.IsNullOrEmpty(Filter))
            value = string.Empty;
        else
            value = Filter.ToLower();

        string deValue;
        if (string.IsNullOrEmpty(Defilter))
            deValue = string.Empty;
        else
            deValue = Defilter.ToLower();
        var list = new List<Transform>();

        foreach (Transform tf in SearchTarget)
        {
            var name = tf.name.ToLower();
            if (name.Contains(value))
                    list.Add(tf);
        }

        MatchTargets = list.ToArray();

        foreach (var target in MatchTargets)
        {
            EditorGUIUtility.PingObject(target);
        }
        Selection.objects = MatchTargets;
        Selection.selectionChanged();
       
    }



}
