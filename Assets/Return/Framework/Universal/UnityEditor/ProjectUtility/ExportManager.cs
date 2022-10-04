using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using System.Text;
using System.Linq;
using System;
using Object = UnityEngine.Object;
using UnityEngine.Assertions;


namespace Return.Editors
{
    [Flags]
    public enum ExportProjectOption
    {
        None = 0,
        Tags = 1 << 0,
        Layers = 1 << 1,
        Inputs = 1 << 2,
        Graphics = 1<<3,
        Packages =1<<4,


        All = Tags | Layers | Inputs | Graphics | Packages
    }


    public class ExportManager : OdinEditorWindow
    {
        [MenuItem("Assets/ExportManager")]
        static void OpenWindow()
        {
            var window = GetWindow<ExportManager>();

            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(700, 700);

            window.LoadConfigs();
        }

        #region Package Option

        [PropertySpace(3, 3)]
        [BoxGroup("Package Config")]
        public ExportPackageOptions packageOption = ExportPackageOptions.Interactive | ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies | ExportPackageOptions.IncludeLibraryAssets;

        [OnValueChanged(nameof(InitProjectOption))]
        [PropertySpace(3, 3)]
        [BoxGroup("Package Config")]
        [ShowInInspector]
        ExportProjectOption projectOption = ExportProjectOption.None;

        void InitProjectOption()
        {
            if (projectOption.HasFlag(ExportProjectOption.Packages))
                LoadPackageInstaller();
        }


        void LoadPackageInstaller()
        {
            PackageInstaller installer;
            if (PackageContents.Count < 100)
            {
                var cache = PackageContents.FirstOrDefault(x => x is PackageInstaller);
                if (cache!=null && cache.Parse(out installer))
                    if (PackageContents.Contains(installer.Mainfest))
                        return;
            }


            installer=PackageInstaller.BuildInstaller();
            
            var assets=EditorUtility.CollectDependencies(new[] { installer });

            foreach (var asset in assets)
                AddTarget(asset);

            var depends = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(installer), true);
            foreach (var depend in depends)
                AddTarget(AssetDatabase.LoadAssetAtPath(depend,typeof(Object)));

            

            AddTarget(installer);
            AddTarget(installer.Mainfest);
        }


        [PropertyOrder(3)]
        [BoxGroup("Package Config")]
        [ShowInInspector]
        FolderPathFinder FilePath = new(nameof(ExportManager));

        [PropertyOrder(4)]
        [PropertySpace(3, 3)]
        [BoxGroup("Package Config")]
        [SerializeField]
        string FileName;

        #endregion


        #region Preference
        [HideInInspector]
        [SerializeField]
        List<ExportBundle> preferences;

        [HideInInspector]
        [SerializeField]
        string[] config_buts;

        int index;

        string GetPrefPath()
        {
            var mono = MonoScript.FromScriptableObject(this);
            Assert.IsNotNull(mono);
            var path = EditorPathUtility.GetAssetFolderPath(mono);
            Assert.IsFalse(string.IsNullOrEmpty(path));

            return path;
        }

        void LoadConfigs()
        {
            var path = GetPrefPath();

            Debug.Log(path);



            var configs = EditorAssetsUtility.GetAssetsAtPath<TextAsset>(path);//.                Where(x=>x is not MonoScript).ToArray();//AssetDatabase.LoadAllAssetRepresentationsAtPath(path).Where(x=> x!=null);//.Where(x => EditorPathUtility.GetExtension(x) == ".json");

            //Debug.Log(AssetDatabase.LoadAllAssetRepresentationsAtPath(path).Length);

            Debug.Log(configs.Count());

            var length = configs.Length;

            preferences = new(length);
            config_buts = new string[length+1];
            var strCache = new Queue<string>(length + 1);
            strCache.Enqueue("Null");

            for (int i = 0; i < length; i++)
            {
                var txt = configs[i];

                if (txt is MonoScript)
                    continue;

                var pref = ExportBundle.LoadJson(txt.text);

                if (pref == null)
                    continue;

                preferences.Add(pref);
                strCache.Enqueue(pref.Name);
            }

            config_buts = strCache.ToArray();

            Assert.IsTrue(config_buts.Length-1 == preferences.Count);
        }

        void AddPref(Object obj)
        {
            if (obj is not TextAsset txt)
            {
                Debug.LogError("Not textAsset.");
                return;
            }

            if (txt is MonoScript)
                return;

            var pref = ExportBundle.LoadJson(txt.text);

            if (pref == null)
            {
                Debug.LogError("Data resolve null "+txt.text);
                return;
            }

            if (preferences.CheckAdd(pref))
            {
                config_buts = config_buts.Append(pref.Name).ToArray();
                index = config_buts.IndexOf(pref.Name);

                LoadPref(pref);
            }
        }


