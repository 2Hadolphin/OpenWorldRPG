using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;

public class AssetsBrowser : OdinEditorWindow
{

    [MenuItem("Tools/Assets/AssetsBrowser")]
    static void CreateWindow()
    {
        AssetsBrowser window = (AssetsBrowser)EditorWindow.GetWindowWithRect(typeof(AssetsBrowser), new Rect(0, 0, 400, 120));
    }

    public string guid;
    public string path;

    [Button("SearchViaGUID")]
    public void GetAssetPathViaGUID()
    {
        string p = AssetDatabase.GUIDToAssetPath(guid);

        Debug.Log(p);
        if (p.Length == 0)
            p = "not matchPackages";

        path= p;
    }

    [Button("SearchViaInstanceID")]
    public void GetAssetPathViaInstanceID()
    {
        if (!int.TryParse(guid, out var id))
            return;

        var ob = EditorUtility.InstanceIDToObject(id);

        Debug.Log(ob.GetType());

        var p = AssetDatabase.GetAssetPath(ob);
        path= p;
    }
}

