using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseCalculator 
{
    public static float[,] GenerateChunk(int startX, int startZ, int chunkSize, float frecuency, int seed, float amplitude)
    {
        float[,] noiseMap = new float[chunkSize, chunkSize];
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                float sampleX = (startX + x + seed) / frecuency;
                float sampleZ = (startZ + z + seed) / frecuency;
                float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ);


                noiseMap[x, z] = perlinValue;

            }
        }
        return noiseMap;
    }

    public static float[,] GenerateNoiseMap(int startX, int startZ, int width, float scale, Vector2[] offsets, int octaves, float persistance, float lacunarity)
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
                    float sampleX = ((startX + (x - halfSize)) / scale * frecuency) + offsets[i].x;
                    float sampleZ = ((startZ + (z - halfSize)) / scale * frecuency) + offsets[i].y;


                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frecuency *= lacunarity;
                }


                noiseMap[x, z] = noiseHeight;
            }
        }


                return noiseMap;
    }
}
