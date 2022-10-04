// Distant Lands 2021.



using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DistantLands.Cozy.EditorScripts
{
    public class E_CozyMenuItems : MonoBehaviour
    {

        [MenuItem("Distant Lands/Cozy/Create Cozy Volume")]
        static void CozyVolumeCreation()
        {


            Camera view = SceneView.lastActiveSceneView.camera;


            GameObject i = new GameObject();
            i.name = "Cozy Volume";
            i.AddComponent<BoxCollider>().isTrigger = true;
            i.AddComponent<CozyVolume>();
            i.transform.position = (view.transform.forward * 5) + view.transform.position;

            Undo.RegisterCreatedObjectUndo(i, "Create Cozy Volume");
            Selection.activeGameObject = i;


        }


        [MenuItem("Distant Lands/Cozy/Create Cozy FX Block Zone")]
        static void CozyBlockZoneCreation()
        {


            Camera view = SceneView.lastActiveSceneView.camera;


            GameObject i = new GameObject();
            i.name = "Cozy FX Block Zone";
            i.AddComponent<BoxCollider>().isTrigger = true;
            i.tag = "FX Block Zone";
            i.transform.position = (view.transform.forward * 5) + view.transform.position;

            Undo.RegisterCreatedObjectUndo(i, "Create Cozy FX Block Zone");
            Selection.activeGameObject = i;


        }


        

        public static List<T> GetAssets<T>(string[] _foldersToSearch, string _filter) where T : UnityEngine.Object
        {
            string[] guids = AssetDatabase.FindAssets(_filter, _foldersToSearch);
            List<T> a = new List<T>();
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                a.Add(AssetDatabase.LoadAssetAtPath<T>(path));
            }
            return a;
        }

    }
}