using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ProceduralTextureGenerator : MonoBehaviour
{
    public static int textureWidth = 512;
    public static int textureHeight = 512;
    public Color baseColor = Color.green;
    public Color darkColor = Color.black;
    public float strength = 0.1f;

    public static Texture2D texture;
    public static Texture2D normalMap;
    Material material;


    private void Start()
    {
        texture = (texture == null) ? new Texture2D(textureWidth, textureHeight) : texture;
        normalMap = (normalMap == null) ? new Texture2D(textureWidth, textureHeight) : normalMap;
        material = GetComponent<Renderer>().material;

        ApplyTexturesToMaterial();
    }
    public void GenerateProceduralTexture()
    {

        for (int x = 0; x < textureWidth; x++)
        {
            for (int y = 0; y < textureHeight; y++)
            {
                // Calcula la posición normalizada en la textura
                float u = (float)x / textureWidth;
                float v = (float)y / textureHeight;

                // Calcula el ruido Perlin en la posición actual
                float noise = Mathf.PerlinNoise(u * 10f, v * 10f);

                // Genera el color base de la textura mezclando el verde con el ruido
                Color color = Color.Lerp(baseColor, darkColor, noise);

                // Genera el vector normal mapeando el ruido Perlin a los componentes X, Y y Z
                Vector3 normal = new Vector3(noise * 2f - 1f, 1f - noise * 2f, 1f).normalized;
                normal *= strength; // Ajusta la intensidad del mapa normal

                // Convierte el vector normal de rango [-1, 1] a [0, 1] para la textura
                Color normalColor = new Color(normal.x * 0.5f + 0.5f, normal.y * 0.5f + 0.5f, normal.z * 0.5f + 0.5f, 1f);

                // Aplica los colores a las texturas
                texture.SetPixel(x, y, color);
                normalMap.SetPixel(x, y, normalColor);
            }
        }

        texture.Apply();
        normalMap.Apply();

        // Aplica los cambios a las texturas

    }

    void ApplyTexturesToMaterial()
    {

        // Aplica las texturas generadas al material
        material.SetTexture("_MainTex", texture); // Textura base
        material.SetTexture("_BumpMap", normalMap); // Mapa normal

    }
}



