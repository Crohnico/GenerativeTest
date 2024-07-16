using Habrador_Computational_Geometry;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LeaveMesh : MonoBehaviour
{
    public bool removeUnwantedTriangles = true;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    public void Init(List<Vector3> trunk, Material material, System.Action callback)
    {
        _meshFilter = gameObject.AddComponent<MeshFilter>();


        _meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (!_meshRenderer) _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        _meshRenderer.material = material;

        HashSet<Vector3> points_Unity = new HashSet<Vector3>(trunk);
        HashSet<MyVector3> points = new HashSet<MyVector3>(points_Unity.Select(x => x.ToMyVector3()));

        //Normalize
        Normalizer3 normalizer = new Normalizer3(new List<MyVector3>(points));
        HashSet<MyVector3> points_normalized = normalizer.Normalize(points);


        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
        timer.Start();
        HalfEdgeData3 convexHull_normalized = _ConvexHull.Iterative_3D(points_normalized, removeUnwantedTriangles, normalizer);
        timer.Stop();
        Debug.Log($"Generated a 3d convex hull in {timer.ElapsedMilliseconds / 1000f} seconds with {convexHull_normalized.faces.Count} triangles");

        if (convexHull_normalized != null)
        {
            HalfEdgeData3 convexHull = normalizer.UnNormalize(convexHull_normalized);

            MyMesh myMesh = convexHull.ConvertToMyMesh("convex hull", MyMesh.MeshStyle.HardAndSoftEdges);
            Mesh convexHullMesh = myMesh.ConvertToUnityMesh(generateNormals: false, myMesh.meshName);


            Vector2[] uvs = new Vector2[convexHullMesh.vertices.Length];

            for (int i = 0; i < convexHullMesh.vertices.Length; i++)
            {
                uvs[i] = new Vector2(convexHullMesh.vertices[i].x, convexHullMesh.vertices[i].z);
            }


            _meshFilter.mesh = convexHullMesh;
            _meshFilter.mesh.uv = uvs;

            _meshFilter.mesh.RecalculateNormals();
            _meshFilter.mesh.RecalculateBounds();
        }

        callback?.Invoke();
    }
}
