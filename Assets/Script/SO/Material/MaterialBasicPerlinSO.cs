using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(fileName = "Basic Texture Generator", menuName = "Texutre/Perlin")]
public class MaterialBasicPerlinSO : CellGenerator
{
    public Color baseColor = Color.green;
    public Color darkColor = Color.black;
    public float strength = 0.1f;

    public Color resultBaseColor = Color.green;
    public Color resultDarkColor = Color.black;

    public float noiseScale = 1;

    public override void Init()
    {
        RotateColors();

        texture = (texture == null) ? new Texture2D(textureWidth, textureHeight) : texture;
        normalMap = (normalMap == null) ? new Texture2D(textureWidth, textureHeight) : normalMap;
        foreach (Material m in assetMaterials)
        {
            m.color = resultBaseColor;
        }

        if (!material) return;
        GenerateProceduralTexture();
        ApplyTexturesToMaterial();
    }
    public void GenerateProceduralTexture()
    {
        float offsetX = UnityEngine.Random.Range(0f, 99999f);
        float offsetY = UnityEngine.Random.Range(0f, 99999f);

        for (int x = 0; x < textureWidth; x++)
        {
            for (int y = 0; y < textureHeight; y++)
            {
                float u = (float)x / textureWidth * noiseScale + offsetX;
                float v = (float)y / textureHeight * noiseScale + offsetY;

                float noise = Mathf.PerlinNoise(u * 10f, v * 10f);

                Color color = Color.black;

                color = Color.Lerp(resultBaseColor, resultDarkColor, noise);

                Vector3 normal = new Vector3(noise * 2f - 1f, 1f - noise * 2f, 1f).normalized;
                normal *= strength;


                Color normalColor = new Color(normal.x * 0.5f + 0.5f, normal.y * 0.5f + 0.5f, normal.z * 0.5f + 0.5f, 1f);


                texture.SetPixel(x, y, color);
                normalMap.SetPixel(x, y, normalColor);
            }
        }

        Color originalColor = resultDarkColor; // Color original
        float darkenFactor = 0.8f; // Factor de oscurecimiento

        Color darkerColor = originalColor * darkenFactor;
        PaintBorder(darkerColor);
        texture.Apply();
        normalMap.Apply();

    }

    void ApplyTexturesToMaterial()
    {
        material.SetTexture("_MainTex", texture);
        material.SetTexture("_BumpMap", normalMap);
    }

    private void RotateColors()
    {
        float rotationAngle = Random.Range(0f, 360f);
        float hueOffset = rotationAngle / 360f;

        resultBaseColor = Rotate(baseColor);
        resultDarkColor = Rotate(darkColor);

        Color Rotate(Color color)
        {
            Color.RGBToHSV(color, out float h, out float s, out float v);
            h = Mathf.Repeat(h + hueOffset, 1f);
            return Color.HSVToRGB(h, s, v);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MaterialBasicPerlinSO))]
public class MaterialBasicPerlinSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MaterialBasicPerlinSO script = (MaterialBasicPerlinSO)target;

        if (GUILayout.Button("Create Texture"))
        {
            script.Init();
        }

    }

}
#endif
