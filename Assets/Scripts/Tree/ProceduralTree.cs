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

    [Range(3, 10)]
    public int height;

    [Header("Gizmos")]
    public float radio = 1.0f;
    [Range(3, 10)]
    public int resolution = 10;
    public float tolerance = 0.1f;

    private float _lastRadio, lastTolerance;
    private int _lastResolution, lastHeight;
    private BezierType _lastType;
    private GeometricForm lastForm;

    private List<Mesh> meshes = new List<Mesh>();
    private Vector3[] points;
    void OnDrawGizmos()
    {
        if (CheckForUpdate())
        {
            points = Beziers.CalculateBezier(transform.position, transform.position + Vector3.up * height * radio, height);
        }

        foreach (Vector3 point in points)
        {
            List<Vector3> form = GeometricPoints.GetForm(point, radio, resolution, tolerance, geometricForm);
            foreach (Vector3 v in form)
                Gizmos.DrawSphere(v, radio * 0.05f);
        }

    }


    public bool CheckForUpdate()
    {
        bool result = false;

        if (_lastRadio != radio) { _lastRadio = radio; result = true; }
        if (lastTolerance != tolerance) { lastTolerance = tolerance; result = true; }
        if (_lastResolution != resolution) { _lastResolution = resolution; result = true; }
        if (lastHeight != height) { lastHeight = height; result = true; }
        if (_lastType != type) { _lastType = type; result = true; }
        if (lastForm != geometricForm) { lastForm = geometricForm; result = true; }

        return result;
    }

    public void InitGeneration()
    {
        List<MeshFilter> meshes = new List<MeshFilter>();

        points = Beziers.CalculateBezier(transform.position, transform.position + Vector3.up * height * radio, height);
        if (CheckForUpdate())
        {
            points = Beziers.CalculateBezier(transform.position, transform.position + Vector3.up * height * radio, height);
        }

        foreach (Vector3 point in points)
        {
            List<Vector3> vertices =  (GeometricPoints.GetForm(point, radio, resolution, tolerance, geometricForm));
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
    }

    public MeshFilter CreateMesh(List<Vector3> vertices, List<int> triangles)
    {
        Mesh mesh = new Mesh();

        GameObject go = new GameObject();

        MeshFilter filter = go.AddComponent<MeshFilter>();

  
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

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
