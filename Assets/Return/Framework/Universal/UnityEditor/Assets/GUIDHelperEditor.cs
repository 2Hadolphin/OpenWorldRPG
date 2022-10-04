using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using System.Text.RegularExpressions;
using UnityEditor.Build.Content;

namespace Return.Editors
{
    public class GUIDHelperEditor : OdinEditorWindow
    {
        [MenuItem("Tools/Assets/GUIDHelperEditor")]
        static void OpenWindow()
        {
            var window = GetWindow<GUIDHelperEditor>();

            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(700, 700);
        }

        #region MenuItem
        [MenuItem("Tools/Assets/Regenerate asset GUIDs")]
        public static void RegenerateGuids()
        {
            RegenerateAllGUIDs();
        }
        #endregion


        [Button]
        static void RegenerateAllGUIDs()
        {
            Debug.LogException(new InvalidOperationException("????????"));


            return;

            if (EditorUtility.DisplayDialog(
                "GUIDs regeneration",
                "You are going to start the process of GUID regeneration. This may have unexpected results. \n\nMAKE A PROJECT BACKUP BEFORE PROCEEDING!",
                "Regenerate GUIDs",
                "Cancel"
                ))
            {
                try
                {
                    AssetDatabase.StartAssetEditing();

                    string path = Path.GetFullPath(".") + Path.DirectorySeparatorChar + "Assets";
                    UnityGuidRegenerator regenerator = new(path);
                    regenerator.RegenerateGuids();
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                    EditorUtility.ClearProgressBar();
                    AssetDatabase.Refresh();
                }
            }
        }

        [SerializeField]
        Object target;


        [SerializeField]
        [FolderPath]
        string FolderPath;

        [Button]
        void RelaceGUIDs()
        {
            Dictionary<string, Dictionary<string, ObjectIdentifier>> dic = new();

            EditorAssetsUtility.ReplaceGUIDs(dic, FolderPath);
        }



        static public string[] LoadPaths(string folderPath)
        {

            var objs=EditorAssetsUtility.GetAssetsAtPath<Object>(folderPath,null,true);

            Debug.Log($"Assets Count : {objs.Length} \n {string.Join("\n",objs.Select(x=>AssetDatabase.GetAssetPath(x)))}");

            var paths = AssetDatabase.GetAllAssetPaths().Where(x => x.StartsWith(folderPath)).ToArray();

            Debug.Log($"Paths Count : {paths.Length} \n {string.Join("\n", paths)}");

            return paths;
        }


        internal class UnityGuidRegenerator
        {
            private readonly string _assetsPath;

            public UnityGuidRegenerator(string assetsPath)
            {
                _assetsPath = assetsPath;
            }

            public void RegenerateGuids(string[] regeneratedExtensions = null)
            {
                if (regeneratedExtensions == null)
                {
                    regeneratedExtensions = EditorAssetsUtility.kDefaultGUIDFileExtensions;
                }

                // Get list of working files
                List<string> filesPaths = new();
                foreach (string extension in regeneratedExtensions)
                {
                    filesPaths.AddRange(
                        Directory.GetFiles(_assetsPath, extension, SearchOption.AllDirectories)
                        );
                }

                // Create dictionary to hold old-to-new GUID map
                Dictionary<string, string> guidOldToNewMap = new();
                Dictionary<string, List<string>> guidsInFileMap = new();

                // We must only replace GUIDs for Resources present in Assets. 
                // Otherwise built-in resources (shader, meshes etc) get overwritten.
                HashSet<string> ownGuids = new();

                // Traverse all files, remember which GUIDs are in which files and generate new GUIDs
                int counter = 0;
                foreach (string filePath in filesPaths)
                {
                    if (!EditorUtility.DisplayCancelableProgressBar("Scanning Assets folder", MakeRelativePath(_assetsPath, filePath),
                        counter / (float)filesPaths.Count))
                    {
                        string contents = File.ReadAllText(filePath);

                        IEnumerable<string> guids = EditorAssetsUtility.GetGuids(contents);
                        bool isFirstGuid = true;
                        foreach (string oldGuid in guids)
                        {
                            // First GUID in .meta fileMatch is always the GUID of the asset itself
                            if (isFirstGuid && Path.GetExtension(filePath) == ".meta")
                            {
                                ownGuids.Add(oldGuid);
                                isFirstGuid = false;
                            }
                            // Generate and save new GUID if we haven't added it before
                            if (!guidOldToNewMap.ContainsKey(oldGuid))
                            {
                                string newGuid = Guid.NewGuid().ToString("N");
                                guidOldToNewMap.Add(oldGuid, newGuid);
                            }

                            if (!guidsInFileMap.ContainsKey(filePath))
                                guidsInFileMap[filePath] = new List<string>();

                            if (!guidsInFileMap[filePath].Contains(oldGuid))
                            {
                                guidsInFileMap[filePath].Add(oldGuid);
                            }
                        }

                        counter++;
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning("GUID regeneration canceled");
                        return;
                    }
                }

                // Traverse the files again and replace the old GUIDs
                counter = -1;
                int guidsInFileMapKeysCount = guidsInFileMap.Keys.Count;
                foreach (string filePath in guidsInFileMap.Keys)
                {
                    EditorUtility.DisplayProgressBar("Regenerating GUIDs", MakeRelativePath(_assetsPath, filePath), counter / (float)guidsInFileMapKeysCount);
                    counter++;

                    string contents = File.ReadAllText(filePath);
                    foreach (string oldGuid in guidsInFileMap[filePath])
                    {
                        if (!ownGuids.Contains(oldGuid))
                            continue;

                        string newGuid = guidOldToNewMap[oldGuid];
                        if (string.IsNullOrEmpty(newGuid))
                            throw new NullReferenceException("newGuid == null");

                        contents = contents.Replace("guid: " + oldGuid, "guid: " + newGuid);
                    }
                    File.WriteAllText(filePath, contents);
                }

                EditorUtility.ClearProgressBar();
            }


