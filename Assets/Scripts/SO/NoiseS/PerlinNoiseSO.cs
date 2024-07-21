using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Perlin Noise", menuName = "Map/Noises/Perlin Noise")]
public class PerlinNoiseSO : NoiseSO
{
    public override float[,] GenerateNoiseMap(int mapSizeX, int mapSizeZ, float noiseScale, float offset)
    {
        float[,] noiseMap = new float[mapSizeX, mapSizeZ];

        Random.InitState(Seed.seed);

        float offsetX = UnityEngine.Random.Range(0f, 99999f);
        float offsetY = UnityEngine.Random.Range(0f, 99999f);

        int centerX = mapSizeX / 2;
        int centerZ = mapSizeZ / 2;

        for (int x = 0; x < mapSizeX; x++)
        {
            for (int z = 0; z < mapSizeZ; z++)
            {
                float xCoord = ((float)x - centerX) / mapSizeX * noiseScale + offsetX;
                float zCoord = ((float)z - centerZ) / mapSizeZ * noiseScale + offsetY;

                float noiseValue = Mathf.PerlinNoise(xCoord, zCoord);
                float value = Mathf.Clamp(noiseValue + offset, 0f, 1f);
                noiseMap[x, z] = value;//Mathf.Clamp(0f,1f,noiseValue + offset);
            }
        }

        return noiseMap;
    }
}
