using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralTree : MonoBehaviour
{
    public BezierType type;
    public GeometricForm geometricForm;

    [Range(3, 10)]
    public int height;

    private int numPoints;


    [Header("Gizmos")]
    public float radio = 1.0f;
    public int resolution = 10;
    public float tolerance = 0.1f;

    void OnDrawGizmos()
    {
        Vector3[] points = Beziers.CalculateBezier(transform.position, transform.position + Vector3.up * height * radio, height);

        foreach (Vector3 point in points)
        {
            List<Vector3> form = GeometricPoints.GetForm(point, radio, resolution, tolerance, geometricForm);
            foreach (Vector3 v in form)
                Gizmos.DrawSphere(v, radio * 0.05f);
        }
    }

}
