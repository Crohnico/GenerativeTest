using System;
using System.Collections.Generic;
using UnityEngine;

public class KMeans
{
    public static List<NoiseCell>[,] ClusterCells(List<NoiseCell>[,] cellGrid, int numBiomes, int maxIterations = 100)
    {
        int width = cellGrid.GetLength(0);
        int height = cellGrid.GetLength(1);

        // Inicialización de centroides aleatorios
        List<Vector2Int> centroids = InitializeCentroids(cellGrid, numBiomes);

        // Inicialización de la asignación de celdas a biomas
        int[,] assignments = new int[width, height];

        // Iteraciones del algoritmo
        for (int iter = 0; iter < maxIterations; iter++)
        {
            // Asignar celdas a biomas basado en la distancia a los centroides
            UpdateAssignments(cellGrid, centroids, assignments);

            // Actualizar la posición de los centroides basado en las celdas asignadas
            UpdateCentroids(cellGrid, centroids, assignments);

            // Comprobar si no hubo cambios en la asignación de celdas
            if (CheckConvergence(cellGrid, centroids, assignments))
            {
                Debug.Log("Convergencia alcanzada en la iteración " + (iter + 1));
                break;
            }
        }

        // Agrupar celdas por bioma
        List<NoiseCell>[,] clusteredGrid = GroupCellsByBiome(cellGrid, assignments, numBiomes);

        return clusteredGrid;
    }

    private static List<Vector2Int> InitializeCentroids(List<NoiseCell>[,] cellGrid, int numBiomes)
    {
        List<Vector2Int> centroids = new List<Vector2Int>();
        int width = cellGrid.GetLength(0);
        int height = cellGrid.GetLength(1);

        for (int i = 0; i < numBiomes; i++)
        {
            int x = UnityEngine.Random.Range(0, width);
            int z = UnityEngine.Random.Range(0, height);
            centroids.Add(new Vector2Int(x, z));
        }

        return centroids;
    }

    private static void UpdateAssignments(List<NoiseCell>[,] cellGrid, List<Vector2Int> centroids, int[,] assignments)
    {
        int width = cellGrid.GetLength(0);
        int height = cellGrid.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                float minDistance = float.MaxValue;
                int nearestBiome = -1;

                for (int i = 0; i < centroids.Count; i++)
                {
                    float distance = Vector2Int.Distance(centroids[i], new Vector2Int(x, z));
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestBiome = i;
                    }
                }

                assignments[x, z] = nearestBiome;
            }
        }
    }

    private static void UpdateCentroids(List<NoiseCell>[,] cellGrid, List<Vector2Int> centroids, int[,] assignments)
    {
        int numBiomes = centroids.Count;
        int[] numCellsInBiome = new int[numBiomes];
        Vector2Int[] centroidSum = new Vector2Int[numBiomes];

        int width = cellGrid.GetLength(0);
        int height = cellGrid.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                int biome = assignments[x, z];
                numCellsInBiome[biome]++;
                centroidSum[biome] += new Vector2Int(x, z);
            }
        }

        for (int i = 0; i < numBiomes; i++)
        {
            centroids[i] = centroidSum[i] / numCellsInBiome[i];
        }
    }

    private static bool CheckConvergence(List<NoiseCell>[,] cellGrid, List<Vector2Int> centroids, int[,] assignments)
    {
        return false;
    }

    private static List<NoiseCell>[,] GroupCellsByBiome(List<NoiseCell>[,] cellGrid, int[,] assignments, int numBiomes)
    {
        int width = cellGrid.GetLength(0);
        int height = cellGrid.GetLength(1);

        List<NoiseCell>[,] clusteredGrid = new List<NoiseCell>[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                int biome = assignments[x, z];
                if (clusteredGrid[x, z] == null)
                {
                    clusteredGrid[x, z] = new List<NoiseCell>();
                }
                clusteredGrid[x, z].AddRange(cellGrid[x, z]);
            }
        }

        return clusteredGrid;
    }
}
