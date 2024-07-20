using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseCalculator
{

    public static float MaxNoiseHeight = float.MinValue;
    public static float MinNoiseHeight = float.MaxValue;
    public static float CheckPlayerPos(int width, float scale, Vector2[] octaveOffsets, int octaves, float persistance, float lacunarity, Vector2Int offsets)
    {

        if (scale <= 0)
            scale = 0.001f;

        float halfSize = width / 2;

        float amplitude = 1;
        float frecuency = 1;
        float noiseHeight = 0;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = ((offsets.x - halfSize) / scale * frecuency) + octaveOffsets[i].x;
            float sampleZ = ((offsets.y - halfSize) / scale * frecuency) + octaveOffsets[i].y;


            float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;
            noiseHeight += perlinValue * amplitude;

            amplitude *= persistance;
            frecuency *= lacunarity;
        }



        return Mathf.InverseLerp(MinNoiseHeight, MaxNoiseHeight, noiseHeight);
    }

    public static float[,] GenerateNoiseMap(int startX, int startZ, int width, float scale, Vector2[] octaveOffsets, int octaves, float persistance, float lacunarity, Vector2Int offsets)
    {
        float[,] noiseMap = new float[width, width];

        if (scale <= 0)
            scale = 0.001f;

        float halfSize = width / 2;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < width; z++)
            {
                float amplitude = 1;
                float frecuency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = ((startX + (x - halfSize)) / scale * frecuency) + octaveOffsets[i].x + offsets.x;
                    float sampleZ = ((startZ + (z - halfSize)) / scale * frecuency) + octaveOffsets[i].y + offsets.y;


                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frecuency *= lacunarity;
                }

                noiseMap[x, z] = noiseHeight;
            }
        }


        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < width; z++)
            {
                noiseMap[x, z] = Mathf.InverseLerp(MinNoiseHeight, MaxNoiseHeight, noiseMap[x, z]);
            }
        }

        return noiseMap;
    }
}
