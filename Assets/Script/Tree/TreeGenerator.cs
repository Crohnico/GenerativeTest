using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Random = UnityEngine.Random;

[System.Serializable]
public class Tree
{
    public Vector3 ID;
    public Vector3 position;
    public bool isTop = false;
    public bool hasTrunk = false;
    public bool hasLeaf = false;

    public List<Vector3> points = new List<Vector3>();
    public List<Triangle> triangles = new List<Triangle>();

    public void UpdatePostion(Vector3 pos)
    {
        position = pos;
    }

}
[System.Serializable]
public class Triangle
{
    public Vector3 corner0;
    public Vector3 corner1;
    public Vector3 corner2;

    public Triangle(Vector3 a, Vector3 b, Vector3 c)
    {
        corner0 = a;
        corner1 = b;
        corner2 = c;
    }
}

public class TreeGenerator : MonoBehaviour
{
    private TreeMeshGenerator meshGenerator;
    public Material trunkMaterial;
    public Material leavesMaterial;

    private List<GameObject> branchesList = new List<GameObject>();
    private List<List<Vector3>> leavesGroup = new List<List<Vector3>>();

    [Header("Trunk")]

    [Range(0, 1)]
    public float height = .4f;
    private int Height => Mathf.RoundToInt(Mathf.Lerp(4f, 10f, height));


    [Range(0, 1)]
    public float width = .5f;
    private float Width => Mathf.Lerp(0.15f, 0.7f, width);

    [Range(0, 1)]
    public float bend = .5f;
    private float Bend => Mathf.Lerp(0.15f, 0.8f, bend);

    [Range(0, 1)]
    public float twist = .2f;

    private float Twist => Mathf.Lerp(0, 60f, twist);


    [Header("Branches")]

    [Range(0f, 1f)]
    public float branchWidth = 0.1f;

    private float BranchWidth => Mathf.Lerp(0.1f, 0.25f, branchWidth);

    [Range(0f, 1f)]
    public float branchDensity = 0.5f;

    [Range(0f, 1f)]
    public float branchLenght = 0.5f;
    private float BranchLenght => Mathf.Lerp(0.4f, 0.75f, branchLenght);

    [Range(0f, 1f)]
    public float branchDeformity = 0.5f;
    private float BranchDeformity => Mathf.Lerp(0f, 0.05f, branchDeformity);




    [Header("Leaves")]
    [Range(0f, 1f)]
    public float leavesDensity = 0.5f;

    private float LeavesDensity => Mathf.Lerp(0f, 1.4f, leavesDensity);


    private int sizeX = 30;
    private int sizeY => Height;
    private int sizeZ = 30;


    private float separatorX => Bend;
    private float _separatorY = .9f;
    private float separatorZ => Bend;

    public float trunkXSize => Width;
    private float trunkYSize = .5f;
    public float trunkZSize => Width;

    private List<Tree> trunkParts = new List<Tree>();
    private Dictionary<Vector3, Tree> TreeMap = new Dictionary<Vector3, Tree>();
    private Dictionary<Vector3, Tree> Leaves = new Dictionary<Vector3, Tree>();

    private float branchXSize => BranchWidth;
    private float branchYSize => BranchWidth;
    private float branchZSize => BranchWidth;

    private int minYBranche => Mathf.RoundToInt(Height / 2);
    private float chanceToBranche => branchDensity;
    private float chanceToGrowBranche => Mathf.Clamp(branchDensity + .25f, 0f, 1f);

    private int branchminLenght => Mathf.RoundToInt(5f * (BranchLenght * 2f));
    private int branchmaxLenght => Mathf.RoundToInt(10f * BranchLenght * 2f);

    private float separatorBrancheX => .25f + Random.Range(0f, BranchDeformity);
    private float separatorBrancheY => .25f + Random.Range(0f, BranchDeformity);
    private float separatorBrancheZ => .25f + Random.Range(0f, BranchDeformity);


    private List<Vector3> startsPoints = new List<Vector3>();

    private float leavesSpread = .5f;

 
    private float leavesX = .5f;

    private float leavesY = .5f;
 
    private float leavesZ = .5f;


