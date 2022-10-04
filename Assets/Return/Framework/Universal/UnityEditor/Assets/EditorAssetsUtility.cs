#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.Assertions;
using System.Linq;
using Object = UnityEngine.Object;
using System.IO;
using UnityEditor.Build.Content;
using System.Text.RegularExpressions;

namespace Return.Editors
{
    

    /// <summary>
    /// Editor util bundle. **GUID  **Asset-Crate-Save-Duplicate-Cut&Paste-Replace-Extract
    /// </summary>
    public static class EditorAssetsUtility
    {
        #region GUID

        public static string GetGUID(int instanceID, AssetPathToGUIDOptions options = AssetPathToGUIDOptions.OnlyExistingAssets)
        {
            string guid;

            if (ObjectIdentifier.TryGetObjectIdentifier(instanceID, out var id))
            {
                guid = id.guid.ToString();
                Debug.Log($"Get guid from : {id.fileType}.");
            }
            else
            {
                Debug.LogError("Failure to get object identifier.");
                var path = AssetDatabase.GetAssetPath(instanceID);
                guid = AssetDatabase.AssetPathToGUID(path, options);
            }

            return guid;
        }

        public static string GetGUID<T>(this T asset, AssetPathToGUIDOptions options = AssetPathToGUIDOptions.OnlyExistingAssets) where T : Object
        {
            string guid;

            if (ObjectIdentifier.TryGetObjectIdentifier(asset, out var id))
            {
                guid = id.guid.ToString();
                Debug.Log($"Get guid from : {id.fileType}.");
            }
            else
            {
                Debug.LogError("Failure to get object identifier.");
                var path = AssetDatabase.GetAssetPath(asset);
                guid = AssetDatabase.AssetPathToGUID(path, options);
            }

            return guid;
        }

        /// <summary>
        /// File extension for **Prefab    **Scene **ScriptableObject.
        /// </summary>
        public static readonly string[] kSwapGUIDFileExtensions = new string[]
        {
            "*.prefab",
            "*.unity",
            "*.asset",
        };

        /// <summary>
        /// All extensions of editor assets which contains guid.
        /// </summary>
        public static readonly string[] kDefaultGUIDFileExtensions = new string[]
        {
            "*.meta",
            "*.mat",
            "*.anim",
            "*.prefab",
            "*.unity",
            "*.asset",
            "*.guiskin",
            "*.fontsettings",
            "*.controller",
        };

 

        /// <summary>
        /// Log asset content guids(not index).
        /// </summary>
        [MenuItem("Assets/LogGUIDs", priority = 1000)]
        public static void LogGUIDs()
        {
            var asset = Selection.activeObject;

            if (asset == null)
                return;

            var filePath = EditorPathUtility.GetIOPath(asset);

            string contents = File.ReadAllText(filePath);

            IEnumerable<string> guids = GetGuids(contents);

            Debug.Log(string.Join("\n", guids));

            var paths = guids.Select(x => AssetDatabase.GUIDToAssetPath(x));



            Debug.Log(string.Join("\n", paths));



            var objs = paths.Where(x => !string.IsNullOrEmpty(x)).Select(x => AssetDatabase.LoadAssetAtPath<Object>(x).name);

            Debug.Log(string.Join("\n", objs));
        }

        /// <summary>
        /// Get guid in asset context.(.meta)
        /// </summary>
        public static IEnumerable<string> GetGuids(string text)
        {
            const string guidStart = "guid: ";
            const int guidLength = 32;
            int textLength = text.Length;
            int guidStartLength = guidStart.Length;
            List<string> guids = new();

            int index = 0;
            while (index + guidStartLength + guidLength < textLength)
            {
                index = text.IndexOf(guidStart, index, StringComparison.Ordinal);
                if (index == -1)
                    break;

                index += guidStartLength;
                string guid = text.Substring(index, guidLength);
                index += guidLength;

                if (IsGuid(guid))
                {
                    guids.Add(guid);
                }
            }

            return guids;
        }

        public static bool IsGuid(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (
                    !((c >= '0' && c <= '9') ||
                      (c >= 'a' && c <= 'z'))
                    )
                    return false;
            }

