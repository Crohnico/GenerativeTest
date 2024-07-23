using GK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MeshCreatorTool : MonoBehaviour
{
    public BezierType type;
    public GeometricForm geometricForm;
    public WidthType widthType;

    public Material material;

    [Range(1, 10)]
    public int height = 5;
    [Range(2, 10)]
    public int numPoints = 5;

    [Range(3, 10)]
    public int polygonFaces = 10;
    public float meshDistance = 0.2f;


    [Range(1,10)]
    public float growthRate = 1;
    public Vector2 growFromTo = new Vector2(.5f, .2f);
    public float cellHeight = 1;

    private Vector3[] points;

    [Header("Quadratic")]
    public Vector3 quadraticOffset;
    [Space(10)]
    [Header("Qubic")]
    private Vector3 cubicOffsetBot = new Vector3(0,0,0);
    public Vector3 cubicOffsetTop;

    private Vector3 _endPoint;

    public void Initialize(Vector3 endPoint, int height, Material material, GeometricForm geometricForm, WidthType widthType, float growthRate, int numPoints,int polygonFaces, Vector2 growFromTo)
    {
        this.material = material;
        _endPoint = endPoint;
        this.height = height;

        this.geometricForm = geometricForm;
        this.widthType = widthType;

        this.growthRate = growthRate;
        this.numPoints = numPoints;
        this.polygonFaces = polygonFaces;
        this.growFromTo = growFromTo;
    }

    void OnDrawGizmos()
    {
        CalculateMesh(gizmos: true);
    }

    void DefineTrackers() 
    {
        float maxX = Mathf.Clamp(cubicOffsetTop.x, -5f, 5f);
        float maxY = Mathf.Clamp(cubicOffsetTop.y, -5f, 5f);
        float maxZ = Mathf.Clamp(cubicOffsetTop.z, -5f, 5f);


        cubicOffsetTop = new Vector3(maxX, maxY, maxZ);
    }

    public void GetBezier()
    {
        DefineTrackers();

        Vector3 startPoint = Vector3.zero;
        Vector3 QuadraticPoint = Vector3.zero + transform.up * (height/2) + quadraticOffset;
        Vector3 endPoint = _endPoint;

        Vector3 CubicPointBot = Vector3.zero + transform.up * (height*.25f) + cubicOffsetBot;
        Vector3 CubicPointTop = Vector3.zero + transform.up * (height*.75f) + cubicOffsetTop;


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

        GeometricPoints.GetForm(points, cellHeight, growFromTo, polygonFaces, geometricForm, widthType, growthRate, transform.up, out List<Vector3> vertices, out List<int> triangles);

        if (gizmos)
        {
            Gizmos.color = Color.blue;
            foreach (Vector3 v in vertices)
                Gizmos.DrawSphere(v + transform.position, 1 * 0.05f);


            GeometricPoints.CalculateBezierDirections(points, out List<Vector3> directions);

            Vector3 CubicPointBot = transform.position + transform.up * (height * .25f) + cubicOffsetBot;
            Vector3 CubicPointTop = transform.position + transform.up * (height * .75f) + cubicOffsetTop;

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(CubicPointBot, 0.1f);
            Gizmos.DrawSphere(CubicPointTop, 0.1f);
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

[CustomEditor(typeof(MeshCreatorTool))]
public class ProceduralTreeEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MeshCreatorTool core = (MeshCreatorTool)target;

        if (GUILayout.Button("Try Generate"))
        {
            core.InitGeneration();
        }
    }
}
#endif
