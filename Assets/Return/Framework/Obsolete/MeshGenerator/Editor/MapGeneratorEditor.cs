#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerator generator = (MapGenerator)target;

        if (DrawDefaultInspector())
        {
            if (generator.AutoGenerate)
            {
                generator.GenerateMap();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            generator.GenerateMap();
        }

        if (GUILayout.Button("Save Mesh"))
        {
            
            AssetDatabase.CreateAsset(generator.SaveMesh(), "Assets/OutputCenter/" + generator.Target().name+DateTime.UtcNow.ToOADate() + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            generator.GenerateMap();
        }

        if (GUILayout.Button("Save Texture"))
        {
            AssetDatabase.CreateAsset(generator.SaveMat().mainTexture, "Assets/OutputCenter/" + generator.Target().name+ DateTime.UtcNow.ToFileTimeUtc() + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            generator.GenerateMap();
        }
    }



    
}

#endif
