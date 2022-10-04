using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using System.IO;
using UnityEngine.Assertions;
using System.Linq;
using System;

public class SortFileUtility:OdinEditorWindow
{
    SortFileUtility()
    {
        this.titleContent = new GUIContent("Sort Files");
    }

    [MenuItem("Tools/Files/Sort")]
    static void Init()
    {
        var window = (SortFileUtility)EditorWindow.GetWindow(typeof(SortFileUtility));
        window.Show();
    }

    [FolderPath]
    public string RootFolder;

    [ShowInInspector]
    [ListDrawerSettings(Expanded =true,ShowIndexLabels =false)]
    public List<UnityEngine.Object> Targets=new();
    [ShowInInspector]
    [ListDrawerSettings(Expanded = true, ShowIndexLabels = false)]
    public List<string> Filters = new();

    [ListDrawerSettings(Expanded = true, ShowIndexLabels = false)]
    [ShowInInspector]
    public Dictionary<Type, string> OverrideExtension=new ();

    [Button("SortFiles")]
    void Sort()
    {
        SortViaFileName(RootFolder, Filters.ToArray(), true,OverrideExtension, Targets.ToArray());
        AssetDatabase.Refresh();
    }

    public static void SortViaFileName(string rootFolder,string[] filters,bool fuzzySearch,Dictionary<Type, string> OverrideExtension, params UnityEngine.Object[] objs)
    {
        var filterDic = EditorHelper.CheckFilterFolders(rootFolder, true, filters);
        var compare = System.StringComparison.CurrentCultureIgnoreCase;
        foreach (var obj in objs)
        {
            var name = obj.name;
            var path = AssetDatabase.GetAssetPath(obj);

            if (fuzzySearch)
            {
                foreach (var dic in filterDic)
                {
                    if(name.Contains(dic.Key, compare))
                    {
                        string ext= EditorHelper.GetExtension(obj);

                        if (OverrideExtension.TryGetValue(obj.GetType(), out var newExtenios))
                            name += "."+newExtenios;
                        else
                            name += "."+ext;

                        var result = AssetDatabase.MoveAsset(path, Path.Combine(dic.Value, name));
                        if (                            !string.IsNullOrEmpty(result))
                            Debug.LogError(result);

                        break;
                    }
                }
            }
            else
            {
                if (filterDic.TryGetValue(name, out var targetFolder))
                {
                    Assert.IsTrue(
                        string.IsNullOrEmpty(
                            AssetDatabase.MoveAsset(path, Path.Combine(targetFolder, name))));

                    //continue;
                }
            }
        }
    }
}
