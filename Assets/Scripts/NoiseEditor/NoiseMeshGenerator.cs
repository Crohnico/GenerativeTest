using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class NoiseMeshGenerator : MonoBehaviour
{
    public float heightMultiplier = 5f; 
    public float uvTileSize = 0.1f;
    public AnimationCurve curve;

    public void GenerateMesh(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[width * height];
        int[] triangles = new int[(width - 1) * (height - 1) * 6];
        Vector2[] uvs = new Vector2[vertices.Length];  

        int vertexIndex = 0;
        int triangleIndex = 0;


        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float heightValue = curve.Evaluate(heightMap[x, y]) * heightMultiplier;  
                vertices[vertexIndex] = new Vector3(x, heightValue, y);

                uvs[vertexIndex] = new Vector2((float)x / (width - 1), (float)y / (height - 1));

                vertexIndex++;
            }
        }

        for (int y = 0; y < height - 1; y++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                int currentIndex = x + y * width;

                triangles[triangleIndex + 0] = currentIndex;
                triangles[triangleIndex + 1] = currentIndex + width;
                triangles[triangleIndex + 2] = currentIndex + width + 1;


                triangles[triangleIndex + 3] = currentIndex;
                triangles[triangleIndex + 4] = currentIndex + width + 1;
                triangles[triangleIndex + 5] = currentIndex + 1;

                triangleIndex += 6;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals(); 

        GetComponent<MeshFilter>().mesh = mesh;
    }
}