            private static string MakeRelativePath(string fromPath, string toPath)
            {
                Uri fromUri = new(fromPath);
                Uri toUri = new(toPath);

                Uri relativeUri = fromUri.MakeRelativeUri(toUri);
                string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

                return relativePath;
            }
        }

        internal class UnityGuidRetarget
        {
            private readonly string _assetsPath;

            public UnityGuidRetarget(string assetsPath)
            {
                _assetsPath = assetsPath;
            }

            public void RegenerateGuids(string[] regeneratedExtensions = null)
            {
                if (regeneratedExtensions == null)
                {
                    regeneratedExtensions = EditorAssetsUtility.kDefaultGUIDFileExtensions;
                }

                // Get list of working files
                List<string> filesPaths = new();
                foreach (string extension in regeneratedExtensions)
                {
                    filesPaths.AddRange(
                        Directory.GetFiles(_assetsPath, extension, SearchOption.AllDirectories)
                        );
                }

                // Create dictionary to hold old-to-new GUID map
                Dictionary<string, string> guidOldToNewMap = new();
                Dictionary<string, List<string>> guidsInFileMap = new();

                // We must only replace GUIDs for Resources present in Assets. 
                // Otherwise built-in resources (shader, meshes etc) get overwritten.
                HashSet<string> ownGuids = new();

                // Traverse all files, remember which GUIDs are in which files and generate new GUIDs
                int counter = 0;
                foreach (string filePath in filesPaths)
                {
                    if (!EditorUtility.DisplayCancelableProgressBar("Scanning Assets folder", MakeRelativePath(_assetsPath, filePath),
                        counter / (float)filesPaths.Count))
                    {
                        string contents = File.ReadAllText(filePath);

                        IEnumerable<string> guids = EditorAssetsUtility.GetGuids(contents);
                        bool isFirstGuid = true;
                        foreach (string oldGuid in guids)
                        {
                            // First GUID in .meta fileMatch is always the GUID of the asset itself
                            if (isFirstGuid && Path.GetExtension(filePath) == ".meta")
                            {
                                ownGuids.Add(oldGuid);
                                isFirstGuid = false;
                            }

                            // Generate and save new GUID if we haven't added it before
                            if (!guidOldToNewMap.ContainsKey(oldGuid))
                            {
                                string newGuid = Guid.NewGuid().ToString("N");
                                guidOldToNewMap.Add(oldGuid, newGuid);
                            }

                            if (!guidsInFileMap.ContainsKey(filePath))
                                guidsInFileMap[filePath] = new List<string>();

                            if (!guidsInFileMap[filePath].Contains(oldGuid))
                            {
                                guidsInFileMap[filePath].Add(oldGuid);
                            }
                        }

                        counter++;
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning("GUID regeneration canceled");
                        return;
                    }
                }

                // Traverse the files again and replace the old GUIDs
                counter = -1;
                int guidsInFileMapKeysCount = guidsInFileMap.Keys.Count;
                foreach (string filePath in guidsInFileMap.Keys)
                {
                    EditorUtility.DisplayProgressBar("Regenerating GUIDs", MakeRelativePath(_assetsPath, filePath), counter / (float)guidsInFileMapKeysCount);
                    counter++;

                    string contents = File.ReadAllText(filePath);
                    foreach (string oldGuid in guidsInFileMap[filePath])
                    {
                        if (!ownGuids.Contains(oldGuid))
                            continue;

                        string newGuid = guidOldToNewMap[oldGuid];
                        if (string.IsNullOrEmpty(newGuid))
                            throw new NullReferenceException("newGuid == null");

                        contents = contents.Replace("guid: " + oldGuid, "guid: " + newGuid);
                    }
                    File.WriteAllText(filePath, contents);
                }

                EditorUtility.ClearProgressBar();
            }


            private static string MakeRelativePath(string fromPath, string toPath)
            {
                Uri fromUri = new(fromPath);
                Uri toUri = new(toPath);

                Uri relativeUri = fromUri.MakeRelativeUri(toUri);
                string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

                return relativePath;
            }
        }
    }

}
