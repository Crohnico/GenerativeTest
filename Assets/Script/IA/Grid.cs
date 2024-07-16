using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public MapParameters mapParameters;

    public Dictionary<(int, int), IACell> gridCells = new Dictionary<(int, int), IACell>();
    public List<IACell> walkableGridCelss = new List<IACell>();
    public List<IACell> entrances = new List<IACell>();
    private Vector3 cellRotationAngles = new Vector3(90, 0, 0);

    private Dictionary<Vector2Int, CellBehaviour> instances = new Dictionary<Vector2Int, CellBehaviour>();

    public static Grid Istance;

    private void Awake()
    {
        if (Istance == null) Istance = this;
        else Destroy(gameObject);
    }
    private void OnDestroy()
    {
        Istance = null;
    }
    public void Initialize(MapParameters mapParameters)
    {
        this.mapParameters = mapParameters;

        mapParameters.Generate += GenerateMap;
        mapParameters.OnEndGeneration += mapParameters.ChangeTextures;
    }

    private void GenerateMap()
    {
        MapGenerator mapGenerator = GetComponent<MapGenerator>();
        mapGenerator.GenerateMap(this);
    }

    public void FinishedGenerating(List<IACell> entrances)
    {
        this.entrances = entrances;
        mapParameters.OnEndGeneration?.Invoke();
    }

    public void AddToPool(Vector2Int position, CellBehaviour go)
    {
        if (instances.ContainsKey(position))
            instances[position] = go;
        else
            instances.Add(position, go);
    }

    public void RemoveFromPool(Vector2Int position)
    {
        CellBehaviour go = instances[position];

        if (go != null)
            Destroy(go.gameObject);

        instances[position] = null;
    }



    public void InstantiateCells(Vector2Int playerPos, int size)
    {
        int playerX = playerPos.x;
        int playerY = playerPos.y;

        int yNegative = -8;

        for (int xOffset = -(size / 2); xOffset <= (size / 2); xOffset++)
        {
            for (int yOffset = yNegative; yOffset <= (size / 2); yOffset++)
            {
                int x = xOffset + playerX;
                int y = yOffset + playerY;

                IACell cell = GetGridCell(x, y);

                if (cell == null) continue;

                UpdateSeashore(cell);

                Vector3 cellPosition = cell.Position;
                Quaternion cellRotation = Quaternion.Euler(cellRotationAngles);

                Vector2Int position = new Vector2Int(x, y);
                bool deinstantiateY = ((yOffset) == Mathf.Abs(size / 2) || yOffset == yNegative);
                bool desintantiateX = Mathf.Abs(xOffset) == Mathf.Abs(size / 2);

                if (deinstantiateY || desintantiateX)
                {
                    if (!instances.ContainsKey(position)) continue;

                    cell.IsOnSight = false;

                    RemoveFromPool(position);
                }
                else
                {
                    if (instances.ContainsKey(position) && instances[position] != null)
                    {
                        continue;
                    }
                    GameObject prefab = mapParameters.GetPrefab(cell.Type, cell.GridX, cell.GridZ);
                    if (prefab == null) continue;

                    GameObject newCell = Instantiate(prefab, cellPosition + cell.height * Vector3.up, cellRotation, transform);
                    CellBehaviour cBehaviour = newCell.GetComponent<CellBehaviour>();
                    cell.isEatable = newCell.TryGetComponent(out CellGrassEatable grass);

                    cBehaviour.SetUp(cell);
                    cell.IsOnSight = true;

                    AddToPool(position, cBehaviour);
                }
            }
        }      
    }

    public void UpdateSeashore(IACell cell)
    {
        if (cell.Type != IACellType.Water) return;

        foreach (var coordinates in cell.neighborCoordinates)
        {
            IACell nCell = GetGridCell(coordinates.Item1, coordinates.Item2);

            if (nCell == null) continue;

            if (!nCell.isSeashore)
                nCell.isSeashore = nCell.IsWalkable;
        }
    }

    public IACell GetRandomExit()
    {
        Random.InitState(Seed.seed);
        int randomIndex = Random.Range(0, entrances.Count);
        return entrances[randomIndex];
    }

    public void RegisterGridCell(IACell cell)
    {
        int x = cell.GridX;
        int z = cell.GridZ;


        if (!gridCells.ContainsKey((x, z)))
        {
            gridCells.Add((x, z), cell);
           if(cell.IsWalkable) walkableGridCelss.Add(cell);

            DetermineNeighbors(cell);
        }
        else
        {
            Debug.LogWarning("La casilla en las coordenadas (" + x + ", " + z + ") ya está registrada en el diccionario.");
        }
    }

    private void DetermineNeighbors(IACell cell)
    {
        int x = cell.GridX;
        int z = cell.GridZ;

        cell.neighborCoordinates = new List<(int, int)>()
        {
            (x - 1, z),
            (x + 1, z),
            (x, z - 1),
            (x, z + 1)
        };
    }


    public IACell GetGridCell(int x, int z)
    {
        if (gridCells.ContainsKey((x, z)))
        {
            return gridCells[(x, z)];
        }
        else
        {
            return null;
        }
    }
}
