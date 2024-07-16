using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(fileName = "Grass Generator", menuName = "Texutre/Grass")]
public class GrassTextureGenerator : CellGenerator
{
    public Color[] defaultColors;
    public int cellSize = 32;

    public Color[] resultColors;

    private float rotationAngle;
    private float hueOffset;

    public override void Init()
    {
        List<Color> holderList = new List<Color>(defaultColors);
        resultColors = holderList.ToArray();

        rotationAngle = Random.Range(0f, 360f);
        hueOffset = rotationAngle / 360f;
        for (int i = 0; i < resultColors.Length; i++) 
        {
            resultColors[i] = RotateColors(resultColors[i]);
        }
        GenerateGrassTexture();

        foreach(Material m in assetMaterials) 
        {
            m.color = resultColors[Random.Range(0, resultColors.Length - 1)];
        }
    }


    void GenerateGrassTexture()
    {
        texture = new Texture2D(textureWidth, textureHeight);

        for (int y = 0; y < textureHeight; y += cellSize)
        {
            for (int x = 0; x < textureWidth; x += cellSize)
            {
                Color cellColor = resultColors[Random.Range(0, resultColors.Length)]; 
                for (int cy = 0; cy < cellSize; cy++)
                {
                    for (int cx = 0; cx < cellSize; cx++)
                    {
                        texture.SetPixel(x + cx, y + cy, cellColor); // Asigna el color a cada píxel de la celda
                    }
                }
            }
        }

        Color originalColor = resultColors[resultColors.Length - 1]; // Color original
        float darkenFactor = 0.8f; // Factor de oscurecimiento

        Color darkerColor = originalColor * darkenFactor;

        PaintBorder(darkerColor);
        texture.Apply();
        material.mainTexture = texture;
        GenerateNormalMap();
    }

    void GenerateNormalMap()
    {
        normalMap = new Texture2D(textureWidth, textureHeight);

        // Genera un mapa de normales plano
        for (int y = 0; y < textureHeight; y++)
        {
            for (int x = 0; x < textureWidth; x++)
            {
                // Normaliza las coordenadas del píxel para obtener un valor entre -1 y 1
                float nx = Mathf.InverseLerp(0, textureWidth, x) * 2 - 1;
                float ny = Mathf.InverseLerp(0, textureHeight, y) * 2 - 1;

                // Define el color del píxel en función de las coordenadas normalizadas
                Color normalColor = new Color(nx, ny, 1, 1);

                normalMap.SetPixel(x, y, normalColor);
            }
        }

        normalMap.Apply(); // Aplica los cambios al mapa de normales
        material.SetTexture("_BumpMap", normalMap); // Asigna el mapa de normales al material
    }

    private Color RotateColors(Color color)
    {
        Color c = color;
        c = Rotate(c);

        Color Rotate(Color _color)
        {
            Color.RGBToHSV(_color, out float h, out float s, out float v);
            h = Mathf.Repeat(h + hueOffset, 1f);
            return Color.HSVToRGB(h, s, v);
        }

        return c;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(GrassTextureGenerator))]
public class GrassTextureGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GrassTextureGenerator script = (GrassTextureGenerator)target;

        if (GUILayout.Button("Create Texture"))
        {
            script.Init();
        }

    }

}
#endif