    private List<GameObject> leaves = new List<GameObject>();
    private Queue<Vector3> leafQueue = new Queue<Vector3>();
    private float rotationAngle;

    private GameObject leaveParent;

    public void Init()
    {
        leafQueue.Clear();
        CreateMap();

        trunkParts.Clear();
        startsPoints.Clear();

        if (leaveParent)
            Destroy(leaveParent);
        leaveParent = null;

        for (int i = leaves.Count - 1; i >= 0; i--)
        {
            Destroy(leaves[i]);
        }

        for (int i = branchesList.Count - 1; i >= 0; i--)
            Destroy(branchesList[i]);

        branchesList.Clear();

        leavesGroup.Clear();
        leaves.Clear();
        Leaves.Clear();

        GenerateTree();
        SaveTreeVertex();

        if (gameObject.TryGetComponent(out meshGenerator))
        {
            meshGenerator.Init(trunkParts, trunkMaterial);
            GenerateBranches();
            GenerateLeaves();
        }
    }

    void GenerateTree()
    {
        int lastX = sizeX / 2;
        int lastZ = sizeZ / 2;

        for (int y = 0; y < sizeY; y++)
        {
            lastX = Mathf.Clamp(lastX, 0, sizeX - 1);
            lastZ = Mathf.Clamp(lastZ, 0, sizeZ - 1);

            Tree part = new Tree
            {
                ID = new Vector3(lastX, y, lastZ),
                hasTrunk = true
            };

            if (y == sizeY - 1)
                part.isTop = true;

            TreeMap[new Vector3(lastX, y, lastZ)] = part;
            part.ID = new Vector3(lastX, y, lastZ);

            part.position = new Vector3(((lastX - (sizeX / 2)) * separatorX), (float)y * _separatorY, (((float)lastZ - (sizeZ / 2)) * separatorZ));

            trunkParts.Add(part);

            int newX = Random.Range(-1, 2);
            int newZ = Random.Range(-1, 2);

            if (newX != 0) lastX += newX;
            else lastZ += newZ;
        }
    }

    void GenerateBranches()
    {
        for (int i = 0; i < trunkParts.Count; i++)
        {
            Tree trunk = trunkParts[i];
            trunk.UpdatePostion(GetMidpoint(trunk.points));

            if (trunk.ID.y < minYBranche)
                continue;

            float chance = Random.Range(0f, 1f);
            float probability = (trunk.ID.y == Height - 1) ? chanceToBranche*2 : chanceToBranche;


            if (probability >= chance)
            {
                Vector3 branchSize = new Vector3(branchXSize, branchYSize, branchZSize);

                BranchGenerator branch = new BranchGenerator(trunk.position, branchSize, separatorBrancheX, separatorBrancheY, separatorBrancheZ, chanceToGrowBranche, branchminLenght, branchmaxLenght);

                startsPoints.AddRange(branch.endPoints);

                foreach (BranchGenerator.Branch b in branch.branches)
                {
                    GameObject go = new GameObject();
                    go.transform.parent = transform;
                    go.transform.localPosition = Vector3.zero;
                    go.name = "Branch";
                    branchesList.Add(go);

                    BranchMeshGenerator meshGenerator = go.AddComponent<BranchMeshGenerator>();
                    meshGenerator.Init(b, trunkMaterial);
                }
            }
        }
    }

    void GenerateLeaves()
    {
        for (int i = 0; i < startsPoints.Count; i++)
        {
            Vector3 position = startsPoints[i];//transform.InverseTransformPoint(startsPoints[i]);
            Tree t = GetNearestTreeCell(position);
            if (t != null)
            {
                if (!Leaves.ContainsKey(t.position))
                {
                    Leaves.Add(t.position, t);
                    startsPoints[i] = t.position;
                }
            }
        }

        foreach (Vector3 s in startsPoints)
        {
            SpreadLeaves(Vector3.zero);
        }

        int petitions = startsPoints.Count;

        for (int i = 0; i < startsPoints.Count; i++)
        {
            float chance = Random.Range(0f, 1f);

            if (leavesGroup[i].Count > 60 && LeavesDensity >= chance)
            {
                GameObject go = new GameObject();

                LeaveMesh leave = go.AddComponent<LeaveMesh>();
                leave.Init(leavesGroup[i], leavesMaterial, OnEndMesh);

                leaves.Add(leave.gameObject);

                go.transform.parent = transform;
                go.transform.localPosition = startsPoints[i] - GetMidpoint(leavesGroup[i]);
            }
            else 
            {
                OnEndMesh();
            }

        }

        void OnEndMesh()
        {
            petitions -= 1;

            if (petitions <= 0)
            {
                StartCoroutine(SumMeshesRoutine());
            }
        }

    }

