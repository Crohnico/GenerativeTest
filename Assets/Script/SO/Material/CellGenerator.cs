using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class CellGenerator : CellSO
{
    public Material material;
    public Material[] assetMaterials;

    public int textureWidth = 512;
    public int textureHeight = 512;

    public Texture2D texture;
    public Texture2D normalMap;

    public int borderGrosor = 10;

    public bool blackOutline = true;

    public IACellType Type => type;
    public GameObject[] Prefab => prefab;
    public bool IsWalkable => isWalkable;
    public Color TestColor => testColor;

    public virtual void Init()
    {

    }
    public GameObject GetPrefab()
    {
        int resultIndex = Random.Range(0, Prefab.Length);
        
        return Prefab[resultIndex];
    }

    protected virtual void PaintBorder(Color darkerColor)
    {
        darkerColor = (blackOutline) ? Color.black : darkerColor;
        for (int x = 0; x < textureWidth; x++)
        {
            for (int y = 0; y < textureHeight; y++)
            {
                Color color = darkerColor;

                if (x < borderGrosor || y < borderGrosor || x >= (textureWidth) - borderGrosor || y >= textureHeight - borderGrosor)
                    texture.SetPixel(x, y, color);
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CellGenerator))]
public class CellGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CellGenerator script = (CellGenerator)target;

        if (GUILayout.Button("Create Texture"))
        {
            script.Init();
        }

    }
}
#endif
