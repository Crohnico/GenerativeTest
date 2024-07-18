using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkArea 
{
    public float[,] GenerateChunk(int startX, int startZ, int chunkSize, float noiseScale, int seed, int lod)
    {
        int _lod = lod + 1;

        int size = Mathf.CeilToInt(chunkSize / _lod);

        float[,] noiseMap = new float[size, size];

        for (int x = 0; x < chunkSize; x += _lod)
        {
            for (int z = 0; z < chunkSize; z += _lod)
            {
                if (x > size || z > size) continue;

                float sampleX = (startX + x + seed) / chunkSize;
                float sampleZ = (startZ + z + seed) / chunkSize;
                float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ);

                noiseMap[x, z] = perlinValue * noiseScale;

            }
        }
        return noiseMap;
    }
}
