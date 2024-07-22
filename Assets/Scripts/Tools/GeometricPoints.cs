using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GeometricPoints
{
    public static void GetForm(Vector3[] centers, float witdh, float height, int resolution, GeometricForm form, out List<Vector3> vertex, out List<int> triangles)
    {
        List<Vector3> geometricForm = new List<Vector3>();
        List<Vector3> Surfaces = new List<Vector3>();
        List<int> SurfacesTriangles = new List<int>();

        List<List<Vector3>> Segments = new List<List<Vector3>>();

        List<Vector3> _vertex = new List<Vector3>();
        List<int> _triangles = new List<int>();

        for (int i = 0; i < centers.Length; i++)
        {

            switch (form)
            {
                case GeometricForm.Sphere:
                    DrawSphere(centers[i], witdh, height, resolution,6, i == 0, i == centers.Length - 1, Segments);
                    break;

                case GeometricForm.Prism:
                    DrawPrism(centers[i], witdh, height, resolution, i == 0, i == centers.Length - 1, Segments);
                    break;

                case GeometricForm.Pyramid:
                    Surfaces = new List<Vector3>();
                    Surfaces.AddRange(DrawPyramid(centers[i], witdh, height, resolution));
                    break;

                case GeometricForm.InvertedPyramid:
                    Surfaces = new List<Vector3>();
                    Surfaces.AddRange(DrawInvertedPyramid(centers[i], witdh, height, resolution));
                    break;

            }


            _vertex.AddRange(Surfaces);
            _triangles.AddRange(SurfacesTriangles);
        }

        GenerateMesh(Segments, out vertex, out triangles);
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

    #region GeometricForms

    public static List<Vector3> DrawPyramid(Vector3 point, float height, float witdh, int resolution)
    {
        List<Vector3> geometricForm = new List<Vector3>();

        // Crear puntos en la base de la pirámide
        float angleStep = 360f / resolution;
        Vector3[] basePoints = new Vector3[resolution];
        for (int i = 0; i < resolution; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = witdh * Mathf.Cos(angle);
            float z = witdh * Mathf.Sin(angle);
            basePoints[i] = point + new Vector3(x, -height / 2, z);
            geometricForm.Add(basePoints[i]);
        }

        // Añadir el vértice superior de la pirámide
        Vector3 topVertex = point + new Vector3(0, height + height * .25f - height / 2, 0);
        geometricForm.Add(topVertex);

        return geometricForm;
    }

    public static List<Vector3> DrawInvertedPyramid(Vector3 point, float height, float witdh, int resolution)
    {
        List<Vector3> geometricForm = new List<Vector3>();

        Vector3 topVertex = point + new Vector3(0, -height * 0.5f, 0);
        geometricForm.Add(topVertex);

        // Crear puntos en la base de la pirámide
        float angleStep = 360f / resolution;
        Vector3[] basePoints = new Vector3[resolution];
        for (int i = 0; i < resolution; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = witdh * Mathf.Cos(angle);
            float z = witdh * Mathf.Sin(angle);
            basePoints[i] = point + new Vector3(x, height + height * .25f - height / 2, z);
            geometricForm.Add(basePoints[i]);
        }

        return geometricForm;
    }

    public static void DrawPrism(Vector3 center, float height, float width, int vertex, bool startPos, bool endPos, List<List<Vector3>> Segments)
    {
        List<Vector3> root = new List<Vector3>();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> topPoint = new List<Vector3>();

        float halfHeight = (height / 2);

        for (int i = 0; i <= vertex; i++)
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

    public static void DrawSphere(Vector3 center, float height, float width, int vertex, int definition, bool startPos, bool endPos, List<List<Vector3>> Segments)
    {
        List<Vector3> root = new List<Vector3>();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> topPoint = new List<Vector3>();


        for (int j = 0; j < definition; j++)
        {
            vertices = new List<Vector3>();
            root = new List<Vector3>();
            topPoint = new List<Vector3>();

            float t = (Mathf.Sin((j / (float)definition * Mathf.PI)) + 1.0f) / 2.0f;
            float interpolatedValue = Mathf.Lerp(width * 0.25f, width, t);

            for (int i = 0; i <= vertex; i++)
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

    #endregion

    #region Triangles
    public static List<int> GeneratePyramidTriangles(List<Vector3> vertices)
    {
        List<int> triangles = new List<int>();
        int numBasePoints = vertices.Count - 1;
        int apexIndex = numBasePoints;

        // Triángulos de la base 
        for (int i = 0; i < numBasePoints - 2; i++)
        {
            triangles.Add(0);
            triangles.Add(i + 1);
            triangles.Add(i + 2);
        }

        // Triángulos de las paredes
        for (int i = 0; i < numBasePoints; i++)
        {
            int nextIndex = (i + 1) % numBasePoints;
            triangles.Add(nextIndex);
            triangles.Add(i);

            triangles.Add(apexIndex);
        }

        return triangles;
    }

    public static List<int> GenerateInvertedPyramidTriangles(List<Vector3> vertices)
    {
        List<int> triangles = new List<int>();
        int numBasePoints = vertices.Count - 1; // Todos los vértices menos el vértice superior
        int topVertexIndex = 0; // El índice del vértice superior
        int baseVertexStartIndex = 1; // El índice de inicio de los vértices de la base

        // Triángulos de la base (base cerrada)
        for (int i = 1; i < numBasePoints - 1; i++)
        {

            triangles.Add(i + baseVertexStartIndex);
            triangles.Add(baseVertexStartIndex);
            triangles.Add(i + baseVertexStartIndex + 1);
        }

        // Triángulos de las paredes
        for (int i = 0; i < numBasePoints; i++)
        {
            int nextIndex = (i + 1) % numBasePoints;
            triangles.Add(baseVertexStartIndex + i);
            triangles.Add(baseVertexStartIndex + nextIndex);
            triangles.Add(topVertexIndex);
        }

        return triangles;
    }

    public static List<int> GenerateSideTriangles(List<Vector3> lower, List<Vector3> upper, int offset)
    {
        List<int> triangles = new List<int>();

        int numLowerVertices = lower.Count;
        int numVertices = upper.Count;

        if (numVertices == 1)
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
            for (int i = 0; i < numVertices; i++)
            {
                int nextIndex = (i + 1) % numVertices;

                triangles.Add(offset + nextIndex);
                triangles.Add(offset + i);
                triangles.Add(offset + numVertices + nextIndex);

                triangles.Add(offset + numVertices + nextIndex);
                triangles.Add(offset + i);
                triangles.Add(offset + numVertices + i);
            }
        }

        return triangles;

    }
    #endregion
}

public enum GeometricForm
{
    Sphere,
    Prism,
    Pyramid,
    InvertedPyramid
}
