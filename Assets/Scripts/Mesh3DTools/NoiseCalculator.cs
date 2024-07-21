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

    public static void GenerateNoiseMap(int startX, int startZ, int width, float scale, float mapSize,Vector2[] octaveOffsets, int octaves, float persistance,
                                      float lacunarity, Vector2Int offsets,float waterThreshold, out float[,] noiseMap, out float[,] fallOff, out bool hasWater)
    {
        noiseMap = new float[width, width];
        fallOff = new float[width, width];
        hasWater = false;

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

                float worldX = startX + (x - halfSize);
                float worldY = startZ + (z - halfSize);

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (worldX / scale * frecuency) + octaveOffsets[i].x + offsets.x;
                    float sampleZ = (worldY / scale * frecuency) + octaveOffsets[i].y + offsets.y;


                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frecuency *= lacunarity;
                }

                noiseMap[x, z] = noiseHeight;

                if (Mathf.Abs(worldX) > Mathf.Abs(mapSize) || Mathf.Abs(worldY) > Mathf.Abs(mapSize))
                {
                    fallOff[x, z] = 1f;
                }
                else 
                {

                    float xO = worldX / (float)mapSize * 2 - 1;
                    float yO = worldY / (float)mapSize * 2 - 1;

                    float value = Mathf.Max(Mathf.Abs(xO), Mathf.Abs(yO));
                    fallOff[x, z] = Evaluate(value);
                }


               // noiseMap[x, z] = Mathf.Clamp01(noiseMap[x, z] - fallOff[x, z]);


                if (noiseMap[x, z] <= waterThreshold)
                    hasWater = true;
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < width; z++)
            {
                noiseMap[x, z] = Mathf.InverseLerp(MinNoiseHeight, MaxNoiseHeight, noiseMap[x, z]);

                noiseMap[x, z] = Mathf.Clamp01(noiseMap[x, z] - fallOff[x, z]);
            }
        }
    }

    public static float[,] GenerateFalloffMap(int size, int chunkSize, int xPos, int yPos)
    {
        float[,] map = new float[chunkSize, chunkSize];

        for (int i = 0; i < chunkSize; i++)
        {
            for (int j = 0; j < chunkSize; j++)
            {
                int worldX = i + xPos;
                int worldY = j + yPos;

              
                if (Mathf.Abs(worldX) > Mathf.Abs(size) || Mathf.Abs(worldY) > Mathf.Abs(size))
                {
                    map[i, j] = 1f;
                    continue;
                }

                float x = worldX / (float)size * 2 - 1;
                float y = worldY / (float)size * 2 - 1;

                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                map[i, j] = Evaluate(value);
            }
        }

        return map;
    }

    static float Evaluate(float value)
    {
        float a = 3;
        float b = 2.2f;

        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow((b - b * value), a));
    }
}
