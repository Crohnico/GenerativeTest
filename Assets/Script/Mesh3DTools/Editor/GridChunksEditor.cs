#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridChunks))]
public class GridChunksEditor : Editor
{
    private float[,] noiseMap = null;
    private float[,] fallOff = null;
    private Texture2D noiseTexture = null;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GridChunks script = (GridChunks)target;

        if (GUILayout.Button("Generate Noise"))
        {
            script.SetOffsets();
            NoiseCalculator.MinNoiseHeight = script.fixedMinHeight;
            NoiseCalculator.MaxNoiseHeight = script.fixedMaxHeight;

            noiseMap = NoiseCalculator.GenerateNoiseMap(0, 0, script.mapSize, script.noise, script.octaveOffsets, script.octaves, script.persistance, script.lacunarity, script.offsets);
            fallOff = FallofGenerator.GenerateFalloffMap(script.mapSize, script.mapSize, 0, 0);

            for(int x = 0; x < script.mapSize; x++) 
            {
                for (int y = 0; y < script.mapSize; y++)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - fallOff[x, y]);
                }
            }

            noiseTexture = GenerateNoiseTexture(noiseMap, script.mapSize, script.chunkSize);
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

