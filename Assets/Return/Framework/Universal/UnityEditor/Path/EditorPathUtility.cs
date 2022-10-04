using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;
using System.IO;
using System;
using Object = UnityEngine.Object;

public static class EditorPathUtility
{
    public const string str_Assets = "Assets";
    public const string str_Resources = "Resources";

    public static string GetProjectName()
    {
        string[] s = Application.dataPath.Split('/');
        string projectName = s[^2];
        //Debug.Log("project = " + projectName);
        return projectName;
    }

    /// <summary>
    /// Get extension
    /// </summary>
    /// <param name="asset"></param>
    /// <returns></returns>
    public static string GetExtension(Object asset)
    {
        var path = AssetDatabase.GetAssetPath(asset);

        if (AssetDatabase.IsValidFolder(path))
            return ".asset";

        if (!string.IsNullOrEmpty(path))
        {
            if (AssetDatabase.IsMainAsset(asset))
            {
                var extension = Path.GetExtension(path);

                if (!extension.Contains('.'))
                    extension = '.' + extension;

                return extension;
            }
        }

        var type = asset.GetType();

        //if (AssetDatabase.IsSubAsset(asset))
        {
            //if (type == typeof(AudioClip))
            //    return ".AudioClip";

            if (type == typeof(AnimationClip))
                return ".anim";

            if (type == typeof(Mesh))
                return ".mesh";

 

            //return '.' + type.Name.ToLower();

            //if (type == typeof(Mesh))
            //    return ".asset";
        }


        return ".asset";
    }


    public static string GetEditorPath(string path)
    {
        Assert.IsFalse(string.IsNullOrEmpty(path));

        var index = path.IndexOf("Assets");

        if(index>0)
            path = path[index..];

        return path;

        //var projectPath = Application.dataPath;
        //projectPath = projectPath.Replace("Assets", null);

        //return projectPath += path;
    }

    /// <summary>
    /// Convert project path to disk path.
    /// </summary>
    public static string GetIOPath(string path)
    {
        Assert.IsFalse(string.IsNullOrEmpty(path));

        var projectPath = Application.dataPath;
        projectPath = projectPath.Replace("Assets", null);

        return projectPath += path;
    }


    public static string GetIOPath(Object asset)
    {
        Assert.IsNotNull(asset);

        if(AssetDatabase.IsSubAsset(asset))
        {
            Debug.LogWarningFormat($"{asset} is sub asset, file might not be there.");
        }

        var path = AssetDatabase.GetAssetPath(asset);
        //path=path.Replace(".asset", null);

        var projectPath = Application.dataPath.Replace("Assets", null);

        return Path.Combine(projectPath, path);
    }

    public static string GetMetaPath(string assetPath)
    {
        return GetIOPath(assetPath) + ".meta";
    }

    public static string GetMetaPath(Object asset)
    {
        return GetIOPath(asset) + ".meta";
    }

    public static bool IsValidFolder(Object asset)
    {
        if (asset == null)
            return false;

        if (!AssetDatabase.IsMainAsset(asset))
            return false;

        var path = AssetDatabase.GetAssetPath(asset);
        return AssetDatabase.IsValidFolder(path);
    }


    /// <summary>
    /// Return folder path from asset.
    /// </summary>
    public static string GetAssetFolderPath(Object asset)
    {
        var path=AssetDatabase.GetAssetPath(asset);

        return GetAssetFolderPath(path);
    }

    /// <summary>
    /// Return folder path from asset path.
    /// </summary>
    public static string GetAssetFolderPath(string assetPath)
    {
        Assert.IsFalse(string.IsNullOrEmpty(assetPath));

        return assetPath[..assetPath.LastIndexOf("/")];
    }


    /// <summary>
    /// Return folder name from asset.
    /// </summary>
    public static string GetAssetFolderName(UnityEngine.Object asset)
    {
        var path = GetAssetFolderPath(asset);

        return GetAssetFolderName(path);
    }

    /// <summary>
    /// Return folder name from asset path.
    /// </summary>
    public static string GetAssetFolderName(string assetPath)
    {
        if (!AssetDatabase.IsValidFolder(assetPath))
            assetPath=GetAssetFolderPath(assetPath);

        return assetPath.Substring(assetPath.LastIndexOf('/')+1).Replace("\\","/");
    }

    public static string GetAssetName(string assetPath,string endStr=null)
    {
        return Path.GetFileName(assetPath);

        var start = assetPath.LastIndexOf('/');

        if(start < 0)
            start = assetPath.LastIndexOf('\\');
        //Debug.Log(start);
        Assert.IsFalse(start < 0,assetPath.Contains('/').ToString());

        if (string.IsNullOrEmpty(endStr) || !assetPath.Contains(endStr, StringComparison.CurrentCultureIgnoreCase))
            endStr = ".";

        var end = assetPath.LastIndexOf(endStr);

        Assert.IsFalse(start < 0);

        if (end > 0)
            end -= start + 1;
        else
            end = 0;

        return assetPath.Substring(start+1, end);
    }

    /// <summary>
    /// Get root asset name
    /// </summary>
    public static string GetRootAssetName(Object asset)
    {
        var path = AssetDatabase.GetAssetPath(asset);
        return GetAssetName(path);
    }

    public static string GetAssetPath(string ioPath)
    {
        var index = ioPath.IndexOf("Assets/");

        var localPath =
            index > 0 ?
            ioPath[index..] :
            ioPath;

        return localPath;
    }
}
