using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    private int _chunkSize;
    private float _noise;
    float[,] _chunkData = null;
    float[,] fallOffData;

    private Vector2Int _position;
    private Material _material;
    private Material _waterMaterial;

    public float maxY = 8;

    private AnimationCurve _heightCurve;
    private float _height;

    private int _octaves;
    private float _lacunarity;
    private float _persistance;

    private Vector2Int _offsets;
    private int _mapSize;

    public Dictionary<int, GameObject> LODDictionary = new Dictionary<int, GameObject>();
    private Vector2[] _octaveOffsets;

    private GameObject _water;
    private bool hasWater;
    private float _waterThreshold = .3f;

    public void SetUp(Vector2[] octaveOffsets, int chunkSize, float noiseScale, Vector2Int position,
                      Material material, Material waterMaterial, AnimationCurve heightCurve, float height, int octave, float persistance,
                      float lacunarity, Vector2Int offsets, int mapSize, float waterThreshold)
    {
        this._octaveOffsets = octaveOffsets;
        this._chunkSize = chunkSize + 1;
        this._noise = noiseScale;
        this._material = material;
        this._heightCurve = heightCurve;
        this._height = height;
        _octaves = octave;
        _lacunarity = lacunarity;
        _persistance = persistance;
        _offsets = offsets;
        _mapSize = mapSize;

        _waterThreshold = waterThreshold;
        _waterMaterial = waterMaterial;

        this._position = position;

        gameObject.SetActive(false);
    }

    public void ActiveChunk(int lod)
    {
        if (_chunkData == null)
        {
             NoiseCalculator.GenerateNoiseMap(_chunkSize + _position.x, _chunkSize + _position.y, _chunkSize, _noise, _mapSize,
                                              _octaveOffsets, _octaves, _persistance, _lacunarity, _offsets,_waterThreshold, out _chunkData, out fallOffData, out hasWater);
        }

        if (!LODDictionary.ContainsKey(lod))
        {
            GameObject mesh = CreateChunkMesh(_chunkData, transform.position, lod, _material);
            mesh.name = $"LOD_{lod}";


            if (hasWater && !_water)
            {
                _water = CreateChunkMesh(_chunkData, transform.position, 25, _waterMaterial, true);
                _water.name = $"Water";
            }

            LODDictionary.Add(lod, mesh);
        }

        foreach (var key in LODDictionary.Keys)
        {
            LODDictionary[key].SetActive(false);
            if (key == lod)
            {
                LODDictionary[key].SetActive(true);
            }
        }

    }

    public GameObject CreateChunkMesh(float[,] noiseMap, Vector3 position, int lod, Material material, bool isWater = false)
    {
        int LOD = lod;
        Mesh mesh = new Mesh();

        int size = Mathf.CeilToInt((float)_chunkSize / (float)LOD);

        Vector3[] vertices = new Vector3[size * size];
        int[] triangles = new int[(size - 1) * (size - 1) * 6];
        Vector2[] uvs = new Vector2[size * size];


        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                int index = x * size + z;

                float value = noiseMap[x * LOD, z * LOD] + 0.01f;
                float height = (!isWater) ? _heightCurve.Evaluate(noiseMap[x * LOD, z * LOD]) * _height : _waterThreshold * _height;

                vertices[index] = new Vector3(x * LOD, height, z * LOD);
                vertices[index] = new Vector3(x * LOD, height, z * LOD);

                if (!isWater)
                    uvs[index] = new Vector2(value, value);
                else
                    uvs[index] = new Vector2((float)x / (size - 1), (float)z / (size - 1));
            }
        }

        int triIndex = 0;
        for (int x = 0; x < size - 1; x++)
        {
            for (int z = 0; z < size - 1; z++)
            {
                int current = x * size + z;
                int next = current + size;

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

        GameObject _chunk = new GameObject("Chunk");
        _chunk.transform.position = position;
        _chunk.layer = 6;

        MeshFilter mf = _chunk.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        MeshRenderer mr = _chunk.AddComponent<MeshRenderer>();
        mr.material = material;

        _chunk.transform.parent = transform;

        if (!isWater)
            _chunk.AddComponent<MeshCollider>();
        return _chunk;
    }
}
