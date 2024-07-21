using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class GridChunks : MonoBehaviour
{
    public Dictionary<Vector2Int, Chunk> chunksMap = new Dictionary<Vector2Int, Chunk>();

    public Material material;
    public Material waterMaterial;

    public int preloadArea;
    public int chunkSize;

    public float fixedMinHeight = -1f;
    public float fixedMaxHeight = 2f;

    public int seed;
    public float noise = 1f;
    public float mapHeight = 5;
    public int octaves = 3;
    public float persistance = 2;
    public float lacunarity = 1;

    public GameObject player;

    public float highResolution = 200;
    public float mediumResolution = 400;
    public float lowResolution = 600;

    private List<Vector2Int> activeChunks = new List<Vector2Int>();
    public float angleOfVision = 40;
    public int activeChunkRadio = 5;

    [Header("Debug")]
    public int mapSize = 1000;
    public int mapMin = 1000;
    private int radio => activeChunkRadio * chunkSize;

    [SerializeField]
    private AnimationCurve heightCurve;


    public Vector2[] octaveOffsets;
    public Vector2Int offsets;

    public TerrainType[] regions;
    public static GridChunks Istance;
    private Camera playerCamera;

    private void Awake()
    {
        Istance = this;
    }

    void Start()
    {
       // seed = Random.Range(int.MinValue, int.MaxValue);
        NoiseCalculator.MinNoiseHeight = fixedMinHeight;
        NoiseCalculator.MaxNoiseHeight = fixedMaxHeight;
      //  NoiseCalculator.GenerateNoiseMap(0, 0, mapSize, noise, octaveOffsets, octaves, persistance, lacunarity, offsets);

        SetRandoms();
        material.mainTexture = TerrainTexture.GetAtlas(252, regions);

        for (int x = 0; x < preloadArea; x += chunkSize)
        {
            for (int z = 0; z < preloadArea; z += chunkSize)
            {
                Vector2Int position = new Vector2Int(x, z);
                CreateChunk(position);
            }
        }

      //  CheckPlayerPosition();
    }

    public void Initialize(GameObject player, Camera camera, bool isOwned)
    {
        if (isOwned)
        {
            this.player = player;
            this.playerCamera = camera;
        }

        CheckPlayerPosition(player);
    }

    public void CheckPlayerPosition(GameObject user)
    {
        user.transform.position = new Vector3(mapSize/2, (mapHeight + mapHeight/2) + 10, mapSize / 2);
    }

    public void SetRandoms() 
    {
        System.Random prng = new System.Random(seed);
        octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000);
            float offsetY = prng.Next(-100000, 100000);
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

         mapSize = 500 + (mapMin * prng.Next(1, 10));
         noise = prng.Next(150, 400);
        regions[0].height = GenerateRandomFloatInRange(prng, 0, .6f);

    }

    float GenerateRandomFloatInRange(System.Random prng, float min, float max)
    {
        return (float)(prng.NextDouble() * (max - min) + min);
    }

    private void Update()
    {
        LoadUnloadChunks();

       // if (Input.GetKeyDown(KeyCode.R))
           // SceneManager.LoadScene(0);
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    private void LoadUnloadChunks()
    {
        if (!playerCamera) return;

        Vector3 playerPosition = playerCamera.transform.position;
        Vector3 playerForward = playerCamera.transform.forward;

        int chunkX = Mathf.FloorToInt(playerPosition.x / chunkSize) * chunkSize;
        int chunkZ = Mathf.FloorToInt(playerPosition.z / chunkSize) * chunkSize;

        Vector2Int chunkPosition = new Vector2Int(chunkX, chunkZ);

        Chunk currentChunk = GetChunk(chunkPosition);

        if (currentChunk != null)
        {
            List<Vector2Int> lastCharged = new List<Vector2Int>(activeChunks);
            activeChunks.Clear();

            for (int x = chunkX - radio; x <= chunkX + radio; x += chunkSize)
            {
                for (int z = chunkZ - radio; z <= chunkZ + radio; z += chunkSize)
                {
                    Vector2Int vectorCoordinates = new Vector2Int(x, z);

                    Vector3 chunkCenter = new Vector3(x + chunkSize / 2f, 0, z + chunkSize / 2f);
                    Vector3 directionToChunk = (chunkCenter - playerPosition).normalized;

                    float angle = Vector3.Angle(playerForward, directionToChunk);
                    float distance = Vector3.Distance(playerPosition, chunkCenter);

                    if (angle < angleOfVision / 2f || distance < chunkSize * 4)
                    {
                        Chunk chunk = GetChunk(vectorCoordinates);

                        if (chunk != null)
                        {
                            chunk.gameObject.SetActive(true);
                            chunk.ActiveChunk(CalculateLOD(distance));

                            activeChunks.Add(vectorCoordinates);
                            if (lastCharged.Contains(vectorCoordinates))
                                lastCharged.Remove(vectorCoordinates);
                        }
                    }
                }
            }

            foreach (Vector2Int coord in lastCharged)
            {
                Chunk chunk = GetChunk(coord);
                if (chunk != null)
                {
                    chunk.gameObject.SetActive(false);
                }
            }
        }
    }
   
    //TODO un sccript aparte para controlar la distancia de dibujado
    private int CalculateLOD(float distance)
    {
        if (distance < highResolution)
        {
            return 2; // Alta resolución
        }
        else if (distance < mediumResolution)
        {
            return 5; // Media resolución
        }
        else if (distance < lowResolution)
        {
            return 10; // Baja resolución
        }
        else { return 25; }
    }

    private Chunk GetChunk(Vector2Int chunkPosition)
    {

        if (chunksMap.ContainsKey(chunkPosition))
        {
            return chunksMap[chunkPosition];
        }
        else
        {
            return CreateChunk(chunkPosition);
        }

    }

    private Chunk CreateChunk(Vector2Int chunkPosition)
    {
        GameObject go = new GameObject();

        go.name = $"{chunkPosition.x},{chunkPosition.y}";

        Chunk chunk = go.AddComponent<Chunk>();
        chunk.SetUp(octaveOffsets, chunkSize, noise, chunkPosition, material, waterMaterial, heightCurve, mapHeight, octaves, persistance, lacunarity, offsets, mapSize, regions[0].height);
        chunk.transform.position = new Vector3(chunkPosition.x, 0, chunkPosition.y);
        chunksMap.Add(chunkPosition, chunk);
        return chunk;
    }
}
