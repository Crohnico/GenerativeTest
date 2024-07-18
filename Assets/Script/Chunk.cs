using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    private int _seed;
    private int _chunkSize;
    private float _noiseScale;
    float[,] _chunkData = null;

    private Vector2Int _position;
    private Material _material;

    private GameObject _chunk;

    public void SetUp(int seed, int chunkSize, float noiseScale, Vector2Int position, Material material)
    {
        this._seed = seed;
        this._chunkSize = chunkSize + 1;
        this._noiseScale = noiseScale;
        this._material = material;

        this._position = position;

        gameObject.SetActive(false);
    }

    public void ActiveChunk() 
    {
        if(_chunkData == null)
            _chunkData = GenerateChunk(_chunkSize + _position.x, _chunkSize + _position.y);

        if (!_chunk) CreateChunkMesh(_chunkData, transform.position);

        _chunk.SetActive(true);
    }
 
    public float[,] GenerateChunk(int startX, int startZ)
    {
        float[,] noiseMap = new float[_chunkSize, _chunkSize];
        for (int x = 0; x < _chunkSize; x++)
        {
            for (int z = 0; z < _chunkSize; z++)
            {
                float sampleX = (startX + x + _seed) / _noiseScale;
                float sampleZ = (startZ + z + _seed) / _noiseScale;
                float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ);
                noiseMap[x, z] = perlinValue;
            }
        }
        return noiseMap;
    }

    public void CreateChunkMesh(float[,] noiseMap, Vector3 position)
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[_chunkSize * _chunkSize];
        int[] triangles = new int[(_chunkSize - 1) * (_chunkSize - 1) * 6];
        Vector2[] uvs = new Vector2[_chunkSize * _chunkSize];

        for (int x = 0; x < _chunkSize; x++)
        {
            for (int z = 0; z < _chunkSize; z++)
            {
                int index = x * _chunkSize + z;
                vertices[index] = new Vector3(x, noiseMap[x, z] * 5, z);
                uvs[index] = new Vector2((float)x / (_chunkSize - 1), (float)z / (_chunkSize - 1));
            }
        }

        int triIndex = 0;
        for (int x = 0; x < _chunkSize - 1; x++)
        {
            for (int z = 0; z < _chunkSize - 1; z++)
            {
                int current = x * _chunkSize + z;
                int next = current + _chunkSize;

                triangles[triIndex] = current;
                triangles[triIndex + 1] = next + 1;
                triangles[triIndex + 2] = next;

                triangles[triIndex + 3] = current;
                triangles[triIndex + 4] = current + 1;
                triangles[triIndex + 5] = next + 1;

                triIndex += 6;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        _chunk = new GameObject("Chunk");
        _chunk.transform.position = position;

        MeshFilter mf = _chunk.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        MeshRenderer mr = _chunk.AddComponent<MeshRenderer>();
        mr.material = _material;

        _chunk.transform.parent = transform;

        _chunk.AddComponent<MeshCollider>();
    }
}
