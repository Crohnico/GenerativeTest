using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MapPainter : MonoBehaviour
{
    public RawImage mapDisplay;
    public int mapSize = 50;
    private Texture2D mapTexture;

    public float maxCellDif;
    public float minCellDif;

    [Range(0f, 1f)]
    public float paintHeight = 0f;

    public int chargeDelay = 10;

    [Range(1, 5)]
    public int brushSize = 1;
    public int cellDivisions = 5;
    private WFCPixel[,] pixels;

    private WFCCell newCell;

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
        pixels = new WFCPixel[mapSize, mapSize];

        newCell = new GameObject().AddComponent<WFCCell>();
        newCell.CraftCell(mapSize, maxCellDif, minCellDif, new Vector2Int(0, 0));

        CreateMap();
    }

    void Update()
    {
        paintHeight = RoundToNearestMultiple(paintHeight, minCellDif);
        slider.value = paintHeight;

        sliderImage.color = colors.Evaluate(paintHeight);
        sliderBarImage.color = colors.Evaluate(paintHeight);

        pixels = newCell.pixels;
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
                float averageValue = newCell.pixels[x, y].GetAverageValue();

                averageValue = (averageValue > 1f) ? .99f : averageValue;
                Color color = colors.Evaluate(averageValue);

                mapTexture.SetPixel(x, y, color);
            }
        }

        mapTexture.filterMode = FilterMode.Point;
        mapTexture.Apply();
        mapDisplay.texture = mapTexture;
        SendHeightMapToMeshGenerator();
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
                    newCell.CollapsePixel(paintX, paintY, paintHeight);
                }
            }
        }

        UpdateTexture();
    }


    public void StartCraft() 
    {
        newCell.StartCraft(chargeDelay);
    }
   

    public NoiseMeshGenerator meshGenerator;

    public void SendHeightMapToMeshGenerator()
    {
        float[,] heightMap = newCell.GetHeightMap();
        meshGenerator.GenerateMesh(heightMap);

        MeshRenderer renderer = meshGenerator.GetComponent<MeshRenderer>();
        renderer.material.mainTexture = mapTexture;
    }

    public void SetValue(float value) 
    {
        paintHeight = value;
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

        if (GUILayout.Button("Generate Mesh"))
        {
            tileInfo.SendHeightMapToMeshGenerator();
        }

    }
}
