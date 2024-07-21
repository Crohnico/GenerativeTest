
using UnityEngine;

public static class Beziers
{
    public static Vector3[] CalculateBezier(Vector3 a, Vector3 b, int numPoints)
    {
        return CalculateLinearBezier(a, b, numPoints);
    }

    public static Vector3[] CalculateBezier(Vector3 a, Vector3 b, Vector3 c, int numPoints)
    {
        return CalculateQuadraticBezier(a, b, c, numPoints);
    }
    public static Vector3[] CalculateBezier(Vector3 a, Vector3 b, Vector3 c, Vector3 d, int numPoints)
    {
        return CalculateCubicBezier(a, b, c, d, numPoints);
    }

    public static Vector3[] CalculateLinearBezier(Vector3 a, Vector3 b, int numPoints)
    {
        Vector3[] points = new Vector3[numPoints];
        for (int i = 0; i < numPoints; i++)
        {
            float t = i / (float)(numPoints - 1);
            points[i] = Vector3.Lerp(a, b, t);
        }
        return points;
    }

    public static Vector3[] CalculateQuadraticBezier(Vector3 a, Vector3 b, Vector3 c, int numPoints)
    {
        Vector3[] points = new Vector3[numPoints];
        for (int i = 0; i < numPoints; i++)
        {
            float t = i / (float)(numPoints - 1);
            points[i] = (1 - t) * (1 - t) * a + 2 * (1 - t) * t * b + t * t * c;
        }
        return points;
    }

    public static Vector3[] CalculateCubicBezier(Vector3 a, Vector3 b, Vector3 c, Vector3 d, int numPoints)
    {
        Vector3[] points = new Vector3[numPoints];
        for (int i = 0; i < numPoints; i++)
        {
            float t = i / (float)(numPoints - 1);
            float oneMinusT = 1 - t;
            float t2 = t * t;
            float oneMinusT2 = oneMinusT * oneMinusT;
            float t3 = t2 * t;
            float oneMinusT3 = oneMinusT2 * oneMinusT;

            points[i] = oneMinusT3 * a + 3 * oneMinusT2 * t * b + 3 * oneMinusT * t2 * c + t3 * d;
        }
        return points;

    }
}

public enum BezierType 
{
    Linear,
    Quadratic,
    Cubic
}
