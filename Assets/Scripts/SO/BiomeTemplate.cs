using System.Collections;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


[CreateAssetMenu(fileName = "Biome Template", menuName = "Map/Biome Template")]
public class BiomeTemplate : ScriptableObject
{
    [Header("Texture Draw Values")]
    public int mapSizeX = 50;
    public int mapSizeZ = 50;
    [HideInInspector]
    public float[,] noiseMap;

    [Header("Texture Water")]
    public CellGenerator waterSO;
    [Range(-0.01f, 0.4f)]
    public float waterThreshold;
    [Range(0f, 1f)]
    public float dryness;
    public float waterValue;// => Mathf.Lerp(waterThreshold, -0.01f, dryness);

    [Header("River Bed")]
    [Range(0.05f, 0.1f)]
    public float riverBedPercent;
    public float riverBedValue;
    public CellGenerator riverBedSO;
 

    [Header("MapPrefab")]
    public List<PrefabEnum> prefabList = new List<PrefabEnum>();


    [Header("Default types")]
    public NoiseSO noiseFiler;

    [Header("Perlin Noise")]
    [Range(-.2f, .2f)]
    public float offset = 0;
    public float noiseScale = 10f;
    [Range(0f, 2f)]
    public float biomeHeight;
  //  public int _seed;
    public void DrawTexture()
    {
        if (biomeHeight < waterValue)
        {
            biomeHeight = waterValue;
        }

        CalculateRealThreshold();
        noiseMap = noiseFiler.GenerateNoiseMap(mapSizeX, mapSizeZ, noiseScale, offset);

    }

    public bool Conatains(IACellType type) 
    {
        foreach (var t in prefabList)
        {
            if (t.cellSO.Type == type)
            {
                return true;
            }
        }

        return false;
    }
    public void CalculateRealThreshold()
    {
        float restantArea = (1 - waterThreshold);

        riverBedValue = waterThreshold + restantArea * riverBedPercent;

        restantArea = (1 - riverBedValue);

        foreach (var prefab in prefabList)
        {
            float value = waterThreshold + restantArea * prefab.percentThreshold;
            value = (value > 1 || prefab.percentThreshold == 1) ? 1.1f : value;
            prefab.resultantThreshold = value;
        }
    }
    public void Initialize(float waterValue, float riverBedValue, float waterThreshold, float riverBedPercent, CellGenerator waterSO, CellGenerator riverBedSO) 
    {
        this.waterThreshold = waterThreshold;
        this.riverBedPercent = riverBedPercent;
        this.waterValue = waterValue;
        this.riverBedValue = riverBedValue;
        this.waterSO = waterSO;
        this.riverBedSO = riverBedSO;

        if (biomeHeight < waterThreshold)
        {
            biomeHeight = waterThreshold;
        }

        CalculateRealThreshold();
    }

    public float[,] GetMap(int width, int height)
    {

        noiseMap = noiseFiler.GenerateNoiseMap(width, height, noiseScale, offset);
        ApplyModifications(width, height);
        return noiseMap;
    }

    public void ApplyModifications(int mapX, int mapZ)
    {

        for (int x = 0; x < mapX; x++)
        {
            for (int z = 0; z < mapZ; z++)
            {
                float value = noiseMap[x, z];

                value *= biomeHeight;

                if (value < waterValue)
                    value = waterValue;

                if (value > biomeHeight)
                    value = biomeHeight;

                if (value > 1)
                    value = 1;

                noiseMap[x, z] = value;
            }
        }
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(BiomeTemplate))]
public class BiomeTemplateEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BiomeTemplate script = (BiomeTemplate)target;

        if (GUILayout.Button("Generate Noise"))
        {
            script.DrawTexture();
        }

        if (script.noiseMap == null) return;
        if (script.noiseMap.Length == 0) return;


        DrawNoiseMapInspector(script.noiseMap, script.mapSizeX, script.mapSizeZ, script.prefabList, script);

    }

    // Método para dibujar el noiseMap en el Inspector
    void DrawNoiseMapInspector(float[,] noiseMap, int mapSizeX, int mapSizeZ, List<PrefabEnum> prefabList, BiomeTemplate script)
    {
        if (noiseMap == null || noiseMap.Length == 0)
        {
            EditorGUILayout.LabelField("Noise Map", "Not generated");
            return;
        }
        prefabList = prefabList.OrderBy(item => item.percentThreshold).ToList();
        Texture2D texture = new Texture2D(mapSizeX, mapSizeZ);

        for (int x = 0; x < mapSizeX; x++)
        {
            for (int z = 0; z < mapSizeZ; z++)
            {
                float value = noiseMap[x, z];

                Color color = Color.gray; // Por defecto, usar gris

                value *= script.biomeHeight;

                if (value < script.waterValue)
                    value = script.waterValue;

                if (value > script.biomeHeight)
                    value = script.biomeHeight;

                if (value > 1)
                    value = 1;

                if (value <= script.waterValue)
                {
                    color = script.waterSO.TestColor;
                }
                else if (value <= script.riverBedValue)
                {
                    color = script.riverBedSO.TestColor;
                }
                else
                {
                    foreach (PrefabEnum prefabEnum in prefabList)
                    {
                        if (value < prefabEnum.resultantThreshold)
                        {
                            color = prefabEnum.cellSO.TestColor;
                            break;
                        }
                    }
                }

                texture.SetPixel(x, z, color);
            }
        }
        texture.Apply();

        EditorGUILayout.LabelField("Noise Map");
        GUILayout.Space(5);
        Rect rect = EditorGUILayout.GetControlRect(false, mapSizeZ * 2); // Ajustar el tamaño de la textura en el Inspector
        EditorGUI.DrawTextureTransparent(rect, texture);
    }
}
#endif
