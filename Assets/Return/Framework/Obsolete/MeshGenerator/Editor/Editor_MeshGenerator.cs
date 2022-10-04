#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PolygonGenerator))]
public class Editor_MeshGenerator : Editor
{
    static int Edge;
    static string Path;
    static DefaultAsset folder;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var ta = target as PolygonGenerator;

   

        GUILayout.BeginHorizontal();

        EditorGUI.BeginDisabledGroup(true);
        folder = EditorGUILayout.ObjectField("Folder", folder, typeof(DefaultAsset), false) as DefaultAsset;
        EditorGUI.EndDisabledGroup();


        if (GUILayout.Button("Select Folder"))
            OpenFolder();

        GUILayout.EndHorizontal();

        if (null == folder)
        {
            EditorGUILayout.HelpBox("m_Path folder not valid!", MessageType.Warning, false);
        }





        Edge = EditorGUILayout.IntField(Edge);
        if (Edge < 3)
            Edge = 3;

        if (GUILayout.Button("GenerateMesh"))
        {
            string path;
            
            if (!AssetDatabase.IsValidFolder(Path))
            {
                OpenFolder();
                return;
            }

            path = Path + "/PolygonPlane_" + Edge+".mesh";

            var mesh = ta.PolygonalPlane(Edge);
            if (mesh)
            {
                AssetDatabase.CreateAsset(mesh, path);
                AssetDatabase.SaveAssets();
                EditorGUILayout.HelpBox("Build PolygonPlane_" + Edge + " successed!", MessageType.Info, false);
            }
            else
            {
                EditorGUILayout.HelpBox("Build PolygonPlane_" +Edge + " fail!", MessageType.Error, false);
            }
        }
    }

    private static void OpenFolder()
    {
        Path = EditorUtility.OpenFolderPanel("Assets storage path", "Assets/Mesh", "");
        if (Path.Contains(Application.dataPath))
        {
            Path = "Assets" + Path.Substring(Application.dataPath.Length);
            folder = AssetDatabase.LoadAssetAtPath(Path, typeof(DefaultAsset)) as DefaultAsset;
        }
    }

}

 public class MeshGeneratorEditorWindow : EditorWindow
{
    private Editor m_MyScriptableObjectEditor;

    [MenuItem("Tools/Mesh/PolygonGenerator")]
    public static void ShowWindow()
    {
        EditorWindow.CreateWindow<MeshGeneratorEditorWindow>();
    }

    private void OnEnable()
    {
        var factory = CreateInstance<PolygonGenerator>();
        factory.hideFlags = HideFlags.DontSave;
        m_MyScriptableObjectEditor = Editor.CreateEditor(factory);
    }

    private void OnGUI()
    {
        m_MyScriptableObjectEditor.OnInspectorGUI();
    }


}

public static class EditorAssist
{
    private static string OpenFolderPanelField(string path, string buttonLabel, string tips)
    {
        bool disabled;

        string content;

        if (string.IsNullOrEmpty(path))
        {
            disabled = true;

            content = tips;
        }
        else
        {
            disabled = false;

            content = path;
        }

        EditorGUILayout.BeginHorizontal();

        EditorGUI.BeginDisabledGroup(disabled);
        EditorGUILayout.TextArea(content, EditorStyles.label);
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button(buttonLabel, GUILayout.Width(100)))
        {
            string currentDirectory;

            if (string.IsNullOrEmpty(path))
            {
                currentDirectory = "Null";
            }
            else
            {
                currentDirectory = path;
            }

            path = EditorUtility.OpenFolderPanel(tips, currentDirectory, "");
        }

        EditorGUILayout.EndHorizontal();

        return path;
    }
}


#endif