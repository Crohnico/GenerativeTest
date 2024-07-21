using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "MapParameters", menuName = "Map/Map Parameters")]
public class MapParameters : ScriptableObject
{
    [Header("Grid Size")]
    public int mapSizeX = 200;
    public int mapSizeZ = 200;
    public Vector2Int biomesGrid = new Vector2Int(20,20);

    public int startAreaDiameter = 21;

    [Header("ResourcesPrefab")]
    public List<PrefabEnum> resourcesList = new List<PrefabEnum>();

    [Header("Biomes")]
    public List<BiomeTemplate> templates = new List<BiomeTemplate>();

    [HideInInspector]
    public List<Biome> biomes = new List<Biome>();

    [Header("Default types")]
    public IACellType fallbackType;
    public IACellType playerArea => fallbackType;
    [HideInInspector]
    public IACellType borderType = IACellType.Border;

    private Dictionary<Vector2Int, Biome> biomeMap = new Dictionary<Vector2Int, Biome>();

    [HideInInspector]
    public float[,] noiseMap;

    [Header("Texture Water")]
    public CellGenerator waterSO;
    [Range(-0.01f, 0.4f)]
    public float waterThreshold;
    [Range(0f, 1f)]
    public float dryness;

    [Header("River Bed")]
    [Range(0.05f, 0.1f)]
    public float riverBedPercent;
    public float riverBedValue;
    public CellGenerator riverBedSO;
    public float waterValue => Mathf.Lerp(waterThreshold, -0.01f, dryness);

    [Range(0, 10)]
    public float mapHeight = 4;


    [Header("SEED")]
    public int seed;
    public bool random = true;

    public Action Generate;
    public Action OnEndGeneration;// += ChangeTextures();

    public int animalTypeAmount = 10;
    public List<NPCTemplate> animals = new List<NPCTemplate>();

    public GameObject GetPrefab(IACellType type, int x, int y)
    {
        GameObject prefab = null;
        BiomeTemplate bTemplate = GetBiomeAtPosition(x, y);

        for(int newX = x - 1; newX <= x + 1; newX++) 
        {
            for (int newY = y - 1; newY <= y + 1; newY++)
            {
                BiomeTemplate vecino = GetBiomeAtPosition(newX, newY);

                if(vecino != null && vecino != bTemplate && vecino.Conatains(type)) 
                {
                    float ran = Random.Range(0f, 1f);
                    
                    if(ran > .5f || bTemplate.biomeHeight < .5f) 
                    {
                        bTemplate = vecino;
                        break;
                    }
                }
            }
        }

        if (bTemplate == null)
        {
            return null;
        }

        foreach (PrefabEnum pEnum in resourcesList)
        {
            if (type == pEnum.cellSO.Type)
                return pEnum.cellSO.GetPrefab();
        }

        foreach (PrefabEnum pEnum in bTemplate.prefabList)
        {
            if (type == pEnum.cellSO.Type)
                return pEnum.cellSO.GetPrefab();
        }

        return prefab;
    }


    public float YModifier(int x, int y)
    {
        return noiseMap[x, y];
    }

    public void SelectAnimals() 
    {
        animalTypeAmount = Random.Range(5, 20);
        animals.Clear();
    
        List<NPCTemplate> templates;

        NPCTemplate[] loadedTemplates = Resources.LoadAll<NPCTemplate>("");
        templates = loadedTemplates.ToList();
        int index = 0;
        SelectAnimal(index);

        void SelectAnimal(int i) 
        {
            i++;

            NPCTemplate t = templates[Random.Range(0, templates.Count)];
            t.scale = t.scalesPosibles[Random.Range(0, t.scalesPosibles.Count)];

            animals.Add(t);
            templates.Remove(t);

            if (i < animalTypeAmount)
                SelectAnimal(i);
        }
    }

    public BiomeTemplate GetBiomeAtPosition(int x, int y)
    {

        if (biomeMap.Count == 0)
        {
            Debug.LogWarning("No hay biomas definidos en la lista.");
            return null;
        }

        Vector2Int position = new Vector2Int(x, y);

        if (biomeMap.ContainsKey(position))
            return biomeMap[position].template;
        else
        {
            return null;
        }
    }

    private void GenerateVirtualBiomeMap()
    {
        biomeMap.Clear();

        foreach (Biome biome in biomes)
        {
            foreach (NoiseCell cell in biome.cells)
            {
                foreach (Vector2Int position in cell.CellPositions)
                {
                    int xParsed = position.x - Mathf.CeilToInt((float)mapSizeX / 2);
                    int yParsed = position.y - Mathf.CeilToInt((float)mapSizeZ / 2);

                    Vector2Int newPosition = new Vector2Int(xParsed, yParsed);

                    if (!biomeMap.ContainsKey(newPosition))
                    {
                        biomeMap.Add(newPosition, biome);
                    }
                }
            }
        }
    }

    public bool IsWalkable(IACellType type, int x, int y)
    {
        BiomeTemplate bTemplate = GetBiomeAtPosition(x, y);
        

        foreach (PrefabEnum pEnum in bTemplate.prefabList)
        {
            if (type == pEnum.cellSO.Type)
                return pEnum.cellSO.IsWalkable;
        }

        foreach (PrefabEnum pEnum in resourcesList)
        {
            if (type == pEnum.cellSO.Type)
                return pEnum.cellSO.IsWalkable;
        }

        if (type == IACellType.Exit)
            return true;

        return false;
    }

