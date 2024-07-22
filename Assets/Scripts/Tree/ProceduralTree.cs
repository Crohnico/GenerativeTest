using GK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ProceduralTree : MonoBehaviour
{
    public BezierType type;
    public GeometricForm geometricForm;

    public Material material;
    //TODO:  Director vector , torsion

    [Range(1, 10)]
    public int height;
    [Range(2, 10)]
    public int numPoints;

    [Range(3, 10)]
    public int resolution = 10;
    public float meshDistance = 0.2f;


    public float cellWitdh = 1;
    public float cellHeight = 1;

    private Vector3[] points;

    private float CellHeight = 1f;

    public Vector3 offset;

    [HideInInspector]
    public MeshFilter lastCreated;
    void OnDrawGizmos()
    {
        CalculateMesh(gizmos: true);
    }

    public void GetBezier()
    {
        Vector3 startPoint = Vector3.zero;
        Vector3 endPoint = Vector3.zero + Vector3.up * (height) + offset;

        cellHeight = (float)height / (float)numPoints;
        cellHeight = cellHeight * 1.2f;
        points = Beziers.CalculateBezier(startPoint, endPoint, numPoints);
    }

    public void InitGeneration()
    {
        Mesh mesh = CalculateMesh();

        GameObject go = new GameObject();
        MeshFilter meshFilter = go.AddComponent<MeshFilter>();
        MeshRenderer renderer = go.AddComponent<MeshRenderer>();
        renderer.material = material;
        go.name = "Combined Meshes";
        go.transform.parent = transform;
        go.transform.localPosition = Vector3.zero;

        meshFilter.mesh = mesh;
        lastCreated = meshFilter;
        go.transform.parent = null;
    }

    public void Simplified()
    {
        GameObject go = new GameObject();
        MeshFilter meshFilter = go.AddComponent<MeshFilter>();
        MeshRenderer renderer = go.AddComponent<MeshRenderer>();
        renderer.material = material;
        go.name = "Simplified Meshes";
        go.transform.parent = transform;
        go.transform.localPosition = Vector3.zero;
        lastCreated.sharedMesh = lastCreated.mesh;

        MeshOptimizer.CombineMeshes(new List<MeshFilter>() { lastCreated, meshFilter });
    }

    Mesh CalculateMesh(bool gizmos = false)
    {
        GetBezier();
        Mesh mesh = new Mesh();

        GeometricPoints.GetForm(points, cellHeight, cellWitdh, resolution, geometricForm, out List<Vector3> vertices, out List<int> triangles);

        if (gizmos)
        {
            foreach (Vector3 v in vertices)
                Gizmos.DrawSphere(v + transform.position, 1 * 0.05f);
        }
        else
        {
            mesh = CreateMesh(vertices, triangles);
        }

        return mesh;
    }

    public Mesh CreateMesh(List<Vector3> vertices, List<int> triangles)
    {
        Mesh mesh = new Mesh();

        Vector2[] uvs = new Vector2[vertices.Count];
        Vector3 center = Vector3.zero;
        float minY = float.MaxValue, maxY = float.MinValue;

        foreach (Vector3 vertex in vertices)
        {
            if (vertex.y < minY) minY = vertex.y;
            if (vertex.y > maxY) maxY = vertex.y;
        }

        //TODO: Fix top vertex

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 vertex = vertices[i];
            float u = (float)(i % (resolution + 1)) / resolution;
            float v = (vertex.y - minY) / (maxY - minY);
            uvs[i] = new Vector2(u, v);
        }


        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs;

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();

        return mesh;
    }

}

#if UNITY_EDITOR

[CustomEditor(typeof(ProceduralTree))]
public class ProceduralTreeEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ProceduralTree core = (ProceduralTree)target;

        if (GUILayout.Button("Try Generate"))
        {
            core.InitGeneration();
        }
        if (core.lastCreated != null)
        {
            if (GUILayout.Button("Try Simplify"))
            {
                core.Simplified();
            }
        }
    }
}
#endif
