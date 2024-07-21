using UnityEngine;
using System.Collections.Generic;

public static class MeshOptimizer
{
    public static void OptimizeMesh(Mesh mesh, float mergeDistance)
    {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        Dictionary<Vector3, int> vertexMap = new Dictionary<Vector3, int>();
        List<Vector3> newVertices = new List<Vector3>();
        List<int> newTriangles = new List<int>();

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];
            Vector3 roundedVertex = RoundVector(vertex, mergeDistance);

            if (!vertexMap.ContainsKey(roundedVertex))
            {
                vertexMap[roundedVertex] = newVertices.Count;
                newVertices.Add(roundedVertex);
            }
        }

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int v0 = vertexMap[RoundVector(vertices[triangles[i]], mergeDistance)];
            int v1 = vertexMap[RoundVector(vertices[triangles[i + 1]], mergeDistance)];
            int v2 = vertexMap[RoundVector(vertices[triangles[i + 2]], mergeDistance)];

            newTriangles.Add(v0);
            newTriangles.Add(v1);
            newTriangles.Add(v2);
        }

        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();
        mesh.RecalculateNormals();
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

    private static Vector3 RoundVector(Vector3 vector, float distance)
    {
        return new Vector3(
            Mathf.Round(vector.x / distance) * distance,
            Mathf.Round(vector.y / distance) * distance,
            Mathf.Round(vector.z / distance) * distance);
    }
}
