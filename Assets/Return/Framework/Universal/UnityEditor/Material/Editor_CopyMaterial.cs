using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class Window_CopyMaterial : EditorWindow
{
    Editor window;
    [MenuItem("Tools/Material/Copy")]
    public static void OpenWindow()
    {
        CreateWindow<Window_CopyMaterial>();
    }
    private void OnEnable()
    {
        window = Editor.CreateEditor(this);
    }

    private void OnGUI()
    {
        window.OnInspectorGUI();
    }

    [HideInInspector]
    public Renderer CopyFrom;
    [HideInInspector]
    public Renderer CopyTo;
    [SerializeField]
    public Material[] Materials;
}
[CustomEditor(typeof(Window_CopyMaterial))]
public class Editor_CopyMaterial : Editor 
{

    public override void OnInspectorGUI()
    {
        var go = target as Window_CopyMaterial;

        
        go.CopyFrom=EditorGUILayout.ObjectField(go.CopyFrom,typeof(Renderer),true) as Renderer;
        go.CopyTo = EditorGUILayout.ObjectField(go.CopyTo, typeof(Renderer), true) as Renderer;

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Copy"))
        {
            if (go.CopyFrom)
                go.Materials = go.CopyFrom.sharedMaterials;
        }

        if (GUILayout.Button("Paste"))
        {
            if (go.CopyTo)
                go.CopyTo.sharedMaterials = go.Materials;
        }
        GUILayout.EndHorizontal();

        base.OnInspectorGUI();
    }
}


