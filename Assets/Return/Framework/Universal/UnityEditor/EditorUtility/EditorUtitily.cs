using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;
using System.IO;
using System;

namespace Return.Editors
{
    /// <summary>
    /// 
    /// </summary>
    public static class EditorUtitily
    {

        #region IDentify


        /// <summary>
        /// Get asset editor folder.
        /// </summary>
        public static string GetEditorFolder(UnityEngine.Object asset)
        {
            var path = AssetDatabase.GetAssetPath(asset);
            var index = path.LastIndexOf('/');

            if (index > 0)
                path = path.Remove(index);

            return path;
        }

        /// <summary>
        /// Get editor asset path without extension.
        /// </summary>
        public static string GetEditorPath(UnityEngine.Object asset)
        {
            var path = AssetDatabase.GetAssetPath(asset);
            var ext = Path.GetExtension(path);

            if (!string.IsNullOrEmpty(ext))
                path = path.Remove(path.Length - ext.Length);

            return path;
        }

       

        #endregion

    }


    public interface IAssetPath
    {
        public string GetPath();
        public string GetFolder();
        public string GetName();
        public string GetExtension();
    }

    public struct AssetPathCatch:IAssetPath
    {
        public AssetPathCatch(UnityEngine.Object @object)
        {
            Asset = @object;
        }

        public readonly UnityEngine.Object Asset;

        public string GetPath()
        {
            return AssetDatabase.GetAssetPath(Asset);
        }

        public string GetFolder()
        {
            return EditorUtitily.GetEditorFolder(Asset);
        }

        public string GetName()
        {
            return Asset.name;
        }

        public string GetExtension()
        {
            return Path.GetExtension(GetPath());
        }





    }
}
