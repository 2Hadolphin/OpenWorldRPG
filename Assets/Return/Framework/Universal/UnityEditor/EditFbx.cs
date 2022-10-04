using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using UnityEditor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using System.Linq;
using System.Text;
using System.IO;
using System;

public class EditFbx : OdinEditorWindow
{

    [MenuItem("Tools/Assets/EditFbx")]
    static void Window()
    {
        var window = GetWindow<EditFbx>();

        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(700, 700);
    }

    [InfoBox("Edit fbx file configs.",GUIAlwaysEnabled =true)]
    public UnityEngine.Object obj;

    [Sirenix.OdinInspector.FilePath]
    public string Path;

    [Button(nameof(Edit))]
    public void Edit()
    {
        LootAssets(AssetDatabase.GetAssetPath(obj),(string x)=>ModelImporter.GetAtPath(x) is ModelImporter,new Action<string, bool>(SetReadable));
        AssetDatabase.Refresh();
    }

    void LootAssets(string path,Func<string,bool> check,Action<string,bool>@action)
    {
        var paths = GetSubFiles(path, Directory.GetDirectories);

        foreach (var mPath in paths)
        {
            if (AssetDatabase.IsValidFolder(mPath))
            {
                LootAssets(mPath, check, action);
            }
        }

        paths = GetSubFiles(path,Directory.GetFiles);

        foreach (var mPath in paths)
        {
            if (check(mPath))
                action(mPath, true);
        }
    }


    void SetReadable(string path,bool enable)
    {
        var im = ModelImporter.GetAtPath(path) as ModelImporter;

        if (im)
        {
            im.isReadable = enable;
            im.SaveAndReimport();
        }


        Debug.Log(path);
    }


    [ReadOnly]
    [Button(nameof(DeleteConvex))]
    public void DeleteConvex()
    {
        var paths=AssetDatabase.GetAllAssetPaths();

        var sb = new StringBuilder();
        foreach (var path in paths)
        {
            var obj = AssetDatabase.LoadMainAssetAtPath(path);

            if (!obj)
                if (AssetDatabase.DeleteAsset(path))
                    sb.AppendLine(path);


        }

        AssetDatabase.Refresh();
        Debug.Log(sb.ToString());
    }


    public static string[] GetSubFiles(string path,Func<string,string[]> searchFunc)
    {
        path = path.Replace("Assets/", string.Empty);

        string[] fileEntries = searchFunc(Application.dataPath + "/" + path);
        var list = new List<string>();

        var length = fileEntries.Length;
        for (var i = 0; i < length; i++)
        {
            var filePath = fileEntries[i];

            if (filePath.Contains(".meta"))
                continue;
            int assetPathIndex = filePath.IndexOf("Assets");
            list.Add(filePath.Substring(assetPathIndex));
        }

        return list.ToArray();
    }



}
