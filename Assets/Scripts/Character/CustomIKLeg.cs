using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomIKLeg : MonoBehaviour
{
    public Transform upperLeg, LowerLeg, Foot;
    public Transform target;

    private float radius;
    void Start()
    {
        radius = Vector3.Distance(upperLeg.transform.position, Foot.transform.position) / 2;
    }


    void Update()
    {
        if (Vector3.Distance(upperLeg.transform.position, Foot.transform.position) > radius * 2)
        {
            Vector3 direction = Foot.transform.position - upperLeg.transform.position;
            Foot.transform.position = upperLeg.transform.position + direction.normalized * (radius * 2);
        }
    }


    void OnDrawGizmos()
    {
        if (upperLeg != null && Foot != null)
        {
            Gizmos.color = Color.green;
            Vector3 direction = (Foot.transform.position - upperLeg.transform.position).normalized;
            DrawCircle(upperLeg.position, radius, direction, out Vector3 normal1);
            DrawCircle(Foot.position, radius, direction, out Vector3 normal2);

            Vector3[] intersections = FindCircleIntersections(upperLeg.position, Foot.position, radius, normal1);
            Vector3 kneePos = Vector3.zero;
            if (intersections != null && intersections.Length != 0)
            {
                Gizmos.color = Color.red;
                Vector3 controlPoint = upperLeg.transform.position - upperLeg.transform.forward;


                float dist1 = Vector3.Distance(controlPoint, intersections[0]);
                float dist2 = Vector3.Distance(controlPoint, intersections[1]);

                if (dist1 < dist2)
                {
                    kneePos = intersections[0];
                }
                else
                {
                    kneePos = intersections[1];
                }
            }
            else 
            {
                kneePos = (upperLeg.transform.position + upperLeg.transform.forward) / 2;
            }

            Gizmos.DrawSphere(kneePos, 0.1f);
        }
    }

    void DrawCircle(Vector3 center, float radius, Vector3 direction, out Vector3 normal)
    {
        int segments = 36;
        float angleIncrement = 360f / segments;
        float angle = 0f;

        Vector3 dir = direction.normalized;

        Vector3 ortho1 = Vector3.Cross(dir, Vector3.up);
        if (ortho1 == Vector3.zero) 
        {
            ortho1 = Vector3.Cross(dir, Vector3.right);
        }
        ortho1.Normalize();

        Vector3 ortho2 = Vector3.Cross(dir, ortho1).normalized;
        normal = ortho1;

        Vector3 lastPoint = center + (dir * radius * Mathf.Cos(0f)) + (ortho2 * radius * Mathf.Sin(0f));

        for (int i = 1; i <= segments; i++)
        {
            angle += angleIncrement;
            float rad = Mathf.Deg2Rad * angle;
            Vector3 nextPoint = center + (dir * radius * Mathf.Cos(rad)) + (ortho2 * radius * Mathf.Sin(rad));
            Gizmos.DrawLine(lastPoint, nextPoint);
            lastPoint = nextPoint;
        }
    }



    Vector3[] FindCircleIntersections(Vector3 centerA, Vector3 centerB, float radius, Vector3 normal)
    {
        Vector3 planePoint = centerA; 
        Vector3 u = Vector3.Cross(normal, Vector3.up).normalized; 
        Vector3 v = Vector3.Cross(normal, u).normalized; 

        Vector2 projectedCenterA = ProjectPointOnPlane(centerA, planePoint, u, v);
        Vector2 projectedCenterB = ProjectPointOnPlane(centerB, planePoint, u, v);


        Vector2[] intersections2D = FindIntersections2D(projectedCenterA, projectedCenterB, radius);

        if (intersections2D == null)
            return null;
        Vector3[] intersections3D = new Vector3[intersections2D.Length];
        for (int i = 0; i < intersections2D.Length; i++)
        {
            intersections3D[i] = UnprojectPointOnPlane(intersections2D[i], planePoint, u, v);
        }

        return intersections3D;
    }

    Vector2 ProjectPointOnPlane(Vector3 point, Vector3 planePoint, Vector3 u, Vector3 v)
    {
        Vector3 offset = point - planePoint;
        float x = Vector3.Dot(offset, u);
        float y = Vector3.Dot(offset, v);
        return new Vector2(x, y);
    }

    Vector3 UnprojectPointOnPlane(Vector2 point, Vector3 planePoint, Vector3 u, Vector3 v)
    {
        return planePoint + point.x * u + point.y * v;
    }

    Vector2[] FindIntersections2D(Vector2 cA, Vector2 cB, float radius)
    {
        float d = Vector2.Distance(cA, cB);

        if (d > 2 * radius)
        {
            return null; 
        }
        if (d < 0.0001f)
        {
            return null; 
        }

        float a = (radius * radius - radius * radius + d * d) / (2 * d);
        float h = Mathf.Sqrt(radius * radius - a * a);

        Vector2 p = cA + a * (cB - cA).normalized;

        Vector2 intersection1 = p + h * new Vector2(cB.y - cA.y, cA.x - cB.x).normalized;
        Vector2 intersection2 = p - h * new Vector2(cB.y - cA.y, cA.x - cB.x).normalized;

        return new Vector2[] { intersection1, intersection2 };
    }
}
