using UnityEngine;

public class WaterMesh : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public float waveHeight = 1.0f;
    public float waveLength = 1.0f;
    public float waveSpeed = 1.0f;
    public Vector2 waveDirection = new Vector2(1, 0);

    private Mesh mesh;
    private Vector3[] vertices;

    void Start()
    {
        GenerateMesh();
    }

    void Update()
    {
        UpdateMesh();
    }

    void GenerateMesh()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        vertices = new Vector3[(width + 1) * (height + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[width * height * 6];

        for (int i = 0, z = 0; z <= height; z++)
        {
            for (int x = 0; x <= width; x++, i++)
            {
                vertices[i] = new Vector3(x, 0, z);
                uv[i] = new Vector2((float)x / width, (float)z / height);
            }
        }

        for (int ti = 0, vi = 0, y = 0; y < height; y++, vi++)
        {
            for (int x = 0; x < width; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + width + 1;
                triangles[ti + 5] = vi + width + 2;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    void UpdateMesh()
    {
        for (int i = 0, z = 0; z <= height; z++)
        {
            for (int x = 0; x <= width; x++, i++)
            {
                float y = GerstnerWave(x, z);
                vertices[i] = new Vector3(x, y, z);
            }
        }
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }

    float GerstnerWave(float x, float z)
    {
        float k = 2 * Mathf.PI / waveLength;
        float c = Mathf.Sqrt(9.8f / k);
        float f = k * (Vector2.Dot(waveDirection.normalized, new Vector2(x, z)) - c * Time.time * waveSpeed);
        return waveHeight * Mathf.Sin(f);
    }
}
