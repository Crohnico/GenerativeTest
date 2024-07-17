using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MeshGeneratorPlane : MonoBehaviour
{
    public int gridSizeX = 10;
    public int gridSizeZ = 10;
    public float spacing = 1f;
    public float heightMultiplier = 1f;
    public float uvScale = 1f;
    public Material material;
    public Grid grid;

    public void Test()
    {
        List<Vector3> vertices = new List<Vector3>();
        Vector2[] uv = new Vector2[(gridSizeX + 1) * (gridSizeZ + 1)];

        for (int z = 0; z <= gridSizeZ; z++)
        {
            for (int x = 0; x <= gridSizeX; x++)
            {
                // Obtener la celda desde la grid
                IACell cell = Grid.Instance.GetGridCell(x, z);

                // Asegurarse de que la celda no sea nula y tenga posición válida
                if (cell != null)
                {
                    float posX = cell.Position.x - (gridSizeX / 2) * spacing;
                    float posZ = cell.Position.z - (gridSizeZ / 2) * spacing;
                    float posY = cell.height * heightMultiplier;

                    // Añadir el vértice y el UV correspondiente
                    vertices.Add(new Vector3(posX, posY, posZ));
                    uv[z * (gridSizeX + 1) + x] = new Vector2((float)x / gridSizeX * uvScale, (float)z / gridSizeZ * uvScale);
                }
                else
                {
                    Debug.LogWarning($"Cell at ({x}, {z}) is null.");
                }
            }
        }

        // Llamar al método para generar la malla
        GenerateMesh(vertices.ToArray(), uv);
    }

    public void GenerateMesh(Vector3[] vertices, Vector2[] uv)
    {
        int[] triangles = new int[gridSizeX * gridSizeZ * 6];
        int index = 0;
        int vertexIndex = 0;

        for (int z = 0; z < gridSizeZ; z++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                int bottomLeft = vertexIndex;
                int bottomRight = vertexIndex + 1;
                int topLeft = vertexIndex + (gridSizeX + 1);
                int topRight = vertexIndex + (gridSizeX + 1) + 1;

                triangles[index++] = bottomLeft;
                triangles[index++] = topLeft;
                triangles[index++] = bottomRight;
                triangles[index++] = bottomRight;
                triangles[index++] = topLeft;
                triangles[index++] = topRight;

                vertexIndex++;
            }
            vertexIndex++;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;

        mesh.RecalculateNormals();

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();

        meshRenderer.material = material;

        meshFilter.mesh = mesh;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MeshGeneratorPlane))]
public class MeshGeneratorPlaneEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MeshGeneratorPlane core = (MeshGeneratorPlane)target;

        if (GUILayout.Button("PintarMapa"))
        {
            core.Test();
        }
    }
}
#endif
