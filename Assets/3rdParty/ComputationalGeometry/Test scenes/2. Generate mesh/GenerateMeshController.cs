using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;

public class GenerateMeshController : MonoBehaviour 
{
    public float width;

    public int cells;

    private void Start()
    {
        HashSet<Triangle2> grid = _GenerateMesh.GenerateGrid(width, cells);

        if (grid != null)
        {

            HashSet<Triangle3<MyVector3>> grid_3d = new HashSet<Triangle3<MyVector3>>();

            foreach (Triangle2 t in grid)
            {
                Triangle3<MyVector3> t_3d = new Triangle3<MyVector3>(t.p1.ToMyVector3_Yis3D(), t.p2.ToMyVector3_Yis3D(), t.p3.ToMyVector3_Yis3D());

                grid_3d.Add(t_3d);
            }

            Mesh meshGrid = _TransformBetweenDataStructures.Triangle3ToMesh(grid_3d);

            MeshFilter filter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
            filter.mesh = meshGrid;

            filter.mesh.RecalculateNormals();
            filter.mesh.RecalculateBounds();

        }
    }
}
