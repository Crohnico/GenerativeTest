using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridChunks : MonoBehaviour
{
    public Dictionary<Vector2Int, Chunk> chunksMap = new Dictionary<Vector2Int, Chunk>();

    public Material material;
    public int mapSize;
    public int chunkSize;

    public float noiseScale;

    public int seed;
    public float frecuency = 1f;

    public GameObject player;

    private List<Vector2Int> activeChunks = new List<Vector2Int>();
    public float angleOfVision = 40;
    public int activeChunkRadio = 5;
    private int radio => activeChunkRadio * chunkSize;

    void Start()
    {
        for(int x = 0; x < mapSize; x += chunkSize) 
        {
            for (int z = 0; z < mapSize; z += chunkSize)
            {
                Vector2Int position = new Vector2Int(x, z);
                GameObject go = new GameObject();

                go.name = $"{x},{z}";

                Chunk chunk = go.AddComponent<Chunk>();
                chunk.SetUp(seed, chunkSize, mapSize, noiseScale, frecuency,position, material);
                chunk.transform.position = new Vector3(x, 0, z);
                chunksMap.Add(position, chunk);
            }
        }

        player.transform.position = new Vector3(mapSize / 2, 5f, mapSize / 2);
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

                    if (angle < angleOfVision / 2f || Vector3.Distance(playerPosition, chunkCenter) < chunkSize * 4)
                    {
                        Chunk chunk = GetChunk(vectorCoordinates);

                        if (chunk != null)
                        {
                            chunk.gameObject.SetActive(true);
                            chunk.ActiveChunk();

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


    private Chunk GetChunk(Vector2Int chunkPosition)
    {
       
        if (chunksMap.ContainsKey(chunkPosition))
        {
            return chunksMap[chunkPosition];
        }
        return null;
    }
}