    IEnumerator SumMeshesRoutine()
    {
        yield return null;

        SumMaeshes(gameObject.GetComponent<MeshFilter>(), branchesList, true, trunkMaterial);

        leaveParent = new GameObject();
        leaveParent.name = "Leaves";

        MeshFilter l = leaveParent.AddComponent<MeshFilter>();
        leaveParent.transform.parent = transform;
        leaveParent.transform.localPosition = Vector3.zero;

        SumMaeshes(l, leaves, false, leavesMaterial);
    }

    void SaveTreeVertex()
    {
        rotationAngle = 0;
        for (int i = 0; i < trunkParts.Count; i++)
        {
            Tree part = trunkParts[i];

            AddPoints(part, part.isTop);
        }
    }

    void AddPoints(Tree part, bool fullBox = false)
    {
        int[] signs = { -1, 1 };
        List<Vector3> points = new List<Vector3>();
        List<Vector3> pointsTop = new List<Vector3>();

        rotationAngle += Random.Range(-Twist, Twist);


        foreach (int xSign in signs)
        {
            foreach (int ySign in signs)
            {
                if (!fullBox && ySign >= 0) continue;

                if (ySign >= 0)
                {
                    foreach (int zSign in signs)
                    {
                        Vector3 corner = part.position + new Vector3(trunkXSize * xSign, ((float)trunkYSize * (float)ySign), trunkZSize * zSign);

                        AddPoint(points, corner);
                    }
                }
                else
                {
                    foreach (int zSign in signs)
                    {
                        Vector3 corner = part.position + new Vector3(trunkXSize * xSign, ((float)trunkYSize * (float)ySign), trunkZSize * zSign);

                        AddPoint(pointsTop, corner);
                    }
                }
            }
        }

        part.points.AddRange(points);
        part.points.AddRange(pointsTop);

        void AddPoint(List<Vector3> list, Vector3 vector)
        {
            Quaternion rotation = Quaternion.Euler(0, rotationAngle, 0);
            Vector3 rotatedCorner = rotation * vector;
            list.Add(rotatedCorner);
        }
    }

    void CreateMap()
    {
        TreeMap.Clear();

        for (int x = -sizeX / 2; x < sizeX + sizeX / 2; x++)
        {
            for (int y = 0; y < sizeY + sizeY / 2; y++)
            {
                for (int z = -sizeZ / 2; z < sizeZ + sizeZ / 2; z++)
                {
                    Tree tree = new Tree
                    {
                        ID = new Vector3(x, y, z),
                        position = new Vector3((x - sizeX / 2) * separatorX, y * _separatorY, (z - sizeZ / 2) * separatorZ)
                    };

                    TreeMap.Add(tree.ID, tree);
                }
            }
        }
    }

