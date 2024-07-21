using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MergeFilter
{
    public static float[,] SmoothTransition(float[,] noiseMap, int mapSizeX, int mapSizeZ, float maxHeightDifference, int filterWindowSize)
    {
        float[,] smoothedMap = new float[mapSizeX, mapSizeZ];

        for (int x = 0; x < mapSizeX; x++)
        {
            for (int z = 0; z < mapSizeZ; z++)
            {

                if (IsNearTransition(noiseMap, x, z, maxHeightDifference))
                {
                    float averageHeight = ApplyFilter(noiseMap, x, z, filterWindowSize);
                    smoothedMap[x, z] = averageHeight;
                }
                else
                {
                    smoothedMap[x, z] = noiseMap[x, z];
                }
            }
        }

        return smoothedMap;
    }

    private static bool IsNearTransition(float[,] noiseMap, int x, int z, float maxHeightDifference)
    {
        float centerHeight = noiseMap[x, z];


        for (int offsetX = -1; offsetX <= 1; offsetX++)
        {
            for (int offsetZ = -1; offsetZ <= 1; offsetZ++)
            {

                if (offsetX == 0 && offsetZ == 0 || x + offsetX < 0 || x + offsetX >= noiseMap.GetLength(0) || z + offsetZ < 0 || z + offsetZ >= noiseMap.GetLength(1))
                    continue;

                float neighborHeight = noiseMap[x + offsetX, z + offsetZ];
                float heightDifference = Mathf.Abs(centerHeight - neighborHeight);

                if (heightDifference > maxHeightDifference)
                    return true;
            }
        }

        return false;
    }


    private static float ApplyFilter(float[,] noiseMap, int x, int z, int filterWindowSize)
    {
        float sum = 0f;
        int count = 0;

        for (int offsetX = -filterWindowSize; offsetX <= filterWindowSize; offsetX++)
        {
            for (int offsetZ = -filterWindowSize; offsetZ <= filterWindowSize; offsetZ++)
            {
                int neighborX = Mathf.Clamp(x + offsetX, 0, noiseMap.GetLength(0) - 1);
                int neighborZ = Mathf.Clamp(z + offsetZ, 0, noiseMap.GetLength(1) - 1);
                sum += noiseMap[neighborX, neighborZ];
                count++;
            }
        }

        return sum / count;
    }
}
