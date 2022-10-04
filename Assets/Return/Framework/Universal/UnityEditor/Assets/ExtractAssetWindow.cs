using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using UnityEditor;
using System.Linq;
using System;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using Object = UnityEngine.Object;
using Microsoft.CSharp;
using UnityEditor.Build.Content;
using System.IO;

namespace Return.Editors
{
    public class ExtractAssetWindow : OdinEditorWindow
    {
        [MenuItem("Tools/Assets/ExtractAssetHelper")]
        static void OpenWindow()
        {
            GetWindow<ExtractAssetWindow>().position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
        }

        #region Search

        bool showExtractFolder => ExtractFolders == null || ExtractFolders.Count == 0;

        [InfoBox("Add folder to searching convex mesh.", nameof(showExtractFolder))]
        [TitleGroup("Search Folders")]
        [BoxGroup("Search Folders/Search", ShowLabel =false)]
        [ListDrawerSettings(NumberOfItemsPerPage = 5)]
        [SerializeField]
        List<Object> ExtractFolders;

        [TitleGroup("Search Results")]
        [BoxGroup("Search Results/Index", ShowLabel = false)]
        [Tooltip("All asset path under the extract folder")]
        [ListDrawerSettings(NumberOfItemsPerPage =5)]
        [SerializeField]
        string[] allPaths;

        [BoxGroup("Search Results/Index")]
        [ListDrawerSettings(NumberOfItemsPerPage = 10)]
        [SerializeField]
        string[] validPaths;

        [TitleGroup("Extract Queue")]
        [BoxGroup("Extract Queue/Quest", ShowLabel = false)]
        [ListDrawerSettings(NumberOfItemsPerPage = 10)]
        [SerializeField]
        Dictionary<string,string> targetFolder;

        #endregion

        #region Prepare

        [PropertySpace(5, 5)]
        [BoxGroup("Search Folders/Search")]
        [Button(ButtonSizes.Medium, Style =ButtonStyle.Box,Expanded =true)]
        public void ScanAssets(string filter = "Convex")
        {
            if (ExtractFolders == null || ExtractFolders.Count==0)
                allPaths = AssetDatabase.GetAllAssetPaths();
            else 
            {
                var paths = ExtractFolders.
                    Where(x => x != null).
                    Select(x => AssetDatabase.GetAssetPath(x)).
                    Where(x => AssetDatabase.IsValidFolder(x)).
                    ToArray();

                allPaths = 
                    AssetDatabase.FindAssets(filter, paths).
                    Select(x=>AssetDatabase.GUIDToAssetPath(x)).
                    ToArray();
            }

            validPaths = allPaths.
                Where(x => x.Contains(filter, StringComparison.CurrentCultureIgnoreCase)).
                Where(x=>AssetDatabase.LoadMainAssetAtPath(x)==null).
                ToArray();            
        }

