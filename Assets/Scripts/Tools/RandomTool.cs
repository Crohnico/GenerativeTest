using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RandomTool
{
    public static float GenerateRandomFloatInRange(System.Random prng, float min, float max)
    {
        return (float)(prng.NextDouble() * (max - min) + min);
    }
}
