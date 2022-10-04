using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;

using UnityEditor;
using System.Text.RegularExpressions;
using UnityEngine.Profiling;
using System;

namespace Return.Editors
{
    public class AssetInspector : OdinEditorWindow
    {
        [MenuItem("Tools/Editor/AssetInspector")]
        static void CreateWindow()
        {
            Open();
        }

        public static AssetInspector Open()
        {
            var window = GetWindow<AssetInspector>("AssetInspector");
            window.autoRepaintOnSceneChange = false;
            return window;
        }



        [ShowInInspector]
        int DataCount => cacheData==null? 0: cacheData.Count;

        static Dictionary<UnityEngine.Object, AssetLog> cacheData=new(10);

        protected override void OnEnable()
        {
            Selection.selectionChanged = (Action)Delegate.Remove(Selection.selectionChanged, new Action(DoCleanTargets));
            Selection.selectionChanged = (Action)Delegate.Combine(Selection.selectionChanged, new Action(DoCleanTargets));
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            Selection.selectionChanged = (Action)Delegate.Remove(Selection.selectionChanged, new Action(DoCleanTargets));

            base.OnDisable();
        }

        static bool dirty;

        void DoCleanTargets()
        {
            dirty = true;
        }

        void CleanTargets()
        {
            dirty = false;
            cacheData.Clear();
        }


        //[OnInspectorGUI]
        void DrawSelected()
        {
            if (dirty)
                CleanTargets();

            var target = Selection.activeObject;

            DrawTarget(target);

            var selectedNum = Selection.count;

            if (selectedNum == 1)
                return;

            var targets = Selection.objects;

            foreach (var asset in targets)
            {
                if (asset == target)
                    continue;

                DrawTarget(target);
            }

            GUIUtility.ExitGUI();
        }

        [OnInspectorGUI]
        void DrawDetail()
        {
            if (dirty)
                CleanTargets();

            var target = Selection.activeObject;

            if (!cacheData.TryGetValue(target, out var log))
                PrepareInspectAsset(target,out log);

            DrawLog(log);

            var selectedNum = Selection.count;

            if (selectedNum >1)
            {
                var targets = Selection.objects;

                foreach (var asset in targets)
                {
                    if (asset == target)
                        continue;

                    if (!cacheData.TryGetValue(asset, out log))
                        PrepareInspectAsset(asset,out log);

                    DrawLog(cacheData[asset]);
                }
            }

            GUIUtility.ExitGUI();
        }

        void PrepareInspectAsset(UnityEngine.Object target,out AssetLog log)
        {
            if (cacheData.TryGetValue(target,out log))
                return;

            var data = new AssetLog(target);
            cacheData.Add(target, data);
            //dataQueue.Enqueue(target);

            //if (cacheData.Count> Capacity && cacheData.Count)
            //{
            //    if (dataQueue.TryDequeue(out var deQueue) && deQueue != Selection.activeObject)
            //        cacheData.Remove(target);
            //}
            log = data;

        }

        GUILayoutOption maxWidth;

        void DrawLog(AssetLog log)
        {
            if (log.IsNull())
            {
                GUILayout.Label("Non object selected");
                return;
            }

            //Debug.Log(log.Name);

            try
            {
                if (EditorGUILayout.BeginFoldoutHeaderGroup(true, log.Name))
                {
                    maxWidth = GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth - 50);


                    DrawLabel("Name", log.Name, maxWidth);
                    DrawLabel("InstanceID", log.InstanceID, maxWidth);
                    DrawLabel("GUID", log.GUID, maxWidth);

                    // Extension
                    DrawLabel("Extension", log.Extension, maxWidth);

                    DrawLabel("m_Path", log.Path, GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth * 0.6f));

                    // State
                    DrawLabel("State", log.State.ToString(), maxWidth);

                    DrawLabel("Size", log.Size.ToString(),maxWidth);

                    if (GUILayout.Button("Duplicate"))
                    {
                        if (EditorAssetsUtility.Duplicate(log.Asset, out var newAsset))
                        {
                            var folder = EditorUtitily.GetEditorFolder(log.Asset);
                            var newPath = AssetDatabase.GenerateUniqueAssetPath(folder + "/" + log.Name);
                            AssetDatabase.CreateAsset(newAsset, newPath + log.Extension);
                        }

                        GUIUtility.ExitGUI();
                    }
                }

            }
            finally
            {
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }


