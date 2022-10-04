using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using System.Linq;
using System.IO;
using UnityEngine.Assertions;


public class FixedMissingMesh : OdinEditorWindow
{
    [MenuItem("Tools/Mesh/FixedMissingMesh")]
    static void CraetePanel()
    {
        var window = (FixedMissingMesh)EditorWindow.GetWindow(typeof(FixedMissingMesh));
        window.Show();
    }

    FixedMissingMesh()
    {
        this.titleContent = new GUIContent("FixedMissingMesh");
    }

    public Dictionary<string, Mesh> MeshCatch;


    [Button("Load Mesh Assets")]
    public virtual void LoadMeshs(UnityEngine.Object @object)
    {
        var path = AssetDatabase.GetAssetPath(@object);
        var objs = SearchFolderAssets<Mesh>(@object.name, path);
        var length = objs.Length;
        if (MeshCatch == null)
            MeshCatch = new Dictionary<string, Mesh>(length);


        for (int i = 0; i < length; i++)
        {
            //if (!MeshCatch.TryAdd(objs[i].name, objs[i]))
                Debug.LogError(AssetDatabase.GetAssetPath(objs[i]));
        }

    }

    public List<MeshFilter> Targets = new List<MeshFilter>();

    [Button("Load Mesh Assets")]
    void SetTargetLODs()
    {
        SetLODAssets(Targets);
    }

    void SetLODAssets(IList<MeshFilter> targets)
    {
        var length = targets.Count;

        for (int i = 0; i < length; i++)
        {
            var target = targets[i];

            if (target.mesh==null)
            {
                var name = target.gameObject.name;
                var prefab=PrefabUtility.GetCorrespondingObjectFromSource(target);
                if (prefab.mesh == null)
                {
                    if(MeshCatch.TryGetValue(name,out var mesh))
                    {
                        prefab.sharedMesh = mesh;
                        Debug.Log(prefab);
                    }    
                }
                else
                {
                    if (MeshCatch.TryGetValue(name, out var mesh))
                    {
                        target.mesh = mesh;
                        Debug.Log(prefab);
                    }
                }
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static T[] SearchFolderAssets<T>(string folderName, string path) where T : UnityEngine.Object
    {
        //var fbxs = GetAssetsAtPath<UnityEngine.Object>(path, true);
        //var list = new List<T>();
        //foreach (var data in fbxs)
        //{
        //    path = AssetDatabase.GetAssetPath(data);
        //    var assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path).Where(x => x.GetType() == typeof(T)).Where(x => !x.name.Contains("preview")).Select(x => x as T).ToArray();
        //    list.AddRange(assets);
        //}
        //return list.ToArray();
        return null;
    }

    [Button("CheckRendererLightScaleInLightmap")]
    public virtual void CheckRendererLight(GameObject @object)
    {
        var renderers = @object.GetComponentsInChildren<MeshRenderer>(true);

        foreach (var ren in renderers)
        {
            if (!ren.gameObject.isStatic)
                continue;

            //ren.receiveGI = ReceiveGI.Lightmaps;
            ren.scaleInLightmap = 0;

        }
    }


}