            return true;
        }


        /// <summary>
        /// Remap guids to prefabs and scene targets.
        /// </summary>
        /// <param name="remapGUID">Old GUID - Old FileID - new GUID & FileID</param>
        static public void ReplaceGUIDs(Dictionary<string, Dictionary<string, ObjectIdentifier>> remapGUID, string folderPath)
        {
            if(!Directory.Exists(folderPath))
                folderPath = EditorPathUtility.GetIOPath(folderPath);

            List<string> filesPaths = new();

            foreach (string extension in EditorAssetsUtility.kSwapGUIDFileExtensions)
            {
                filesPaths.AddRange(
                    Directory.GetFiles(folderPath, extension, SearchOption.AllDirectories)
                    );
            }

            ReplaceGUIDs(remapGUID, filesPaths.ToArray());
        }

        /// <summary>
        /// Remap guids.
        /// </summary>
        /// <param name="remapGUID">Old GUID - Old FileID - new GUID & FileID</param>
        static public void ReplaceGUIDs(Dictionary<string, Dictionary<string, ObjectIdentifier>> remapGUID, params string[] filesPaths)
        {
            Debug.Log($"GUID files count : {filesPaths.Length} \n {string.Join("\n", filesPaths)}");


            //Debug.Log($"Load {data}");

            var guidRegx = new Regex("guid:\\s?([a-fA-F0-9]+)");
            var fileRegx = new Regex("fileID:\\s?([0-9]+)");

            int modify = 0;

            try
            {
                AssetDatabase.StartAssetEditing();

                var fileNum = filesPaths.Length;

                const string title = "Replace GUID.";

                for (int index = 0; index < fileNum; index++)
                {
                    try
                    {
                        var file = filesPaths[index];

                        var contents = File.ReadAllLines(file);

                        if (EditorUtility.DisplayCancelableProgressBar(title, $"Proccessing changing guid..{Path.GetFileName(file)}", (float)index / fileNum))
                        {
                            Debug.LogWarning("Guid swap has not finish, some reference might be broken.");
                            break;
                        }


                        bool edit = false;

                        var length = contents.Length;

                        for (int i = 0; i < length; i++)
                        {
                            var context = contents[i];

                            var guidMatch = guidRegx.Match(context);

                            if (!guidMatch.Success)
                                continue;

                            var fileMatch = fileRegx.Match(context);

                            if (!fileMatch.Success)
                                continue;

                            var oldGUID = guidMatch.Groups[1].Value;
                            var oldFileID = fileMatch.Groups[1].Value;


                            // valid guid qualify
                            if (!remapGUID.TryGetValue(oldGUID, out var dic))
                                continue;

                            // valid fileID qualify
                            if (!dic.TryGetValue(oldFileID, out var newID))
                                continue;

                            // replace guid and fileID
                            {
                                contents[i] = context.Replace(oldFileID, newID.localIdentifierInFile.ToString()).Replace(oldGUID, newID.guid.ToString());
                                edit = true;
                                modify++;
                                Debug.Log($"Match GUID FileID : {oldFileID} GUID : {oldGUID}");
                            }
                        }

                        // update file if context changed
                        if (edit)
                            File.WriteAllLines(file, contents);

                    }
                    catch (Exception e)
                    {
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
                EditorUtility.ClearProgressBar();
            }


            Debug.Log($"Edit {modify} guid references.");
        }



        #endregion


        #region Log


        /// <summary>
        /// Debug log selection.
        /// </summary>
        [MenuItem("Assets/LogSelection", priority = 1000)]
        public static void LogSelection()
        {
            var log = Log(Selection.activeObject);

            log = $"InstanceID : {Selection.activeInstanceID}\n{log}";

            Debug.Log(log);
        }

        public static void LogTarget(Object obj)
        {
            var log = Log(obj);

            log = $"InstanceID : {obj.GetInstanceID()}\n{log}";

            Debug.Log(log);
        }

        /// <summary>
        /// Log asset infomation.
        /// </summary>
        public static string Log(this Object obj)
        {
            string str = null; 


            if (obj != null)
            {
                string guid;
                
                if (ObjectIdentifier.TryGetObjectIdentifier(obj, out var id))
                {
                    guid = id.guid.ToString();
                    
                }
                else
                {
                    Debug.LogError("Failure to get object identifier.");
                    guid = obj.GetGUID();
                }

                str += $"Object : {obj.name}\n";
                str += $"GUID : {guid}\n";
                str += $"AssetType : {id.fileType}\n";
                str += $"Index : {id.localIdentifierInFile}\n";
                str += $"Type : {obj.GetType()}\n";
                str += $"MainAsset : {AssetDatabase.IsMainAsset(obj)}\n";
                str += $"NativeAsset : {AssetDatabase.IsNativeAsset(obj)}\n";
                str += $"ForeignAsset : {AssetDatabase.IsForeignAsset(obj)}\n";
                str += $"CanOpen : {AssetDatabase.IsOpenForEdit(obj)}\n";
            }

            return str;
        }

        #endregion

        #region Search

        /// <summary>
        /// Try to get asset at path.
        /// </summary>
        public static bool TryGetAsset<T>(string path,out T asset) where T : Object
        {
            asset = AssetDatabase.LoadAssetAtPath<T>(path);
            return asset != null;
        }

        /// <summary>
        /// Try to get asset at path.
        /// </summary>
        public static bool TryGetAsset<T>(out T asset) where T :Object
        {
            asset = null;

            var paths=AssetDatabase.FindAssets($"t:{typeof(T).Name}");

            //Debug.Log($"Cache {paths.Length} paths of {typeof(T).Name} via t:{typeof(T).Name}");

            foreach (var path in paths)
            {
                if (TryGetAsset(AssetDatabase.GUIDToAssetPath(path), out asset))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Get asset from guid.
        /// </summary>
        public static Object GetAsset(string guid)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
                return null;
            else
                return AssetDatabase.LoadAssetAtPath(path, typeof(Object));
        }

        /// <summary>
        /// Find asset with type in project.
        /// </summary>
        public static IEnumerable<T> GetAssets<T>() where T : Object
        {
            var paths = AssetDatabase.FindAssets($"t:{typeof(T).Name}");

            foreach (var path in paths)
            {
                if (TryGetAsset<T>(path, out var asset))
                    yield return asset;
            }

            yield break;
        }

        /// <summary>
        /// Get script asset assetsPath, return false if no mono script asset matchPackages.
        /// </summary>
        public static bool TryGetScriptAsset<T>(out MonoScript mono,bool matchName=false)
        {
            mono = null;

            var type = typeof(T);

            var paths=AssetDatabase.FindAssets("t:MonoScript");

            foreach (var path in paths)
            {
                var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

                if (asset == null)
                    continue;

                if (asset.GetClass() == type)
                    return true;

                if (matchName)
                    if (asset.name.Equals(typeof(T).Name, StringComparison.CurrentCultureIgnoreCase))
                        return true;
            }

            

            return false;
        }

        /// <summary>
        /// Load all asset under path. ** Null asset will be ignore
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">IO path.</param>
        /// <param name="includeChildFolder"></param>
        /// <returns></returns>
        public static T[] GetAssetsAtPath<T>(string path,string searchPattern=null,bool includeChildFolder=false) where T : Object
        {
            if (string.IsNullOrEmpty(searchPattern))
                searchPattern = "*.*";
            
            if (path.StartsWith("Assets"))
                path = EditorPathUtility.GetIOPath(path);

             SearchOption option = includeChildFolder ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            void SolvePaths(ArrayList list, string[] paths)
            {

                foreach (string filePath in paths)
                {
                    //Debug.Log(filePath);

                    if (filePath.Contains(".meta"))
                        continue;

                    var localPath = EditorPathUtility.GetAssetPath(filePath);

                    //Debug.Log(localPath);

                    var t = AssetDatabase.LoadAssetAtPath<T>(localPath);

                    if (t != null)
                        list.Add(t);
                    else
                        Debug.LogWarning($"Target asset is null, file will be ignore : {localPath}.");
                }
            }


            string[] fileEntries = Directory.GetFiles(path, searchPattern, option);
            Debug.Log($"{nameof(GetAssetsAtPath)} {path} found {fileEntries.Length} files.");

            // root folder will be ignore
            var folders = Directory.GetDirectories(path, searchPattern, option);
            Debug.Log($"{nameof(GetAssetsAtPath)} {path} found {folders.Length} folders.");

            var list = new ArrayList(fileEntries.Length+folders.Length);

            SolvePaths(list, fileEntries);
            SolvePaths(list, folders);

            T[] result = new T[list.Count];
            for (int i = 0; i < list.Count; i++)
                result[i] = (T)list[i];

            return result;
        }

        #endregion

        #region Creation

        [MenuItem("Assets/CreateAsset", true)]
        static bool CheckCreateSO()
        {
            if (Selection.activeObject is not MonoScript mono)
                return false;

            if (mono.GetClass() == null)
                return false;

            if (mono.GetClass().IsClass)
                return mono.IsChild(typeof(ScriptableObject), typeof(MonoBehaviour));
            else
                return false;
        }

        [MenuItem("Assets/CreateAsset", priority = 1000)]
        public static void Create()
        {
            var ob = Selection.activeObject;
            if (ob is MonoScript mono)
                Create(mono);
        }

        public static void Create(MonoScript mono)
        {
            var type = mono.GetClass();

            try
            {
                UnityEngine.Object obj;

                var folderPath = EditorUtitily.GetEditorFolder(mono);

                if (mono.IsChild(typeof(ScriptableObject)))
                {
                    obj = EditorAssetsUtility.CreateInstanceSO(type, false, folderPath, mono.name);
                }
                else if (mono.IsChild(typeof(MonoBehaviour)))
                {
                    var go = new GameObject(type.Name, type);
                    obj = EditorAssetsUtility.Archive(go, type.Name, false, folderPath);
                    UnityEngine.Object.DestroyImmediate(go);
                }
                else
                    throw new NotImplementedException(type.Name);

                EditorGUIUtility.PingObject(obj);
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("{0} is not a scriptable object script. {1}", mono.name, e));
            }
        }



        public static ScriptableObject CreateInstanceSO(Type type, bool folderout = false, string path = null, string fileName = null)
        {
            Assert.IsNotNull(type);

            var so = ScriptableObject.CreateInstance(type);

            if (so == null)
            {
                EditorHelper.PopMessage("Error", type + " is not valide 0_<", "Got it !");
                throw new KeyNotFoundException();
            }
            else
            {
                return Archive(so, string.IsNullOrEmpty(fileName) ? so.GetType().Name : fileName, folderout, path) as ScriptableObject;
            }

        }

        public static ScriptableObject CreateInstanceSO(string name, bool folderout = false, string path = null, string fileName = null)
        {
            var so = ScriptableObject.CreateInstance(name);

            if (so == null)
            {
                EditorHelper.PopMessage("Error", name + " is not valide 0_<", "Got it !");
                throw new KeyNotFoundException();
            }
            else
            {
                return Archive(so, string.IsNullOrEmpty(fileName) ? so.GetType().Name : fileName, folderout, path) as ScriptableObject;
            }

        }

        public static T CreateInstanceSO<T>(bool folderout = false, string path = null, string fileName = null) where T : ScriptableObject
        {
            var so = ScriptableObject.CreateInstance<T>();
            return Archive(so, string.IsNullOrEmpty(fileName) ? so.GetType().Name : fileName, folderout, path) as T;
        }

        #endregion

        #region Save Asset

        /// <summary>
        /// Will not update guid.
        /// </summary>
        [Obsolete]
        [MenuItem("CONTEXT/Component/Dirty")]
        static void DoubleMass(MenuCommand command)
        {
            Component comp = (Component)command.context;
            EditorUtility.SetDirty(comp);
            Debug.Log($"Set dirty {comp}.");
        }

        public static T CreatePrefab<T>(T @object, IAssetPath pathGetter, string name = null) where T : Object
        {
            if (string.IsNullOrEmpty(name))
                name = @object.name;

            var path = string.Format("{0}/{1}{2}", pathGetter.GetFolder(), name, ".prefab");

            if (@object is Component component)
            {
                var go = PrefabUtility.SaveAsPrefabAsset(component.gameObject, path, out var success);
                Assert.IsTrue(success);
#pragma warning disable UNT0014 // Invalid type for call to GetComponent
                return go.GetComponent<T>();
#pragma warning restore UNT0014 // Invalid type for call to GetComponent
            }
            else if (@object is GameObject gameObject)
            {
                var go = PrefabUtility.SaveAsPrefabAsset(gameObject, path, out var success);
                Assert.IsTrue(success);
                return go as T;
            }
            else
                throw new NotImplementedException();
        }


        public static T Archive<T>(T @object, string name, bool folderout, string path,bool overwrite=false) where T : UnityEngine.Object
        {

            Debug.Log($"Name : {name} Path : {path}");

            if (string.IsNullOrEmpty(path) || folderout)
                path = EditorHelper.ChoseFolder(path);

            Debug.Log(path);

            return EditorHelper.WriteAsset(@object, name, path,overwrite);
        }

        #endregion

        #region Duplicate

        /// <summary>
        /// Duplicate asset.
        /// </summary>
        /// <returns>Return true if successfully copy asset.</returns>
        public static bool Duplicate<T>(this T asset, out T newAsset, GameObject go = null) where T : Object
        {
            GameObject gameObj()
            {
                if (go == null)
                {
                    if (asset is Component component)
                        go = component.gameObject;
                }

                return go;
            }

            var valid = asset.GetType().Instance(out var obj, gameObj);

            if (valid && obj is T copyAsset)
            {
                EditorUtility.CopySerialized(asset, copyAsset);
                newAsset = copyAsset;
                return true;
            }
            else
            {
                newAsset = null;
                Debug.LogError("Duplicate asset failure : " + (obj.IsNull() ? "Null" : obj));
                return false;
            }
        }

        #endregion

        #region Cut
        const string cutGUID = "CutGUID";

        [MenuItem("Assets/Cut", true)]
        static bool CheckCutAsset()
        {
            foreach (var selected in Selection.objects)
            {
                if (selected == null)
                    return false;

                if (!AssetDatabase.IsMainAsset(selected))
                    return false;
            }

            return true;
        }

        static Object[] sp_CutAssets;

        [MenuItem("Assets/Cut", priority = 1000)]
        static void CacheAsset()
        {
            var length = Selection.objects.Length;
            var str = new Queue<string>(length);

            foreach (var selected in Selection.objects)
            {
                var guid = selected.GetGUID();
                str.Enqueue(guid);
            }

            EditorGUIUtility.systemCopyBuffer = cutGUID + string.Join("$", str);
        }


        [MenuItem("Assets/Paste", true)]
        static bool CheckPasteAsset()
        {
            var str=EditorGUIUtility.systemCopyBuffer;
            if (!str.StartsWith(cutGUID))
                return false;

            var folder = Selection.activeObject;

            if (folder == null)
                return false;

            return true;
        }

        [MenuItem("Assets/Paste", priority = 1000)]
        static void PasteAsset()
        {
            Debug.LogFormat("Paste target folder {0}", Selection.activeObject);

            var targetAssetPath = AssetDatabase.GetAssetPath(Selection.activeObject);

            var newFolder = 
                AssetDatabase.IsValidFolder(targetAssetPath)?
                EditorUtitily.GetEditorPath(Selection.activeObject):
                EditorUtitily.GetEditorFolder(Selection.activeObject);


            if (string.IsNullOrEmpty(newFolder))
                return;

            Debug.LogFormat("Move assets to new folder : {0}", newFolder);

            var str = EditorGUIUtility.systemCopyBuffer;
            if (!str.StartsWith(cutGUID))
                return;


            var paths = str.Remove(0, cutGUID.Length).
                Split("$").
                Select(x => AssetDatabase.GUIDToAssetPath(x));

            AssetDatabase.StartAssetEditing();

            foreach (var path in paths)
            {
                try
                {
                    var asset = AssetDatabase.LoadAssetAtPath<Object>(path);

                    if (asset.IsNull())
                    {
                        Debug.LogErrorFormat("Copy assetsPath can't matchPackages asset to move : {0}.", path);
                        continue;
                    }

                    var extension = EditorPathUtility.GetExtension(asset);
                    var newPath = newFolder + "/" + asset.name;//.Replace(".asset",null);// + extension;

                    if (!AssetDatabase.IsValidFolder(path))
                        newPath += extension;


                    Debug.Log($"Move asset {asset.name} to new path : {newPath}");
                    AssetDatabase.MoveAsset(path, newPath);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            EditorGUIUtility.systemCopyBuffer=null;

            AssetDatabase.StopAssetEditing();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        #endregion

        #region Replace

        public static void Replace<T>(this T oldTextAsset, T newTextAsset) where T : Object
        {
            var name = oldTextAsset.name;
            var path = AssetDatabase.GetAssetPath(oldTextAsset);
            var folder = EditorPathUtility.GetAssetFolderPath(oldTextAsset);
            AssetDatabase.DeleteAsset(path);

            Archive(newTextAsset, name, false, folder);
        }

        #endregion

        #region Extract

        /// <summary>
        /// Remove missing script root and extract assets
        /// </summary>
        [MenuItem("Assets/ExtractNull", priority = 1000)]
        static void ExtractNull()
        {
            var rootID = Selection.activeInstanceID;

            Debug.LogFormat("Selection instanceID {0}", rootID);

            var path = AssetDatabase.GetAssetPath(rootID);
            Debug.Log($"Path : {path}");

            var folder = EditorPathUtility.GetAssetFolderPath(path);

            var parent = EditorUtility.InstanceIDToObject(rootID);

            if (parent.NotNull())
            {
                Debug.LogFormat("Main asset {0}", parent);
                Debug.LogFormat("Main asset  type {0}", parent.GetType());
            }

            if (!AssetDatabase.IsValidFolder(folder + "/Convex"))
                folder = AssetDatabase.CreateFolder(folder, "Convex");
            else
                folder += "/Convex";

            ExtractSubAssets(path, null, folder);


            AssetDatabase.DeleteAsset(path);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }



        /// <summary>
        /// Remove missing script root and extract assets
        /// </summary>
        [MenuItem("Assets/SubAsset/ExtractSubAsset", priority = 1000)]
        static void ExtractSubAsset()
        {
             var subAssetID = Selection.activeInstanceID;


            var path = AssetDatabase.GetAssetPath(subAssetID);


            var depends=AssetDatabase.GetDependencies(path);

            Debug.Log("Depend assets : \n"+string.Join("\n", depends));

            string guid= GetGUID(subAssetID);

            Object asset;

            if (ObjectIdentifier.TryGetObjectIdentifier(subAssetID, out var id))
            {
                asset = ObjectIdentifier.ToObject(id);
            }
            else
            {
                Debug.LogError("Failure to found asset");
                asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            }


            Debug.Log($"Path : {path} AssetGUID : {AssetDatabase.GUIDFromAssetPath(path)} InstanceGUID : {guid}");


            AssetDatabase.RemoveObjectFromAsset(asset);

            path = AssetDatabase.GenerateUniqueAssetPath(path);
 
            AssetDatabase.CreateAsset(asset, path);
          
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(Log(asset));

            var folder = EditorUtility.OpenFolderPanel("Select Folder", "Chose folder to overwrite guid of assets.", null);

            if (string.IsNullOrEmpty(folder))
                return;

            var valid =ObjectIdentifier.TryGetObjectIdentifier(subAssetID, out var newID) ;
            Assert.IsTrue(valid);


            Dictionary<string, Dictionary<string, ObjectIdentifier>> dic = new();
            var binding = new Dictionary<string, ObjectIdentifier>();
            binding.Add(id.localIdentifierInFile.ToString(), newID);

            dic.Add(id.guid.ToString(), binding);


            ReplaceGUIDs(dic, folder);
        }




        public static Object[] ExtractSubAssets(string assetsPath,Func<string,string> namingFunc = null, string targetFolder = null)
        {
            if (string.IsNullOrEmpty(targetFolder))
                targetFolder = EditorPathUtility.GetAssetFolderPath(assetsPath);

            Debug.Log($"Target folder : {targetFolder}");

            var name = EditorPathUtility.GetAssetName(assetsPath);
            
            if(namingFunc!=null)
                name = namingFunc(name);

            var assets = AssetDatabase.LoadAllAssetsAtPath(assetsPath);


            Debug.Log($"Load {assets.Length} assets.");


            var multi = assets.Length > 2;
            var sn = 0;

            foreach (var asset in assets)
            {
                //Debug.Log($"Asset {(asset == null ? "is" : "not")} null.");

                if (asset == null)
                    continue;

                //Debug.LogFormat("Subasset instanceID {0}", asset.GetInstanceID());

                //Debug.Log($"Sub asset assetsPath : {AssetDatabase.GetAssetPath(asset.GetInstanceID())}.");

                var sub = EditorUtilityTools.IsSubAsset(asset);

                Debug.Log($"{asset.name} is sub asset : {sub}");

                if (!sub)
                    continue;

                AssetDatabase.RemoveObjectFromAsset(asset);

                //Debug.Log(asset);

                var extension = EditorPathUtility.GetExtension(asset);

                Debug.Log(extension);

                var newPath = $"{targetFolder}/{name}";

                if (multi)
                    newPath += $"_{sn}{extension}";
                else
                    newPath += $"{extension}";

                newPath = AssetDatabase.GenerateUniqueAssetPath(newPath);
                //Debug.Log($"New assetsPath {targetAssetPath}.");
                AssetDatabase.CreateAsset(asset, newPath);

                //Debug.Log(asset);
                //Debug.Log(AssetDatabase.IsMainAsset(asset));

                sn++;
                continue;
            }

            return assets.Where(x => x != null).ToArray();
        }


        /// <summary>
        /// Extract sub assets. 
        /// </summary>
        /// <param name="assets">Target to extract.</param>
        /// <param name="targetFolder">Destination folder.</param>
        /// <param name="bindings">Guid bindings to replace.</param>
        /// <param name="naming">Rename sub asset function.</param>
        /// <returns></returns>
        public static void ExtractSubAssets(Object[] assets, string targetFolder, out IEnumerable<KeyValuePair<ObjectIdentifier, ObjectIdentifier>> bindings,Func<Object,string> naming=null)
        {
            bool customFolder = string.IsNullOrEmpty(targetFolder) && AssetDatabase.IsValidFolder(targetFolder);

            Debug.Log($"Extract sub assets to : {targetFolder}");

            var length = assets.Length;

            var assetbindings = new Queue<KeyValuePair<ObjectIdentifier, ObjectIdentifier>>(length);

            for (int i = 0; i < length; i++)
            {
                try
                {
                    var asset = assets[i];

                    if (asset == null)
                        continue;

                    //Debug.LogFormat("Subasset instanceID {0}", asset.GetInstanceID());

                    //Debug.Log($"Sub asset assetsPath : {AssetDatabase.GetAssetPath(asset.GetInstanceID())}.");

                    // valid
                    {
                        var sub = EditorUtilityTools.IsSubAsset(asset);

                        if (!sub)
                        {
                            Debug.LogError($"{asset.name} is not a sub asset : {asset}");
                            continue;
                        }
                    }

                    if (!ObjectIdentifier.TryGetObjectIdentifier(asset, out var oldID))
                    {
                        Debug.LogException(new InvalidDataException($"Failure to get identifier from {asset}"));
                        continue;
                    }



                    string targetAssetPath;

                    // 
                    {
                        var name = naming?.Invoke(asset);

                        if (string.IsNullOrEmpty(name))
                            name = EditorPathUtility.GetRootAssetName(asset);

                        if (Path.HasExtension(name))
                            name = Path.GetFileNameWithoutExtension(name);

                        name += EditorPathUtility.GetExtension(asset);

                        targetAssetPath = Path.Combine(targetFolder, name);// $"{targetFolder}/{name}";
                        targetAssetPath = AssetDatabase.GenerateUniqueAssetPath(targetAssetPath);

                        //Debug.Log($"New assetsPath {targetAssetPath}.");
                    }

                    // split asset
                    AssetDatabase.RemoveObjectFromAsset(asset);

                    AssetDatabase.CreateAsset(asset, targetAssetPath);

                    var valid = ObjectIdentifier.TryGetObjectIdentifier(asset, out var newID);

                    Assert.IsTrue(valid, "Sub asset should be able to identifi.");


                    assetbindings.Enqueue(new(oldID, newID));
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    continue;
                }
            }

            bindings = assetbindings;
        }


        #endregion

        #region Config

        public static void SetCustomIconOnGameObject<T>(T obj, Texture2D iconResource) where T : Object
        {
            //var go = new GameObject();
            //var icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/MyIcon.png");

            EditorGUIUtility.SetIconForObject(obj, iconResource);
        }

        #endregion

        #region Sub Asset

        [MenuItem("Assets/SubAsset/DeleteSubAsset", true)]
        static bool CheckDeleteSubAsset()
        {
            var ob = Selection.activeObject;

            if (ob.IsNull())
                return false;

            return AssetDatabase.IsSubAsset(ob);
        }

        [MenuItem("Assets/SubAsset/DeleteSubAsset", priority = 1001)]
        static void Delete()
        {
            //var ob = Selection.activeObject;
            //DeleteSubAsset(ob);
            var obs = Selection.objects.ToArray();

            foreach (var obj in obs)
            {
                if (AssetDatabase.IsSubAsset(obj))
                    DeleteSubAsset(obj,false,false);
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Delete sub asset.
        /// </summary>
        /// <param name="alert">Set true to display confirm info message before delete asset.</param>
        public static void DeleteSubAsset(Object obj, bool alert = true, bool refresh = true)
        {
            if (obj.IsNull())
            {
                Debug.LogError("Value should not be null");
                return;
            }

            if (!AssetDatabase.IsSubAsset(obj))
            {
                Debug.LogWarningFormat("Delete target is not sub asset : {0}", obj.name);
                return;
            }

            if (alert)
            {
                if (!EditorUtility.DisplayDialog("Confirm", string.Format("Delete subAsset {0} ?", obj), "Yes", "No"))
                    return;
            }


            AssetDatabase.RemoveObjectFromAsset(obj);

            Object.DestroyImmediate(obj, true);


            if (refresh)
                AssetDatabase.Refresh();
        }

        /// <summary>
        /// Whether mono script is child of target type.
        /// </summary>
        public static bool IsChild(this MonoScript mono, params Type[] types)
        {
            var monoType = mono.GetClass();

            foreach (var type in types)
            {
                if (monoType.IsSubclassOf(type))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Add object under mian asset.
        /// </summary>
        /// <param name="asset">Main asset to contain sub-assets.</param>
        /// <param name="object">Sub asset.</param>
        /// <param name="name">Name of sub asset.</param>
        /// <returns></returns>
        public static T AddSubAsset<T>(this UnityEngine.Object asset, T @object, string name = null) where T : UnityEngine.Object
        {
            Assert.IsTrue(AssetDatabase.IsMainAsset(asset), asset + " must be as asset.");
            Assert.IsFalse(@object is Component || @object is GameObject);

            if (!string.IsNullOrEmpty(name))
            {
                if (AssetDatabase.IsMainAsset(@object))
                    AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(@object), name);
                else
                    @object.name = name;
            }

            AssetDatabase.AddObjectToAsset(@object, asset);

            return @object;
        }


        public static IEnumerable<T> GetSubObjectsOfType<T>(Object asset) where T : Object
        {
            Object[] objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(asset));

            foreach (Object o in objs)
                if (o is T value)
                    yield return value;

            yield break;
        }

        #endregion
    }
}
#endif