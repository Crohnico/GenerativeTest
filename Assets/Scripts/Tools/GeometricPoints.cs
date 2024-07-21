using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GeometricPoints
{
    public static List<Vector3> GetForm(Vector3 point, float radius, int resolution, float tolerance, GeometricForm form)
    {
        List<Vector3> geometricForm = new List<Vector3>();

        switch (form)
        {
            case GeometricForm.Sphere:
                return DrawSphere(point, radius, resolution, tolerance);

            case GeometricForm.Cube:
                return DrawCube(point, radius, resolution, tolerance);

        }

        return geometricForm;
    }

    public static List<Vector3> DrawSphere(Vector3 point, float radius, int resolution, float tolerance)
    {
        List<Vector3> geometricForm = new List<Vector3>();


        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                for (int k = 0; k < resolution; k++)
                {
                    float x = (i / (float)(resolution - 1)) * 2 - 1;
                    float y = (j / (float)(resolution - 1)) * 2 - 1;
                    float z = (k / (float)(resolution - 1)) * 2 - 1;

                    float distance = x * x + y * y + z * z;

                    if (Mathf.Abs(distance - 1) <= tolerance)
                    {
                        geometricForm.Add(point + new Vector3(x, y, z) * radius);
                    }
                }
            }
        }
            return geometricForm;
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
}

public enum GeometricForm
{
    Sphere,
    Cube
}
