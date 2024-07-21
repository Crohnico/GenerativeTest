using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class TableGenerator : MonoBehaviour
{
    public enum CellType
    {
        NONSET,
        FLOOR,
        TREE,
        WATER,
        NONE
    }

    [System.Serializable]
    public class EnumPrefab
    {
        public CellType type;
        public GameObject prefab;
    }
    public List<EnumPrefab> prefabs = new List<EnumPrefab>();


    public List<Cell> createdCells = new List<Cell>();
    private List<Cell> cellsToEdit = new List<Cell>();

    public int maxIteration = 200;
    public int iteration = 0;

    public class Cell : MonoBehaviour
    {
        public string name;

        public int x = 0;
        public int z = 0;

        public CellType type;

        [HideInInspector]
        public List<Cell> neightbours = new List<Cell>();

        public int chanceToWater = 0;
        public int chanceToTree = 10;
        public int chanceToNone = 0;
        public int chancheToDefault => GetDefault();

        private const int treeModifier = -15;
        private const int noneModifier = 0;
        private const int waterModifier = 20;

        public void SetUp(Cell cell)
        {
            name = cell.name;

            x = cell.x;
            z = cell.z;

            chanceToWater = cell.chanceToWater;
            chanceToTree = cell.chanceToTree;
            chanceToNone = cell.chanceToNone;

            neightbours = cell.neightbours;

            type = cell.type;
        }
        private void SetModifier()
        {
            foreach (Cell cell in neightbours)
            {
                if (cell.type == CellType.WATER)
                    chanceToWater += waterModifier;
                else if (cell.type == CellType.TREE)
                    chanceToTree += treeModifier;
                else if (cell.type == CellType.NONE)
                    chanceToNone += noneModifier;
            }
        }

        private int GetDefault()
        {
            int defaultValue = 100;
            defaultValue = defaultValue - (chanceToWater + chanceToTree + chanceToNone);

            return (defaultValue < 0) ? 0 : defaultValue;
        }

        public void SetType()
        {
            SetModifier();

            int dice = chanceToWater + chanceToTree + chanceToNone + chancheToDefault;

            int water = chanceToWater;
            int tree = water + chanceToTree;
            int none = tree + chanceToNone;

            int value = Random.Range(0, dice);

            if (value < chanceToWater)
                type = CellType.WATER;
            else if (value < tree)
                type = CellType.TREE;
            else if (value < none)
                type = CellType.NONE;
            else
                type = CellType.FLOOR;
        }
    }

    public void StartCreation()
    {
        createdCells.Clear();

        Cell originCell = new Cell();
        originCell.type = CellType.FLOOR;
        originCell.name = $"{originCell.x},{ originCell.z}";
        createdCells.Add(originCell);

        GenerateTable(originCell);
    }

    public void NextStep()
    {
        StartCoroutine(ReviewCreatedList());
    }

    public bool AlreadyExist(Cell cell)
    {
        foreach (Cell created in createdCells)
            if (created.name == cell.name)
                return true;

        foreach (Cell created in cellsToEdit)
            if (created.name == cell.name)
                return true;


        return false;
    }


    public void GenerateTable(Cell cellOrigin)
    {

        for (int x = -1; x <= 1; x++)
        {
            if (x == 0) continue;

            Cell cell = new Cell();
            cell.x = cellOrigin.x + x;
            cell.z = cellOrigin.z;
            cell.type = CellType.NONSET;
            cell.name = $"{cell.x},{ cell.z}";

            if (AlreadyExist(cell))
                continue;

            cellOrigin.neightbours.Add(cell);
            cell.neightbours.Add(cellOrigin);
            cellsToEdit.Add(cell);
        }

        for (int z = -1; z <= 1; z++)
        {
            if (z == 0) continue;

            Cell cell = new Cell();
            cell.x = cellOrigin.x;
            cell.z = cellOrigin.z + z;
            cell.type = CellType.NONSET;
            cell.name = $"{cell.x},{ cell.z}";

            if (AlreadyExist(cell))
                continue;

            cellOrigin.neightbours.Add(cell);
            cell.neightbours.Add(cellOrigin);
            cellsToEdit.Add(cell);
            cell.type = CellType.NONSET;
        }

    }
    public IEnumerator InstantiateShit()
    {

        foreach (Cell cell in createdCells)
        {
            GameObject prefab = GetEnum(cell);

            if (prefab != null)
            {
                GameObject go = Instantiate(prefab, new Vector3(cell.x, transform.position.y, cell.z), Quaternion.Euler(new Vector3(90, 0, 0)));
                go.transform.SetParent(transform);
                Cell newCell = go.AddComponent<Cell>();
                newCell.SetUp(cell);
                yield return null;
            }
        }

        GameObject GetEnum(Cell cell)
        {
            foreach (EnumPrefab ePrefab in prefabs)
            {
                if (ePrefab.type == cell.type)
                    return ePrefab.prefab;
            }

            return null;
        }


    }
    public IEnumerator ReviewCreatedList()
    {
        int nextCell = Random.Range(0, cellsToEdit.Count - 1);

        if (cellsToEdit.Count == 1)
            nextCell = 1;
        else if(cellsToEdit.Count <= 0)
        {
            StartCoroutine(InstantiateShit());
            yield break;
        }

        Cell cell = cellsToEdit[nextCell];

        cell.SetType();

        createdCells.Add(cell);
        cellsToEdit.Remove(cell);


        foreach (Cell passedCells in cellsToEdit)
        {
            if (passedCells.type == CellType.NONSET)
                passedCells.chanceToNone += 1;
        }

        if (cell.type == CellType.NONE)
        {
            StartCoroutine(ReviewCreatedList());
            yield break;
        }

        List<Cell> generatedCell = new List<Cell>();
        generatedCell.Add(cell);


        yield return StartCoroutine(CreateCells(generatedCell));


        iteration++;

        if (iteration < maxIteration)
        {
            StartCoroutine(ReviewCreatedList());
        }
        else { StartCoroutine(InstantiateShit()); }

    }


    public IEnumerator CreateCells(List<Cell> lastCreated)
    {
        foreach (Cell cell in lastCreated)
        {
            if (cell.type == CellType.NONE)
                continue;

            GenerateTable(cell);
        }

        yield return null;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TableGenerator))]
class DecalMeshHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var myScript = target as TableGenerator;

        if (GUILayout.Button("StartCreation"))
            myScript.StartCreation();

        if (GUILayout.Button("NextStep"))
            myScript.NextStep();
    }
}
#endif