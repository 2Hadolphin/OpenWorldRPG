
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Serialization;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR

using UnityEditor;

public class mEditorFileIO
{
    public static T TryGetAsset<T>(string optionalName = "") where T : UnityEngine.Object
    {
        // Gets all files with the Directory System.IO class
        string[] files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories);
        T asset = null;

        // move through all files
        foreach (var file in files)
        {
            // use the GetRightPartOfPath utility method to cut the path so it looks like this: Assets/folderblah
            string path = GetRightPartOfPath(file, "Assets");

            // Then I try and load the asset at the current path.
            asset = AssetDatabase.LoadAssetAtPath<T>(path);

            // check the asset to see if it's not null
            if (asset)
            {
                // if the optional name is nothing then we skip this step
                if (optionalName != "")
                {
                    if (asset.name == optionalName)
                    {

                        Debug.Log("Found the database at path: " + path + "with name: " + asset.name);
                        break;
                    }
                }
                else
                {
                    Debug.Log("Found the database at path: " + path);
                    break;
                }
            }

        }

        return asset;
    }

    private static string GetRightPartOfPath(string path, string after)
    {
        var parts = path.Split(Path.DirectorySeparatorChar);
        int afterIndex = Array.IndexOf(parts, after);

        if (afterIndex == -1)
        {
            return null;
        }

        return string.Join(Path.DirectorySeparatorChar.ToString(),
        parts, afterIndex, parts.Length - afterIndex);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="path">Singleton keep this null</param>
    public static T InstanceSO<T>(string path = null) where T : ScriptableObject
    {
        var so = ScriptableObject.CreateInstance<T>();
        if (null == path)
            path = "Assets/Resources/" + nameof(T) + ".asset";

        EditorHelper.WriteAsset(so,null, path);
        return so;
    }


}
#endif