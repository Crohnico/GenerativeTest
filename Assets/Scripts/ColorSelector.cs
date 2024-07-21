using UnityEngine;

public class ColorSelector : MonoBehaviour
{
    public Color baseGrassColor = Color.green;
    public Color baseWaterColor = Color.blue;
    public Color baseDirtColor = new Color(0.4f, 0.2f, 0f); 
    public Color baseSandColor = new Color(1f, 0.8f, 0.2f);

    public float rotationAngle = 45f; // Ángulo de rotación en grados

    public Color grassColor;
    public Color waterColor;
    public Color dirtColor;
    public Color sandColor;

    private void Start()
    {
        // Rotar los colores base
        RotateColors();



        Debug.Log("Grass Color: " + grassColor);
        Debug.Log("Water Color: " + waterColor);
        Debug.Log("Dirt Color: " + dirtColor);
        Debug.Log("Sand Color: " + sandColor);
    }

    private void RotateColors()
    {
        rotationAngle = Random.Range(0f, 360f);
        float hueOffset = rotationAngle / 360f; // Convertir el ángulo a porcentaje

        // Rotar los colores base en el espacio de color HSL
        Color.RGBToHSV(baseGrassColor, out float h, out float s, out float v);
        h = Mathf.Repeat(h + hueOffset, 1f); // Asegurarse de que el valor esté entre 0 y 1
        grassColor = Color.HSVToRGB(h, s, v);

        Color.RGBToHSV(baseWaterColor, out h, out s, out v);
        h = Mathf.Repeat(h + hueOffset, 1f);
        waterColor = Color.HSVToRGB(h, s, v);

        Color.RGBToHSV(baseDirtColor, out h, out s, out v);
        h = Mathf.Repeat(h + hueOffset, 1f);
        dirtColor = Color.HSVToRGB(h, s, v);

        Color.RGBToHSV(baseSandColor, out h, out s, out v);
        h = Mathf.Repeat(h + hueOffset, 1f);
        sandColor = Color.HSVToRGB(h, s, v);
    }
}
