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
    //TODO: Calcular la altura en funcion de los puntos y la "scale",Shape: Cilinder , Director vector , torsion

    [Range(1, 10)]
    public int height;
    [Range(2, 10)]
    public int numPoints;

    [Range(3, 10)]
    public int resolution = 10;
    public float meshDistance = 0.2f;


    public float cellWitdh = 1;
    public float cellHeight = 1;

    private List<Mesh> meshes = new List<Mesh>();
    private Vector3[] points;

    private float CellHeight = 1f;
    void OnDrawGizmos()
    {
        GetBezier();

        foreach (Vector3 point in points)
        {
            List<Vector3> form = GeometricPoints.GetForm(point, cellHeight, cellWitdh, resolution, geometricForm);
            foreach (Vector3 v in form)
                Gizmos.DrawSphere(v, 1 * 0.05f);
        }

    }

    public void GetBezier() 
    {
        Vector3 startPoint = transform.position + Vector3.up;
        Vector3 endPoint = transform.position + Vector3.up * (height);

        cellHeight = (float)height/ (float)numPoints;
        cellHeight = cellHeight*1.2f;
        points = Beziers.CalculateBezier(startPoint , endPoint, numPoints);
    }

    public void InitGeneration()
    {
        List<MeshFilter> meshes = new List<MeshFilter>();

        GetBezier();

        foreach (Vector3 point in points)
        {
            List<Vector3> vertices = (GeometricPoints.GetForm(point, cellHeight, cellWitdh, resolution, geometricForm));
            List<int> triangles = (GeometricPoints.GetTriangles(vertices, geometricForm, resolution));
            meshes.Add(CreateMesh(vertices, triangles));
        }

        Mesh combinedMeshes = MeshOptimizer.CombineMeshes(meshes);
        GameObject go = new GameObject();
        MeshFilter meshFilter = go.AddComponent<MeshFilter>();
        MeshRenderer renderer = go.AddComponent<MeshRenderer>();
        renderer.material = material;
        go.name = "Combined Meshes";

        meshFilter.mesh = combinedMeshes;
        meshFilter.mesh = MeshOptimizer.OptimizeMesh(meshFilter.mesh, meshDistance);

        for (int i = meshes.Count - 1; i >= 0; i--)
            Destroy(meshes[i].gameObject);
    }

    public MeshFilter CreateMesh(List<Vector3> vertices, List<int> triangles)
    {
        Mesh mesh = new Mesh();

        GameObject go = new GameObject();

        MeshFilter filter = go.AddComponent<MeshFilter>();

        Vector2[] uvs = new Vector2[vertices.Count];

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 vertex = vertices[i];
            float u = Mathf.Atan2(vertex.z, vertex.x) / (2 * Mathf.PI) + 0.5f;
            float v = Mathf.Asin(vertex.y / vertex.magnitude) / Mathf.PI + 0.5f;
            uvs[i] = new Vector2(u, v);
        }


        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs;

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();

        filter.mesh = mesh;

        return filter;
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
    }
}
#endif
