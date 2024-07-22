using UnityEngine;
using System.Collections.Generic;
using Habrador_Computational_Geometry;
using System.Linq;

public static class MeshOptimizer
{
    public static void OptimizeMesh(Mesh mesh, float mergeDistance)
    {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector2[] uvs = mesh.uv;


        Dictionary<Vector3, int> vertexMap = new Dictionary<Vector3, int>();

        List<Vector3> newVertices = new List<Vector3>();
        List<Vector2> newUVs = new List<Vector2>();
        List<int> newTriangles = new List<int>();

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];
            Vector3 roundedVertex = RoundVector(vertex, mergeDistance);

            if (!vertexMap.ContainsKey(roundedVertex))
            {
                vertexMap[roundedVertex] = newVertices.Count;
                newVertices.Add(roundedVertex);
                newUVs.Add(uvs[i]);
            }
        }


        for (int i = 0; i < triangles.Length; i += 3)
        {
            int v0 = vertexMap[RoundVector(vertices[triangles[i]], mergeDistance)];
            int v1 = vertexMap[RoundVector(vertices[triangles[i + 1]], mergeDistance)];
            int v2 = vertexMap[RoundVector(vertices[triangles[i + 2]], mergeDistance)];

            if (v0 != v1 && v1 != v2 && v2 != v0)
            {
                newTriangles.Add(v0);
                newTriangles.Add(v1);
                newTriangles.Add(v2);
            }
        }

        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();
        mesh.uv = newUVs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }


    public static Mesh CombineMeshes(List<MeshFilter> meshes)
    {
        List<CombineInstance> combineInstances = new List<CombineInstance>();

        foreach (MeshFilter meshFilter in meshes)
        {
            Mesh mesh = meshFilter.sharedMesh;
            if (mesh == null) continue;

            CombineInstance combineInstance = new CombineInstance
            {
                mesh = mesh,
                transform = meshFilter.transform.localToWorldMatrix
            };
            combineInstances.Add(combineInstance);

        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combineInstances.ToArray(), true, true);
        return combinedMesh;
    }

    private static Vector3 RoundVector(Vector3 v, float distance)
    {
        return new Vector3(
            Mathf.Round(v.x / distance) * distance,
            Mathf.Round(v.y / distance) * distance,
            Mathf.Round(v.z / distance) * distance
        );
    }

    public static void SimplifyMesh(MeshFilter from, MeshFilter to)
    {
        //Has to be sharedMesh if we are using Editor tools
        Mesh meshToSimplify = from.sharedMesh;
        MyMesh myMeshToSimplify = new MyMesh(meshToSimplify);


        Normalizer3 normalizer = new Normalizer3(myMeshToSimplify.vertices);

        myMeshToSimplify.vertices = normalizer.Normalize(myMeshToSimplify.vertices);

        HalfEdgeData3 myMeshToSimplify_HalfEdge = new HalfEdgeData3(myMeshToSimplify, HalfEdgeData3.ConnectOppositeEdges.Fast);

        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

        timer.Start();

        HalfEdgeData3 mySimplifiedMesh_HalfEdge = MeshSimplification_QEM.Simplify(myMeshToSimplify_HalfEdge, maxEdgesToContract: 10000, maxError: Mathf.Infinity, normalizeTriangles: true);

        timer.Stop();

        Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds to simplify the mesh");

        timer.Reset();
        timer.Start();

        //From half-edge to mesh
        MyMesh mySimplifiedMesh = mySimplifiedMesh_HalfEdge.ConvertToMyMesh("Simplified mesh", MyMesh.MeshStyle.HardEdges);

        //Un-Normalize
        mySimplifiedMesh.vertices = normalizer.UnNormalize(mySimplifiedMesh.vertices);

        //Convert to global space
        Transform trans = from.transform;

        mySimplifiedMesh.vertices = mySimplifiedMesh.vertices.Select(x => trans.TransformPoint(x.ToVector3()).ToMyVector3()).ToList();

        //Convert to mesh
        Mesh unitySimplifiedMesh = mySimplifiedMesh.ConvertToUnityMesh(generateNormals: true, meshName: "simplified mesh");

        //Attach to new game object
        to.mesh = unitySimplifiedMesh;

        timer.Stop();

        Debug.Log($"It took {timer.ElapsedMilliseconds / 1000f} seconds to finalize the mesh after simplifying");
    }
}
