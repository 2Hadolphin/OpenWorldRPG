#if UNITY_EDITOR

using UnityEngine;
using System.IO;
using UnityEditor;
using System;
using System.Collections;
using Object = UnityEngine.Object;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using UnityEngine.Assertions;




public partial class EditorHelper
{

    #region Asset
    public static void OpenFile<T>(ref string path, ref T field) where T : UnityEngine.Object
    {
        path = EditorUtility.OpenFilePanel("Assets path" + typeof(T).ToString(), "Assets", "");
        //path = EditorUtilityTools.OpenFolderPanel("Assets path" + typeof(T).ToString(), "Assets", "");

        if (path.Contains(Application.dataPath))
        {
            path = "Assets" + path.Substring(Application.dataPath.Length);
            field = AssetDatabase.LoadAssetAtPath(path, typeof(T)) as T;
        }
    }

    public static string SaveFilePanel(string title,string path,string filenameWithExtension,string extension=null,bool inEditor=true)
    {
        if (string.IsNullOrEmpty(extension))
            extension = ".asset";

        var filePath = EditorUtility.SaveFilePanel(title,path,filenameWithExtension,extension);
        if(inEditor)
        filePath = filePath.Substring(filePath.IndexOf("Assets"));

        return filePath;
    }

    /// <summary>
    /// Rename obj to new ID;
    /// </summary>
    /// <param name="object"></param>
    /// <param name="newID"></param>
    public static void Rename(UnityEngine.Object @object, string newID)
    {
        string assetPath = AssetDatabase.GetAssetPath(@object.GetInstanceID());
        AssetDatabase.RenameAsset(assetPath, newID);
        AssetDatabase.SaveAssets();
    }


    public static T WriteFile<T>(Object targetFolder,T file, string fileName = null) where T : Object
    {
        var path = GetFolder(targetFolder);
        return WriteAsset(file, fileName, path);
    }

    public static T WriteAsset<T>(T file, string fileName = null, string path = null, bool overwrite=false) where T : Object
    {
        var type = file.GetType();

        
        Debug.Log($"Name : {fileName} Path : {path}");

        if (string.IsNullOrEmpty(fileName))
        {
            if (!string.IsNullOrEmpty(path))
                fileName = EditorPathUtility.GetAssetName(path);

            Debug.Log(fileName);

            if (string.IsNullOrEmpty(fileName))
            {
                var originPath = AssetDatabase.GetAssetPath(file);

                if(!string.IsNullOrEmpty(originPath))
                    fileName=EditorPathUtility.GetAssetName(originPath);

                //var split = originPath.LastIndexOf('/');
                //if (split > 0)
                //    fileName = originPath.Substring(split);

                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = file.name;
                    fileName += DateTime.Now + type.ToString();
                }
            }
        }

        if (!AssetDatabase.IsValidFolder(path))
        {
            if (string.IsNullOrEmpty(path))
            {
                SafeCreateDirectory("Assets/TempArchiveFolder");
                path = "Assets/TempArchiveFolder/" + string.Format("{0}{1:yyyy_MM_dd_HH_mm_ss}_{2}+.asset", fileName, Time.time, type.ToString());
                Debug.LogWarning(path + " not matchPackages! Save at new path : " + path + new IOException());
            }
            else
            {
                SafeCreateDirectory(path);
            }
        }

        path = Path.Combine(path, fileName);

        if (!Path.HasExtension(path))
            path += EditorPathUtility.GetExtension(file);

        Debug.Log(path);

        if (overwrite)
        {
            // valid exist
            var ioPath = EditorPathUtility.GetIOPath(path);
            if (File.Exists(ioPath))
                AssetDatabase.DeleteAsset(path);

        }
        else
        {
            path = AssetDatabase.GenerateUniqueAssetPath(path);

            var log = string.Format("Archive one {0} file at {1}", type.ToString(), path);

            Debug.Log(log);
        }


        path = EditorPathUtility.GetEditorPath(path);

        try
        {
            if (file is Component c)
                PrefabUtility.SaveAsPrefabAsset(c.gameObject, path);
            else if (file is GameObject go)
                PrefabUtility.SaveAsPrefabAsset(go, path);
            else
                AssetDatabase.CreateAsset(file, path);
        }
        catch (Exception e)
        {
            Debug.LogError($"error archive path {path}");
            Debug.LogException(e);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorGUIUtility.PingObject(file);

        return AssetDatabase.LoadAssetAtPath<T>(path);
    }