    public IACellType DetermineCellType(int x, int z, int offsetX = 0, int offsetZ = 0)
    {
        IACellType type = fallbackType;
        BiomeTemplate bTemplate = GetBiomeAtPosition(x - offsetX, z - offsetZ);

        bTemplate.prefabList = bTemplate.prefabList.OrderBy(item => item.percentThreshold).ToList();

        float noiseValue = noiseMap[x, z];


        if (noiseValue <= waterValue)
        {
            type = waterSO.Type;
        }
        else if (noiseValue <= riverBedValue)
        {
            type = riverBedSO.Type;
        }
        else
        {
            foreach (PrefabEnum prefabEnum in bTemplate.prefabList)
            {
                if (noiseValue <= prefabEnum.percentThreshold)
                {
                    type = prefabEnum.cellSO.Type;

                    if (prefabEnum.density < 1)
                    {
                        if (UnityEngine.Random.value > prefabEnum.density)
                        {
                            type = fallbackType;
                        }
                    }
                    break;
                }
            }
        }
        return type;
    }

    public IACellType DetermineBorderCellType(int x, int z)
    {
        if (x < 3 || x >= mapSizeX - 3 || z < 3 || z >= mapSizeZ - 3)
        {
            return borderType;
        }
        else
        {
            return IACellType.Water;
        }
    }

    public void GenerateMap()
    {
        Generate?.Invoke();
    }

    public void ShuffleTemplates()
    {
        templates = Shuffle(templates);
    }

    List<BiomeTemplate> Shuffle(List<BiomeTemplate> list)
    {
        int n = list.Count;

        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n);
            BiomeTemplate value = list[k];
            list[k] = list[n];
            list[n] = value;
        }

        return list;
    }


    public void GenerateNoiseMap()
    {
        if (random) 
        {
            Seed.seed = Random.Range(0, int.MaxValue);
            seed = Seed.seed;
        }

        //  templates = Shuffle(templates,seed);
        float restantArea = (1 - waterThreshold);

        riverBedValue = waterThreshold + restantArea * riverBedPercent;

        foreach (BiomeTemplate t in templates)
        {
            t.Initialize(waterValue, riverBedValue, waterThreshold, riverBedPercent, waterSO, riverBedSO);
        }

        BiomeSetter bSetter = new BiomeSetter();

        noiseMap = new float[mapSizeX, mapSizeZ];

        for (int x = 0; x < mapSizeX; x++)
        {
            for (int z = 0; z < mapSizeZ; z++)
            {
                noiseMap[x, z] = 0.0f;
            }
        }

        biomes = bSetter.Initialize(mapSizeX, mapSizeZ, biomesGrid, templates.Count);

        foreach (Biome b in biomes)
        {
            b.template = templates[b.cells[0].bBiomeID];
        }

        foreach (BiomeTemplate t in templates)
        {
            t.prefabList = t.prefabList.OrderBy(item => item.percentThreshold).ToList();
        }


        GenerateVirtualBiomeMap();
        ApplyToMap();

    }
    public void ChangeTextures() 
    {
        
        foreach (BiomeTemplate t in templates)
        {
            foreach (PrefabEnum pE in t.prefabList)
            {
                pE.cellSO.Init();
            }

        }

        foreach(PrefabEnum t in resourcesList) 
        {
            t.cellSO.Init();
        }
    }

    private void ApplyToMap()
    {
        float[,] resultMerge = ApplyBiomesToGlobalMap(noiseMap);

        for (int i = 0; i < 10; i++)
        {
            resultMerge = MergeFilter.SmoothTransition(resultMerge, mapSizeX, mapSizeZ, .04f, 3);
        }

        noiseMap = resultMerge;
    }

    public float[,] ApplyBiomesToGlobalMap(float[,] globalMap)
    {
        float[,] result = globalMap;

        foreach (var biome in biomes)
        {
            biome.GenerateNoiseMap();
            biome.ApplyToGlobalMap(result);
        }

        return result;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MapParameters))]
public class MapParametersEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MapParameters script = (MapParameters)target;

        if (GUILayout.Button("Shuffle biomes templates"))
        {
            script.ShuffleTemplates();
        }

        if (GUILayout.Button("Generate Random Noise"))
        {
            Seed.seed = Random.Range(0, int.MaxValue);
            script.seed = Seed.seed;
            script.GenerateNoiseMap();
        }
        if (GUILayout.Button("Generate Noise"))
        {
            script.GenerateNoiseMap();
        }
        if (GUILayout.Button("Generate Map"))
        {
            script.GenerateMap();
        }

        if (script.noiseMap == null) return;
        if (script.noiseMap.Length == 0) return;

        DrawNoiseMapInspector(script.noiseMap, script.mapSizeX, script.mapSizeZ, script.biomes, script);

    }

    // Método para dibujar el noiseMap en el Inspector
    void DrawNoiseMapInspector(float[,] noiseMap, int mapSizeX, int mapSizeZ, List<Biome> biomes, MapParameters script)
    {

        if (noiseMap == null || noiseMap.Length == 0)
        {
            EditorGUILayout.LabelField("Noise Map", "Not generated");
            return;
        }

        Texture2D texture = new Texture2D(mapSizeX, mapSizeZ);
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int z = 0; z < mapSizeZ; z++)
            {
                float value = noiseMap[x, z];
                Color color = Color.gray;

                BiomeTemplate bTemplate = script.GetBiomeAtPosition(x - mapSizeX / 2, z - mapSizeZ / 2);

                if (bTemplate == null) continue;

                foreach (PrefabEnum prefabEnum in bTemplate.prefabList)
                {
                    if (value <= script.waterValue)
                    {
                        color = script.waterSO.TestColor;
                    }
                    else if (value <= script.riverBedValue)
                    {
                        color = script.riverBedSO.TestColor;
                    }
                    else if (value <= prefabEnum.percentThreshold)
                    {
                        color = prefabEnum.cellSO.TestColor;
                        break;
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