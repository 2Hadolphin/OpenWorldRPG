using UnityEngine;
using System.IO;
using UnityEditor;
using System.Collections;
using Object = UnityEngine.Object;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

public partial class EditorHelper
{
    public const string RootPath = "Assets";

    #region Path
    public static string[] GetSubFiles(string path, bool inculdeChildFolder)
    {
        var root = RootPath;
        path = path.Substring(path.IndexOf(root));

        if (path.Contains(root + "/"))
            path = path.Replace(root + "/", string.Empty);
        else
            path = path.Replace(root, string.Empty);


        path = Application.dataPath + "/" + path;

        if (!Directory.Exists(path))
            return null;

        var fileEntries = Directory.GetFiles(path).ToList();

        if (inculdeChildFolder)
        {
            var subFolders = Directory.GetDirectories(path);
            foreach (var folder in subFolders)
            {
                fileEntries.AddRange(GetSubFiles(folder, inculdeChildFolder));
            }
        }

        var length = fileEntries.Count;
        for (var i = 0; i < length; i++)
        {
            var filePath = fileEntries[i];
            int assetPathIndex = filePath.IndexOf(RootPath);
            fileEntries[i] = filePath.Substring(assetPathIndex);
        }

        return fileEntries.ToArray();
    }

    public static bool SplitEditorTargetPath(string target, out string path, out string fileName)
    {
        var chars = target.Split('/');
        var length = chars.Length;
        var _path = chars[0];
        for (int i = 1; i < length; i++)
        {
            if (AssetDatabase.IsValidFolder(_path))
            {
                Debug.Log(_path);
                _path += chars[i];
            }
            else
            {
                fileName = chars[i - 1];
                path = _path.Replace(chars[i - 1], string.Empty);
                return true;
            }
        }

        path = target;
        fileName = string.Empty;
        return false;
    }
    public static string ValidFolder(string path = null)
    {
        if (string.IsNullOrEmpty(path) || !AssetDatabase.IsValidFolder(path))
            path = EditorUtility.OpenFolderPanel("Selected Folder", "Assets", "");


        if (path.Contains(Application.dataPath))
        {
            path = "Assets" + path.Substring(Application.dataPath.Length);
        }

        return path;
    }

    public static string InvalidFolderOpenPenel (string path = null)
    {
        if (string.IsNullOrEmpty(path) || !AssetDatabase.IsValidFolder(path))
            return ChoseFolder(path);
        else
            return path;
    }

    public static string ChoseFolder(string path=null)
    {
        if (string.IsNullOrEmpty(path) || !AssetDatabase.IsValidFolder(path))
            path = EditorUtility.OpenFolderPanel("Selected Folder", "Assets", "");
        else
            path = EditorUtility.OpenFolderPanel("Selected Folder", path, "");


        if (path.Contains(Application.dataPath))
        {
            path = "Assets" + path.Substring(Application.dataPath.Length);
        }

        return path;
    }

    public static DirectoryInfo SafeCreateDirectory(string path)
    {
        return Directory.Exists(path) ? null : Directory.CreateDirectory(path);
    }
    public static string CurrentPath()
    {
        string folderPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
        if (folderPath.Contains("."))
            folderPath = folderPath.Remove(folderPath.LastIndexOf('/'));
        return folderPath;
    }

    #endregion


    #region Notice



    public static void PopMessage(string title,string msg,string okay)
    {
        EditorUtility.DisplayDialog(title, msg, okay);
    }
    #endregion



    #region Layout

    public static GUILayoutOption MiddleLittleButton(int numbers)
    {
        var width = EditorGUIUtility.currentViewWidth / numbers;
        width = width < EditorGUIUtility.labelWidth ? EditorGUIUtility.labelWidth : width;
        return GUILayout.Width(width);
    }

    #endregion


}