    public static T[] GetAssetsAtPath<T>(string path, bool inculdeChildFolder = false) where T : Object
    {

        EditorUtility.DisplayProgressBar("Loading", "Loading " + typeof(T) + " files..", 0);
        var fileEntries = GetSubFiles(path, true);
        EditorUtility.ClearProgressBar();

        if (fileEntries == null)
            return new T[0];

        ArrayList al = new ArrayList();

        foreach (string filePath in fileEntries)
        {
            Object t = AssetDatabase.LoadAssetAtPath<T>(filePath);
            if (t != null)
                al.Add(t);
        }
        T[] result = new T[al.Count];
        for (int i = 0; i < al.Count; i++)
            result[i] = (T)al[i];

        return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="file"></param>
    /// <param name="path">Must inculde file name</param>
    /// <returns></returns>
    public static void WriteFile(UnityEngine.Object file, Type type = null, string path = null,string extension=null)
    {

        if (string.IsNullOrEmpty(extension))
        {
            extension = "asset";

            if (type != null)
            {
                var @switch = new Dictionary<Type, Action>
                {
                    { typeof(Mesh), () => extension="mesh"},
                    { typeof(AnimationClip), () => extension="anim"},

                };

                if(@switch.TryGetValue(type,out var action))
                    action?.Invoke();
                
            }
        }

        if (string.IsNullOrEmpty(path))
        {
            SafeCreateDirectory("Assets/TempArchiveFolder");
            path = string.Format("Assets/TempArchiveFolder/{0}{1:yyyy_MM_dd_HH_mm_ss}{2}.{3}", file.ToString(), DateTime.Now, type.ToString(),extension);
        }
        else
        {
            path = path + '.' + extension;
        }


        var uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath(path);

        var log = string.Format("Archive one {0} file at {1}", type.ToString(), path);
        Debug.Log(log);
        AssetDatabase.CreateAsset(file, uniqueAssetPath);
        AssetDatabase.Refresh();

        EditorGUIUtility.PingObject(file);
    }

    public static void WriteAnimationFile(AnimationClip humanPose)
    {
        SafeCreateDirectory("Assets/Resources");

        var path = string.Format("Assets/Resources/RecordMotion_{0}{1:yyyy_MM_dd_HH_mm_ss}.asset", humanPose.name, DateTime.Now);

        var uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath(path);


        AssetDatabase.CreateAsset(humanPose, uniqueAssetPath);
        AssetDatabase.Refresh();
    }


    public static string GetFolder(UnityEngine.Object @object)
    {
        var path = AssetDatabase.GetAssetPath(@object);
        path = path.Substring(0, path.IndexOf(@object.name));
        return path;
    }

    public static Dictionary<string,string> CheckFilterFolders(string rootFolder, bool createIfNull = false, params string[] filters)
    {
        Assert.IsTrue(AssetDatabase.IsValidFolder(rootFolder));
        var filterDic = new Dictionary<string, string>(filters.Length);
        var folders = AssetDatabase.GetSubFolders(rootFolder);
        var compare = System.StringComparison.CurrentCultureIgnoreCase;
        foreach (var folder in folders)
        {
            foreach (var filter in filters)
            {
                if (filterDic.ContainsKey(filter))
                    continue;

                if (folder.Contains(filter, compare))
                {
                    filterDic.Add(filter, folder);
                    continue;
                }
            }
        }

        if(createIfNull)
            foreach (var filter in filters)
            {
                if (filterDic.ContainsKey(filter))
                    continue;

                var guid = AssetDatabase.CreateFolder(rootFolder, filter);
                Assert.IsFalse(string.IsNullOrEmpty(guid));
                var path = AssetDatabase.GUIDToAssetPath(guid);
                filterDic.Add(filter, path);
            }

        return filterDic;
    }


    public static string GetExtension(UnityEngine.Object @object)
    {
        var path = AssetDatabase.GetAssetPath(@object);
        path = path.Substring(path.LastIndexOf("Assets"));
        var extension = Path.GetExtension(Path.Combine(Application.dataPath, path));
        return extension;
    }

    #endregion

}

#endif