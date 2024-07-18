using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridChunks : MonoBehaviour
{
    public Dictionary<Vector2Int, Chunk> chunksMap = new Dictionary<Vector2Int, Chunk>();

    public Material material;
    public int mapSize;
    public int chunkSize;
    public float mapHeight = 5;

    public float noiseScale;

    public int seed;
    public float frecuency = 1f;

    public GameObject player;

    float highResolution = 200;
    float mediumResolution = 400;

    private List<Vector2Int> activeChunks = new List<Vector2Int>();
    public float angleOfVision = 40;
    public int activeChunkRadio = 5;

    private int radio => activeChunkRadio * chunkSize;

    [SerializeField]
    private AnimationCurve heightCurve;

    void Start()
    {
        for (int x = 0; x < mapSize; x += chunkSize)
        {
            for (int z = 0; z < mapSize; z += chunkSize)
            {
                Vector2Int position = new Vector2Int(x, z);
                CreateChunk(position);
            }
        }

        Chunk c = GetChunk(new Vector2Int(mapSize / 2, mapSize / 2));
        player.transform.position = new Vector3(mapSize / 2, c.maxY + 5, mapSize / 2);
    }


    private void Update()
    {
        LoadUnloadChunks();
    }

    private void LoadUnloadChunks()
    {
        Vector3 playerPosition = Camera.main.transform.position;
        Vector3 playerForward = Camera.main.transform.forward;

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
        else
        {
            return 10; // Baja resolución
        }
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
        chunk.SetUp(seed, chunkSize, noiseScale, chunkPosition, material, heightCurve, mapHeight);
        chunk.transform.position = new Vector3(chunkPosition.x, 0, chunkPosition.y);
        chunksMap.Add(chunkPosition, chunk);
        return chunk;
    }
}