        [PropertySpace(5, 5)]
        bool readyBindIndex => validPaths != null && validPaths.Length>0;
        [InfoBox("Prepare folder for sub asset before extract.",VisibleIf =nameof(readyBindIndex))]
        [EnableIf(nameof(readyBindIndex))]
        [BoxGroup("Search Results/Index", ShowLabel = false)]
        [Button(ButtonSizes.Medium, Style = ButtonStyle.Box)]
        /// <summary>
        /// Cache asset path and target assetFolder 
        /// </summary>
        public void InitFolder()
        {
            if (targetFolder == null)
                targetFolder = new(validPaths.Length);


            Dictionary<string,string> cacheFolder=new(validPaths.Length);

            AssetDatabase.StartAssetEditing();

            try
            {
                foreach (var path in validPaths)
                {
                    // check repeat assetFolder
                    var assetFolder = EditorPathUtility.GetAssetFolderPath(path);

                    if(!cacheFolder.TryGetValue(assetFolder,out var targetFolderPath))
                    {
                        targetFolderPath = AddFolder(path);
                        cacheFolder.Add(assetFolder, targetFolderPath);
                    }

                    if (targetFolder.ContainsKey(path))
                    {
                        //Debug.Log($"Repeat asset path {path}");
                        continue;
                    }

                    targetFolder.Add(path, targetFolderPath);
                }

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }


            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static string AddFolder(string path)
        {
            var folder = EditorPathUtility.GetAssetFolderPath(path);

            if (!AssetDatabase.IsValidFolder(folder + "/Convex"))
            {
                var guid = AssetDatabase.CreateFolder(folder, "Convex");
                folder = AssetDatabase.GUIDToAssetPath(guid);
            }
            else
                folder += "/Convex";

            return folder;
        }

        #endregion

        #region Extract

        bool readyExtract => targetFolder != null && validPaths != null;

        [InfoBox("Setup extract config and execute operation.",VisibleIf =nameof(readyExtract))]
        [PropertySpace(5,5)]
        [PropertyOrder(10)]
        [TitleGroup("Extract Results")]
        [BoxGroup("Extract Results/Results", ShowLabel = false)]
        [EnableIf(nameof(readyExtract))]
        [Button(ButtonSizes.Medium, Name = "Extract", Style = ButtonStyle.Box)]
        public void ExtractAssets()
        {
            if (extractAssets == null)
                extractAssets = new(validPaths.Length*2);

            var length = validPaths.Length;

            AssetDatabase.StartAssetEditing();

            Dictionary<string, Dictionary<string, ObjectIdentifier>> dict = new(length*3);

            void AddBinding(KeyValuePair<ObjectIdentifier,ObjectIdentifier> pair)
            {
                var oldGuid = pair.Key.guid.ToString();

                if(!dict.TryGetValue(oldGuid,out var binding))
                {
                    binding = new(1);
                    dict.Add(oldGuid, binding);
                }

                var oldFileID = pair.Key.localIdentifierInFile.ToString();

                if (binding.ContainsKey(oldFileID))
                {
                    Debug.LogException(new InvalidOperationException($"Repeat binding is impossiable {oldGuid}-{oldFileID}"));
                    return;
                }

                binding.Add(oldFileID, pair.Value);
            }

            try
            {
                for (int i = 0; i < length; i++)
                {
                    try
                    {
                        var path = validPaths[i];

                        var msg = $"Extracting {i}/{length}..\n..{path}";
                        if (EditorUtility.DisplayCancelableProgressBar("Extracting assets..", msg, i / length))
                            break;

                        if (!targetFolder.TryGetValue(path, out var folder))
                        {
                            folder = AddFolder(path);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        }

                        var assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);

                        EditorAssetsUtility.ExtractSubAssets(assets, folder, out var bindings);

                        foreach (var binding in bindings)
                            AddBinding(binding);

                        //extractAssets.AddRange(EditorAssetsUtility.ExtractSubAssets(path, FileNamingRule, folder));

                        if(destroyRootAsset)
                            AssetDatabase.DeleteAsset(path);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failure to extract subasset at {validPaths[i]}.");
                        Debug.LogException(e);
                    }
                }

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            // GetNewGUID
            Debug.Log($"Start rebind {dict.Count} guid to assets.");
            EditorAssetsUtility.ReplaceGUIDs(dict, OverwriteGUIDFolder);


            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();


            //EditorGUIUtility.PingObject(extractAssets.First());
        }

        [BoxGroup("Extract Results/Results")]
        [SerializeField]
        bool destroyRootAsset;

        [Tooltip("Sub-asset will create new guid and break all reference,\nset a project scope to overwrite obsolete guid,\ndo not set as root directory it will takes a long time.")]
        [Required]
        [BoxGroup("Extract Results/Results")]
        [SerializeField]
        [FolderPath(RequireExistingPath =true)]
        string OverwriteGUIDFolder;

        [PropertyOrder(11)]
        [BoxGroup("Extract Results/Results")]
        [ListDrawerSettings(NumberOfItemsPerPage = 10)]
        [SerializeField]
        List<Object> extractAssets = new();

        
        public static string FileNamingRule(string name)
        {
            var lastIndex = name.LastIndexOf("Convex", StringComparison.CurrentCultureIgnoreCase);
            lastIndex += "convex".Length;
            name = name[..lastIndex];

            return name;
        }

        #endregion
    }


}
