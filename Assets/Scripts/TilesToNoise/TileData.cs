using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TileData", menuName = "Tiles/Tile Data")]
public class TileData : ScriptableObject
{
    public string tileName;

    public string topSocket;
    public string bottomSocket;
    public string rightSocket;
    public string leftSocket;

    [SerializeField]
    private List<float> textureDataList = new List<float>();

    public int textureSize = 3;
    public Texture2D texture;

    public void SetTextureData(float bottomLeft, float bottomRight, float topLeft, float topRight)
    {
        textureDataList.Clear();

        texture = new Texture2D(textureSize, textureSize);

        int halfSize = textureSize / 2;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {

                if (x < halfSize && y < halfSize)
                {
                    texture.SetPixel(x, y, new Color(bottomLeft, bottomLeft, bottomLeft));
                }

                else if (x >= halfSize && y < halfSize)
                {
                    texture.SetPixel(x, y, new Color(bottomRight, bottomRight, bottomRight));
                }

                else if (x < halfSize && y >= halfSize)
                {
                    texture.SetPixel(x, y, new Color(topLeft, topLeft, topLeft));
                }

                else if (x >= halfSize && y >= halfSize)
                {
                    texture.SetPixel(x, y, new Color(topRight, topRight, topRight));
                }
            }
        }

        texture.Apply();


        topSocket = $"{topLeft.ToString("0.0")}|{topRight.ToString("0.0")}";
        bottomSocket = $"{bottomLeft.ToString("0.0")}|{bottomRight.ToString("0.0")}";
        leftSocket = $"{bottomLeft.ToString("0.0")}|{topLeft.ToString("0.0")}";
        rightSocket = $"{bottomRight.ToString("0.0")}|{topRight.ToString("0.0")}";
    }
}

[CustomEditor(typeof(TileData))]
public class TileDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();
        TileData tileInfo = (TileData)target;

        if (GUILayout.Button("Craft Texture"))
        {
            tileInfo.SetTextureData(0.1f, 0.1f, 0.0f, 0.0f);
        }


        if (tileInfo.texture != null)
        {
            GUILayout.Label("Generated Texture", EditorStyles.boldLabel);
            GUILayout.Label(tileInfo.texture, GUILayout.Width(128), GUILayout.Height(128));
        }
    }
}
