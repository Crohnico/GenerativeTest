using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


public enum Scale
{
    SHORT,
    MEDIUM,
    LARGE,
    NONE
}
[System.Serializable]
[CreateAssetMenu(fileName = "NPC Template", menuName = "NPC/Template")]
public class NPCTemplate : ScriptableObject
{
    public Sprite image;
    [SerializeField]
    public List<Scale> scalesPosibles = new List<Scale>();

    [System.NonSerialized]
    public Scale scale = Scale.NONE;
}

#if UNITY_EDITOR

[CustomEditor(typeof(NPCTemplate))]
public class NPCTemplateEditor : Editor
{

    private List<Scale> selectedTypes = new List<Scale>();
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        NPCTemplate npcTemplate = (NPCTemplate)target;
        selectedTypes = new List<Scale>(npcTemplate.scalesPosibles);

        EditorUtility.SetDirty(npcTemplate);

        if (GUILayout.Button("Asignar Sprite"))
       {


            int number = ExtractNumberFromName(npcTemplate.name);

            string id = (number < 10) ? $"0{number}" : number.ToString();

            string spritePath = $"Assets/Imagenes/NPCs/animal{id}.png";

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

            if (sprite != null)
            {
                npcTemplate.image = sprite;
                Debug.Log("Sprite asignado correctamente.");
            }
            else
            {
                Debug.LogError("Sprite no encontrado.");
            }

            AssetDatabase.SaveAssets();

        }

        if (npcTemplate.image != null)
        {
            GUILayout.Label("Sprite:");
            EditorGUILayout.ObjectField(npcTemplate.image, typeof(Sprite), false);
            GUILayout.Label(npcTemplate.image.texture, GUILayout.Height(400)); 
        }

        GUILayout.BeginHorizontal();

        foreach (Scale type in System.Enum.GetValues(typeof(Scale)))
        {
            if (type == Scale.NONE) continue;
            // Obtener el estilo del botón dependiendo de si el tipo está seleccionado o no
            GUIStyle buttonStyle = selectedTypes.Contains(type) ? "Button" : "Button";


            bool isSelected = selectedTypes.Contains(type);
            bool newSelection = GUILayout.Toggle(isSelected, type.ToString(), buttonStyle);
            if (newSelection != isSelected)
            {
            
                if (newSelection)
                {
                    selectedTypes.Add(type);
                }
                else
                {
                    selectedTypes.Remove(type);
                }
            }
        }

        GUILayout.EndHorizontal();

        if (npcTemplate.scalesPosibles.Count != selectedTypes.Count)
            AssetDatabase.SaveAssets();
        npcTemplate.scalesPosibles = selectedTypes;
    }

    private int ExtractNumberFromName(string name)
    {
        // Utilizar una expresión regular para encontrar números en el nombre
        Match match = Regex.Match(name, @"\d+");

        // Si se encuentra un número, convertirlo a entero y devolverlo
        if (match.Success)
        {
            return int.Parse(match.Value);
        }
        else
        {
            return 0; // Si no se encuentra ningún número, devolver 0 o el valor predeterminado que desees
        }
    }
}

#endif
