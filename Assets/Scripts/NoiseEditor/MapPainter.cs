using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public enum BorderType
{
    TOP,
    BOT,
    LEFT,
    RIGHT
}

public class MapPainter : MonoBehaviour
{
    public Vector2Int debugTryCollapse = new Vector2Int(2, 2);

    public RawImage mapDisplay;
    public int mapSize = 50;
    private Texture2D mapTexture;

    public int maxCellDif = 2;
    public float minCellDif;

    [Range(0f, 1f)]
    public float paintHeight = 0f;

    public int chargeDelay = 10;

    [Range(1, 5)]
    public int brushSize = 1;
    public int cellDivisions = 5;

    private WFCCell parentCell;
    private Dictionary<Vector2Int, WFCCell> cells = new Dictionary<Vector2Int, WFCCell>();

    public Slider slider;

    public Image sliderImage;
    public Image sliderBarImage;

    public Toggle toggle;

    public Gradient colors = new Gradient();

    void Start()
    {
        StartMap();
        slider.value = paintHeight;
        slider.onValueChanged.AddListener(SetValue);

        toggle.onValueChanged.AddListener((value) => mapDisplay.enabled = (value));
    }

    public void StartMap()
    {
        for (int x = 0; x < cellDivisions; x++)
        {
            for (int y = 0; y < cellDivisions; y++)
            {
                GameObject go = new GameObject();

                Vector2Int coordinates = new Vector2Int(x, y);
                go.name = $"{coordinates}";

                NoiseMeshGenerator _meshGenerator = go.AddComponent<NoiseMeshGenerator>();

                go.transform.SetParent(meshGenerator.transform);
                go.transform.localPosition = new Vector3((mapSize / cellDivisions) * x - x, 0, (mapSize / cellDivisions) * y - y);
                go.transform.localRotation = Quaternion.identity;

                _meshGenerator.curve = meshGenerator.curve;
                _meshGenerator.heightMultiplier = meshGenerator.heightMultiplier;
                _meshGenerator.uvTileSize = meshGenerator.uvTileSize;

                cells.Add(coordinates, go.AddComponent<WFCCell>());
                cells[coordinates].CraftCell(mapSize / cellDivisions, maxCellDif, minCellDif, coordinates, _meshGenerator, UpdateNeightbours);
                cells[coordinates].colors = colors;
            }
        }

        parentCell = new GameObject().AddComponent<WFCCell>();
        parentCell.CraftCell(mapSize, maxCellDif, minCellDif, new Vector2Int(0, 0), meshGenerator, null);

        CreateMap();
    }

    void Update()
    {
        paintHeight = RoundToNearestMultiple(paintHeight, minCellDif);
        slider.value = paintHeight;

        sliderImage.color = colors.Evaluate(paintHeight);
        sliderBarImage.color = colors.Evaluate(paintHeight);

        UpdateTexture();
    }

    public float RoundToNearestMultiple(float value, float multiple)
    {
        if (multiple == 0)
            return value;

        float roundedValue = Mathf.Round(value / multiple) * multiple;
        return Mathf.Clamp(roundedValue, 0f, 1f);
    }

    void CreateMap()
    {
        mapTexture = new Texture2D(mapSize, mapSize);
        UpdateTexture();
    }

    void UpdateTexture()
    {
        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                float averageValue = parentCell.pixels[x, y].GetAverageValue();

                averageValue = (averageValue > 1f) ? .99f : averageValue;
                Color color = colors.Evaluate(averageValue);

                mapTexture.SetPixel(x, y, color);
            }
        }

        mapTexture.filterMode = FilterMode.Point;
        mapTexture.Apply();
        mapDisplay.texture = mapTexture;
    }

    public void PaintAtPosition(Vector2 pixelPos)
    {
        int x = Mathf.FloorToInt(pixelPos.x);
        int y = Mathf.FloorToInt(pixelPos.y);

        for (int i = 0; i <= brushSize; i++)
        {
            for (int j = 0; j <= brushSize; j++)
            {
                int paintX = x + i;
                int paintY = y + j;

                if (paintX >= 0 && paintX < mapSize && paintY >= 0 && paintY < mapSize)
                {
                    parentCell.CollapsePixel(paintX, paintY, paintHeight);
                }
            }
        }

        UpdateTexture();
    }


    public void StartCraft()
    {
        UpdateChilds();

        cells[debugTryCollapse].StartCraft(chargeDelay);

    }

    public NoiseMeshGenerator meshGenerator;

    public void SetValue(float value)
    {
        paintHeight = value;
    }

    private void UpdateChilds()
    {
        foreach (var child in cells.Values)
        {
            child.SetGrid(parentCell.pixels);
        }
    }

    void UpdateNeightbours(Vector2Int coordinates, WFCPixel[,] matrix, BorderType border)
    {
        if (!cells.ContainsKey(coordinates)) return;

        switch (border)
        {
            case BorderType.TOP:
                cells[coordinates].UpdateTopBorder(matrix);
                break;
            case BorderType.BOT:
                cells[coordinates].UpdateBotBorder(matrix);
                break;
            case BorderType.RIGHT:
                cells[coordinates].UpdateRightBorder(matrix);
                break;
            case BorderType.LEFT:
                cells[coordinates].UpdateLeftBorder(matrix);
                break;
        }

        /*parentCell.PropagateChanges(() =>
        {
            if (cells.ContainsKey(coordinates))
                cells[coordinates].SetGrid(parentCell.pixels);
          });*/
    }

}

[CustomEditor(typeof(MapPainter))]
public class MapPainterEditor : Editor
{

    public override void OnInspectorGUI()
    {

        DrawDefaultInspector();
        EditorGUILayout.Space();
        MapPainter tileInfo = (MapPainter)target;

        if (GUILayout.Button("Collapse"))
        {
            tileInfo.StartCraft();
        }



    }
}
