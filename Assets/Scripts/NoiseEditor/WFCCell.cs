using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFCCell : MonoBehaviour
{
    public WFCPixel[,] pixels;
    private int _mapSize;
    private float _loadDelay;
    public Vector2Int coordinates;

    private float _minCellDif;
    private float _maxCellDif;


    public void CraftCell (int mapSize, float maxCellDif, float minCellDif, Vector2Int coordinates) 
    {
        pixels = new WFCPixel[mapSize, mapSize];
        _mapSize = mapSize;
        this.coordinates = coordinates;
        _minCellDif = minCellDif;
        _maxCellDif = maxCellDif;
        StartMap();
    }

    public void StartMap()
    {
        for (int y = 0; y < _mapSize; y++)
        {
            for (int x = 0; x < _mapSize; x++)
            {
                pixels[x, y] = new WFCPixel(_maxCellDif, _minCellDif);
            }
        }
    }
    
    public void CollapsePixel(int x, int y, float value)
    {
        pixels[x, y].CollapseTo(value);
        PropagateChanges();
    }

    public void PropagateChanges()
    {
        Queue<(int, int)> pixelsToProcess = new Queue<(int, int)>();

        for (int y = 0; y < _mapSize; y++)
        {
            for (int x = 0; x < _mapSize; x++)
            {
                if (pixels[x, y].collapsed)
                {
                    pixelsToProcess.Enqueue((x, y));
                }
            }
        }

        while (pixelsToProcess.Count > 0)
        {
            var (x, y) = pixelsToProcess.Dequeue();

            float collapsedValue = pixels[x, y].finalValue;

            if (RestrictNeighbor(x + 1, y, pixels[x, y])) pixelsToProcess.Enqueue((x + 1, y));
            if (RestrictNeighbor(x - 1, y, pixels[x, y])) pixelsToProcess.Enqueue((x - 1, y));
            if (RestrictNeighbor(x, y + 1, pixels[x, y])) pixelsToProcess.Enqueue((x, y + 1));
            if (RestrictNeighbor(x, y - 1, pixels[x, y])) pixelsToProcess.Enqueue((x, y - 1));
        }
    }

    bool RestrictNeighbor(int x, int y, WFCPixel neighbor)
    {
        if (x >= 0 && x < _mapSize && y >= 0 && y < _mapSize && !pixels[x, y].collapsed)
        {
            int previousEntropy = pixels[x, y].Entropy;
            pixels[x, y].UpdatePossibles(neighbor.MinValue, neighbor.MaxValue);

            return pixels[x, y].Entropy != previousEntropy;
        }

        return false;
    }

    public void StartCraft(float loadDelay)
    {
        _loadDelay = loadDelay;
        StartCoroutine(StartCollapse());
    }
    public IEnumerator StartCollapse()
    {
        int index = 0;
        while (true)
        {

            (int, int)? pixelWithLowestEntropy = FindPixelWithLowestEntropy();

            if (pixelWithLowestEntropy == null)
            {
                Debug.Log("Colapso completado");
                yield break;
            }

            int x = pixelWithLowestEntropy.Value.Item1;
            int y = pixelWithLowestEntropy.Value.Item2;

            pixels[x, y].Collapse();

            PropagateChanges();

            index++;

            if (index >= _loadDelay)
            {
                yield return null;
                index = 0;
            }

        }
    }

    private (int, int)? FindPixelWithLowestEntropy()
    {
        int? minEntropy = null;
        (int, int)? pixelWithLowestEntropy = null;

        for (int y = 0; y < _mapSize; y++)
        {
            for (int x = 0; x < _mapSize; x++)
            {
                if (!pixels[x, y].collapsed)
                {
                    int entropy = pixels[x, y].Entropy;


                    if (minEntropy == null || entropy < minEntropy)
                    {
                        minEntropy = entropy;
                        pixelWithLowestEntropy = (x, y);
                    }
                }
            }
        }

        return pixelWithLowestEntropy;
    }

    public float[,] GetHeightMap()
    {
        float[,] heightMap = new float[_mapSize, _mapSize];

        for (int y = 0; y < _mapSize; y++)
        {
            for (int x = 0; x < _mapSize; x++)
            {
                heightMap[x, y] = pixels[x, y].finalValue;
            }
        }

        return heightMap;
    }
}