        void DrawTarget(UnityEngine.Object target)
        {
            if (target.IsNull())
            {
                GUILayout.Label("Non object selected");
                return;
            }

            if(EditorGUILayout.BeginFoldoutHeaderGroup(true, target.name))
            {
                maxWidth = GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth - 50);

                var path = AssetDatabase.GetAssetPath(target);
                var folder = EditorUtitily.GetEditorFolder(target);
                var subAsset = AssetDatabase.IsSubAsset(target);

                DrawLabel("Name", target.name, maxWidth);
                DrawLabel("InstanceID", target.GetInstanceID().ToString(), maxWidth);
                DrawLabel("GUID", EditorAssetsUtility.GetGUID(target), maxWidth);

                // Extension
                var extension = AssetDatabase.IsValidFolder(path) ? "Folder" : EditorPathUtility.GetExtension(target);
                DrawLabel("Extension", extension, maxWidth);

                var size=Profiler.GetRuntimeMemorySizeLong(target);


                DrawLabel("m_Path", AssetDatabase.GetAssetPath(target),GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth*0.6f));

                // State
                {
                    var assetState =
                        subAsset ?
                        "SubAsset" :
                        AssetDatabase.IsMainAsset(target) ?
                        "MainAsset" :
                        "Unknow";
                    DrawLabel("State", assetState, maxWidth);
                }

                if (GUILayout.Button("Duplicate"))
                {
                    if(EditorAssetsUtility.Duplicate(target,out var newAsset))
                    {
                        var newPath = AssetDatabase.GenerateUniqueAssetPath(folder + "/" + target.name);
                        AssetDatabase.CreateAsset(newAsset, newPath + extension);
                    }

                    GUIUtility.ExitGUI();
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

        }

        void DrawLabel(string title,string context,params GUILayoutOption[] options)
        {
            //if (Event.current.type == EventType.Layout)
            {
                EditorGUILayout.BeginHorizontal();

                //GUILayout.Label(title + " : " + context, maxWidth);

                if (options == null || options.Length == 0)
                    GUILayout.Label(title + " : " + context, maxWidth);
                else
                    GUILayout.Label(title + " : " + context, options);

                if (GUILayout.Button("Copy", GUILayout.MaxWidth(50)))
                {
                    GUIUtility.systemCopyBuffer = context;
                    //GUIUtility.ExitGUI();
                }

                EditorGUILayout.EndHorizontal();
            }

        }
    }


    [Serializable]
    public class AssetLog
    {
        public AssetLog(UnityEngine.Object target)
        {
            Load(target);
        }

        public void Load(UnityEngine.Object target)
        {
            Asset = target;

            Name = target.name;
            InstanceID = target.GetInstanceID().ToString();
            GUID = EditorAssetsUtility.GetGUID(target);

            //var folder = EditorUtitily.GetEditorFolder(target);
            Path = AssetDatabase.GetAssetPath(target);

            var subAsset = AssetDatabase.IsSubAsset(target);
            State = subAsset ? AssetState.SubAsset : AssetState.MainAsset;

            Extension = AssetDatabase.IsValidFolder(Path) ? "Folder" : EditorPathUtility.GetExtension(target);

            Size = Profiler.GetRuntimeMemorySizeLong(target) / 1024;
        }

        public UnityEngine.Object Asset;

        public string Name;
        public string InstanceID;
        public string GUID;

        public string Extension;

        public string Path;
        public AssetState State;

        public float Size;
    }

    public enum AssetState
    {
        MainAsset = 0,
        SubAsset = 1,
    }
}
