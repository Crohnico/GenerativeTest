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
    //TODO:  Director vector , torsion, size per area

    [Range(1, 10)]
    public int height;
    [Range(2, 10)]
    public int numPoints;

    [Range(3, 10)]
    public int polygonFaces = 10;
    public float meshDistance = 0.2f;


    public float cellWitdh = 1;
    public float cellHeight = 1;

    private Vector3[] points;

    public Vector3 offset;

    [Header("Quadratic")]
    public Vector3 quadraticOffset;
    [Space(10)]
    [Header("Qubic")]
    public Vector3 cubicOffsetBot;
    public Vector3 cubicOffsetTop;

    void OnDrawGizmos()
    {
        CalculateMesh(gizmos: true);
    }

    public void GetBezier()
    {
        Vector3 startPoint = Vector3.zero;
        Vector3 QuadraticPoint = Vector3.zero + Vector3.up * (height/2) + quadraticOffset;
        Vector3 endPoint = Vector3.zero + Vector3.up * (height) + offset;

        Vector3 CubicPointBot = Vector3.zero + Vector3.up * (height*.25f) + cubicOffsetBot;
        Vector3 CubicPointTop = Vector3.zero + Vector3.up * (height*.75f) + cubicOffsetTop;


        cellHeight = (float)height / (float)numPoints;
        cellHeight = cellHeight * 1.2f;


        switch (type) 
        {
            case BezierType.Linear:
                points = Beziers.CalculateBezier(startPoint, endPoint, numPoints);
                break;
            case BezierType.Quadratic:
                points = Beziers.CalculateBezier(startPoint, QuadraticPoint, endPoint, numPoints);
                break;
            case BezierType.Cubic:
                points = Beziers.CalculateBezier(startPoint, CubicPointBot, CubicPointTop, endPoint, numPoints);
                break;
        }
       
    }
    public MeshFilter lastSaved; 
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

        lastSaved = meshFilter;
        meshFilter.mesh = mesh;
        go.transform.parent = null;
    }

    Mesh CalculateMesh(bool gizmos = false)
    {
        GetBezier();
        Mesh mesh = new Mesh();

        GeometricPoints.GetForm(points, cellHeight, cellWitdh, polygonFaces, geometricForm, Vector3.up,out List<Vector3> vertices, out List<int> triangles);

        if (gizmos)
        {
            Gizmos.color = Color.blue;
            foreach (Vector3 v in vertices)
                Gizmos.DrawSphere(v + transform.position, 1 * 0.05f);


            GeometricPoints.CalculateBezierDirections(points, out List<Vector3> directions);

            Vector3 QuadraticPoint = Vector3.zero + Vector3.up * (height / 2) + quadraticOffset;

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(QuadraticPoint, 0.1f);

            for (int i = 0; i < directions.Count - 1; i++) 
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(points[i], points[i] + directions[i]);
            }
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

        GeometricPoints.GenerateUVs(vertices, triangles, out uvs);

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
    }
}
#endif
