using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;
using System.IO;
using UnityEditor.Callbacks;
using Sirenix.Utilities.Editor;
using UnityEditor.PackageManager;
using System.Linq;
using System;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using Return;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
using System.Threading.Tasks;

namespace Return.Editors
{
    public partial class PackageInstaller : ScriptableObject
    {
        //public static event Action<PackageConfig> OnPackageAdd;
        //public static event Action<PackageConfig> OnPackageRemove;

        #region ProjectUtil

        /// <summary>
        /// Use to valid different project then invoke install function.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        string ProjectID;

        [SerializeField]
        bool DisplayOnStart;

        

        public void Awake()
        {
            if (!DisplayOnStart)
                return;

            Debug.Log("Valid installer");

            var id= EditorPathUtility.GetProjectName();
            if (ProjectID == id)
                DisplayOnStart = false;
            else
                ProjectID = id;

            // show load package msg

            EditorGUIUtility.PingObject(this);
            LoadPackageInfos();
        }

        void RefreshProjectID()
        {
            DisplayOnStart = true;
            ProjectID = EditorPathUtility.GetProjectName();
            EditorUtility.SetDirty(this);
        }

        [MenuItem("Project/PackageInstaller")]
        public static PackageInstaller BuildInstaller()
        {
            if (EditorAssetsUtility.TryGetAsset<PackageInstaller>(out var installer))
            {
                if(installer.Mainfest==null)
                {
                    installer.LoadPackageInfos();
                    installer.SavePackageInfos();
                }

                installer.RefreshProjectID();
                return installer;
            }


            // create new installer asset.

            UnityEngine.Object assetFolder;

            // get script asset path.
            if (EditorAssetsUtility.TryGetScriptAsset<PackageInstaller>(out var mono))
                assetFolder = mono;
            else
            {
                Debug.LogError($"Failure to find mono script asset of {nameof(PackageInstaller)}");
                assetFolder = AssetDatabase.LoadAssetAtPath("ProjectSettings", typeof(Object));
            }

            installer = ScriptableObject.CreateInstance<PackageInstaller>();
            var path = EditorPathUtility.GetAssetFolderPath(assetFolder);

            installer = EditorAssetsUtility.Archive(installer, nameof(PackageInstaller), false, path);

            installer.SavePackageInfos();
            installer.RefreshProjectID();

            return installer;
        }

        #endregion

        [Tooltip("Show debug logs.")]
        [SerializeField]
        bool Log;

        [SerializeField]
        TextAsset m_packageInfo;
        public TextAsset Mainfest => m_packageInfo;

        [PropertySpace(7, 7)]
        [ListDrawerSettings(Expanded = true)]
        [ShowInInspector]
        HashSet<PackageSource> m_filter = new() { PackageSource.Git, PackageSource.Registry };

        [PropertySpace(SpaceBefore = 5, SpaceAfter = 5)]
        [HorizontalGroup("packageOption")]
        [PropertyTooltip("Load package info from project or data.")]
        [Button(ButtonSizes.Gigantic, ButtonStyle.Box)]
        public void LoadPackageInfos()
        {
            // init from project
            if (m_packageInfo == null)
            {
                IEnumerable<PackageInfo> infos = PackageInfo.GetAllRegisteredPackages();

                if (Log)
                    Debug.Log($"Found {infos.Count()} packages from project.");

                // filter
                if (m_filter.NotNull() && m_filter.Count > 0)
                    infos = infos.TakeWhile(x => m_filter.Contains(x.source));

                // cast
                Infos = infos.
                    Select(x => (PackageConfig)x).
                    ToHashSet();
            }
            else // load data
            {
                Infos = JsonUtil.getJsonArray<PackageConfig>(m_packageInfo.text).ToHashSet();
            }

            if (Log)
                Debug.Log($"Cache {Infos.Count} packages to list.");

            AnalyzePackages();
        }

