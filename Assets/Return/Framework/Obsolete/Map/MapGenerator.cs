using System.Collections;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode {NoiseMap,HeightMap,Mesh}
    public DrawMode drawMode;

    const int mapChunkSize = 241;
    [Range(0,6)]
    public int levelOfDetail;
    [Range(0,mapChunkSize)]
    public int MapWidth;
    [Range(0,mapChunkSize)]
    public int MapHeight;
    public float MapHeightMultip;
    public AnimationCurve MapHeightCurve;
    public float NoiseScale;
    public bool AutoGenerate;

    public int octave;
    [Range(0, 1)]
    public float decay;
    [Range(0, 100)]
    public float lacunarity;

    public int Seed;
    public Vector2 Offset;

    public enum Terrains { Void0, Void1, Void2, Void3, Void4, Void5, Void6, Void7, Void8, Void9 }
    public Terrains TypeOfTerrain;

    public TerrainType[] regions;

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GeneratorNoiseMap(MapWidth, MapHeight, Seed, NoiseScale, octave, decay, lacunarity, Offset);

        Color[] colorMap = new Color[MapWidth * MapHeight];
        for (int y = 0; y < MapHeight; y++)
        {
            for (int x = 0; x < MapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colorMap[y * MapWidth + x] = regions[i].color;
                        break;
                    }
                }
            }

        }

        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            mapDisplay.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        }
        else if(drawMode==DrawMode.HeightMap)
        {
            mapDisplay.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap,MapWidth,MapHeight));

        }
        else if (drawMode == DrawMode.Mesh)
        {
            mapDisplay.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, MapHeightMultip,MapHeightCurve,levelOfDetail), TextureGenerator.TextureFromColorMap(colorMap,MapWidth,MapHeight));
        }



    }

    public Mesh SaveMesh()
    {
        Mesh newMesh = gameObject.GetComponent<MapDisplay>().meshFilter.sharedMesh;

        return  newMesh;
    }

    public Material SaveMat()
    {
        Material newMat = gameObject.GetComponent<MapDisplay>().meshRenderer.sharedMaterial;
        return newMat;
    }

    public GameObject Target()
    {
        GameObject target = gameObject.GetComponent<MapDisplay>().meshRenderer.gameObject;
        return target;
    }

    private void OnValidate()
    {
        MapHeight = (MapHeight < 2) ? 2 : MapHeight;

        MapWidth = (MapWidth < 2) ? 2: MapWidth;

        lacunarity = (lacunarity < 1) ? 1 : lacunarity;

        octave = (octave < 0) ? 0 : octave;
    }
}

[System.Serializable]
public struct TerrainType
{
    public string Name;
    public float height;
    public Color color;

}


public struct TerrainStorge 
{
    public class Storage
    {
        public string Name;
        public float height;
        public Color color;
    }
    public Storage[] storages; 

}

