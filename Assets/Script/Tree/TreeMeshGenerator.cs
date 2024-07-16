using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TreeMeshGenerator : MonoBehaviour
{
    public List<Tree> trunkParts;
    private Material material;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    public List<int> triangles = new List<int>();

    public void Init(List<Tree> trunkParts, Material material)
    {
        this.trunkParts = trunkParts;
        this.material = material;
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

        for(int i = 0; i < trunkParts.Count; i++) 
        {
            Tree part = trunkParts[i];

            vertexs.AddRange(part.points);

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
        List<int> init = new List<int>
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

        if(index == trunkParts.Count - 1) 
        {
            init = new List<int>
            {
                0,1,3,
                3,2,0,
            };
        }

        for(int i = 0; i < init.Count; i++) 
        {
            init[i] = init[i] + _index;
        }

        return init;
    }
   
}
