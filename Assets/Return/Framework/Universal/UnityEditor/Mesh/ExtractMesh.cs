using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using System.Linq;
using System.IO;
using Return;
using System;
using Object = UnityEngine.Object;

namespace Return.Editors
{
    [Obsolete]
    public class ExtractMesh : OdinEditorWindow
    {
        const string key = nameof(ExtractMesh);

        //[MenuItem("Tools/Mesh/ExtractAsset")]
        //static void CraetePanel()
        //{
        //    var window = (ExtractMesh)GetWindow(typeof(ExtractMesh));
        //    window.Show();
        //}

        ExtractMesh()
        {
            titleContent = new GUIContent("Mesh Extractor");
        }



        [BoxGroup("Config")]
        [ShowInInspector]
        [FolderPath]
        public static string TargetFolder
        {
            get => EditorPrefs.GetString(key);
            set => EditorPrefs.SetString(key, value);
        }

        [BoxGroup("Config")]
        public bool SwitchWithNewMesh = true;



        public List<Object> Targets = new List<Object>();


        [Button("Extract")]
        void ExtractMeshAssets()
        {
            var length = Targets.Count;

            for (int i = 0; i < length; i++)
            {
                var target = Targets[i];

                if (AssetDatabase.IsMainAsset(target) || AssetDatabase.IsSubAsset(target))
                    ExtractAsset<Mesh>(target);
                else if (target is GameObject go)
                {
                    foreach (var ren in go.GetComponentsInChildren<SkinnedMeshRenderer>(false))
                    {
                        ren.sharedMesh = ExtractSkinnedMesh(ren.sharedMesh);
                    }
                }
                else if (target is SkinnedMeshRenderer renderer)
                {
                    renderer.sharedMesh = ExtractSkinnedMesh(renderer.sharedMesh);
                }


            }


            //AssetDatabase.DeleteAsset(path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(TargetFolder));
        }

        
        public static Mesh ExtractSkinnedMesh(Mesh mesh)
        {
            Debug.Log(mesh);
            return ExtractAsset<Mesh>(mesh);
        }

        [Obsolete]
        public static T ExtractAsset<T>(Object target) where T : Object
        {
            var path = AssetDatabase.GetAssetPath(target);


            if (AssetDatabase.IsValidFolder(path))
            {
                T[] assets = SearchFolderAssets<T>(target.name, path);

                foreach (var asset in assets)
                {
                    //ModelImporter.GetAssetsAtPath()

                    ExtractAsset<T>(asset);
                }

                return assets.Length > 0 ? assets[0] : null;
            }
            else // if asset
            {
                var newPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(TargetFolder, target.name + ".asset"));

                if (AssetDatabase.IsMainAsset(target))
                {
                    AssetDatabase.RemoveObjectFromAsset(target);
                    AssetDatabase.CreateAsset(target, newPath);
                }
                else if (target is Mesh mesh)
                {
                    AssetDatabase.CreateAsset(mesh.Copy(), newPath);
                }

                Debug.Log(newPath);

                return AssetDatabase.LoadAssetAtPath<T>(newPath);
            }
        }

        [Obsolete]
        public static T[] SearchFolderAssets<T>(string folderName, string path) where T : Object
        {
            var fbxs = EditorAssetsUtility.GetAssetsAtPath<Object>(path);
            var list = new List<T>();
            foreach (var data in fbxs)
            {
                path = AssetDatabase.GetAssetPath(data);
                var assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path).Where(x => x.GetType() == typeof(T)).Where(x => !x.name.Contains("preview")).Select(x => x as T).ToArray();
                list.AddRange(assets);
            }
            return list.ToArray();
        }


    
    }
}