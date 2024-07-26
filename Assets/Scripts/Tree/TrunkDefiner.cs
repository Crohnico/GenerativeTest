using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TrunkDefiner : MonoBehaviour
{
    public enum TrunkType
    {
        Solo,
        Multi
    }
    public TrunkType trunkType;
    public bool hasBranches;

    public BezierType bezierType;
    public BezierType branchBeziersType;
    public GeometricForm geometricForm;
    public WidthType widthType;

    public float minGrosor = float.MaxValue;

    private float growthRate;

    public int minHeight = 8, maxHeight = 14;

    [Range(1, 5)]
    public int _trunkAmount = 1;

    [Range(6, 14)]
    public int height;

    private int numPoints;
    private int polygonFaces;
    private int baseScope;
    private Vector2 growFromTo;

    public int seed;
    [HideInInspector]
    public int oldSeed = int.MinValue;

    private List<Vector3> treeHighPoints = new List<Vector3>();
    private List<BezierTools> bezierTools = new List<BezierTools>();

    private List<MeshCreatorTool> trunks = new List<MeshCreatorTool>();
    private List<MeshCreatorTool> branches = new List<MeshCreatorTool>();
    public Material trunkMaterial;
    public Material leaveMaterial;

    [HideInInspector]
    public List<List<Vector3>> trunkCenters = new List<List<Vector3>>();
    public List<List<Vector3>> branchCenters = new List<List<Vector3>>();

    public BranchDefiner branchDefiner;
    public LeavesDefiner leavesDefiner;

    public List<Vector3> leavesPositions = new List<Vector3>();
    public List<Vector3> leavesDirections = new List<Vector3>();

    System.Random rng;

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            foreach (Vector3 heightPoint in treeHighPoints)
                Gizmos.DrawSphere(heightPoint, 0.05f);

            GestionateTrunk();
        }
    }

    private void GestionateTrunk()
    {
        if (seed != oldSeed)
        {
            minGrosor = float.MaxValue;

            branches.Clear();
            leavesPositions.Clear();




            System.Random rng = new System.Random(seed);
            this.rng = rng;
            ClearOldTrunks();

            trunkType = DetermineTrunkType(rng);
            _trunkAmount = DetermineTrunkAmount(rng);
            height = rng.Next(minHeight, maxHeight);
            geometricForm = (GeometricForm)rng.Next(0, System.Enum.GetValues(typeof(GeometricForm)).Length);
            bezierType = (BezierType)rng.Next(0, System.Enum.GetValues(typeof(BezierType)).Length);
            branchBeziersType = BezierType.Cubic;

            hasBranches = rng.NextDouble() < 0.8;

            DetermineWidthType(rng);

            growthRate = RandomTool.GenerateRandomFloatInRange(rng, 1, 10);
            numPoints = rng.Next(4, 14);
            polygonFaces = rng.Next(3, 10);

            SetGrowFromTo(rng);

            AdjustTrunkListSize();

            float scope = baseScope + (_trunkAmount - 1) * 0.5f;
            float adjustedScope = scope + _trunkAmount;

            GenerateTreeHighPointsAndBezierTools(rng, adjustedScope);

            InitializeTrunks();
            branchDefiner.Init(this, rng);

            leavesDefiner.Init(this, rng, leavesPositions.ToArray(), trunkCenters, branchCenters);
        }

        oldSeed = seed;
    }

    private void ClearOldTrunks()
    {
        trunkCenters.Clear();
        branchCenters.Clear();
    }

    private void DetermineWidthType(System.Random rng)
    {
        if (trunkType == TrunkType.Multi)
        {
            widthType = (WidthType)rng.Next(0, 1);
        }
        else
        {
            widthType = (WidthType)rng.Next(0, System.Enum.GetValues(typeof(WidthType)).Length);
        }
    }

    private void SetGrowFromTo(System.Random rng)
    {
        if (widthType == WidthType.Static)
        {
            float baseX = (trunkType == TrunkType.Multi) ? RandomTool.GenerateRandomFloatInRange(rng, 0.5f, 0.75f) : RandomTool.GenerateRandomFloatInRange(rng, 0.5f, 1f);
            float baseY = RandomTool.GenerateRandomFloatInRange(rng, 0.2f, 0.5f);
            growFromTo = new Vector2(baseX, baseY);
        }
        else
        {
            float baseX = (trunkType == TrunkType.Multi) ? RandomTool.GenerateRandomFloatInRange(rng, 0.5f, 1.5f) : RandomTool.GenerateRandomFloatInRange(rng, 0.5f, 2f);
            float baseY = RandomTool.GenerateRandomFloatInRange(rng, 0.2f, 0.5f);
            growFromTo = new Vector2(baseX, baseY);
        }
    }

    private void AdjustTrunkListSize()
    {
        if (trunks.Count > _trunkAmount)
        {
            for (int i = trunks.Count - 1; i >= _trunkAmount; i--)
            {
                MeshCreatorTool trunk = trunks[i];
                trunks.Remove(trunk);
                Destroy(trunk);
            }
        }
    }

    private void GenerateTreeHighPointsAndBezierTools(System.Random rng, float adjustedScope)
    {
        treeHighPoints = new List<Vector3>();
        bezierTools = new List<BezierTools>();

        for (int i = 0; i < _trunkAmount; i++)
        {
            treeHighPoints.Add(GetHightPoint(rng, i, _trunkAmount, adjustedScope));
            bezierTools.Add(GenerateBezierTools(rng, bezierType, treeHighPoints[i], i, _trunkAmount, adjustedScope));
        }
    }

    public void InitializateBranches(GameObject go, System.Random rng)
    {
        MeshCreatorTool definer = go.AddComponent<MeshCreatorTool>();
        Vector3 branchEnd = go.transform.position + go.transform.up * RandomTool.GenerateRandomFloatInRange(rng, (float)height * .15f, (float)height * .45f);

        BezierTools bezierTools = GenerateBezierTools(rng, branchBeziersType, branchEnd - go.transform.position, 1, 1, baseScope);
        Vector2 _grow = new Vector2(RandomTool.GenerateRandomFloatInRange(rng, growFromTo.x * .25f, growFromTo.x * .75f), RandomTool.GenerateRandomFloatInRange(rng, growFromTo.y * .25f, growFromTo.y * .5f));
        WidthType _widthType = (widthType == WidthType.Decreasing) ? WidthType.Increasing : widthType;

        if (_widthType == WidthType.Static) minGrosor = (_grow.x < minGrosor) ? _grow.x : minGrosor;
        else minGrosor = (_grow.y < minGrosor) ? _grow.y : minGrosor;

        Vector3[] positions = definer.Initialize(branchEnd - go.transform.position, Mathf.RoundToInt((branchEnd - go.transform.position).y), geometricForm, _widthType, growthRate, numPoints, polygonFaces, _grow, branchBeziersType, bezierTools);
        branches.Add(definer);
        leavesPositions.Add(branchEnd);


        List<Vector3> wordPosition = new List<Vector3>();
        foreach (var value in positions)
        {
            wordPosition.Add(value + go.transform.position);
        }
        branchCenters.Add(wordPosition);
    }

    private void InitializeTrunks()
    {
        if (trunks.Count < _trunkAmount)
        {
            for (int i = 0; i < _trunkAmount; i++)
            {
                MeshCreatorTool definer = gameObject.AddComponent<MeshCreatorTool>();
                trunks.Add(definer);
            }
        }

        for (int i = 0; i < treeHighPoints.Count; i++)
        {
            if (widthType == WidthType.Static) minGrosor = (growFromTo.x < minGrosor) ? growFromTo.x : minGrosor;
            else minGrosor = (growFromTo.y < minGrosor) ? growFromTo.y : minGrosor;

            trunkCenters.Add(trunks[i].Initialize(treeHighPoints[i], Mathf.RoundToInt(treeHighPoints[i].y), geometricForm, widthType, growthRate, numPoints, polygonFaces, growFromTo, bezierType, bezierTools[i]).ToList());
        }
    }

    BezierTools GenerateBezierTools(System.Random rng, BezierType bType, Vector3 end, int index, int totalTrunks, float adjustedScope)
    {
        BezierTools bezierTools = new BezierTools();

        float midHeight = RandomTool.GenerateRandomFloatInRange(rng, 0.5f, 0.8f) * end.y;
        float radius = adjustedScope / 2f;
        float angle = 2 * Mathf.PI * index / totalTrunks;
        float distance = radius / 2f;

        if (trunkType == TrunkType.Multi)
        {
            if (bType == BezierType.Quadratic)
            {
                float controlX = distance * Mathf.Cos(angle);
                float controlZ = distance * Mathf.Sin(angle);
                float controlY = RandomTool.GenerateRandomFloatInRange(rng, 0.2f, midHeight);

                bezierTools.Quadratic = new Vector3(controlX, controlY, controlZ);
            }
            else if (bType == BezierType.Cubic)
            {
                float controlX1 = distance * Mathf.Cos(angle);
                float controlZ1 = distance * Mathf.Sin(angle);
                float controlY1 = RandomTool.GenerateRandomFloatInRange(rng, 0.2f, midHeight);

                bezierTools.botQubic = new Vector3(controlX1, controlY1, controlZ1);

                float controlX2 = distance * Mathf.Cos(angle + Mathf.PI / totalTrunks);
                float controlZ2 = distance * Mathf.Sin(angle + Mathf.PI / totalTrunks);
                float controlY2 = RandomTool.GenerateRandomFloatInRange(rng, 0.5f * midHeight, end.y);

                bezierTools.topQubic = new Vector3(controlX2, controlY2, controlZ2);
            }
        }
        else
        {
            if (bType == BezierType.Quadratic)
            {
                float controlX = RandomTool.GenerateRandomFloatInRange(rng, -1f, 1f);
                float controlZ = RandomTool.GenerateRandomFloatInRange(rng, -1f, 1f);
                float controlY = RandomTool.GenerateRandomFloatInRange(rng, 0.2f, midHeight);

                bezierTools.Quadratic = new Vector3(controlX, controlY, controlZ);
            }
            else if (bType == BezierType.Cubic)
            {
                float controlX1 = RandomTool.GenerateRandomFloatInRange(rng, -.5f, .5f);
                float controlZ1 = RandomTool.GenerateRandomFloatInRange(rng, -.5f, .5f);
                float controlY1 = RandomTool.GenerateRandomFloatInRange(rng, 0.2f, midHeight);

                bezierTools.botQubic = new Vector3(controlX1, controlY1, controlZ1);

                float controlX2 = RandomTool.GenerateRandomFloatInRange(rng, -1f, 1f);
                float controlZ2 = RandomTool.GenerateRandomFloatInRange(rng, -1f, 1f);
                float controlY2 = RandomTool.GenerateRandomFloatInRange(rng, 0.5f * midHeight, end.y);

                bezierTools.topQubic = new Vector3(controlX2, controlY2, controlZ2);
            }
        }

        return bezierTools;
    }

    Vector3 GetHightPoint(System.Random rng, int index, int totalTrunks, float adjustedScope)
    {
        float angle = 2 * Mathf.PI * index / totalTrunks;
        float radius = adjustedScope / 2f;

        float x = radius * Mathf.Cos(angle);
        float z = radius * Mathf.Sin(angle);
        float y = RandomTool.GenerateRandomFloatInRange(rng, 4f, height);

        return new Vector3(x, y, z);
    }

    TrunkType DetermineTrunkType(System.Random rng)
    {
        float probability = RandomTool.GenerateRandomFloatInRange(rng, 0f, 1f);
        return probability < 0.85f ? TrunkType.Solo : TrunkType.Multi;
    }

    int DetermineTrunkAmount(System.Random rng)
    {
        if (trunkType == TrunkType.Solo) return 1;

        float probability = (float)rng.NextDouble();
        if (probability < 0.5f) return 2;
        else if (probability < 0.8f) return 3;
        else if (probability < 0.95f) return 4;
        else return 5;
    }

    public void TryGenerate()
    {
        GestionateTrunk();

        GameObject go = new GameObject();
        go.transform.position = transform.position;
        go.name = $"Tree_{seed}";

        List<MeshFilter> filters = new List<MeshFilter>();
        foreach (MeshCreatorTool trunk in trunks)
        {
            GameObject trunkObject = trunk.InitGeneration();
            trunkObject.transform.parent = go.transform;
            filters.Add(trunkObject.GetComponent<MeshFilter>());

        }
        foreach (MeshCreatorTool branch in branches)
        {
            GameObject branchObject = branch.InitGeneration();
            branchObject.transform.parent = go.transform;
            filters.Add(branchObject.GetComponent<MeshFilter>());
        }

        Mesh newMesh = MeshOptimizer.CombineMeshes(filters);

        float optimization = (minGrosor * .8f > .2f) ? .2f : minGrosor * .8f;

        List<MeshFilter> leaves = leavesDefiner.TryGenerate(rng);

        if (leaves != null && leaves.Count != 0)
        {
            Mesh leavesMesh = MeshOptimizer.CombineMeshes(leaves);

            GameObject LeavesGo = new GameObject();

            MeshRenderer LeavesRenderer = LeavesGo.AddComponent<MeshRenderer>();
            LeavesRenderer.material = leaveMaterial;

            MeshFilter LeavesFilter = LeavesGo.AddComponent<MeshFilter>();
            LeavesFilter.mesh = leavesMesh;

            LeavesGo.transform.position = go.transform.position;
            LeavesGo.transform.parent = go.transform;
            LeavesGo.name = $"Leaves_{seed}";

            MeshOptimizer.OptimizeMesh(LeavesFilter.mesh, optimization);
        }

        MeshRenderer renderer = go.AddComponent<MeshRenderer>();
        renderer.material = trunkMaterial;

        MeshFilter filter = go.AddComponent<MeshFilter>();
        filter.mesh = newMesh;

        for (int i = filters.Count - 1; i >= 0; i--)
            Destroy(filters[i].gameObject);

        MeshOptimizer.OptimizeMesh(filter.mesh, optimization);

        go.transform.localScale = Vector3.one * 2f;
    }
}

public enum TrunkType
{
    Solo,
    Multi
}

public class BezierTools
{
    public Vector3 Quadratic;
    public Vector3 botQubic;
    public Vector3 topQubic;
}

#if UNITY_EDITOR

[CustomEditor(typeof(TrunkDefiner))]
public class TrunkDefinerEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        TrunkDefiner core = (TrunkDefiner)target;

        if (GUILayout.Button("Next"))
        {
            core.seed += 1;
        }

        if (GUILayout.Button("Try Generate"))
        {
            core.TryGenerate();
        }
    }
}
#endif
