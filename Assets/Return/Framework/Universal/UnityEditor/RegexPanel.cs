using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
public class RegexPanel : EditorWindow
{
    public string Input;
    public string Patten;
    public Vector2 scroll;
    RegexPanel()
    {
        this.titleContent = new GUIContent("Regex Converter");
    }

    [MenuItem("Tools/Test/Regex")]
    static void Init()
    {
        var window = (RegexPanel)EditorWindow.GetWindow(typeof(RegexPanel));
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Context");
        Input=EditorGUILayout.TextField(Input);

        GUILayout.Label("Expression");
        Patten = EditorGUILayout.TextField(Patten);
        Matches();
    }

    void Matches()
    {
        if (string.IsNullOrEmpty(Input) | string.IsNullOrEmpty(Patten))
            return;

        try
        {
            var values = Regex.Matches(Input, Patten);

            scroll = GUILayout.BeginScrollView(scroll);
            EditorGUI.BeginDisabledGroup(true);
            foreach (Match value in values)
            {
                GUILayout.TextField(value.Value);
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndScrollView();
        }
        catch (System.Exception e)
        {
            EditorGUILayout.HelpBox("Expression error ! " + e, MessageType.Error);
        }
  
    }
}