        void LoadPref(ExportBundle pref)
        {
            packageOption = pref.PackageOption;
            projectOption = pref.ProjectOption;

            FilePath.Path = pref.FilePath;
            FileName = pref.FileName;

            var objs = pref.GUIDs.Select(x => EditorAssetsUtility.GetAsset(x)).Where(x => x != null);

            foreach (var obj in objs)
                AddTarget(obj);
        }


        ExportBundle SavePref()
        {

            var path = GetPrefPath();

            path = EditorUtility.SaveFilePanel(
                 "Save preference",
                 EditorPathUtility.GetIOPath(path),
                 "PackagePreference",
                 "json"
                 );

            Debug.Log(path);

            path = EditorPathUtility.GetEditorPath(path);

            Debug.Log(path);

            var name = EditorPathUtility.GetAssetName(path);
            path = EditorPathUtility.GetAssetFolderPath(path);

            var pref = new ExportBundle()
            {
                Name=name,
                PackageOption = packageOption,
                ProjectOption = projectOption,

                FilePath = FilePath,
                FileName = FileName,

                GUIDs = PackageContents.Select(x => EditorAssetsUtility.GetGUID(x)).ToArray()
            };


            var json = EditorJsonUtility.ToJson(pref, true);

            var text = new TextAsset(ExportBundle.confirm+json);


            EditorAssetsUtility.Archive(text, name,false,path,true);

            return pref;
        }

        [PropertySpace(5,5)]
        [OnInspectorGUI]
        void DrawConfig()
        {
            GUILayout.BeginHorizontal();

            if (preferences == null)
            {
                GUILayout.Label("Null preset package bundle",GUILayout.ExpandWidth(true));
            }
            else
            {
                var selected = EditorGUILayout.Popup("Config", index, config_buts,GUILayout.ExpandWidth(true));

                if (selected != index)
                {
                    index = selected;

                    if(selected!=0)
                    {
                        //load preference
                        var config = preferences[selected - 1];
                        LoadPref(config);
                    }
                }
            }

            if(GUILayout.Button("Add Preference", GUILayout.Width(120)))
            {
                try
                {
                    var mono = MonoScript.FromScriptableObject(this);
                    var path = EditorUtility.OpenFilePanel("Add Preference", EditorPathUtility.GetIOPath(mono), "*");

                    Debug.Log($"File path : {path}");

                    if (string.IsNullOrEmpty(path))
                        return;

                    path = EditorPathUtility.GetEditorPath(path);
                    var pref = AssetDatabase.LoadAssetAtPath(path, typeof(Object));

                    AddPref(pref);


                    Debug.Log($"File path : {path}");
                }
                finally
                {
                    GUIUtility.ExitGUI();
                }
            }

            if (GUILayout.Button("Save Preference", GUILayout.Width(120)))
            {
                try
                {
                    var pref = SavePref();
                    LoadConfigs();
                }
                finally
                {
                    GUIUtility.ExitGUI();
                }
    
            }


            GUILayout.EndHorizontal();
        }

        #endregion


        #region Operate

        [PropertyOrder(5)]
        [PropertySpace(3, 3)]
        [BoxGroup("Package Config")]
        [Button("ExportPackage",ButtonHeight =41)]
        public void Export()
        {
            var exportedPackageAssetList = new List<string>();
            var length = PackageContents.Count;

            Dictionary<Object, string> d = default;

            var options= mFlagExtension.GetFlagsValue<ExportProjectOption>((int)projectOption);
            foreach (ExportProjectOption option in options)
            {
                var binding = GetBinding(option);

                Debug.Log($"Project option {option} binding {binding}");

                if (string.IsNullOrEmpty(binding))
                    continue;

                exportedPackageAssetList.Add(binding);
            }

            //if (projectOption.HasFlag(ExportProjectOption.Tags))
            //    exportedPackageAssetList.Add("ProjectSettings/TagManager.asset");

            //if (projectOption.HasFlag(ExportProjectOption.Layers))
            //    exportedPackageAssetList.Add("ProjectSettings/ProjectSettings.asset");

            //if (projectOption.HasFlag(ExportProjectOption.Inputs))
            //    exportedPackageAssetList.Add("ProjectSettings/InputManager.asset");



            //if (CombineFolder)
            //{
            //    d = PackageContents.ToDictionary(x => x, x => AssetDatabase.GetAssetPath(x));

            //    if (!AssetDatabase.IsValidFolder(CombineFolderPath))
            //    {
            //        //var folders = CombineFolderPath.Split('/');
            //        EditorUtilityTools.DisplayDialog("Error", CombineFolderPath + " is not vaild folder","Okay");
            //        return;
            //    }

            //    foreach (var target in d)
            //    {
            //        AssetDatabase.MoveAsset(target.Value, CombineFolderPath+"/"+target.Key.name+".asset");
            //    }
            //    exportedPackageAssetList.Add(CombineFolderPath);
            //    //AssetDatabase.Refresh();
            //}
            //else
            {
                for (int i = 0; i < length; i++)
                {
                    //EditorUtilityTools.DisplayProgressBar("ExportPackage", string.Format("Packing {0}", PackageContents[i].name), (float)i / length);
                    var guid = AssetDatabase.GetAssetPath(PackageContents[i]);
                    exportedPackageAssetList.Add(guid);
                }

            }


            //EditorUtilityTools.ClearProgressBar();

            //if(projectOption)
            //    AssetDatabase.ExportPackage(exportedPackageAssetList.ToArray(), PlayerSettings.productName + ".unitypackage", packageOption);
            //else

            AssetDatabase.ExportPackage(
                exportedPackageAssetList.ToArray(),
                FilePath + "/" + FileName + ".unitypackage",
                packageOption
                );


            //if (CombineFolder)
            //{
            //    var obs = AssetDatabase.LoadAllAssetsAtPath(CombineFolderPath);
            //    foreach (var item in obs)
            //    {
            //        if(d.TryGetValue(item,out var oldPath))
            //        {
            //            var newPath = AssetDatabase.GetAssetPath(item);
            //            AssetDatabase.MoveAsset(newPath, oldPath);
            //        }
            //        else
            //        {
            //            Debug.LogError(item+" missing old path !");
            //        }
            //    }
            //}

            AssetDatabase.Refresh();
        }

