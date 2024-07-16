using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Biome
{
    public int id;
    public List<NoiseCell> cells;
    public BiomeTemplate template;
    public float[,] NoiseMap { get; set; }

    private int minX = int.MaxValue;
    private int maxX = int.MinValue;
    private int minZ = int.MaxValue;
    private int maxZ = int.MinValue;

    int width;
    int height;

    public Biome(int id)
    {
        this.id = id;
        cells = new List<NoiseCell>();
    }

    public void GenerateNoiseMap()
    {
        foreach (var cell in cells)
        {
            foreach (var pos in cell.CellPositions)
            {
                minX = Mathf.Min(minX, pos.x);
                maxX = Mathf.Max(maxX, pos.x);
                minZ = Mathf.Min(minZ, pos.y);
                maxZ = Mathf.Max(maxZ, pos.y);
            }
        }

        int width = maxX - minX + 1;
        int height = maxZ - minZ + 1;

        NoiseMap = new float[width, height];

        Random.InitState(Seed.seed);
        float biomeNoiseValues = Random.Range(0.0f, 1.0f);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                NoiseMap[x, y] = biomeNoiseValues;
            }
        }

        NoiseMap = template.GetMap(width, height);//template.noiseFiler.GenerateNoiseMap(width, height, template.noiseScale, template.noiseScale);
    }

    public void AddCell(NoiseCell cell)
    {
        cells.Add(cell);
    }

    public void ApplyToGlobalMap(float[,] map)
    {

        foreach (var cell in cells)
        {
            foreach (var pos in cell.CellPositions)
            {
                int localX = pos.x - minX;
                int localZ = pos.y - minZ;

                if (localX >= 0 && localX < NoiseMap.GetLength(0) &&
                    localZ >= 0 && localZ < NoiseMap.GetLength(1))
                {
                    int globalX = pos.x;
                    int globalZ = pos.y;

                    if (globalX >= 0 && globalX < map.GetLength(0) &&
                        globalZ >= 0 && globalZ < map.GetLength(1))
                    {
                        map[globalX, globalZ] = NoiseMap[localX, localZ];
                    }
                }
            }
        }
    }
}
[System.Serializable]
public class NoiseCell
{
    public List<Vector2Int> CellPositions { get; set; }
    public int xID;
    public int zID;
    public int bBiomeID = -1;
    public int groupID = -1;

    public NoiseCell(int startX, int startZ, int sizeX, int sizeZ, int xID, int zID)
    {
        CellPositions = new List<Vector2Int>();

        this.xID = xID;
        this.zID = zID;

        for (int x = startX; x < startX + sizeX; x++)
        {
            for (int z = startZ; z < startZ + sizeZ; z++)
            {
                CellPositions.Add(new Vector2Int(x, z));
            }
        }
    }
}
