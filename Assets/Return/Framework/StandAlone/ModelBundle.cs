using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Newtonsoft.Json;


public class ModelBundle : SerializedScriptableObject
{
    public static string Path = "Assets/";

    public static ModelBundle NewBundle(Mesh mesh)
    {
        var bundle = CreateInstance<ModelBundle>();
        bundle.Title = mesh.name;
        //var path = UnityEditor.AssetDatabase.GetAssetPath(mesh);
        //path=path.Substring(0,path.IndexOf("\\"))+"Model"+mesh.name+".asset";
        var path=Path + "Model-" + mesh.name + ".asset";
        //UnityEditor.AssetDatabase.CreateAsset(bundle, path);
        //UnityEditor.AssetDatabase.RemoveObjectFromAsset(mesh);
        if(UnityEditor.AssetDatabase.CopyAsset(UnityEditor.AssetDatabase.GetAssetPath(mesh),path))
        {
            UnityEditor.AssetDatabase.MoveAsset(path, UnityEditor.AssetDatabase.GetAssetPath(bundle));
            bundle.m_Mesh = mesh;
        }
        else
        {
            Debug.LogError("CopyFail");
        }

        return bundle;
        
    }

    public string Title;

    [SerializeField]
    public Mesh m_Mesh;

    [SerializeField]
    public Mesh[] m_Convexs;

    [SerializeField]
    public Material[] m_Materials;

    [SerializeField]
    public Texture2D[] m_UVs;

    [Button(nameof(LoadAsSelfResource))]
    public virtual void LoadAsSelfResource()
    {
        //var setting = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
        //var meshData = JsonConvert.SerializeObject(m_Mesh, setting);//JsonUtility.ToJson(m_Mesh);
        //m_Mesh = new Mesh();
        ////JsonUtility.FromJsonOverwrite(meshData, meshData);
        //JsonConvert.PopulateObject(meshData, m_Mesh, setting);
        //UnityEditor.AssetDatabase.AddObjectToAsset(m_Mesh, this);
        SaveAsset(this, ref m_Mesh);

        var length = m_Convexs.Length;
        for (int i = 0; i < length; i++)
        {
            SaveAsset(this, ref m_Convexs[i]);
        }
    }

    static void SaveAsset<T>(UnityEngine.Object rootAsset,ref T childAsset) where T:UnityEngine.Object,new ()
    {
        var setting = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
        var meshData = JsonConvert.SerializeObject(childAsset, setting);//JsonUtility.ToJson(m_Mesh);
        childAsset = new T();
        //JsonUtility.FromJsonOverwrite(meshData, meshData);
        JsonConvert.PopulateObject(meshData, childAsset, setting);
        UnityEditor.AssetDatabase.AddObjectToAsset(childAsset, rootAsset);
    }
}
