using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FallofGenerator 
{
    public static float[,] GenerateFalloffMap(int size, int chunkSize, int xPos, int yPos)
    {
        float[,] map = new float[chunkSize, chunkSize];

        for (int i = 0; i < chunkSize; i++)
        {
            for (int j = 0; j < chunkSize; j++)
            {
                float x = (i + xPos) / (float)size * 2 - 1;
                float y = (j + yPos) / (float)size * 2 - 1;

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
