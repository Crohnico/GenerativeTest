using System;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    private List<IACell> entrances = new List<IACell>();
    private MapParameters mapParameters;

    int mapSizeX, mapSizeZ;

    public void GenerateMap(Grid grid)
    {
        mapParameters = grid.mapParameters;
        mapSizeX = mapParameters.mapSizeX;
        mapSizeZ = mapParameters.mapSizeZ;

        //mapParameters.GenerateNoiseMap();
        GenerateCells();
        GenerateEntrances();
        ForcePathsToExit();
        grid.FinishedGenerating(entrances);
    }

    void GenerateCells()
    {
        int centerX = mapSizeX / 2;
        int centerZ = mapSizeZ / 2;
        float startAreaRadius = mapParameters.startAreaDiameter / 2f;

        for (int x = 0; x < mapSizeX; x++)
        {
            for (int z = 0; z < mapSizeZ; z++)
            {
                Vector3 cellPosition = new Vector3(x - centerX, 0, z - centerZ);

                IACellType cellType;

                // Zona de inicio del jugador (círculo)
                float distanceToCenter = Mathf.Sqrt((x - centerX) * (x - centerX) + (z - centerZ) * (z - centerZ));
                if (distanceToCenter <= startAreaRadius)
                {
                    cellType = mapParameters.fallbackType;
                }
                else if (x <= 2 || x >= mapSizeX - 3 || z <= 2 || z >= mapSizeZ - 3)
                {
                    cellType = mapParameters.DetermineBorderCellType(x, z);
                }
                else
                {
                    cellType = mapParameters.DetermineCellType(x, z, centerX, centerZ);
                }

                IACell cell = new IACell(cellPosition, cellType);
                cell.IsWalkable = mapParameters.IsWalkable(cellType, cell.GridX, cell.GridZ);
                cell.height = mapParameters.YModifier(x, z) * mapParameters.mapHeight;

                Grid.Instance.RegisterGridCell(cell);

            }
        }
    }



    void GenerateEntrances()
    {
        int centerX = mapSizeX / 2;
        int centerZ = mapSizeZ / 2;

        GenerateExit(0, 0);
        List<IACell> listToReverse = new List<IACell>(Grid.Instance.gridCells.Values);

        listToReverse = Shuffle(listToReverse);

        foreach (IACell cell in listToReverse)
        {
            if (!cell.IsWalkable) continue;
            if (Vector2.Distance(new Vector2(0, 0), new Vector2(cell.GridX, cell.GridZ)) < mapParameters.startAreaDiameter*2) continue;
            if (cell.GridZ > (mapSizeZ/2) - 6) continue;
            if (cell.GridX > (mapSizeX/2) - 6) continue;

            cell.Type = IACellType.Exit;
            entrances.Add(cell);
            break;
        }

        List<IACell> Shuffle(List<IACell> list)
        {

            System.Random rng = new System.Random(Seed.seed);
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                IACell value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }
    }

    void GenerateExit(int x, int z)
    {
        IACell cell = Grid.Instance.GetGridCell(x, z);
        cell.Type = IACellType.Exit;
        entrances.Add(cell);
    }

    void ForcePathsToExit()
    {
        foreach (IACell entrance in entrances)
        {
            for (int xOffset = -3; xOffset <= 3; xOffset++)
            {
                for (int zOffset = -3; zOffset <= 3; zOffset++)
                {

                    int xNeighbor = entrance.GridX + xOffset;
                    int zNeighbor = entrance.GridZ + zOffset;


                    if (xNeighbor < mapSizeX && zNeighbor < mapSizeZ)
                    {
                        IACell neighborCell = Grid.Instance.GetGridCell(xNeighbor, zNeighbor);

                        if (neighborCell != null)
                        {
                            if (neighborCell.Type != IACellType.Ground && neighborCell.Type != IACellType.Exit)
                            {
                                neighborCell.Type = IACellType.Ground;
                            }
                        }
                    }
                }
            }
        }
    }
}




