using System.Collections;
using UnityEngine;


public static class Noise 
{
    public static float [,] GeneratorNoiseMap(int mapWidth,int mapHeight,int seed,float scale,int octave, float decay, float lacunarity,Vector2 offset)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random dice = new System.Random(seed);

        Vector2[] octaveOffset = new Vector2[octave];
        for (int i = 0; i < octave; i++)
        {
            float offsetX = dice.Next(-100000, 100000)+offset.x;
            float offsetY = dice.Next(-100000, 100000)+offset.y;
            octaveOffset[i] = new Vector2(offsetX, offsetY);
        }
        
        if (scale <= 0)
        {
            scale = 0.00001f;
        }

        float halfwidth = mapWidth / 2f;
        float halfheight = mapHeight / 2f;

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1f;
                float frequency = 1f;
                float noiseheight = 0;

                for (int i = 0; i < octave; i++)
                {
                    float sampleX = (x-halfwidth) / scale*frequency+octaveOffset[i].x;
                    float sampleY = (y-halfheight) / scale*frequency+octaveOffset[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY)*2-1;
                    noiseheight += perlinValue * amplitude;

                    amplitude *= decay;
                    frequency *= lacunarity;
                }

                if (noiseheight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseheight;
                }
                else if (noiseheight < minNoiseHeight)
                {
                    minNoiseHeight = noiseheight;
                }

                noiseMap[x, y] = noiseheight;

            }
        }


        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }
}
