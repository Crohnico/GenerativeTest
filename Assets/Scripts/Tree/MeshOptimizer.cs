using UnityEngine;
using System.Collections.Generic;
using Habrador_Computational_Geometry;
using System.Linq;

public static class MeshOptimizer
{
    private class Vector3EqualityComparer : IEqualityComparer<Vector3>
    {
        public bool Equals(Vector3 v1, Vector3 v2)
        {
            return v1.Equals(v2);
        }

        public int GetHashCode(Vector3 v)
        {
            return v.GetHashCode();
        }
    }
    public static void OptimizeMesh(Mesh mesh, float mergeDistance)
    {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector2[] uvs = mesh.uv;

        Dictionary<Vector3, int> vertexMap = new Dictionary<Vector3, int>(new Vector3EqualityComparer());
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
                newVertices.Add(vertex);
                newUVs.Add(uvs.Length > i ? uvs[i] : Vector2.zero);
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

        mesh.Clear();
        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();
        mesh.uv = newUVs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    public static Vector3 RoundVector(Vector3 vector, float tolerance)
    {
        float x = Mathf.Round(vector.x / tolerance) * tolerance;
        float y = Mathf.Round(vector.y / tolerance) * tolerance;
        float z = Mathf.Round(vector.z / tolerance) * tolerance;
        return new Vector3(x, y, z);
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
}


