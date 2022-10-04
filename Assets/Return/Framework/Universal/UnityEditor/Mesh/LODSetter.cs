//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Sirenix.OdinInspector;
//using Sirenix.OdinInspector.Editor;
//using UnityEditor;
//using System.Linq;
//using System.IO;
//using Return;
//using UnityEngine.Assertions;
//using Impostors;
//using Impostors.Structs;

//public class LODSetter : OdinEditorWindow
//{
//    [MenuItem("Tools/Mesh/LODSetter")]
//    static void CraetePanel()
//    {
//        var window = (LODSetter)EditorWindow.GetWindow(typeof(LODSetter));
//        window.Show();
//    }

//    LODSetter()
//    {
//        this.titleContent = new GUIContent("LOD Setter");
//    }

//    public List<UnityEngine.Object> PackageContents = new List<Object>();

//    [Button("Set LOD")]
//    void SetTargetLODs()
//    {
//        SetLODAssets(PackageContents);
//    }

//    void SetLODAssets(IList<UnityEngine.Object> targets)
//    {
//        var length = targets.Count;

//        for (int i = 0; i < length; i++)
//        {
//            var target = targets[i];
//            var path = AssetDatabase.GetAssetPath(target);
//            GameObject[] objs;
//            if (AssetDatabase.IsValidFolder(path))
//            {

//                objs = SearchFolderAssets<GameObject>(target.name, path);
//                foreach (var obj in objs)
//                {
//                    var renderers = obj.GetComponentsInChildren<Renderer>();
//                    if (renderers.Length > 0)
//                    {
//                        var lod = obj.InstanceIfNull<LODGroup>();
//                        //if (lod.lodCount == 0)
//                        Assert.IsNotNull(lod);
//                        lod.SetLODs(new LOD[] { new LOD(0.005f, renderers) { fadeTransitionWidth = 1f } });

//                    }
//                }
//            }
//            else // if asset
//            {
//                if(target is GameObject @object)
//                {
//                    var renderers = @object.GetComponentsInChildren<Renderer>();
//                    if (renderers.Length>0)
//                    {
//                        var lod=@object.InstanceIfNull<LODGroup>();
//                        //if (lod.lodCount == 0)
//                            lod.SetLODs(new LOD[] { new LOD(0.005f, renderers) {fadeTransitionWidth=1f } });
                        
//                    }
//                }
//            }
//        }
//        AssetDatabase.SaveAssets();
//        AssetDatabase.Refresh();
//    }

//    public static T[] SearchFolderAssets<T>(string folderName, string path) where T : UnityEngine.Object
//    {
//        var fbxs = EditorHelper.GetAssetsAtPath<UnityEngine.Object>(path,true);
//        var list = new List<T>();
//        foreach (var data in fbxs)
//        {
//            path = AssetDatabase.GetAssetPath(data);
//            var assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path).Where(x => x.GetType() == typeof(T)).Where(x => !x.name.Contains("preview")).Select(x => x as T).ToArray();
//            list.AddRange(assets);
//        }
//        return list.ToArray();
//    }

//    [Button("Set ImpostorLODGroup")]
//    protected virtual void SetLOD()
//    {
//        var selected = PackageContents.Where(x=>x is GameObject).Select(x => ((GameObject)x).transform);


//        Undo.SetCurrentGroupName("Setup Impostor(s)");
//        bool hasProblems = false;

//        foreach (ReadOnlyTransform trans in selected)
//        {
//            var lodGroup = trans.GetComponent<LODGroup>();
//            if (lodGroup == null && lodGroup.GetComponent<ImpostorLODGroup>() == null)
//            {
//                hasProblems = true;
//                Debug.LogError(
//                    $"[Impostors] Cannot add {nameof(ImpostorLODGroup)} to '{trans.name}' because it has no {nameof(LODGroup)} component. Impostors work only in combination with {nameof(LODGroup)} component. Click to navigate to this object.",
//                    trans);
//                continue;
//            }

//            SetupImpostorLODGroupToObject(lodGroup);
//        }

//        if (hasProblems)
//            EditorUtilityTools.DisplayDialog(
//                "Impostors: Setup Impostor(s)",
//                "Automatic imposter setup faced some problems. Look at the console for more details.",
//                "Ok");
//    }

//    private static void SetupImpostorLODGroupToObject(LODGroup lodGroup)
//    {
//        var lods = lodGroup.GetLODs();
//        Undo.RegisterCompleteObjectUndo(lodGroup.gameObject, "Setup ImpostorLODGroup");
//        lodGroup.gameObject.SetActive(false);

//        if(!lodGroup.gameObject.TryGetComponent(out ImpostorLODGroup impostorLODGroup))
//            impostorLODGroup = Undo.AddComponent<ImpostorLODGroup>(lodGroup.gameObject);

//        ImpostorLOD impostorLodEmpty = new ImpostorLOD();
//        impostorLodEmpty.screenRelativeTransitionHeight =
//            Mathf.Clamp(lods[lods.Length - 1].screenRelativeTransitionHeight, 0.1f, 0.9f);
//        impostorLodEmpty.renderers = new Renderer[0];
//        ImpostorLOD impostorLOD = new ImpostorLOD();
//        impostorLOD.screenRelativeTransitionHeight = 0.005f;
//        impostorLOD.renderers = lods.Last().renderers;
//        impostorLODGroup.zOffset = .25f;

//        impostorLODGroup.LODs = new[] { impostorLodEmpty, impostorLOD };

//        impostorLODGroup.RecalculateBounds();
//        lodGroup.gameObject.SetActive(true);
//    }

//    [Button("RemoveAll")]
//    public virtual void RemoveAll()
//    {
//        var selected = PackageContents.Where(x => x is GameObject).Select(x => (GameObject)x);


//        Undo.SetCurrentGroupName("Setup Impostor(s)");

//        AssetDatabase.StartAssetEditing();
//        foreach (var trans in selected)
//        {
//            if (trans.TryGetComponent<LODGroup>(out var group))
//            {
//                var path = AssetDatabase.GetAssetPath(trans);

//                var rootGO = PrefabUtility.LoadPrefabContents(path);
//                if (rootGO.TryGetComponent<ImpostorLODGroup>(out var Impostor))
//                    DestroyImmediate( Impostor);

//                DestroyImmediate(rootGO.GetComponent<LODGroup>());

//                PrefabUtility.SaveAsPrefabAsset(rootGO, path);
//                PrefabUtility.UnloadPrefabContents(rootGO);
//            }
//        }
//        AssetDatabase.StopAssetEditing();
//        AssetDatabase.SaveAssets();
//    }
//}
