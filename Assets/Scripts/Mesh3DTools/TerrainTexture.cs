using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TerrainTexture
{
    public static Vector2 GetUVs(float value)
    {
        if (value < 0.4f)
        {
            return new Vector2(1f, 1f);
        }
        else
        {
            return new Vector2(0f, 0f);
        }
    }

    public static Texture2D GetAtlas(int width, TerrainType[] colors)
    {
        Texture2D texture = new Texture2D(width, width);

        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float height = (float)y / (float)width;
                Color pixelColor = Color.black;

                for (int i = 0; i < colors.Length; i++)
                {
                    if (height <= colors[i].height)
                    {
                        pixelColor = colors[i].color;

                        if (i > 0 && colors[i].blendable && colors[i - 1].blendable)
                        {
                            float t = (height - colors[i - 1].height) / (colors[i].height - colors[i - 1].height);
                            pixelColor = Color.Lerp(colors[i - 1].color, colors[i].color, t * 2);
                        }
                        break;
                    }
                }

                texture.SetPixel(x, y, pixelColor);
            }
        }

        texture.Apply();
        return texture;
    }

}
