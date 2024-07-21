using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GeometricPoints
{
    public static List<Vector3> GetForm(Vector3[] centers, float radius, int resolution, float tolerance, GeometricForm form)
    {
        List<Vector3> geometricForm = new List<Vector3>();
        List<Vector3> Surfaces = new List<Vector3>();

        switch (form)
        {
            case GeometricForm.Sphere:
                 Surfaces = new List<Vector3>();
                for(int i = 0; i < centers.Length; i++) 
                {
                    Surfaces.AddRange(DrawSphere(centers[i], radius, resolution));
                }

                geometricForm = ValidateSphereSurfacePoints(centers, Surfaces.ToArray(), radius);

                return geometricForm;

            case GeometricForm.Cube:

                Surfaces = new List<Vector3>();
                for (int i = 0; i < centers.Length; i++)
                {
                    Surfaces.AddRange(DrawCube(centers[i], radius, resolution, tolerance));
                }
                geometricForm = ValidateCubeSurfacePoints(centers, Surfaces.ToArray(), radius);
                return geometricForm;

            case GeometricForm.Pyramid:

                Surfaces = new List<Vector3>();
                for (int i = 0; i < centers.Length; i++)
                {
                    Surfaces.AddRange(DrawPyramid(centers[i], radius,radius, resolution));
                }
                //geometricForm = ValidateCubeSurfacePoints(centers, Surfaces.ToArray(), radius);
                return Surfaces;

            case GeometricForm.InvertedPyramid:

                Surfaces = new List<Vector3>();
                for (int i = 0; i < centers.Length; i++)
                {
                    Surfaces.AddRange(DrawInvertedPyramid(centers[i], radius, radius, resolution));
                }
                //geometricForm = ValidateCubeSurfacePoints(centers, Surfaces.ToArray(), radius);
                return Surfaces;

        }

        return geometricForm;
    }

    #region GeometricForms
    public static List<Vector3> DrawSphere(Vector3 center, float radius, int resolution)
    {
        List<Vector3> vertList = new List<Vector3>();


        for (int i = 0; i < resolution; i++)
        {
            float theta = 2 * Mathf.PI * i / resolution; // Azimuthal angle
            for (int j = 0; j < resolution; j++)
            {
                float phi = Mathf.PI * j / resolution; // Polar angle

                float x = radius * Mathf.Sin(phi) * Mathf.Cos(theta);
                float y = radius * Mathf.Sin(phi) * Mathf.Sin(theta);
                float z = radius * Mathf.Cos(phi);

                vertList.Add(center + new Vector3(x, y, z));
            }
        }

        return vertList;
    }

    public static List<Vector3> DrawCube(Vector3 point, float radius, int resolution, float tolerance)
    {
        List<Vector3> geometricForm = new List<Vector3>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if (x != 0 && z != 0 && y != 0)
                        geometricForm.Add(point + new Vector3(x, y, z) * (radius / 2));
                }
            }
        }

        return geometricForm;
    }

    public static List<Vector3> DrawPyramid(Vector3 point, float baseRadius, float height, int resolution)
    {
        List<Vector3> geometricForm = new List<Vector3>();

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

        Vector3 topVertex = point + new Vector3(0, height, 0);
        geometricForm.Add(topVertex);

        foreach (Vector3 basePoint in basePoints)
        {
            for (int i = 1; i <= (int)(resolution/2); i++)
            {
                float t = i / (float)((int)(resolution / 2) + 1);
                Vector3 sidePoint = Vector3.Lerp(basePoint, topVertex, t);
                geometricForm.Add(sidePoint);
            }
        }

        return geometricForm;
    }

    public static List<Vector3> DrawInvertedPyramid(Vector3 point, float baseRadius, float height, int resolution)
    {
        List<Vector3> geometricForm = new List<Vector3>();


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


        Vector3 bottomVertex = point + new Vector3(0, -height, 0); 
        geometricForm.Add(bottomVertex);


        foreach (Vector3 basePoint in basePoints)
        {
            for (int i = 1; i <= (int)(resolution / 2); i++)
            {
                float t = i / (float)((int)(resolution / 2) + 1);
                Vector3 sidePoint = Vector3.Lerp(basePoint, bottomVertex, t);
                geometricForm.Add(sidePoint);
            }
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
    public static List<int> GenerateSphereTriangles(List<Vector3> vertices, int resolution)
    {
        List<int> triangles = new List<int>();

        int numLatitude = resolution;
        int numLongitude = resolution;

        for (int lat = 0; lat < numLatitude - 1; lat++)
        {
            for (int lon = 0; lon < numLongitude - 1; lon++)
            {
                int current = lat * numLongitude + lon;
                int next = current + numLongitude;

                triangles.Add(current);
                triangles.Add(next);
                triangles.Add(current + 1);

                triangles.Add(next);
                triangles.Add(next + 1);
                triangles.Add(current + 1);
            }
        }

        return triangles;
    }

    public static List<int> GenerateCubeTriangles()
    {
        return new List<int>
    {
        0, 2, 1,
        1, 2, 3,
        
        4, 5, 6,
        5, 7, 6,
        
        8, 10, 9,
        9, 10, 11,
        
        12, 13, 14,
        13, 15, 14,
        
        16, 18, 17,
        17, 18, 19,
        
        20, 21, 22,
        21, 23, 22
    };
    }

    public static List<int> GeneratePyramidTriangles(List<Vector3> vertices)
    {
        List<int> triangles = new List<int>();
        int numBasePoints = vertices.Count - 1;
        int apexIndex = numBasePoints;

        for (int i = 0; i < numBasePoints - 2; i++)
        {
            triangles.Add(0);
            triangles.Add(i + 1);
            triangles.Add(i + 2);
        }

        for (int i = 0; i < numBasePoints; i++)
        {
            int nextIndex = (i + 1) % numBasePoints;
            triangles.Add(i);
            triangles.Add(nextIndex);
            triangles.Add(apexIndex);
        }

        return triangles;
    }

    public static List<int> GenerateInvertedPyramidTriangles(List<Vector3> vertices)
    {
        List<int> triangles = new List<int>();
        int numBasePoints = vertices.Count - 1;
        int apexIndex = numBasePoints;

        for (int i = 0; i < numBasePoints - 2; i++)
        {
            triangles.Add(0); 
            triangles.Add(i + 1);
            triangles.Add(i + 2);
        }

        for (int i = 0; i < numBasePoints; i++)
        {
            int nextIndex = (i + 1) % numBasePoints;
            triangles.Add(i);
            triangles.Add(nextIndex);
            triangles.Add(apexIndex);
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
