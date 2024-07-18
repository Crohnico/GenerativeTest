using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkArea 
{
    public float[,] GenerateChunk(int startX, int startZ, int chunkSize, float noiseScale, int seed)
    {
        float[,] noiseMap = new float[chunkSize, chunkSize];
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                float sampleX = (startX + x + seed) / noiseScale;
                float sampleZ = (startZ + z + seed) / noiseScale;
                float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ);


                noiseMap[x, z] = perlinValue;

            }
        }
        return noiseMap;
    }
}
