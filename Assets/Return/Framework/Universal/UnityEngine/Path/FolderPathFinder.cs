using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Quick access folder path.
/// </summary>
[HideLabel]
[System.Serializable]
public struct FolderPathFinder
{
    public FolderPathFinder(string bindingKey,string defaultPath=null)
    {
        PathKey = nameof(FolderPathFinder) +'_'+ bindingKey;

        //if(string.IsNullOrEmpty(defaultPath))
        //    defaultPath = GetPrefPath(PathKey);

        DefaultPath = defaultPath;

        m_Path = null;
    }

    string DefaultPath;

    [HideInInspector]
    [SerializeField]
    string PathKey;

    string m_Path;

    [FolderPath(AbsolutePath = true)]
    [ShowInInspector]
    public string Path
    {
        get 
        {
            if (string.IsNullOrEmpty(m_Path))
                m_Path = GetPrefPath(PathKey, DefaultPath);
            
            return m_Path;
        }

        set
        {
#if UNITY_EDITOR
            EditorPrefs.SetString(PathKey, value);
#else
            PlayerPrefs.SetString(PathKey, value);
#endif
            m_Path = value;
        }
    }

    static string GetPrefPath(string key, string defaultPath)
    {
#if UNITY_EDITOR
        return EditorPrefs.GetString(key, GetDefaultPath(defaultPath));
#else
        return PlayerPrefs.GetString(key, GetDefaultPath(defaultPath));
#endif
    }


    static string GetDefaultPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
#if UNITY_EDITOR
            return "Assets";
#else
            return Application.persistentDataPath;
#endif
        }
        else
            return path;
    }

 
    //void PushPath()
    //{
    //    if (string.IsNullOrEmpty(m_Path))
    //        return;
    //    else
    //        Path = m_Path;
    //}

    //[FolderPath(AbsolutePath = true)]
    //[OnValueChanged(nameof(PushPath))]
    //public string m_Path;


    public static implicit operator string(FolderPathFinder pathUtility)
    {
        //if (string.IsNullOrEmpty(pathUtility.Path))
        //    return pathUtility.Path;
        //else
            return pathUtility.Path;
    }
}
