using UnityEditor;
using UnityEngine;

namespace DistantLands.Cozy.EditorScripts
{
    [CustomEditor(typeof(CozySaveLoad))]
    public class E_CozySaveLoad : Editor
    {

        CozySaveLoad saveLoad;

        void OnEnable()
        {

            saveLoad = (CozySaveLoad)target;

        }

        public override void OnInspectorGUI()
        {

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Save"))
                saveLoad.Save();
            if (GUILayout.Button("Load"))
                saveLoad.Load();

            EditorGUILayout.EndHorizontal();

        }

    }
}