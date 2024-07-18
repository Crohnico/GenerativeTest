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

    public GameObject player;

    public Chunk lastChunk = null;

    private List<Vector2Int> activeChunks = new List<Vector2Int>();
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
                chunk.SetUp(seed, chunkSize, noiseScale, position, material);
                chunk.transform.position = new Vector3(x, 0, z);
                chunksMap.Add(position, chunk);
            }
        }

        player.transform.position = new Vector3(mapSize / 2, 0.5f, mapSize / 2);
    }

    private void Update()
    {
        Vector3 playerPosition = player.transform.position;

        int chunkX = Mathf.FloorToInt(playerPosition.x / chunkSize) * chunkSize;
        int chunkZ = Mathf.FloorToInt(playerPosition.z / chunkSize) * chunkSize;

        Vector2Int chunkPosition = new Vector2Int(chunkX, chunkZ);

        Chunk currentChunk = GetChunk(chunkPosition);

        if (currentChunk != null && lastChunk != currentChunk)
        {
            lastChunk = currentChunk;

            // Limpiar la lista de chunks activos anteriores
            foreach (Vector2Int coord in activeChunks)
            {
                Chunk chunk = GetChunk(coord);
                if (chunk != null)
                {
                    if (!chunkPosition.Equals(coord))
                    {
                        chunk.gameObject.SetActive(false);
                    }
                }
            }
            activeChunks.Clear();

            // Activar los nuevos chunks en el radio activo
            for (int x = chunkX - radio; x <= chunkX + radio; x += chunkSize)
            {
                for (int z = chunkZ - radio; z <= chunkZ + radio; z += chunkSize)
                {

                    Vector2Int vectorCoordinates = new Vector2Int(x, z);
                    Chunk chunk = GetChunk(vectorCoordinates);

                    if (chunk != null)
                    {
                        chunk.gameObject.SetActive(true);
                        chunk.ActiveChunk();

                        activeChunks.Add(vectorCoordinates);
                    }
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
