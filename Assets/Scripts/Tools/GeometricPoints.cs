using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GeometricPoints
{
    public static void GetForm(Vector3[] centers, float witdh, float height, int resolution, GeometricForm form, Vector3 growDirection, out List<Vector3> vertex, out List<int> triangles)
    {
        List<List<Vector3>> Segments = new List<List<Vector3>>();

        List<Vector3> growDirections = new List<Vector3>() { growDirection };
        CalculateBezierDirections(centers, out List <Vector3> bezierDirections);

        growDirections.AddRange(bezierDirections);

        for (int i = 0; i < centers.Length; i++)
        {
            switch (form)
            {
                case GeometricForm.Sphere:
                    DrawSphere(centers[i], witdh, height, resolution, 6, i == 0, i == centers.Length - 1, growDirections[i], Segments, false);
                    break;
                case GeometricForm.InverseSphere:
                    DrawSphere(centers[i], witdh, height, resolution, 6, i == 0, i == centers.Length - 1, growDirections[i],Segments, true);
                    break;

                case GeometricForm.Prism:
                    DrawPrism(centers[i], witdh, height, resolution, i == 0, i == centers.Length - 1,  growDirections[i], Segments);
                    break;

                case GeometricForm.Pyramid:
                    DrawPyram(centers[i], witdh, height, resolution, 6, i == 0, i == centers.Length - 1, growDirections[i], Segments, false);
                    break;

                case GeometricForm.InvertedPyramid:
                    DrawPyram(centers[i], witdh, height, resolution, 6, i == 0, i == centers.Length - 1, growDirections[i], Segments, true);
                    break;

            }
        }

        GenerateMesh(Segments, out vertex, out triangles);
    }

    public static void CalculateBezierDirections(Vector3[] bezierPoints, out List<Vector3> directions)
    {
        int count = bezierPoints.Length;

        List<Vector3> newDirections = new List<Vector3>();

        if (count >= 2)
        {
            for (int i = 0; i < count - 1; i++)
            {
                Vector3 direction = bezierPoints[i + 1] - bezierPoints[i];
                newDirections.Add(direction.normalized);
            }

            if (count > 1)
            {
                Vector3 lastDirection = bezierPoints[count - 1] - bezierPoints[count - 2];
                newDirections.Add(lastDirection.normalized);
            }
        }

        directions = newDirections;
    }

    public static void GenerateMesh(List<List<Vector3>> layers, out List<Vector3> vertices, out List<int> triangles)
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();

        for (int i = 0; i < layers.Count; i++)
        {
            vertices.AddRange(layers[i]);

            if (i > 0)
            {
                List<int> sideTriangles = GenerateSideTriangles(layers[i - 1], layers[i], vertices.Count - layers[i].Count - layers[i - 1].Count);
                triangles.AddRange(sideTriangles);
            }
        }
    }

    public static void DrawPrism(Vector3 center, float height, float width, int vertex, bool startPos, bool endPos, Vector3 growDirection, List<List<Vector3>> Segments)
    {
        List<Vector3> root = new List<Vector3>();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> topPoint = new List<Vector3>();

        float halfHeight = (height / 2);

        for (int i = 0; i < vertex; i++)
        {
            float theta = 2 * Mathf.PI * i / vertex;
            float x = width * Mathf.Cos(theta);
            float z = width * Mathf.Sin(theta);

            if (startPos)
                root.Add(center + new Vector3(x, 0, z));

            vertices.Add(center + new Vector3(x, height, z));
        }

        if (endPos)
            topPoint.Add(center + new Vector3(0, height, 0));

        if (root.Count != 0) Segments.Add(root);
        if (vertices.Count != 0) Segments.Add(vertices);
        if (topPoint.Count != 0) Segments.Add(topPoint);
    }

    public static void DrawSphere(Vector3 center, float height, float width, int vertex, int definition, bool startPos, bool endPos, Vector3 growDirection, List<List<Vector3>> Segments, bool inverse)
    {
        List<Vector3> root = new List<Vector3>();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> topPoint = new List<Vector3>();


        for (int j = 0; j < definition; j++)
        {
            vertices = new List<Vector3>();
            root = new List<Vector3>();
            topPoint = new List<Vector3>();

            float t =  (Mathf.Sin((j / (float)definition * Mathf.PI)) + 1.0f) / 2.0f;
            float interpolatedValue = (!inverse) ? Mathf.Lerp(width * 0.25f, width, t) : Mathf.Lerp(width , width * 0.25f, t);

            for (int i = 0; i < vertex; i++)
            {
                float theta = 2 * Mathf.PI * i / vertex;
                float x = interpolatedValue * Mathf.Cos(theta);
                float z = interpolatedValue * Mathf.Sin(theta);

                if (startPos && j == 0)
                    root.Add(center + new Vector3(x, 0, z));

                vertices.Add(center + new Vector3(x, height * ((float)j / (float)definition), z));
            }


            if (endPos && j == definition - 1)
                topPoint.Add(center + new Vector3(0, height, 0));

            if (root.Count != 0) Segments.Add(root);
            if (vertices.Count != 0) Segments.Add(vertices);
            if (topPoint.Count != 0) Segments.Add(topPoint);
        }
    }

    public static void DrawPyram(Vector3 center, float height, float width, int vertex, int definition, bool startPos, bool endPos, Vector3 growDirection, List<List<Vector3>> Segments, bool invert)
    {
        List<Vector3> root = new List<Vector3>();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> topPoint = new List<Vector3>();

        for (int j = 0; j < definition; j++)
        {
            vertices = new List<Vector3>();
            root = new List<Vector3>();
            topPoint = new List<Vector3>();

            float t = (!invert) ? ((float)j / (float)definition) : 1 - ((float)j / (float)definition);
            float interpolatedValue = Mathf.Lerp(width , width * 0.25f, t);

            for (int i = 0; i < vertex; i++)
            {
                float theta = 2 * Mathf.PI * i / vertex;
                float x = interpolatedValue * Mathf.Cos(theta);
                float z = interpolatedValue * Mathf.Sin(theta);

                if (startPos && j == 0)
                    root.Add(center + new Vector3(x, 0, z));

                vertices.Add(center + new Vector3(x, height * ((float)j / (float)definition), z));
            }

            if (endPos && j == definition - 1)
                topPoint.Add(center + new Vector3(0, height, 0));

            if (root.Count != 0) Segments.Add(root);
            if (vertices.Count != 0) Segments.Add(vertices);
            if (topPoint.Count != 0) Segments.Add(topPoint);
        }
    }

    public static List<int> GenerateSideTriangles(List<Vector3> lower, List<Vector3> upper, int offset)
    {
        List<int> triangles = new List<int>();

        int numLowerVertices = lower.Count;
        int numUpperVertices = upper.Count;


        if (numUpperVertices == 1)
        {
            int centralIndex = offset + numLowerVertices;

            for (int i = 0; i < numLowerVertices; i++)
            {
                int nextIndex = (i + 1) % numLowerVertices;

                triangles.Add(offset + nextIndex);
                triangles.Add(offset + i);
                triangles.Add(centralIndex);
            }
        }

        else
        {
            for (int i = 0; i < numLowerVertices; i++)
            {
                int nextIndex = (i + 1) % numLowerVertices;

                triangles.Add(offset + nextIndex);
                triangles.Add(offset + i);
                triangles.Add(offset + numLowerVertices + nextIndex);

                triangles.Add(offset + numLowerVertices + nextIndex);
                triangles.Add(offset + i);
                triangles.Add(offset + numLowerVertices + i);
            }
        }

        return triangles;
    }

    public static void GenerateUVs(List<Vector3> vertices, List<int> triangles, out Vector2[] uvs)
    {
        uvs = new Vector2[vertices.Count];

        // Find the min and max y-values
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        float minX = float.MaxValue;
        float maxX = float.MinValue;

        foreach (var vertex in vertices)
        {
            if (vertex.y < minY) minY = vertex.y;
            if (vertex.y > maxY) maxY = vertex.y;
            if (vertex.x < minX) minX = vertex.x;
            if (vertex.x > maxX) maxX = vertex.x;
        }

        float rangeX = maxX - minX;
        float rangeY = maxY - minY;

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 vertex = vertices[i];
            // Normalize u based on the full width
            float u = (vertex.x - minX) / rangeX;
            // Normalize v based on the height
            float v = (vertex.y - minY) / rangeY;
            uvs[i] = new Vector2(u, v);
        }
    }
}

public enum GeometricForm
{
    Sphere,
    Prism,
    Pyramid,
    InvertedPyramid,
    InverseSphere
}
