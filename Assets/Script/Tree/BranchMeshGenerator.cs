using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BranchMeshGenerator : MonoBehaviour
{
    public List<BranchGenerator.BranchPart> branchPart;
    private Material material;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    public List<int> triangles = new List<int>();

    public List<int> negatives = new List<int>
            {
                0,5,1,
                5,0,4,

                1,7,3,
                7,1,5,

                3,6,2,
                6,3,7,

                2,4,0,
                4,2,6
            };

    public List<int> positives = new List<int>
            {
                0,1,5,
                5,4,0,

                1,3,7,
                7,5,1,

                3,2,6,
                6,7,3,

                2,0,4,
                4,6,2
            };

    public List<int> closePositives = new List<int>
            {
                0,3,1,
                3,0,2,
            };

    public List<int> closeNegative = new List<int>
            {
                0,1,3,
                3,2,0,
            };

public List<int> pattern = new List<int>(0);
    public List<int> closePattern = new List<int>(0);

    public void Init(BranchGenerator.Branch branch, Material material)
    {
        this.branchPart = branch.branch;
        this.material = material;

        if (branch.growDir.x > 0 || branch.growDir.z > 0 || branch.growDir.y > 0)
        {
            pattern = negatives;

            if (branch.growDir.x > 0) { closePattern = new List<int> { 0, 3, 1, 3, 0, 2 }; }
            else closePattern = new List<int> { 3, 0, 1, 3, 0, 2 };
            }
        else
        {
            pattern = positives;
            closePattern = closeNegative;
        }

        GenerateMesh();
    }

    void GenerateMesh()
    {
        meshFilter = gameObject.GetComponent<MeshFilter>();
        if (!meshFilter) meshFilter = gameObject.AddComponent<MeshFilter>();

        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (!meshRenderer) meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material;

        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        List<Vector3> vertexs = new List<Vector3>();
        triangles = new List<int>();

        for (int i = 0; i < branchPart.Count; i++)
        {
            BranchGenerator.BranchPart part = branchPart[i];
    
            vertexs.AddRange(part.corners);

            List<int> custom = GetList(i);
            triangles.AddRange(custom);

        }

        Vector2[] uvs = new Vector2[vertexs.Count];

        for (int i = 0; i < vertexs.Count; i++)
        {
            uvs[i] = new Vector2(vertexs[i].x, vertexs[i].z);
        }

        mesh.vertices = vertexs.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }


    public List<int> GetList(int index)
    {
        int _index = index * 4;

        List<int> init = new List<int>(pattern);

        if (index == branchPart.Count - 1)
        {
            init = new List<int>
            {
                0,3,1,
                3,0,2,
            };
        }

        for (int i = 0; i < init.Count; i++)
        {
            init[i] = init[i] + _index;
        }

        return init;
    }

}
