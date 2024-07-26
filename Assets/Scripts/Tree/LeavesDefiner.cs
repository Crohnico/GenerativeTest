using Habrador_Computational_Geometry;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LeavesDefiner : MonoBehaviour
{
    public LeavesType type;

    private TrunkDefiner trunkDefiner;

    public float minHeight = 1, maxheigt = 2;

    private Vector3[] positions;

    private List<GameObject> helpers = new List<GameObject>();
    private int polygonFaces;

    List<Vector3> trunkPositions = new List<Vector3>();
    private Queue<Vector3> leafQueue = new Queue<Vector3>();

    Dictionary<Vector3, LeavesTool> trunkMap = new Dictionary<Vector3, LeavesTool>();
    public GameObject test;

    public List<List<Vector3>> leaves = new List<List<Vector3>>();

    public class LeavesTool
    {
        public bool hastrunk;
        public bool hasleaves;
    }

    System.Random rng;

    public void Init(TrunkDefiner trunkDefiner, System.Random rng, Vector3[] positions, List<List<Vector3>> trunk, List<List<Vector3>> branches)
    {
        this.trunkDefiner = trunkDefiner;
        this.positions = positions;

        trunkPositions.Clear();
        trunkMap.Clear();
        leafQueue.Clear();
        leaves.Clear();

        for (float x = -10f; x <= 10f; x += .5f)
        {
            for (float y = 0f; y <= 20f; y += .5f)
            {
                for (float z = -10f; z <= 10f; z += .5f)
                {
                    LeavesTool tool = new LeavesTool();
                    trunkMap.Add(new Vector3(x, y, z), tool);
                }
            }
        }


        foreach (var segment in trunk)
        {
            foreach (var centers in segment)
            {
                trunkPositions.Add(centers);
                float x = Mathf.Round(centers.x * 2) / 2;
                float y = Mathf.Round(centers.y * 2) / 2;
                float z = Mathf.Round(centers.z * 2) / 2;

                Vector3 mapPos = new Vector3(x, y, z);

                if (trunkMap.ContainsKey(mapPos))
                    trunkMap[mapPos].hastrunk = true;
                else
                {
                    LeavesTool tool = new LeavesTool();
                    tool.hastrunk = true;
                    trunkMap.Add(new Vector3(x, y, z), tool);
                }
            }
        }


        foreach (var segment in branches)
        {
            foreach (var centers in segment)
            {
                trunkPositions.Add(centers);
                float x = Mathf.Round(centers.x * 2) / 2;
                float y = Mathf.Round(centers.y * 2) / 2;
                float z = Mathf.Round(centers.z * 2) / 2;

                Vector3 mapPos = new Vector3(x, y, z);

                if (trunkMap.ContainsKey(mapPos))
                    trunkMap[mapPos].hastrunk = true;
                else
                {
                    LeavesTool tool = new LeavesTool();
                    tool.hastrunk = true;
                    trunkMap.Add(new Vector3(x, y, z), tool);
                }
            }
        }
        this.rng = rng;

        if (helpers.Count != 0)
        {
            for (int i = helpers.Count - 1; i >= 0; i--) { Destroy(helpers[i]); }
        }
        helpers.Clear();




        GenerateLeaves(rng);
    }

    private void GenerateLeaves(System.Random rng)
    {
        if (positions.Length == 0) return;
        type = (LeavesType)rng.Next(0, System.Enum.GetValues(typeof(LeavesType)).Length);
        polygonFaces = rng.Next(3, 11);

    }

    public List<MeshFilter> TryGenerate(System.Random rng)
    {
        if (positions.Length == 0) return null;

        List<MeshFilter> meshes = new List<MeshFilter>();


        for (int i = 0; i < positions.Length; i++)
        {

            float x = Mathf.Round(positions[i].x * 2) / 2;
            float y = Mathf.Round(positions[i].y * 2) / 2;
            float z = Mathf.Round(positions[i].z * 2) / 2;

            Vector3 mapPos = new Vector3(x, y, z);

            SpreadLeaves(mapPos);
        }

        int faces = rng.Next(3, 11);
        float witdh = RandomTool.GenerateRandomFloatInRange(rng, 1f, 2f);

        foreach (var segment in leaves)
        {
            List<Vector3> vertex = new List<Vector3>();

            foreach (var centers in segment)
            {
                vertex.AddRange(GetCubeVertices(centers, faces, witdh));
            }
            if (vertex.Count != 0)
                meshes.Add(ConvexHull(vertex, null));
        }

        return meshes;
    }


    public Mesh CreateMesh(List<Vector3> vertices, List<int> triangles)
    {
        Mesh mesh = new Mesh();

        Vector2[] uvs = new Vector2[vertices.Count];
        Vector3 center = Vector3.zero;
        float minY = float.MaxValue, maxY = float.MinValue;

        foreach (Vector3 vertex in vertices)
        {
            if (vertex.y < minY) minY = vertex.y;
            if (vertex.y > maxY) maxY = vertex.y;
        }

        GeometricPoints.GenerateUVs(vertices, triangles, out uvs);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs;

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();

        return mesh;
    }


    public MeshFilter ConvexHull(List<Vector3> trunk, System.Action callback)
    {
        Mesh mesh = new Mesh();

        HashSet<Vector3> points_Unity = new HashSet<Vector3>(trunk);
        HashSet<MyVector3> points = new HashSet<MyVector3>(points_Unity.Select(x => x.ToMyVector3()));

        //Normalize
        Normalizer3 normalizer = new Normalizer3(new List<MyVector3>(points));
        HashSet<MyVector3> points_normalized = normalizer.Normalize(points);


        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
        timer.Start();
        HalfEdgeData3 convexHull_normalized = _ConvexHull.Iterative_3D(points_normalized, true, normalizer);
        timer.Stop();

        if (convexHull_normalized != null)
        {
            HalfEdgeData3 convexHull = normalizer.UnNormalize(convexHull_normalized);

            MyMesh myMesh = convexHull.ConvertToMyMesh("convex hull", MyMesh.MeshStyle.HardAndSoftEdges);
            Mesh convexHullMesh = myMesh.ConvertToUnityMesh(generateNormals: false, myMesh.meshName);


            Vector2[] uvs = new Vector2[convexHullMesh.vertices.Length];

            for (int i = 0; i < convexHullMesh.vertices.Length; i++)
            {
                uvs[i] = new Vector2(convexHullMesh.vertices[i].x, convexHullMesh.vertices[i].z);
            }


            mesh = convexHullMesh;
            mesh.uv = uvs;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        GameObject go = new GameObject();
        //go.transform.parent = transform;
        go.transform.position = transform.position;

        MeshFilter filter = go.AddComponent<MeshFilter>();
        filter.mesh = mesh;


        go.transform.parent = transform;
        helpers.Add(go);
        return filter;

        callback?.Invoke();
    }

    private void SpreadLeaves(Vector3 initialPosition)
    {
        float spread = RandomTool.GenerateRandomFloatInRange(rng, 2f, 10f);
        List<Vector3> leaveCloud = new List<Vector3>();
        List<Vector3> spreadPosibilities = new List<Vector3>();

        leaveCloud.Add(initialPosition);
        trunkMap[initialPosition].hasleaves = true;

        while (spread > 0)
        {
            spreadPosibilities = GetNextSpreadPosibilities(leaveCloud);

            if (spreadPosibilities.Count == 0) break;

            int nextToAdd = rng.Next(0, spreadPosibilities.Count);
            leaveCloud.Add(spreadPosibilities[nextToAdd]);
            trunkMap[spreadPosibilities[nextToAdd]].hasleaves = true;

            spread -= RandomTool.GenerateRandomFloatInRange(rng, .1f, .5f);
        }

        leaves.Add(leaveCloud);
    }

    List<Vector3> GetNextSpreadPosibilities(List<Vector3> mapPosition)
    {
        List<Vector3> next = new List<Vector3>();

        foreach (var currentLeaves in mapPosition)
        {
            for (float x = -.5f; x <= .5f; x += .5f)
            {
                for (float y = -.5f; y <= .5f; y += .5f)
                {
                    for (float z = -.5f; z <= .5f; z += .5f)
                    {
                        Vector3 newCordinates = new Vector3(currentLeaves.x + x, currentLeaves.y + y, currentLeaves.z + z);
                        if (trunkMap.ContainsKey(newCordinates) && !trunkMap[newCordinates].hastrunk && !trunkMap[newCordinates].hasleaves)
                        {
                            next.Add(newCordinates);
                        }
                    }
                }
            }
        }

        return next;
    }


    public static List<Vector3> GetCubeVertices(Vector3 center, float faces, float witdh)
    {
        int definition = 6;
        List<Vector3> vertices = new List<Vector3>();

        for (int j = 0; j < definition; j++)
        {

            float t = (Mathf.Sin((j / (float)definition * Mathf.PI)) + 1.0f) / 2.0f;
            float interpolatedValue = Mathf.Lerp(witdh * 0.25f, witdh, t);

            for (int i = 0; i < faces; i++)
            {
                float theta = 2 * Mathf.PI * i / faces;
                float x = interpolatedValue * Mathf.Cos(theta);
                float z = interpolatedValue * Mathf.Sin(theta);


                vertices.Add(center + new Vector3(x, witdh * ((float)j / (float)definition), z));
            }

        }

        return vertices;
    }
}

public enum LeavesType
{
    None,
    ConvexHull,
    Geometric
}