        [PropertySpace(SpaceBefore = 5, SpaceAfter = 5)]
        [HorizontalGroup("packageOption")]
        [EnableIf(nameof(hasData))]
        [PropertyTooltip("Save package info as text data.")]
        [Button(ButtonSizes.Gigantic, ButtonStyle.Box)]
        public void SavePackageInfos()
        {
            if (Infos == null)
                return;

            var data = Infos.ToArray();

            if (Log)
                Debug.Log($"Archive {data.Length} packages to file.");

            try
            {
                var json = JsonUtil.arrayToJson(data, true);
                var asset = new TextAsset(json);

                if (m_packageInfo.NotNull())
                {
                    m_packageInfo.Replace(asset);
                    m_packageInfo = asset;
                }
                else
                {
                    m_packageInfo = EditorAssetsUtility.Archive(asset, "mainfest.txt", false, EditorPathUtility.GetAssetFolderPath(this));
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorUtility.ClearProgressBar();
            }


            EditorUtility.SetDirty(this);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        #region Install

        [PropertySpace(SpaceBefore = 10, SpaceAfter = 10)]
        [Title("Packages", TitleAlignment = TitleAlignments.Centered)]
        [EnableIf(nameof(hasData))]
        //[HorizontalGroup("packageOption")]
        [Button(ButtonSizes.Gigantic, ButtonStyle.Box)]
        public async void InstallAllPackages()
        {
            if (!AnalyzePackages())
                return;

            var msgs = 
                states.Select(
                x =>
                {
                    var state = x.Value.Installed ? x.Value.Lastest ? "Reinstall" : "Update" : "Install";

                    return $"{state} {x.Key.DisplayName}-{x.Key.Version}";
                });

            var msg ="Confirm the fallowing packages changes : \n"+
                string.Join("\n", msgs);

            if (!EditorUtility.DisplayDialog("Confirm Packages", msg, "Yes", "No"))
                return;

            var valid = true;

            foreach (var state in states)
            {
                if (state.Value.Installed)
                {
                    valid &= await Install(state.Key, state.Key,false);
                }
                else
                {
                    valid &= await Install(state.Key,null,false);
                }

                //try
                //{
     
                //}
                //catch (Exception e)
                //{
                //    Debug.LogException(e);
                //    valid = false;
                //    continue;
                //}
            }

            if (valid)
                DisplayOnStart = false;
        }

        public async Task<bool> Install(PackageConfig install, PackageConfig unistall = null,bool userCheck=true)
        {
            if (userCheck)
            {
                if (unistall != null)
                {
                    var lastest = install.CompareTo(unistall);

                    var title = "Package Confirm";
                    string msg;

                    if (lastest>0)
                        msg = $"Down grade {unistall.DisplayName} from {unistall.Version} to {install.Version} ?";
                    else if(lastest < 0)
                        msg = $"Up grade {unistall.DisplayName} from {unistall.Version} to {install.Version} ?";
                    else
                        msg = $"Reinstall {unistall.DisplayName} - {install.Version} ?";

                    if (!EditorUtility.DisplayDialog(title,msg,"Yes","No"))
                        return false;
                }
            }

            try
            {
                await install.Install(unistall);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }

            if(states.TryGetValue(install,out var state))
            {
                state.Installed = true;
                state.Lastest = true;
            }

            return true;
        }

        public async Task<bool> Uninstall(PackageConfig unistall,bool userCheck=true)
        {
            if (userCheck)
            {
                if (unistall != null)
                {
                    var title = "Package Confirm";
                    string msg = $"Uninstall {unistall.DisplayName} - {unistall.Version}?";

                    if (!EditorUtility.DisplayDialog(title, msg, "Yes", "No"))
                        return false;
                }
            }

            try
            {
                await unistall.Uninstall();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }

            if (states.TryGetValue(unistall, out var state))
            {
                state.Installed = false;
                state.Lastest = false;
            }

            return true;
        }


        #endregion

        #region EditPackages

        [HideInInspector]
        HashSet<PackageInfo> allPackages;

        HashSet<PackageInfo> matchPackages;

        HashSet<PackageInfo> InList;

        string index;

        void InitSkin()
        {
            if(labelStyle==null)
                labelStyle = GUI.skin.FindStyle("PreviewPackageInUse");

            if (buttonOptions == null)
                buttonOptions = new GUILayoutOption[] { GUILayout.ExpandHeight(true), GUILayout.Width(80) };
        }
        
        [OnInspectorGUI]
        void SearchPackages()
        {
            InitSkin();

            EditorGUI.BeginChangeCheck();
            index =EditorGUILayout.TextField(index);
            if(EditorGUI.EndChangeCheck())
            {
                if (Infos == null)
                    LoadPackageInfos();

                if (allPackages == null)
                    allPackages = PackageInfo.GetAllRegisteredPackages().ToHashSet();

                InList = new();

                if (string.IsNullOrEmpty(index))
                    matchPackages = new();
                else
                {
                    matchPackages = allPackages.
                            Where(x =>
                            {
                                var mathch =
                                x.name.Contains(index, StringComparison.CurrentCultureIgnoreCase) ||
                                x.packageId.Contains(index, StringComparison.CurrentCultureIgnoreCase) ||
                                x.displayName.Contains(index, StringComparison.CurrentCultureIgnoreCase);

                                //Debug.Log($"{index} do not match \n{x.name}\n{x.packageId}\n{x.displayName}");
                                return mathch;
                            }).
                            ToHashSet();

                    if(Log)
                        Debug.Log($"{index} match {matchPackages.Count} packages from {allPackages.Count} packages.");

                    foreach (var match in matchPackages)
                    {
                        foreach (var info in Infos)
                        {
                            if(info.DisplayName.Equals(match.displayName))
                            {
                                InList.Add(match);
                                break;
                            }
                        }
                    }
                }

                //Debug.Log("Search index change");
            }


            if(matchPackages!=null)
                foreach (var match in matchPackages)
                {
                    GUILayout.BeginHorizontal();

                    var str = $"{match.displayName} - {match.version}";
                    if (GUILayout.Button(str, labelStyle, GUILayout.ExpandWidth(true)))
                        EditorGUIUtility.systemCopyBuffer = match.packageId;

                    var exist = InList.Contains(match);

                    EditorGUI.BeginDisabledGroup(exist);

                    if (GUILayout.Button("AddToList",buttonOptions))
                    {
                        PackageConfig config = match;

                        if (Infos.Add(config))
                            states.Add(config, new PackageState() { Installed = true, Lastest = true });
                    }

                    EditorGUI.EndDisabledGroup();

                    GUILayout.EndHorizontal();
                }


        }



        [PropertyOrder(10)]
        [BoxGroup("Data", ShowLabel = false)]
        [ShowIf(nameof(Log))]
        [ShowInInspector]
        HashSet<PackageConfig> Infos;

        [PropertyOrder(11)]
        [BoxGroup("Data")]
        [ShowIf(nameof(Log))]
        [ShowInInspector]
        Dictionary<PackageConfig, PackageState> states;

        bool hasData => Infos != null && Infos.Count > 0;

        [NonSerialized]
        Vector2 scroll;

        GUIStyle labelStyle;
        [NonSerialized]
        GUILayoutOption[] buttonOptions;

        [PropertyOrder(12)]
        [BoxGroup("Data")]
        [ShowIf(nameof(hasData))]
        [OnInspectorGUI]
        void DrawPackages()
        {
            GUILayout.Space(10);

            if (GUILayout.Button("Clear"))
            {
                Infos.Clear();
                return;
            }

            scroll = GUILayout.BeginScrollView(scroll);

            InitSkin();

            var infos = Infos.OrderBy(x => x.DisplayName);

            foreach (var info in infos)
            {
                if (info == null)
                    return;

                GUILayout.BeginHorizontal();

                var str = $"{info.DisplayName} - {info.Version}";

                if (GUILayout.Button(str, labelStyle, GUILayout.ExpandWidth(true)))
                    EditorGUIUtility.systemCopyBuffer = info.ID;

                if (states==null || !states.TryGetValue(info, out var state))
                {
                    Debug.LogError($"Failure to find package state : {info.DisplayName}");
                    continue;
                }
 

                if (GUILayout.Button("unlist", buttonOptions))
                {
                    Infos.Remove(info);
                    states.Remove(info);
                    EditorGUIUtility.ExitGUI();
                    break;
                }

                if (state.Installed)
                {
                    if(state.Lastest)
                    {
                        if (GUILayout.Button("uninstall", buttonOptions))
                        {
                            Uninstall(info);
                            //Infos.Remove(info);
                            //EditorGUIUtility.ExitGUI();
                            //break;
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("update", buttonOptions))
                        {
                            Install(info,info);
                            //Infos.Remove(info);
                            //EditorGUIUtility.ExitGUI();
                            //break;
                        }
                    }
                }
                else
                {
                    if (GUILayout.Button("install", buttonOptions))
                    {
                        Install(info);
                        //Infos.Remove(info);
                        //EditorGUIUtility.ExitGUI();
                        //break;
                    }
                }

                GUILayout.EndHorizontal();

                GUILayout.Space(5);
            }

            GUILayout.EndScrollView();
        }

        public bool AnalyzePackages()
        {
            if (Infos == null)
                return false;

            if (states.IsNull())
                states = new(Infos.Count);
            else
                states.Clear();

            var curInfos =
                            PackageInfo.GetAllRegisteredPackages().
                            Select(x => (PackageConfig)x).ToHashSet();

            var valid = true;

            foreach (var info in Infos)
            {
                try
                {
                    var state = new PackageState();

                    if (curInfos.TryGetValue(info, out var installed))
                    {
                        // check version
                        state.Lastest = info.CompareTo(installed) <= 0; // lasted or none
                        state.Installed = true;
                    }
                    else
                    {
                        // install new package
                        state.Installed = false;
                    }

                    states.Add(info, state);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failur to install package {info.DisplayName}");
                    Debug.LogException(e);
                    valid = false;
                }
            }

            return valid;
        }

        #endregion

    }

    #region Container

    // config
    public partial class PackageInstaller
    {
        [Serializable]
        public class PackageConfig : IEquatable<PackageConfig>, IComparable<PackageConfig>
        {
            public string DisplayName;
            [HideInInspector]
            public string Name;
            public string ID;
            public string Version;

            public static int[] GetVersion(string str)
            {
                var strs = str.Split('.');
                var vers = new Queue<int>(strs.Length);

                foreach (var ver in strs)
                {
                    if (int.TryParse(ver, out var result))
                        vers.Enqueue(result);
                }

                return vers.ToArray();
            }

            public int CompareTo(PackageConfig other)
            {
                var ver = GetVersion(Version);
                var otherVersion = GetVersion(other.Version);

                var length = Math.Min(ver.Length, otherVersion.Length);

                for (int i = 0; i < length; i++)
                {
                    if (ver[i] > otherVersion[i])
                        return -1;
                    if (ver[i] < otherVersion[i])
                        return 1;
                }

                return 0;
            }

            public bool Equals(PackageConfig other)
            {
                //if (other == this)
                //    return true;

                if (other.Name != Name)
                    return false;

                return true;
            }

            public override int GetHashCode()
            {
                return Name.GetHashCode();
            }

            public static implicit operator PackageConfig(PackageInfo info)
            {
                if (info == null)
                    return null;

                return new PackageConfig()
                {
                    Name = info.name,
                    DisplayName = info.displayName,
                    ID = info.packageId,
                    Version = info.version
                };
            }

            public async Task<bool> Install(PackageConfig remove = null)
            {
                if (remove != null)
                    await Uninstall(remove);

                  var status = Client.Add(ID);
                //status.Error.message;

                while (!status.IsCompleted)
                    await Task.Yield();


                switch (status.Status)
                {
                    case StatusCode.InProgress:
                        throw new NotImplementedException($"Install package {DisplayName} failure.");
                    case StatusCode.Success:

                        break;
                    case StatusCode.Failure:
                        throw new NotImplementedException($"Install package {DisplayName} failure.");
                }

                return true;
            }

            public async Task<bool> Uninstall(PackageConfig info = null)
            {
                if (info == null)
                    info = this;

                var requet = Client.Remove(info.Name);

                while (!requet.IsCompleted)
                    await Task.Yield();

                switch (requet.Status)
                {
                    case StatusCode.InProgress:
                        throw new NotImplementedException($"Install package {info.DisplayName} failure.");
                    case StatusCode.Success:

                        break;
                    case StatusCode.Failure:
                        throw new NotImplementedException($"Install package {info.DisplayName} failure.");
                }

                return true;
            }


        }

    }

    // state
    public partial class PackageInstaller
    {
        public class PackageState
        {
            public bool Lastest;
            public bool Installed;
        }
    }

    #endregion
}
