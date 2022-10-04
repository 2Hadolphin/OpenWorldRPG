using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using UnityEditor.Build.Content;

namespace Return.Editors
{
    public class GUIDHelperEditorWnidow : OdinEditorWindow
    {
        [MenuItem("Tools/Assets/GUIDToolkit")]
        static void OpenWindow()
        {
            var window = GetWindow<GUIDHelperEditorWnidow>();

            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(700, 700);
        }


        [FolderPath(AbsolutePath = true,RequireExistingPath =false)]
        [SerializeField]
        string SourceFolder;

        [ShowInInspector]
        [OnValueChanged(nameof(LoadItem))]
        [DropZone(typeof(Object))]
        Object ScriptAsset;


        [SerializeField]
        MonoScript script;

        void LoadItem()
        {
            if (ScriptAsset == null)
                return;

            script = ScriptAsset as MonoScript;

            ScriptAsset = null;
        }

        [HideInInspector]
        [SerializeField]
        Dictionary<string, string> sourceFilePathcache;

        Dictionary<string, string> sourceGUIDCache;


        [ShowInInspector]
        HashSet<string> RepeatBindings;

        [Button]
        void LoadSourceCache()
        {
            var filePaths = Directory.GetFiles(SourceFolder, "*.meta", SearchOption.AllDirectories).
                Where(x => x.Contains(".cs"));

            sourceFilePathcache = new(500);//= filePaths.ToDictionary(key => Path.GetFileName(key), value => value);

            RepeatBindings = new(20);

            foreach (var path in filePaths)
            {
                var name = Path.GetFileName(path);

                if (sourceFilePathcache.ContainsKey(name))
                {
                    RepeatBindings.Add(path);
                    continue;
                }

                sourceFilePathcache.Add(name, path);
            }

            Debug.LogError($"Repeate binding : {string.Join("\n",RepeatBindings)}");

            sourceGUIDCache = new(sourceFilePathcache.Count);

            foreach (var path in sourceFilePathcache.Values)
            {
                var meta = File.ReadAllText(path);
                if (GetGUID(meta, out var guid))
                    sourceGUIDCache.Add(guid, path);
            }
        }

        [SerializeField]
        List<MonoScript> SwapTargets;

        [Button]
        void SwapScripts()
        {
      

            try
            {
                AssetDatabase.StartAssetEditing();

                foreach (var script in SwapTargets)
                {
                    var path=AssetDatabase.GetAssetPath(script);
                    SwapScript(path);
                }

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }
        }

        [Button]
        void SwapGUID()
        {
            if (script == null)
                return;

            var asset = script;

            SwapScript(AssetDatabase.GetAssetPath(asset));

            AssetDatabase.Refresh();
        }

        [Button]
        void SwapScript(string assetPath)
        {
            var metaPath = EditorPathUtility.GetMetaPath(assetPath);
            var name = Path.GetFileName(metaPath);

            Debug.Log($"Asset path {metaPath}");

            if (!sourceFilePathcache.TryGetValue(name, out var targetPath))
            {
                if (RepeatBindings.Any(x => EditorPathUtility.GetAssetPath(x) == assetPath))
                {
                    Debug.LogError("Load from repeat binding");
                    targetPath = RepeatBindings.First(x => EditorPathUtility.GetAssetPath(x) == assetPath);
                }
                else
                {
                    Debug.LogError("Failue to found script file.");
                    return;
                }
            }

            Debug.Log($"Match {name} at {targetPath}");

            var curMeta = GetMeta(assetPath);
            var oldMeta = File.ReadAllText(targetPath);

            if (!GetGUID(curMeta, out var curGUID))
            {
                Debug.LogError($"Failure to found guids {curGUID}");
                return;
            }


            if (!GetGUID(oldMeta, out var lastGUID))
            {
                Debug.LogError($"Failure to found guids {lastGUID}");
                return;
            }

            var valid = string.Equals(curGUID, lastGUID, StringComparison.CurrentCultureIgnoreCase);

            Debug.Log($"Compare : {valid} {curGUID} : {lastGUID}");

            if (!valid)
            {
                var newMeta = curMeta.Replace(curGUID, lastGUID);
                File.WriteAllText(metaPath, newMeta);
            }
        }

        bool GetGUID(string context, out string guid)
        {
            var guidRegx = new Regex("guid:\\s?([a-fA-F0-9]+)");

            var guidMatch = guidRegx.Match(context);

            if (!guidMatch.Success)
            {
                Debug.LogError($"Failure to find script guid.");
                guid = null;
                return false;
            }

            guid = guidMatch.Groups[1].Value;
            return true;
        }

        string GetMeta(string assetPath)
        {
            var metaPath = EditorPathUtility.GetMetaPath(assetPath);
            return File.ReadAllText(metaPath);
        }

        [FolderPath]
        [SerializeField]
        string SearchFolder;

        [ShowInInspector]
        List<string> InvalidPath;

        [ShowInInspector]
        HashSet<string> InvalidGUID;

        [Button]
        void SearchMissingReference()
        {
            var folder = SearchFolder;

            var allPaths = AssetDatabase.GetAllAssetPaths();


            var length = allPaths.Length;

            InvalidPath = new(length / 3);

            InvalidGUID = new(50);

            try
            {
                for (int i = 0; i < length; i++)
                {
                    var path = allPaths[i];

                    if (EditorUtility.DisplayCancelableProgressBar("Search project GUIDs", path, (float)i / length * 100f))
                        break;

                    if (!path.Contains(folder))
                        continue;

                    if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
                        continue;

                    InvalidPath.Add(path);

                    var meta = GetMeta(path);

                    if (GetGUID(meta, out var guid))
                        InvalidGUID.Add(guid);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        [Button]
        void SolveGUID()
        {
            foreach (var guid in InvalidGUID)
            {
                Debug.Log($"Solve guid : {guid}");

                if (!sourceGUIDCache.TryGetValue(guid, out var sourcePath))
                {
                    Debug.LogError("Failure to find source guid");
                    continue;
                }


                Debug.Log($"Valid script name : {Path.GetFileNameWithoutExtension(sourcePath)}");
            }


        }


        [Button]
        void CacheGUID()
        {
            if (!Directory.Exists(SourceFolder))
            {
                Debug.LogError($"Missing target folder {SourceFolder}");
                return;
            }

            var filePaths = Directory.GetFiles(SourceFolder, "*.meta", SearchOption.AllDirectories).
                Where(x => x.Contains(".cs"));


            //Debug.LogError(string.Join("\n", filePaths.Select(key => Path.GetFileNameWithoutExtension(key))));

            var guidRegx = new Regex("guid:\\s?([a-fA-F0-9]+)");

            foreach (var filePath in filePaths)
            {
                var context = File.ReadAllText(filePath);

                var guidMatch = guidRegx.Match(context);

                if (!guidMatch.Success)
                {
                    Debug.LogError($"Failure to find script guid : {filePath}");
                    continue;
                }

                var guid = guidMatch.Groups[1].Value;

                Debug.Log($"{Path.GetFileName(filePath)} : {guid}");

                break;
            }
        }




        [Button]
        void SearchCompontent(GameObject go)
        {
            var comps = go.GetComponentsInChildren(typeof(Component));

            if (comps.Length > 0)
                foreach (var comp in comps)
                {
                    if (comp != null)
                        continue;

                    if (ObjectIdentifier.TryGetObjectIdentifier(comp, out var id))
                        Debug.Log(id.guid);
                    else
                        Debug.Log(EditorAssetsUtility.Log(comp));
                }
        }



    }
}
