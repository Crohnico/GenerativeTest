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
    public int mapSize;
    public int chunkSize;

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
    public int mapSizeDebug = 1000;
    private int radio => activeChunkRadio * chunkSize;

    [SerializeField]
    private AnimationCurve heightCurve;

    public Vector2[] octaveOffsets;

    void Start()
    {
        SetOffsets();

        for (int x = 0; x < mapSize; x += chunkSize)
        {
            for (int z = 0; z < mapSize; z += chunkSize)
            {
                Vector2Int position = new Vector2Int(x, z);
                CreateChunk(position);
            }
        }

        Chunk c = GetChunk(new Vector2Int(mapSize / 2, mapSize / 2));
        player.transform.position = new Vector3(mapSize / 2, 51, mapSize / 2);
    }

    public void SetOffsets() 
    {
        System.Random prng = new System.Random(seed);
        octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000);
            float offsetY = prng.Next(-100000, 100000);
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }
    }

    private void Update()
    {
        LoadUnloadChunks();

        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(0);
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
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
        chunk.SetUp(octaveOffsets, chunkSize, noise, chunkPosition, material, heightCurve, mapHeight, octaves, persistance, lacunarity);
        chunk.transform.position = new Vector3(chunkPosition.x, 0, chunkPosition.y);
        chunksMap.Add(chunkPosition, chunk);
        return chunk;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(GridChunks))]
public class GridChunksEditor : Editor
{
    private float[,] noiseMap = null;
    private Texture2D noiseTexture = null;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GridChunks script = (GridChunks)target;

        if (GUILayout.Button("Generate Noise"))
        {
            script.SetOffsets();
            noiseMap = NoiseCalculator.GenerateNoiseMap(0, 0, script.mapSizeDebug, script.noise, script.octaveOffsets, script.octaves, script.persistance, script.lacunarity);
            noiseTexture = GenerateNoiseTexture(noiseMap, script.mapSizeDebug, script.chunkSize);
        }

        if (noiseMap == null || noiseMap.Length == 0)
            return;

        DrawNoiseMapInspector(noiseTexture);
    }

    private Texture2D GenerateNoiseTexture(float[,] noiseMap, int mapSize, int gridSize)
    {
        // Asegúrate de que el tamaño de la textura coincide con el tamaño del noiseMap
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                float value = Mathf.InverseLerp(-1f, 2f, noiseMap[x, z]);
                Color color = Color.Lerp(Color.black, Color.white, value);

                // Dibujar las líneas de la cuadrícula
                if (x % gridSize == 0 || z % gridSize == 0)
                    color = Color.blue;

                texture.SetPixel(x, z, color);
            }
        }
        texture.Apply();
        return texture;
    }

    private void DrawNoiseMapInspector(Texture2D texture)
    {
        if (texture == null)
        {
            EditorGUILayout.LabelField("Noise Map", "Not generated");
            return;
        }

        EditorGUILayout.LabelField("Noise Map");

        // Ajusta el tamaño de la textura para que se vea claramente en el inspector
        float aspectRatio = (float)texture.width / texture.height;
        float inspectorWidth = EditorGUIUtility.currentViewWidth - 30; // Ajusta el ancho del inspector
        float inspectorHeight = inspectorWidth / aspectRatio;

        Rect rect = EditorGUILayout.GetControlRect(false, inspectorHeight);
        rect.width = inspectorWidth;
        EditorGUI.DrawTextureTransparent(rect, texture, ScaleMode.ScaleToFit);
    }

    // Limpiar la textura cuando se destruye el editor
    private void OnDisable()
    {
        if (noiseTexture != null)
        {
            DestroyImmediate(noiseTexture);
        }
    }
}
#endif