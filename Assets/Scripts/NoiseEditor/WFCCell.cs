using System;
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

    private NoiseMeshGenerator _meshGenerator;
    public Gradient colors = new Gradient();

    private Texture2D mapTexture;

    public bool isColapsed = false;

    private Action<Vector2Int, WFCPixel[,], BorderType> OnUpdateNeigtbours;


    public void CraftCell(int mapSize, float maxCellDif, float minCellDif, Vector2Int coordinates, NoiseMeshGenerator meshGenerator, Action<Vector2Int, WFCPixel[,], BorderType> updateNeightbours)
    {
        pixels = new WFCPixel[mapSize, mapSize];
        _mapSize = mapSize;
        this.coordinates = coordinates;
        _minCellDif = minCellDif;
        _maxCellDif = maxCellDif;
        _meshGenerator = meshGenerator;
        mapTexture = new Texture2D(mapSize, mapSize);
        StartMap();

        OnUpdateNeigtbours += updateNeightbours;
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
        PropagateChanges(null);
    }

    public void PropagateChanges(Action onEnd)
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

        onEnd?.Invoke();
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
                isColapsed = true;
                UpdateNeightbours();
                SendHeightMapToMeshGenerator();
                yield break;
            }

            int x = pixelWithLowestEntropy.Value.Item1;
            int y = pixelWithLowestEntropy.Value.Item2;

            pixels[x, y].Collapse();

            PropagateChanges(null);

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

    public void SetGrid(WFCPixel[,] parentGrid)
    {
        for (int y = 0; y < _mapSize; y++)
        {
            for (int x = 0; x < _mapSize; x++)
            {
                int largeGridX = coordinates.x * _mapSize + x;
                int largeGridY = coordinates.y * _mapSize + y;

                pixels[x, y] = parentGrid[largeGridX, largeGridY];
            }
        }
    }

    public void SendHeightMapToMeshGenerator()
    {
        float[,] heightMap = GetHeightMap();
        _meshGenerator.GenerateMesh(heightMap);

        MeshRenderer renderer = _meshGenerator.GetComponent<MeshRenderer>();
        UpdateTexture();
        renderer.material.mainTexture = mapTexture;
    }

    void UpdateTexture()
    {
        for (int y = 0; y < _mapSize; y++)
        {
            for (int x = 0; x < _mapSize; x++)
            {
                float averageValue = pixels[x, y].GetAverageValue();

                averageValue = (averageValue > 1f) ? .99f : averageValue;
                Color color = colors.Evaluate(averageValue);

                mapTexture.SetPixel(x, y, color);
            }
        }

        mapTexture.filterMode = FilterMode.Point;
        mapTexture.Apply();
    }

    public void UpdateNeightbours()
    {
        SetBorders();

    }

    //new int[filas, columnas];
    public void SetBorders()
    {
        WFCPixel[,] right = new WFCPixel[_mapSize, 1];
        WFCPixel[,] left = new WFCPixel[_mapSize, 1];
        WFCPixel[,] top = new WFCPixel[1, _mapSize];
        WFCPixel[,] bot = new WFCPixel[1, _mapSize];

        for (int x = 0; x < _mapSize; x++)
        {

            right[x, 0] = pixels[_mapSize - 1, x];
            left[x, 0] = pixels[0, x];
            top[0, x] = pixels[x, _mapSize - 1];
            bot[0, x] = pixels[x, 0];
        }

        OnUpdateNeigtbours?.Invoke(coordinates + Vector2Int.up, top, BorderType.BOT);
        OnUpdateNeigtbours?.Invoke(coordinates + Vector2Int.down, bot, BorderType.TOP);  
        OnUpdateNeigtbours?.Invoke(coordinates + Vector2Int.right, right, BorderType.LEFT);
        OnUpdateNeigtbours?.Invoke(coordinates + Vector2Int.left, left, BorderType.RIGHT);
    }

    public void UpdateTopBorder(WFCPixel[,] border)
    {
        if (isColapsed) return;

        int y = _mapSize - 1;

        for (int x = 0; x < _mapSize; x++)
        {
            pixels[x, y].CollapseTo(border[0, x].finalValue, true);
        }
    }

    public void UpdateBotBorder(WFCPixel[,] border)
    {
        if (isColapsed) return;

        int y = 0;

        for (int x = 0; x < _mapSize; x++)
        {
            pixels[x, y].CollapseTo(border[0, x].finalValue, true);
        }
    }

    public void UpdateRightBorder(WFCPixel[,] border)
    {
        if (isColapsed) return;

        int x = _mapSize - 1;

        for (int y = 0; y < _mapSize; y++)
        {
            pixels[x, y].CollapseTo(border[y, 0].finalValue, true);
        }
    }

    public void UpdateLeftBorder(WFCPixel[,] border)
    {
        if (isColapsed) return;

        int x = 0;

        for (int y = 0; y < _mapSize; y++)
        {
            pixels[x, y].CollapseTo(border[y, 0].finalValue, true);
        }
    }


}
