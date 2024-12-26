using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GenerateItems : MonoBehaviour
{
    public GameObject prefab;
    public int itemAmount;

    private List<GameObject> items = new List<GameObject>();

    public void Generate() 
    {

        for(int i = 0; i < itemAmount; i++) 
        {
            items.Add(Instantiate(prefab, transform));
        }
    }
}

[CustomEditor(typeof(GenerateItems))]
public class GenerateItemsEditor : Editor
{
    public override void OnInspectorGUI()
    {

        DrawDefaultInspector();
        EditorGUILayout.Space();
        GenerateItems tileInfo = (GenerateItems)target;

        if (GUILayout.Button("Start Grid"))
        {
            tileInfo.Generate();
        }

    }
}
