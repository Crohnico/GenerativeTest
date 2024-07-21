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

    private List<Vector3> form;
    void OnDrawGizmos()
    {
        if (CheckForUpdate())
        {
            Vector3[] points = Beziers.CalculateBezier(transform.position, transform.position + Vector3.up * height * radio, height);
            form = GeometricPoints.GetForm(points, radio, resolution, tolerance, geometricForm);
        }

        foreach (Vector3 point in form)
        {
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
        StartCoroutine(GenerateMesh());
    }

    public IEnumerator GenerateMesh()
    {
        var calc = new ConvexHullCalculator();
        var verts = new List<Vector3>();
        var tris = new List<int>();
        var normals = new List<Vector3>();

        Vector3[] points = Beziers.CalculateBezier(transform.position, transform.position + Vector3.up * height * radio, height);
        form = GeometricPoints.GetForm(points, radio, resolution, tolerance, geometricForm);

        calc.GenerateHull(form, true, ref verts, ref tris, ref normals);

        var rock = new GameObject();

        rock.transform.SetParent(transform, false);
        rock.transform.localPosition = Vector3.zero;
        rock.transform.localRotation = Quaternion.identity;
        rock.transform.localScale = Vector3.one;

        var mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetNormals(normals);

        MeshRenderer renderer = rock.AddComponent<MeshRenderer>();
        renderer.material = material;
        rock.AddComponent<MeshFilter>().mesh = mesh;
        rock.AddComponent<MeshCollider>().sharedMesh = mesh;
        rock.transform.position = transform.position;

        yield return new WaitForSeconds(0.5f);

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
