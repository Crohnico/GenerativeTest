using System.Collections.Generic;
using UnityEngine;

public class AStar
{
    public static List<Vector3> FindPath(IACell startPosition, IACell targetPosition)
    {
        List<Vector3> path = new List<Vector3>();

        IACell startCell = startPosition;
        IACell targetCell = targetPosition;


        List<IACell> openSet = new List<IACell>();
        HashSet<IACell> closedSet = new HashSet<IACell>();
        openSet.Add(startCell);

        int iterations = 0;

        while (openSet.Count > 0)
        {
            iterations++;

            if (iterations > 50)
            {
                path = RetracePath(startCell, openSet[openSet.Count - 1]);
                return path;
            }

            IACell currentCell = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost < currentCell.FCost || (openSet[i].FCost == currentCell.FCost && openSet[i].HCost < currentCell.HCost))
                {
                    currentCell = openSet[i];
                }
            }

            openSet.Remove(currentCell);
            closedSet.Add(currentCell);

            if (currentCell == targetCell)
            {
                path = RetracePath(startCell, targetCell);
                break;
            }

            List<(int, int)> neighborCoordinates  = currentCell.neighborCoordinates;//GetNeighbors(grid, gridCells, currentCell);
            foreach (var cordinate in neighborCoordinates)
            {
                IACell neighbor = Grid.Instance.GetGridCell(cordinate.Item1, cordinate.Item2);

                if (neighbor == null) continue;

                if (!neighbor.IsWalkable || closedSet.Contains(neighbor))
                {
                    continue;
                }

                float newCostToNeighbor = currentCell.GCost + GetDistance(currentCell, neighbor);
                if (newCostToNeighbor < neighbor.GCost || !openSet.Contains(neighbor))
                {
                    neighbor.GCost = newCostToNeighbor;
                    neighbor.HCost = GetDistance(neighbor, targetCell);
                    neighbor.Parent = currentCell;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        return path;
    }
    private static List<Vector3> RetracePath(IACell startCell, IACell targetCell)
    {
        List<Vector3> path = new List<Vector3>();
        IACell currentCell = targetCell;

        // Agregar la posición de la celda de inicio al principio del camino
        path.Add(startCell.Position);

        while (currentCell != startCell)
        {
            path.Add(new Vector3(currentCell.Position.x, currentCell.height, currentCell.Position.z));
            currentCell = currentCell.Parent;
        }
        path.Reverse();
        return path;
    }




    private static List<IACell> GetNeighbors(Grid grid, List<IACell> gridCells, IACell cell)
    {
        List<IACell> neighbors = new List<IACell>();

        int cellX = cell.GridX;
        int cellZ = cell.GridZ;

        for (int xOffset = -1; xOffset <= 1; xOffset++)
        {
            for (int zOffset = -1; zOffset <= 1; zOffset++)
            {
                if (xOffset == 0 && zOffset == 0)
                {
                    continue;
                }
                if (xOffset != 0 && zOffset != 0) continue;

                int checkX = cellX + xOffset;
                int checkZ = cellZ + zOffset;


                if (Mathf.Abs(checkX) < (grid.mapParameters.mapSizeX / 2) + 1 && Mathf.Abs(checkZ) < (grid.mapParameters.mapSizeZ / 2) + 1)
                {
                    IACell neighbor = grid.GetGridCell(checkX, checkZ);
                    if (neighbor != null)
                    {
                        neighbors.Add(neighbor);
                    }
                }
            }
        }

        return neighbors;
    }



    private static int GetDistance(IACell cellA, IACell cellB)
    {
        int dstX = Mathf.Abs(cellA.GridX - cellB.GridX);
        int dstZ = Mathf.Abs(cellA.GridZ - cellB.GridZ);

        return dstX + dstZ;
    }

}