        #endregion

        /// <summary>
        /// Return project output binding.
        /// </summary>
        public static string GetBinding(ExportProjectOption option)
        {
            return option switch
            {
                ExportProjectOption.None => null,
                ExportProjectOption.All => null,
                ExportProjectOption.Tags => "ProjectSettings/TagManager.asset",
                ExportProjectOption.Layers => "ProjectSettings/ProjectSettings.asset",
                ExportProjectOption.Inputs => "ProjectSettings/InputManager.asset",
                ExportProjectOption.Graphics => "GraphicsSettings.asset",
                ExportProjectOption.Packages => "Packages/packages-lock.json",

                _ => throw new NotImplementedException(option.ToString()),
            };
        }

     

        #region Package Content Selection

        [PropertySpace(10, 10)]
        [OnValueChanged(nameof(LoadDropZone))]
        [DropZone(Text ="Drop Content Here")]
        [SerializeField]
        Object NewTarget;

        void LoadDropZone()
        {
            if (NewTarget == null)
                return;

            if (Selection.objects != null && Selection.objects.Length > 1)
            {
                //Debug.LogFormat("Drop selection count : {0}", Selection.objects.Length);

                foreach (var obj in Selection.objects)
                {
                    AddTarget(obj);
                }
            }
            else
            {
                AddTarget(NewTarget);
            }


            NewTarget = null;
        }

        /// <summary>
        /// Push asset as package content.
        /// </summary>
        void AddTarget(Object obj)
        {
            if (obj == null)
                return;

            if (string.IsNullOrEmpty(FileName))
            {
                if (EditorPathUtility.IsValidFolder(obj))
                    FileName = obj.name;
                else
                    FileName = EditorPathUtility.GetAssetFolderName(obj);
            }


            PackageContents.CheckAdd(obj);
        }

        [ListDrawerSettings(CustomAddFunction =(nameof(AddTarget)))]
        public List<Object> PackageContents;

        #endregion
    }

    /// <summary>
    /// Archive export config as preference.
    /// </summary>
    [Serializable]
    public class ExportBundle
    {
        public const string confirm = "$ExportBundle";

        public string Name;

        public ExportPackageOptions PackageOption;
        public ExportProjectOption ProjectOption;

        public string FilePath;
        public string FileName;

        public string[] GUIDs;

        public static ExportBundle LoadJson(string json)
        {
            Debug.Log($"Contains " + json.Contains(nameof(PackageOption)));

            if (json.StartsWith(confirm))
                json = json[confirm.Length..];
            else if (!json.Contains(nameof(PackageOption)))
                return null;


            //Debug.Log($"name : {config.name} type : {config.GetType()}");
            var bundle = new ExportBundle();

            try
            {
                EditorJsonUtility.FromJsonOverwrite(json, bundle);
            }
            catch (Exception e)
            {
                Debug.Log(json);
                Debug.LogException(e);
                return null;
            }


            if (string.IsNullOrEmpty(bundle.Name))
            {
                if(bundle.GUIDs==null || bundle.GUIDs.Length==0)
                    return null;

                bundle.Name = "Unknow";
            }

            return bundle;
        }

    }
}