    private void SpreadLeaves(Vector3 initialPosition)
    {
        leafQueue.Enqueue(initialPosition);

        float spread = leavesSpread;
        List<Vector3> leaveCloud = new List<Vector3>();

        while (leafQueue.Count > 0 && spread > 0)
        {
            Vector3 currentPos = leafQueue.Dequeue();
            leaveCloud.AddRange(GetCubeCorners(currentPos, leavesX / 2, leavesY / 2, leavesZ / 2));

            PropagateToNeighbor(currentPos + Vector3.right * leavesX, leafQueue);
            PropagateToNeighbor(currentPos - Vector3.right * leavesX, leafQueue);
            PropagateToNeighbor(currentPos + Vector3.forward * leavesZ, leafQueue);
            PropagateToNeighbor(currentPos - Vector3.forward * leavesZ, leafQueue);
            PropagateToNeighbor(currentPos + Vector3.up * leavesY, leafQueue);

            spread -= .1f;
        }

        leavesGroup.Add(leaveCloud);

        List<Vector3> GetCubeCorners(Vector3 center, float xOffset, float yOffset, float zOffset)
        {
            List<Vector3> corners = new List<Vector3>();

            Vector3 corner = new Vector3();

            int[] signs = { -1, 0, 1 };
            foreach (int xSign in signs)
            {
                foreach (int ySign in signs)
                {
                    foreach (int zSign in signs)
                    {
                        corner = center + new Vector3(xSign * xOffset, ySign * yOffset, zSign * zOffset);
                        corners.Add(corner);
                    }
                }
            }

            corner = center + new Vector3(2 * xOffset, 0 * yOffset, 0 * zOffset);
            corners.Add(corner);

            corner = center + new Vector3(-2 * xOffset, 0 * yOffset, 0 * zOffset);
            corners.Add(corner);

            corner = center + new Vector3(0 * xOffset, 2 * yOffset, 0 * zOffset);
            corners.Add(corner);

            corner = center + new Vector3(0 * xOffset, -2 * yOffset, 0 * zOffset);
            corners.Add(corner);

            corner = center + new Vector3(0 * xOffset, 0 * yOffset, 2 * zOffset);
            corners.Add(corner);

            corner = center + new Vector3(0 * xOffset, 0 * yOffset, -2 * zOffset);
            corners.Add(corner);

            return corners;
        }
    }

    public Vector3 GetMidpoint(List<Vector3> points)
    {
        if (points == null || points.Count == 0)
        {
            return Vector3.zero;
        }

        Vector3 sum = Vector3.zero;
        foreach (Vector3 point in points)
        {
            sum += point;
        }

        Vector3 midpoint = sum / points.Count;

        return midpoint;
    }


    private void PropagateToNeighbor(Vector3 neighborPos, Queue<Vector3> leafQueue)
    {
        Tree nearest = GetNearestTreeCell(neighborPos);

        if (nearest != null && TreeMap.ContainsValue(nearest) && !TreeMap[nearest.ID].hasTrunk)
        {
            float dice = Random.Range(0f, 1f);
            if (dice < leavesSpread && !Leaves.ContainsKey(neighborPos))
            {
                Leaves.Add(neighborPos, nearest);
                leafQueue.Enqueue(neighborPos);
            }
        }
    }

    public Tree GetNearestTreeCell(Vector3 posicionLocal)
    {
        Tree nearestCell = null;
        float minDistance = float.MaxValue;

        foreach (var kvp in TreeMap)
        {
            if (nearestCell == null) nearestCell = kvp.Value;

            Tree tree = kvp.Value;
            float distance = Vector3.Distance(posicionLocal, tree.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestCell = tree;
            }
        }

        return nearestCell;
    }
    private void SumMaeshes(MeshFilter meshParent, List<GameObject> childrends, bool addParent, Material material, bool conserveLocals = true)
    {
        List<MeshFilter> meshes = new List<MeshFilter>();

        if (addParent) meshes.Add(meshParent);

        foreach (var child in childrends)
            meshes.Add(child.GetComponent<MeshFilter>());

        CombineInstance[] combine = new CombineInstance[meshes.Count];

        int i = 0;
        while (i < meshes.Count)
        {
            combine[i].mesh = meshes[i].sharedMesh;
            if (conserveLocals)
                combine[i].transform = transform.worldToLocalMatrix * meshes[i].transform.localToWorldMatrix;
            else
                combine[i].transform = transform.worldToLocalMatrix;
            i++;
        }

        meshParent.mesh.CombineMeshes(combine);

        if (!meshParent.gameObject.GetComponent<MeshRenderer>())
        {
            MeshRenderer rendered = meshParent.gameObject.AddComponent<MeshRenderer>();
            rendered.material = material;
        }

        for (int e = childrends.Count - 1; e >= 0; e--)
            Destroy(childrends[e]);

        childrends.Clear();

    }
}


#if UNITY_EDITOR

[CustomEditor(typeof(TreeGenerator))]
public class TreeGeneratorEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        TreeGenerator core = (TreeGenerator)target;
        if (GUILayout.Button("Generar Tronco"))
        {
            core.Init();
        }
    }
}
#endif
