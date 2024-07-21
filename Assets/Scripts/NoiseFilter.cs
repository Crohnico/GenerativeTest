using UnityEngine;

public static class NoiseFilter
{
    public static float[,] ApplyMedianFilter(float[,] noiseMap, int windowSize)
    {
        int mapSizeX = noiseMap.GetLength(0);
        int mapSizeZ = noiseMap.GetLength(1);

        float[,] filteredMap = new float[mapSizeX, mapSizeZ];


        for (int x = 0; x < mapSizeX; x++)
        {
            for (int z = 0; z < mapSizeZ; z++)
            {

                float[] neighborValues = GetNeighborValues(noiseMap, x, z, windowSize);
                float medianValue = CalculateMedian(neighborValues);

                filteredMap[x, z] = medianValue;
            }
        }

        return filteredMap;
    }

    private static float[] GetNeighborValues(float[,] noiseMap, int centerX, int centerZ, int windowSize)
    {
        int mapSizeX = noiseMap.GetLength(0);
        int mapSizeZ = noiseMap.GetLength(1);

        // Calcula el índice inicial y final para recorrer los vecinos
        int startIndexX = Mathf.Max(0, centerX - windowSize / 2);
        int endIndexX = Mathf.Min(mapSizeX - 1, centerX + windowSize / 2);
        int startIndexZ = Mathf.Max(0, centerZ - windowSize / 2);
        int endIndexZ = Mathf.Min(mapSizeZ - 1, centerZ + windowSize / 2);

        // Almacena los valores de los vecinos en un arreglo
        float[] neighborValues = new float[(endIndexX - startIndexX + 1) * (endIndexZ - startIndexZ + 1)];
        int index = 0;
        for (int x = startIndexX; x <= endIndexX; x++)
        {
            for (int z = startIndexZ; z <= endIndexZ; z++)
            {
                neighborValues[index] = noiseMap[x, z];
                index++;
            }
        }

        return neighborValues;
    }

    private static float CalculateMedian(float[] values)
    {

        System.Array.Sort(values);

        int middleIndex = values.Length / 2;
        if (values.Length % 2 == 0)
        {
            return (values[middleIndex - 1] + values[middleIndex]) / 2f;
        }
        else
        {
            return values[middleIndex];
        }
    }
}

