using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BiomeSetter
{
    // Diccionario para mapear nombres/identificadores a objetos Biome
    private Dictionary<int, Biome> biomeDictionary = new Dictionary<int, Biome>();

    private Dictionary<Vector2Int, NoiseCell> cells = new Dictionary<Vector2Int, NoiseCell>();

    private Dictionary<Vector2Int, int> cellBiomeMap = new Dictionary<Vector2Int, int>();
    private float spreadStrength = 0.8f;

    private int biomesAmount;
    private int mapSizeX;
    private int mapSizeZ;


    private Vector2Int[] neighborOffsets = new Vector2Int[]
            {
               new Vector2Int(-1, 0),
               new Vector2Int(1, 0),
               new Vector2Int(0, -1),
               new Vector2Int(0, 1)
            };

    public List<Biome> Initialize(int globalMapSizeX, int globalMapSizeZ, Vector2Int gridSize, int biomeAmount)
    {
        mapSizeX = globalMapSizeX;
        mapSizeZ = globalMapSizeZ;
        biomesAmount = biomeAmount;

        List<NoiseCell>[,] noiseCells = DivideMap(globalMapSizeX, globalMapSizeZ, gridSize.x, gridSize.y);

        StartAssingingBiomes();
        CheckForIsolatesBiomes();

        ConsolidateAdjacentBiomes();

        List<Biome> returnList = new List<Biome>();

        foreach (var biome in biomeDictionary)
        {
            if (biome.Value.cells == null || biome.Value.cells.Count == 0) continue;

            returnList.Add(biome.Value);
        }
        return returnList;
    }
    private List<NoiseCell>[,] DivideMap(int globalMapSizeX, int globalMapSizeZ, int cellSizeX, int cellSizeZ)
    {
        int numCellsX = Mathf.CeilToInt((float)globalMapSizeX / cellSizeX);
        int numCellsZ = Mathf.CeilToInt((float)globalMapSizeZ / cellSizeZ);


        List<NoiseCell>[,] cellGrid = new List<NoiseCell>[numCellsX, numCellsZ];

        for (int x = 0; x < numCellsX; x++)
        {
            for (int z = 0; z < numCellsZ; z++)
            {
                int startX = x * cellSizeX;
                int startZ = z * cellSizeZ;

                int sizeX = Mathf.Min(cellSizeX, globalMapSizeX - startX);
                int sizeZ = Mathf.Min(cellSizeZ, globalMapSizeZ - startZ);

                cellGrid[x, z] = new List<NoiseCell>();
                NoiseCell cell = new NoiseCell(startX, startZ, sizeX, sizeZ, x, z);
                cells.Add(new Vector2Int(x, z), cell);
                cellGrid[x, z].Add(cell);
            }
        }

        return cellGrid;
    }

    private void StartAssingingBiomes()
    {
        Random.InitState(Seed.seed);
        int initCell = Random.Range((int)0, (int)cells.Count - 1);
        List<Vector2Int> keys = new List<Vector2Int>(cells.Keys);

        Vector2Int randomKey = keys[initCell];
        NoiseCell randomCell = cells[randomKey];


        int biomeID = 0;
        int groupID = 0;
        spreadStrength = 0.8f;

        SpreadBiomesWithQueue(randomCell, biomeID, groupID);
    }

    private void SpreadBiomesWithQueue(NoiseCell initialCell, int biomeID, int groupID)
    {
        Queue<NoiseCell> cellQueue = new Queue<NoiseCell>();
        spreadStrength = 0.8f;
        cellQueue.Enqueue(initialCell);

        while (cellQueue.Count > 0)
        {
            NoiseCell currentCell = cellQueue.Dequeue();

            // Ejecutar el método SpreadBiomes para la celda actual
            SpreadBiomes(currentCell, biomeID, groupID, cellQueue);
        }

        NoiseCell cell = cells.Values.FirstOrDefault(x => x.bBiomeID == -1);

        if (cell != null)
        {
            biomeID = (biomeID + 1) % biomesAmount;
            groupID += 1;
            SpreadBiomesWithQueue(cell, biomeID, groupID);
        }
    }

    private void SpreadBiomes(NoiseCell cell, int biomeID, int groupID, Queue<NoiseCell> cellQueue)
    {
        Random.InitState(Seed.seed.GetHashCode() + cell.xID.GetHashCode() + cell.zID.GetHashCode());

        if (!CanSpreadOut(cell.xID, cell.zID))
        {
            int neighborBiomeID = GetNeighborBiomeID(cell.xID, cell.zID);
            SetMapPositionToBiome(cell.xID, cell.zID, neighborBiomeID);
            AddCellToBiome(groupID, cell);
            return;
        }
        else
        {
            cell.bBiomeID = biomeID;
            SetMapPositionToBiome(cell.xID, cell.zID, biomeID);
            AddCellToBiome(groupID, cell);
        }

        foreach (var neighborOffset in neighborOffsets)
        {
            int newX = cell.xID + neighborOffset.x;
            int newZ = cell.zID + neighborOffset.y;

            NoiseCell neighborCell = GetCellAtCoordinate(newX, newZ);

            if (neighborCell != null && neighborCell.bBiomeID == -1)
            {
                float randomValue = Random.value;

                if (randomValue <= spreadStrength)
                {
                    spreadStrength -= Random.Range(0f, 0.1f);
                    cellQueue.Enqueue(neighborCell);
                }
            }

        }

    }

    private void CheckForIsolatesBiomes()
    {
        List<Biome> isolatesBiomes = new List<Biome>();

        foreach (Biome biome in biomeDictionary.Values)
        {
            if (biome.cells.Count == 1)
            {
                isolatesBiomes.Add(biome);
            }
        }

        foreach (Biome b in isolatesBiomes)
        {
            DeleteFromBiomeDictionary(b);

            foreach (NoiseCell cell in b.cells)
            {
                NoiseCell neighbor = GetNeighborBiomeNoise(cell.xID, cell.zID);
                SetMapPositionToBiome(cell.xID, cell.zID, neighbor.bBiomeID);
                AddCellToBiome(neighbor.groupID, cell);
            }
        }
    }


    private bool IsWithinMapBounds(int x, int z)
    {
        return x >= 0 && x < mapSizeX && z >= 0 && z < mapSizeZ;
    }
    private NoiseCell GetCellAtCoordinate(int x, int y)
    {
        NoiseCell cell = null;
        Vector2Int id = new Vector2Int(x, y);

        if (cells.ContainsKey(id))
        {
            return cells[id];
        }

        return cell;
    }

    private void SetMapPositionToBiome(int x, int y, int biomeID)
    {
        NoiseCell cell = GetCellAtCoordinate(x, y);
        cell.bBiomeID = biomeID;

        foreach (Vector2Int position in cell.CellPositions)
        {
            Vector2Int id = new Vector2Int(position.x, position.y);

            if (!cellBiomeMap.ContainsKey(id))
            {
                cellBiomeMap.Add(id, biomeID);
            }
            else { cellBiomeMap[id] = biomeID; }
        }
    }

    private void ConsolidateAdjacentBiomes()
    {
        List<Biome> BiomesToRemove = new List<Biome>();

        foreach (Biome currentBiome in biomeDictionary.Values.ToList())
        {
            if (BiomesToRemove.Contains(currentBiome)) continue;

            foreach (var cell in currentBiome.cells.ToList())
            {
                Biome neightborBiome = IsAdjacentToSameBiome(cell);

                if (BiomesToRemove.Contains(neightborBiome)) continue;

                if (neightborBiome != null)
                {
                    BiomesToRemove.Add(neightborBiome);
                    currentBiome.cells.AddRange(neightborBiome.cells);
                    break;
                }
            }
        }

        foreach (Biome b in BiomesToRemove)
        {
            DeleteFromBiomeDictionary(b);
        }
    }

    private Biome IsAdjacentToSameBiome(NoiseCell cell)
    {
        Biome biome = null;

        foreach (var neighborOffset in neighborOffsets)
        {
            int newX = cell.xID + neighborOffset.x;
            int newZ = cell.zID + neighborOffset.y;

            NoiseCell neighborCell = GetCellAtCoordinate(newX, newZ);

            if (neighborCell != null && neighborCell.bBiomeID == cell.bBiomeID && neighborCell.groupID != cell.groupID)
            {

                return GetBiomeAtID(neighborCell.groupID);
            }

        }

        return null;
    }


    private bool CanSpreadOut(int x, int z)
    {
        List<NoiseCell> neightBours = new List<NoiseCell>();

        for (int xOffset = -1; xOffset <= 1; xOffset++)
        {
            for (int zOffset = -1; zOffset <= 1; zOffset++)
            {
                if (xOffset == 0 && zOffset == 0) continue;
                if (xOffset != 0 && zOffset != 0) continue;

                int newX = x + xOffset;
                int newZ = z + zOffset;

                NoiseCell cell = GetCellAtCoordinate(newX, newZ);

                if (cell != null) neightBours.Add(cell);
            }
        }

        return neightBours.Any(cell => cell.bBiomeID == -1);
    }

    private int GetNeighborBiomeID(int x, int z)
    {
        int biomeID = -1;
        List<NoiseCell> neightBours = new List<NoiseCell>();

        for (int xOffset = -1; xOffset <= 1; xOffset++)
        {
            for (int zOffset = -1; zOffset <= 1; zOffset++)
            {
                if (xOffset == 0 && zOffset == 0) continue;
                if (xOffset != 0 && zOffset != 0) continue;

                int newX = x + xOffset;
                int newZ = z + zOffset;

                NoiseCell cell = GetCellAtCoordinate(newX, newZ);

                if (cell != null) neightBours.Add(cell);
            }
        }
        if (neightBours.Count != 0) biomeID = neightBours[0].bBiomeID;

        return biomeID;
    }

    private NoiseCell GetNeighborBiomeNoise(int x, int z)
    {
        NoiseCell neight = null;
        List<NoiseCell> neightBours = new List<NoiseCell>();

        for (int xOffset = -1; xOffset <= 1; xOffset++)
        {
            for (int zOffset = -1; zOffset <= 1; zOffset++)
            {
                if (xOffset == 0 && zOffset == 0) continue;
                if (xOffset != 0 && zOffset != 0) continue;

                int newX = x + xOffset;
                int newZ = z + zOffset;

                NoiseCell cell = GetCellAtCoordinate(newX, newZ);

                if (cell != null) neightBours.Add(cell);
            }
        }
        if (neightBours.Count != 0) neight = neightBours[0];

        return neight;
    }

    #region BiomeUtils
    private Biome GetBiomeAtID(int id)
    {
        if (biomeDictionary.ContainsKey(id))
        {
            return biomeDictionary[id];
        }
        else
        {
            Biome newBiome = new Biome(id);
            newBiome.cells = new List<NoiseCell>();
            biomeDictionary.Add(id, newBiome);
            return newBiome;
        }
    }

    public void AddCellToBiome(int biomeId, NoiseCell cell)
    {
        cell.groupID = biomeId;
        Biome biome = null;

        if (biomeDictionary.ContainsKey(biomeId))
        {
            biome = biomeDictionary[biomeId];

            if (!biome.cells.Contains(cell))
                biome.AddCell(cell);
        }
        else
        {
            biome = new Biome(biomeId);
            biome.AddCell(cell);
            biomeDictionary.Add(biomeId, biome);
        }
    }

    private void DeleteFromBiomeDictionary(Biome biome)
    {
        int keyToRemove = biomeDictionary.First(kvp => kvp.Value == biome).Key;
        biomeDictionary.Remove(keyToRemove);
    }


    #endregion
}
