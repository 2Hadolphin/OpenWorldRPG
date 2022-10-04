//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Sirenix.OdinInspector;
//using Sirenix.OdinInspector.Editor;
//using UnityEditor;
//using Sirenix.Utilities.Editor;
//using Sirenix.Utilities;
//using System.Linq;
//using System.Text;
//using System.IO;
//using Return;
//public class SyntyModelExportToolkit : OdinEditorWindow
//{
//    [MenuItem("Tools/Synty/ModelExportToolkit")]
//    static void OpenWindow()
//    {
//        var window = GetWindow<SyntyModelExportToolkit>();

//        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(700, 700);
//    }

//    [Button("ExportModel")]
//    public virtual void ExtractModel(GameObject @object)
//    {
//        var fliters = @object.GetComponentsInChildren<MeshFilter>();
//        foreach (var filter in fliters)
//        {
//            var rend = filter.GetComponent<Renderer>();
//            var mats = rend.sharedMaterials;

//            var cols = filter.GetComponents<MeshCollider>();

//            Debug.Log(filter.sharedMesh.name+string.Join("**",cols.Select(x => x.sharedMesh.name)));
//            var bundle = ModelBundle.NewBundle(filter.sharedMesh);
//            bundle.m_Convexs = cols.Select(x=>x.sharedMesh).ToArray();
//            bundle.m_Materials = new Material[mats.Length];
//            mats.CopyTo(bundle.m_Materials, 0);

//            bundle.LoadAsSelfResource();
//            var path = AssetDatabase.GetAssetPath(@object);
//            var guid = AssetDatabase.AssetPathToGUID(path);
//            path = AssetDatabase.GetTextMetaFilePathFromAssetPath(AssetDatabase.GetAssetPath(@object));
//            //path = path.Substring(path.LastIndexOf("Assets"));
//            path = path.Replace("Assets", string.Empty);
//            Debug.Log(guid + Application.dataPath);
//            var newPath = Application.dataPath + path; //m_Path.Combine(Application.dataPath, path);


//            Debug.Log(newPath);
//            using (var stream = new StreamReader(newPath))
//            {
//                stream.ReadLine();
//                Debug.Log(stream.ReadLine());
//            }

//        }
//    }

//    [Button("TestRead")]
//    void ReadObject(UnityEngine.Object @object)
//    {
//        var path = AssetDatabase.GetAssetPath(@object);
//        var guid = AssetDatabase.AssetPathToGUID(path);
//        path = AssetDatabase.GetTextMetaFilePathFromAssetPath(AssetDatabase.GetAssetPath(@object));
//        //path = path.Substring(path.LastIndexOf("Assets"));
//        path = path.Replace("Assets", string.Empty);
//        Debug.Log(guid + Application.dataPath);
//        var newPath = Application.dataPath+ path; //m_Path.Combine(Application.dataPath, path);


//        Debug.Log(newPath);
//        using (var stream = new StreamReader(newPath))
//        {
//            stream.ReadLine();
//            Debug.Log(stream.ReadLine());
//        }
//    }

//}
