using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GeometricPoints
{
    public static List<Vector3> GetForm(Vector3 center, float radius, int resolution, float tolerance, GeometricForm form)
    {
        List<Vector3> geometricForm = new List<Vector3>();
        List<Vector3> Surfaces = new List<Vector3>();

        switch (form)
        {
            case GeometricForm.Sphere:
                 Surfaces = new List<Vector3>();
                 Surfaces.AddRange(DrawSphere(center, radius, resolution));
                return Surfaces;

            case GeometricForm.Cube:
                Surfaces = new List<Vector3>();
                Surfaces.AddRange(DrawCube(center, radius));
                return Surfaces;

            case GeometricForm.Pyramid:

                Surfaces = new List<Vector3>();
                Surfaces.AddRange(DrawPyramid(center, radius, radius, resolution));
                return Surfaces;

            case GeometricForm.InvertedPyramid:

                Surfaces = new List<Vector3>();
                Surfaces.AddRange(DrawInvertedPyramid(center, radius, radius, resolution));
                return Surfaces;

        }

        return geometricForm;
    }

    #region GeometricForms
    public static List<Vector3> DrawSphere(Vector3 center, float radius, int resolution)
    {
        List<Vector3> vertList = new List<Vector3>();

        for (int i = 0; i <= resolution; i++)
        {
            float theta = 2 * Mathf.PI * i / resolution; 
            for (int j = 0; j <= resolution; j++) 
            {
                float phi = Mathf.PI * j / resolution; 

                float x = radius * Mathf.Sin(phi) * Mathf.Cos(theta);
                float y = radius * Mathf.Sin(phi) * Mathf.Sin(theta);
                float z = radius * Mathf.Cos(phi);

                vertList.Add(center + new Vector3(x, y, z));
            }
        }

        return vertList;
    }

    public static List<Vector3> DrawCube(Vector3 center, float radius)
    {
        List<Vector3> vertices = new List<Vector3>();

        float half = radius / 2;

        vertices.Add(center + new Vector3(-half, -half, -half));  // 0
        vertices.Add(center + new Vector3(half, -half, -half));   // 1
        vertices.Add(center + new Vector3(-half, half, -half));   // 2
        vertices.Add(center + new Vector3(half, half, -half));    // 3
        vertices.Add(center + new Vector3(-half, -half, half));   // 4
        vertices.Add(center + new Vector3(half, -half, half));    // 5
        vertices.Add(center + new Vector3(-half, half, half));    // 6
        vertices.Add(center + new Vector3(half, half, half));     // 7

        return vertices;
    }

    public static List<Vector3> DrawPyramid(Vector3 point, float baseRadius, float height, int resolution)
    {
        List<Vector3> geometricForm = new List<Vector3>();

        // Crear puntos en la base de la pirámide
        float angleStep = 360f / resolution;
        Vector3[] basePoints = new Vector3[resolution];
        for (int i = 0; i < resolution; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = baseRadius * Mathf.Cos(angle);
            float z = baseRadius * Mathf.Sin(angle);
            basePoints[i] = point + new Vector3(x, 0, z);
            geometricForm.Add(basePoints[i]);
        }

        // Añadir el vértice superior de la pirámide
        Vector3 topVertex = point + new Vector3(0, height, 0);
        geometricForm.Add(topVertex);

        return geometricForm;
    }

    public static List<Vector3> DrawInvertedPyramid(Vector3 point, float baseRadius, float height, int resolution)
    {
        List<Vector3> geometricForm = new List<Vector3>();

        Vector3 topVertex = point + new Vector3(0, 0, 0);
        geometricForm.Add(topVertex);

        // Crear puntos en la base de la pirámide
        float angleStep = 360f / resolution;
        Vector3[] basePoints = new Vector3[resolution];
        for (int i = 0; i < resolution; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = baseRadius * Mathf.Cos(angle);
            float z = baseRadius * Mathf.Sin(angle);
            basePoints[i] = point + new Vector3(x, height, z);
            geometricForm.Add(basePoints[i]);
        }

        return geometricForm;
    }

    #endregion

    #region ValidateAreas

    public static List<Vector3> ValidateSphereSurfacePoints(Vector3[] centers, Vector3[] surfaces, float radius)
    {
        List<Vector3> result = new List<Vector3>();

        foreach (Vector3 point in surfaces)
        {
            bool isInside = false;

            foreach (Vector3 center in centers)
            {
                if (Vector3.Distance(point, center) < radius)
                {
                    isInside = true;
                    break;
                }
            }

            if (!isInside)
            {
                result.Add(point);
            }
        }

        return result;
    }

    public static List<Vector3> ValidateCubeSurfacePoints(Vector3[] centers, Vector3[] surfaces, float sideLength)
    {
        List<Vector3> result = new List<Vector3>();

        // Iterar sobre cada punto en 'surfaces'
        foreach (Vector3 point in surfaces)
        {
            bool isInsideAnyCube = false;

            // Verificar si el punto está dentro de algún cubo
            foreach (Vector3 center in centers)
            {
                if (IsPointInsideCube(center, sideLength, point))
                {
                    isInsideAnyCube = true;
                    break;
                }
            }

            // Si el punto no está dentro de ningún cubo, añadirlo a la lista de resultados
            if (!isInsideAnyCube)
            {
                result.Add(point);
            }
        }

        // Si no se encontraron puntos válidos, devolver todos los puntos
        if (result.Count == 0)
        {
            result = new List<Vector3>(surfaces);
        }

        return result;
    }

    private static bool IsPointInsideCube(Vector3 center, float sideLength, Vector3 point)
    {
        float halfSide = sideLength / 2f;

        // Comprobar si el punto está dentro del cubo definido por el centro y el lado
        return point.x >= center.x - halfSide &&
               point.x <= center.x + halfSide &&
               point.y >= center.y - halfSide &&
               point.y <= center.y + halfSide &&
               point.z >= center.z - halfSide &&
               point.z <= center.z + halfSide;
    }

    #endregion

    #region Triangles

    public static List<int> GetTriangles(List<Vector3> vertex, GeometricForm type, int resolution) 
    {
        List<int> triangles = new List<int>();

        switch (type)
        {
            case GeometricForm.Sphere:
                triangles = GenerateSphereTriangles(vertex, resolution);
                break;
            case GeometricForm.Cube:
                triangles = GenerateCubeTriangles();
                break;
            case GeometricForm.Pyramid:
                triangles = GeneratePyramidTriangles(vertex);
                break;
            case GeometricForm.InvertedPyramid:
                triangles = GenerateInvertedPyramidTriangles(vertex);
                break;
        }

        return triangles;
    }
    public static List<int> GenerateSphereTriangles(List<Vector3> vertices, int resolution)
    {
        List<int> triangles = new List<int>();

        int numLongitude = resolution + 1;

        for (int lat = 0; lat < resolution; lat++)
        {
            for (int lon = 0; lon < resolution; lon++)
            {
                int current = lat * numLongitude + lon;
                int next = current + numLongitude;

                // Primer triángulo
                triangles.Add(current);
                triangles.Add(current + 1);
                triangles.Add(next);

                // Segundo triángulo
                triangles.Add(current + 1);
                triangles.Add(next + 1);
                triangles.Add(next);
            }
        }

        return triangles;
    }

    public static List<int> GenerateCubeTriangles()
    {
        return new List<int>
    {
        // Cara trasera
        0, 2, 1,
        1, 2, 3,
        
        // Cara frontal
        4, 5, 6,
        5, 7, 6,
        
        // Cara izquierda
        0, 4, 2,
        2, 4, 6,
        
        // Cara derecha
        1, 3, 5,
        3, 7, 5,
        
        // Cara inferior
        0, 1, 4,
        1, 5, 4,
        
        // Cara superior
        2, 6, 3,
        3, 6, 7
    };
    }

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

    #endregion
}

public enum GeometricForm
{
    Sphere,
    Cube,
    Pyramid,
    InvertedPyramid
}
