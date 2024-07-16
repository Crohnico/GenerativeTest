using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelInitializer : MonoBehaviour
{
    public MapParameters mapParameter;
    public PlayerController player;
    public int instantiateRadius = 20;

    public int mapSizeX = 200;
    public int mapSizeZ = 200;
    public Vector2Int biomesGrid = new Vector2Int(20, 20);

    [Header("ResourcesPrefab")]
    public List<PrefabEnum> resourcesList = new List<PrefabEnum>();

    [Header("Biomes")]
    public List<BiomeTemplate> templates = new List<BiomeTemplate>();

    [Header("Texture Water")]
    public CellGenerator waterSO;
    [Range(-0.01f, 0.4f)]
    public float waterThreshold;
    [Range(0f, 1f)]
    public float dryness;

    [Header("River Bed")]
    [Range(0.05f, 0.1f)]
    public float riverBedPercent;
    public CellGenerator riverBedSO;
    private Dictionary<int, bool> usedIndex = new Dictionary<int, bool>();
    private Dictionary<string, NPCCore> animals = new Dictionary<string, NPCCore>();

    public List<NPCTemplate> animalsTemplate = new List<NPCTemplate>();

    public GameObject animalPrefab;

    [ContextMenu("Start")]
    public void Initialize()
    {
        DOTween.SetTweensCapacity(500, 312);
        mapParameter = new MapParameters();
        mapParameter.mapSizeX = mapSizeX;
        mapParameter.mapSizeZ = mapSizeZ;
        mapParameter.biomesGrid = biomesGrid;
        mapParameter.resourcesList = resourcesList;
        mapParameter.templates = templates;
        mapParameter.waterSO = waterSO;
        mapParameter.waterThreshold = waterThreshold;
        mapParameter.dryness = dryness;
        mapParameter.riverBedPercent = riverBedPercent;
        mapParameter.riverBedSO = riverBedSO;
        mapParameter.mapHeight = 10;

        mapParameter.OnEndGeneration += () =>
            {
                mapParameter.SelectAnimals();
                animalsTemplate = mapParameter.animals;
                //  Debug.Log(Grid.Istance.walkableGridCelss.Count);
                IACell cell = Grid.Istance.walkableGridCelss[Random.Range(0, Grid.Istance.walkableGridCelss.Count)];
                player.transform.position = new Vector3(cell.GridX, cell.height, cell.GridZ);
                GenerateRandomAnimals();
            };

        Grid.Istance.Initialize(mapParameter);

        mapParameter.GenerateNoiseMap();
        mapParameter.GenerateMap();

        player.Initialize();
        OnPlayerMoveUpdate();
    }

    public void Start()
    {
        Invoke("Initialize", .1f);
    }

    private void GenerateRandomAnimals()
    {
        NameGenerator nameGenerator = new NameGenerator();
        foreach (var anim in animalsTemplate)
        {
            int ind = Random.Range(20, 40);
            int sylab = Random.Range(1, 4);

            string race = nameGenerator.GenerateProceduralName(sylab);

            int trys = 0;
            int index = GetIndex();


            for (int i = 0; i < ind; i++)
            {
                GenerateAnimal(anim, race, i, index);

                int value = Random.Range(trys, 10);
                if (value > trys)
                {
                    trys++;
                    index = SumIndex(index);
                }
                else
                {
                    trys = 0;
                    index = GetIndex();
                }
            }
        }

        int GetIndex()
        {
            int index = Random.Range(0, Grid.Istance.walkableGridCelss.Count);
            if (usedIndex.ContainsKey(index))
                return GetIndex();
            else
            {
                usedIndex.Add(index, true);
                return index;
            }
        }

        int SumIndex(int index)
        {
            index = (index + 1) % (Grid.Istance.walkableGridCelss.Count - 1);

            if (usedIndex.ContainsKey(index))
                return SumIndex(index);
            else
            {
                usedIndex.Add(index, true);
                return index;
            }
        }
    }

    private void GenerateAnimal(NPCTemplate template, string race, int id, int index)
    {
        GameObject go = Instantiate(animalPrefab);

        IACell cell = Grid.Istance.walkableGridCelss[index];

        go.transform.position = new Vector3(cell.GridX, cell.height, cell.GridZ);

        if (go.TryGetComponent(out NPCCore core))
        {
            string uid = $"{id}{race}";
            animals.Add(uid, core);
            core.SetUp(template, race, uid);
        }

    }


    private void OnEnable()
    {
        TurnHolder.resolveTurn += OnPlayerMoveUpdate;
    }

    private void OnDisable()
    {
        TurnHolder.resolveTurn -= OnPlayerMoveUpdate;
    }
    public void OnPlayerMoveUpdate()
    {
        Vector2Int playerPos = new Vector2Int(player.x, player.z);
        Grid.Istance.InstantiateCells(playerPos, instantiateRadius);
    }
}
