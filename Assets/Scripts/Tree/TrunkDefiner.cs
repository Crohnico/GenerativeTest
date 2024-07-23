using System.Collections.Generic;
using UnityEngine;

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

    public BezierType bezierType;
    public GeometricForm geometricForm;
    public WidthType widthType;

    private float growthRate;

    public TrunkType trunkType;

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

    public bool hasBranches;
    private Dictionary<Vector3, Vector3> branchesPair = new Dictionary<Vector3, Vector3>();
    private List<Vector3> treeHighPoints = new List<Vector3>();
    private List<BezierTools> bezierTools = new List<BezierTools>();

    private List<MeshCreatorTool> trunks = new List<MeshCreatorTool>();

    public Material material;

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
        System.Random rng = new System.Random(seed);

        trunkType = DetermineTrunkType(rng);
        _trunkAmount = DetermineTrunkAmount(rng);
        height = rng.Next(minHeight, maxHeight);
        geometricForm = (GeometricForm)rng.Next(0, System.Enum.GetValues(typeof(GeometricForm)).Length);
        bezierType = (BezierType)rng.Next(0, System.Enum.GetValues(typeof(BezierType)).Length);



        if (trunkType == TrunkType.Multi)
        {
            widthType = (WidthType)rng.Next(0, 1);
        }
        else
        {
            widthType = (WidthType)rng.Next(0, System.Enum.GetValues(typeof(WidthType)).Length);
        }

        growthRate = GenerateRandomFloatInRange(rng, 1, 10);
        numPoints = rng.Next(4, 14);
        polygonFaces = rng.Next(3, 10);

        if (widthType == WidthType.Static)
        {
            float baseX = (trunkType == TrunkType.Multi) ? GenerateRandomFloatInRange(rng, 0.5f, 0.75f) : GenerateRandomFloatInRange(rng, 0.5f, 1f);
            float baseY = GenerateRandomFloatInRange(rng, 0.2f, 0.5f);
            growFromTo = new Vector2(baseX, baseY);
        }
        else
        {
            float baseX = (trunkType == TrunkType.Multi) ? GenerateRandomFloatInRange(rng, 0.5f, 1.5f) : GenerateRandomFloatInRange(rng, 0.5f, 2f);
            float baseY = GenerateRandomFloatInRange(rng, 0.2f, 0.5f);
            growFromTo = new Vector2(baseX, baseY);
        }

        if (trunks.Count > _trunkAmount)
        {
            for (int i = trunks.Count - 1; i >= _trunkAmount; i--)
            {
                MeshCreatorTool trunk = trunks[i];
                trunks.Remove(trunk);
                Destroy(trunk);
            }
        }

        float scope = baseScope + (_trunkAmount - 1) * 0.5f;
        float adjustedScope = scope + _trunkAmount;

        treeHighPoints = new List<Vector3>();
        bezierTools = new List<BezierTools>();

        for (int i = 0; i < _trunkAmount; i++)
        {
            treeHighPoints.Add(GetHightPoint(rng, i, _trunkAmount, adjustedScope));
            bezierTools.Add(GenerateBezierTools(rng, treeHighPoints[i], i, _trunkAmount, adjustedScope));
        }

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
            trunks[i].Initialize(treeHighPoints[i], Mathf.RoundToInt(treeHighPoints[i].y), material, geometricForm, widthType, growthRate, numPoints, polygonFaces, growFromTo, bezierType, bezierTools[i]);
        }
    }

    BezierTools GenerateBezierTools(System.Random rng, Vector3 end, int index, int totalTrunks, float adjustedScope)
    {
        BezierTools bezierTools = new BezierTools();

        float midHeight = GenerateRandomFloatInRange(rng, 0.5f, 0.8f) * end.y;
        float radius = adjustedScope / 2f;
        float angle = 2 * Mathf.PI * index / totalTrunks;
        float distance = radius / 2f;

        if (trunkType == TrunkType.Multi)
        {
            if (bezierType == BezierType.Quadratic)
            {
                float controlX = distance * Mathf.Cos(angle);
                float controlZ = distance * Mathf.Sin(angle);
                float controlY = GenerateRandomFloatInRange(rng, 0.2f, midHeight);

                bezierTools.Quadratic = new Vector3(controlX, controlY, controlZ);
            }
            else if (bezierType == BezierType.Cubic)
            {
                float controlX1 = distance * Mathf.Cos(angle);
                float controlZ1 = distance * Mathf.Sin(angle);
                float controlY1 = GenerateRandomFloatInRange(rng, 0.2f, midHeight);

                bezierTools.botQubic = new Vector3(controlX1, controlY1, controlZ1);

                float controlX2 = distance * Mathf.Cos(angle + Mathf.PI / totalTrunks);
                float controlZ2 = distance * Mathf.Sin(angle + Mathf.PI / totalTrunks);
                float controlY2 = GenerateRandomFloatInRange(rng, 0.5f * midHeight, end.y);

                bezierTools.topQubic = new Vector3(controlX2, controlY2, controlZ2);
            }
        }
        else 
        {
            if (bezierType == BezierType.Quadratic)
            {
                float controlX = GenerateRandomFloatInRange(rng, -1f, 1f);
                float controlZ = GenerateRandomFloatInRange(rng, -1f, 1f);
                float controlY = GenerateRandomFloatInRange(rng, 0.2f, midHeight);

                bezierTools.Quadratic = new Vector3(controlX, controlY, controlZ);
            }
            else if (bezierType == BezierType.Cubic)
            {
                float controlX1 = GenerateRandomFloatInRange(rng, -1f, 1f);
                float controlZ1 = GenerateRandomFloatInRange(rng, -1f, 1f);
                float controlY1 = GenerateRandomFloatInRange(rng, 0.2f, midHeight);

                bezierTools.botQubic = new Vector3(controlX1, controlY1, controlZ1);

                float controlX2 = GenerateRandomFloatInRange(rng, -1f, 1f);
                float controlZ2 = GenerateRandomFloatInRange(rng, -1f, 1f);
                float controlY2 = GenerateRandomFloatInRange(rng, 0.5f * midHeight, end.y);

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
        float y = GenerateRandomFloatInRange(rng, 4f, height);

        return new Vector3(x, y, z);
    }

    TrunkType DetermineTrunkType(System.Random rng)
    {
        float probability = GenerateRandomFloatInRange(rng, 0f, 1f);
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

    float GenerateRandomFloatInRange(System.Random prng, float min, float max)
    {
        return (float)(prng.NextDouble() * (max - min) + min);
    }

    public void TryGenerate()
    {
        foreach (MeshCreatorTool trunk in trunks) trunk.InitGeneration();
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
