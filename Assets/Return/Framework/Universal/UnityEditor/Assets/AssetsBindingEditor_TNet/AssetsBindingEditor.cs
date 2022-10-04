using Sirenix.OdinInspector.Editor.Drawers;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using Return;
using UnityEditor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Serialization;
using System;
using Object = UnityEngine.Object;


namespace Return.Editors
{
    public class AssetsBindingEditor : OdinEditorWindow
    {
        [MenuItem("Tools/Assets/AssetsBindingHelper")]
        static void CreateWindow()
        {
            AssetsBindingEditor window = (AssetsBindingEditor)EditorWindow.GetWindowWithRect(typeof(AssetsBindingEditor), new Rect(0, 0, 800, 600));
        }

        #region Serialize

        [Button]
        protected void LoadData()
        {
            var filePath = EditorUtility.OpenFilePanel("Open Binding File", Application.dataPath, "*");

            if (!File.Exists(filePath))
            {
                Debug.LogError("Invalid binding file path, search assets operation will be cancel.");
                return;
            }

            var st = File.OpenText(filePath);

            var json = st.ReadToEnd();

            st.Close();
            st.Dispose();

            GUIDs = JsonUtil.getJsonArray<string>(json);
        }


        string[] GUIDs;

        [Button]
        protected void SaveData()
        {
            var path=EditorUtility.SaveFilePanel("Save Binding File", Application.dataPath, "BindingData", "txt");

            var json = JsonUtil.arrayToJson(GUIDs, true);

            // save json file
            File.WriteAllText(path, json);
        }

        #endregion

        [Title("Search Prefabs")]
        [SerializeField]
        string filter = "t:GameObject";

        [FolderPath]
        [SerializeField]
        string path;

        [Button]
        void BatchSerachPrefabs()
        {
            GUIDs = AssetDatabase.FindAssets(filter, new string[] { path }).ToHashSet().ToArray();

            Debug.Log(string.Join("\n", GUIDs));

            SaveData();
        }


        [Button]
        void SearchData()
        {
            if(GUIDs.NullorEmpty())
                LoadData();
  

            if (BindingTargets == null)
                BindingTargets = new(GUIDs.Length);
            else
                BindingTargets.Capacity=GUIDs.Length;

            foreach (var guid in GUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);

                if(!string.IsNullOrEmpty(path))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                    BindingTargets.Add(asset);
                }
                else
                    Debug.LogError($"Binding asset missing reference : {guid}.");

            }

        }

        [ListDrawerSettings(Expanded = true)]
        [SerializeField]
        List<Object> BindingTargets;

    }








    //public class AssetsBindingEditor : OdinEditorWindow
    //{
    //    [MenuItem("Tools/Assets/AssetsBindingHelper")]
    //    static void CreateWindow()
    //    {
    //        AssetsBindingEditor window = (AssetsBindingEditor)EditorWindow.GetWindowWithRect(typeof(AssetsBindingEditor), new Rect(0, 0, 800, 600));
    //    }

    //    #region Serialize

    //    [SerializeField]
    //    DataNodeHandler m_DataNodeHandler = new() { FileName = nameof(GUIDCache), ArchiveOption = ArchiveOption.Custom, DataPath = Application.dataPath };
    //    public DataNodeHandler DataNodeHandler { get => m_DataNodeHandler; set => m_DataNodeHandler = value; }
    //    DataNode Datas;

    //    [ShowInInspector]
    //    [HideLabel]
    //    public GUIDCache GUIDData { get; protected set; }

    //    [Button]
    //    protected void LoadData()
    //    {
    //        if (GUIDData == null)
    //            GUIDData = new();

    //        if (Datas == null)
    //        {
    //            if (!DataNodeHandler.TryLoadData(out Datas))
    //            {
    //                Debug.LogError($"Data path return empty {DataNodeHandler.GetFilePath()}.");
    //                Datas = new DataNode(nameof(GUIDs));

    //                return;
    //            }
    //        }

    //        GUIDData.Deserialize(Datas);

    //    }

    //    [HideInInspector]
    //    [SerializeField]
    //    public string[] GUIDs;

    //    [Button]
    //    protected void SaveData()
    //    {
    //        if (Datas != null)
    //        {
    //            GUIDData.Serialize(Datas);
    //            DataNodeHandler.SaveData(Datas);
    //        }
    //    }

    //    [Serializable]
    //    public class GUIDCache : DataNodeBased
    //    {
    //        [ShowInInspector]
    //        public string[] GUIDs
    //        {
    //            get => dataNode.GetChild(nameof(GUIDs), new string[0]);
    //            set => dataNode.SetChild(nameof(GUIDs), value);
    //        }
    //    }


    //    #endregion

    //    public GameObject Go;


    //    [Title("Search Prefabs")]
    //    [SerializeField]
    //    string filter = "t:GameObject";

    //    [FolderPath]
    //    [SerializeField]
    //    string path;

    //    [Button]
    //    void BatchSerachPrefabs()
    //    {
    //        //var tfs = Go.GetComponentsInChildren<Transform>();

    //        GUIDs = AssetDatabase.FindAssets(filter, new string[] { path }).ToHashSet().ToArray();
    //        var json = EditorJsonUtility.ToJson(GUIDData, true);



    //        Debug.Log(json);

    //        //.Select(x => AssetDatabase.AssetPathToGUID(x))
    //        LoadData();

    //        GUIDData.GUIDs = GUIDs;

    //        SaveData();

    //        Debug.Log(string.Join("\n", GUIDs));

    //        //HashSet<string> paths=new(tfs.Length);

    //        //foreach (var go in tfs)
    //        //{
    //        //    paths.Add(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go.gameObject));
    //        //}

    //        Debug.Log(GUIDs.Length);

    //    }


    //    [Button]
    //    void SearchData()
    //    {
    //        var filePath = EditorUtility.OpenFilePanel("Open Binding File", DataNodeHandler.GetFolderPath(), "*");

    //        if (!File.Exists(filePath))
    //        {
    //            Debug.LogError("Invalid binding file path, search assets operation will be cancel.");
    //            return;
    //        }

    //        var name = Path.GetFileName(filePath);
    //        filePath = filePath.Remove(filePath.IndexOf(name));

    //        var handler = DataNodeHandler;
    //        handler.DataPath = filePath;
    //        handler.FileName = name;
    //        DataNodeHandler = handler;

    //        LoadData();

    //        //if(!DataNodeHandler.TryLoadData(out Datas))
    //        //{
    //        //    Debug.LogError($"Failure to load binding data from {DataNodeHandler.GetFilePath()}.");
    //        //    return;
    //        //}

    //        //GUIDData.Deserialize(Datas);

    //        Debug.Log(string.Join("\n", GUIDData.GUIDs));

    //        GUIDs = GUIDData.GUIDs;

    //        Debug.Log(GUIDs.Length);

    //        if (BindingTargets == null)
    //            BindingTargets = new(GUIDs.Length);
    //        else
    //            BindingTargets.Capacity = GUIDs.Length;

    //        foreach (var guid in GUIDs)
    //        {
    //            var path = AssetDatabase.GUIDToAssetPath(guid);

    //            if (!string.IsNullOrEmpty(path))
    //            {
    //                var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
    //                BindingTargets.Add(asset);
    //            }
    //            else
    //                Debug.LogError($"Binding asset missing reference : {guid}.");

    //        }

    //    }

    //    [ListDrawerSettings(Expanded = true)]
    //    [SerializeField]
    //    List<Object> BindingTargets;



    //}
}
