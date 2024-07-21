using ProceduralNoiseProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Noise", menuName = "Map/Noises/Noise")]
public class NoiseSO : ScriptableObject
{
    public NOISE_TYPE noiseType = NOISE_TYPE.PERLIN;

    public int octaves = 4;

    public float frequency = 1.0f;

    public int width = 512;

    public int height = 512;


    public virtual float[,] GenerateNoiseMap(int mapSizeX, int mapSizeZ, float noiseScale, float offset)
    {
        float[,]  noiseMap = new float[mapSizeX, mapSizeZ];
        Random.InitState(Seed.seed);

        INoise noise = GetNoise();
        FractalNoise fractal = new FractalNoise(noise, octaves, frequency);

        float[,] arr = new float[width, height];

        //Sample the 2D noise and add it into a array.
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float fx = x / (mapSizeX - 1.0f);
                float fy = y / (mapSizeZ - 1.0f);

                arr[x, y] = fractal.Sample2D(fx, fy);
            }
        }

        //Some of the noises range from -1-1 so normalize the data to 0-1 to make it easier to see.
        NormalizeArray(arr);

        noiseMap = arr;

        return noiseMap;
    }

    private INoise GetNoise()
    {
        switch (noiseType)
        {
            case NOISE_TYPE.PERLIN:
                return new PerlinNoise( 20);

            case NOISE_TYPE.VALUE:
                return new ValueNoise( 20);

            case NOISE_TYPE.SIMPLEX:
                return new SimplexNoise( 20);

            case NOISE_TYPE.VORONOI:
                return new VoronoiNoise( 20);

            case NOISE_TYPE.WORLEY:
                return new WorleyNoise( 20, 1.0f);

            default:
                return new PerlinNoise( 20);
        }
    }

    private void NormalizeArray(float[,] arr)
    {

        float min = float.PositiveInfinity;
        float max = float.NegativeInfinity;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {

                float v = arr[x, y];
                if (v < min) min = v;
                if (v > max) max = v;

            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float v = arr[x, y];
                arr[x, y] = (v - min) / (max - min);
            }
        }

    }
}
