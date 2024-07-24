using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BranchDefiner : MonoBehaviour
{
    private TrunkDefiner trunkDefiner;
    private Material branchMaterial;
    private List<GameObject> branches = new List<GameObject>();

    public void Init(TrunkDefiner trunkDefiner, Material branchMaterial, System.Random rng)
    {
        this.trunkDefiner = trunkDefiner;
        this.branchMaterial = branchMaterial;

        ClearOldBranches();

        if (trunkDefiner.hasBranches)
        {
            if (trunkDefiner.seed != trunkDefiner.oldSeed)
            {
                GenerateBranches(rng);
                EnsureMinimumBranches(rng);
            }
        }
    }

    private void ClearOldBranches()
    {
        foreach (var branch in branches)
        {
            DestroyImmediate(branch);
        }
        branches.Clear();
    }

    private void GenerateBranches(System.Random rng)
    {
        Dictionary<Vector3, int> branchPoints = new Dictionary<Vector3, int>();

        for (int i = 0; i < trunkDefiner.trunkCenters.Count; i++)
        {
            for (int j = 0; j < trunkDefiner.trunkCenters[i].Count; j++)
            {
                float heightFactor = (float)j / trunkDefiner.trunkCenters[i].Count;
                if (trunkDefiner.trunkType == TrunkDefiner.TrunkType.Multi && heightFactor < 0.5f) continue;

                float baseBranchProbability = 0.05f;
                float maxBranchProbability = 0.3f;
                float branchProbability = Mathf.Lerp(baseBranchProbability, maxBranchProbability, heightFactor);

                if (rng.NextDouble() < branchProbability)
                {
                    int branchCount = GenerateBranchCount(rng);
                    if (!branchPoints.ContainsKey(trunkDefiner.trunkCenters[i][j]))
                    {
                        branchPoints[trunkDefiner.trunkCenters[i][j]] = branchCount;
                    }
                    else
                    {
                        branchPoints[trunkDefiner.trunkCenters[i][j]] += branchCount;
                    }
                }
            }
        }

        foreach (var branchPoint in branchPoints)
        {
            for (int k = 0; k < branchPoint.Value; k++)
            {
                Vector3 direction = FindValidBranchDirection(branchPoint.Key, rng, 0.2f);
                GenerateBranch(branchPoint.Key, direction, k);
            }
        }

        EnsureMinimumBranches(rng);

        foreach (var branch in branches)
        {
            trunkDefiner.InitializateBranches(branch, rng);
        }
    }

    private Vector3 GenerateBranchDirection(int trunkIndex, System.Random rng)
    {
        float angle = GenerateValidBranchAngle(trunkIndex, rng);
        float radians = angle * Mathf.Deg2Rad;

        Vector3 direction = new Vector3(Mathf.Cos(radians), -1f, Mathf.Sin(radians));
        direction.Normalize();

        return direction;
    }

    private int GenerateBranchCount(System.Random rng)
    {
        double probability = rng.NextDouble();

        if (probability < 0.5) return 1;
        else if (probability < 0.8) return 2;
        else if (probability < 0.95) return 3;
        else if (probability < 0.99) return 4;
        else return 5;
    }

    private float GenerateValidBranchAngle(int trunkIndex, System.Random rng)
    {
        float minAngle = 0f;
        float maxAngle = 360f;

        if (trunkDefiner.trunkType == TrunkDefiner.TrunkType.Multi)
        {
            minAngle = (360f / trunkDefiner._trunkAmount) * trunkIndex;
            maxAngle = minAngle + (360f / trunkDefiner._trunkAmount);

            if (trunkDefiner._trunkAmount > 1)
            {
                if (trunkIndex % 2 == 0)
                {
                    minAngle += 5f;
                    maxAngle -= 5f;
                }
            }
        }

        float angle;
        do
        {
            angle = (float)rng.NextDouble() * (maxAngle - minAngle) + minAngle;
        } while (IsAngleTooCloseToExistingAngles(angle));

        return angle;
    }

    private bool IsAngleTooCloseToExistingAngles(float angle)
    {
        foreach (var existingBranch in branches)
        {
            float existingAngle = Quaternion.LookRotation(existingBranch.transform.forward).eulerAngles.y;
            if (Mathf.Abs(Mathf.DeltaAngle(existingAngle, angle)) < 10f)
            {
                return true;
            }
        }
        return false;
    }

    private void GenerateBranch(Vector3 startPoint, Vector3 direction, int branchIndex)
    {
        GameObject branch = new GameObject("Branch");
        branch.transform.position = startPoint; 
        branch.transform.parent = this.transform;

        float branchLength = trunkDefiner.GenerateRandomFloatInRange(new System.Random(trunkDefiner.seed), 1f, 3f);
        branch.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

        branches.Add(branch);
    }

    private Vector3 CalculateOffset(int branchIndex, float spreadRadius)
    {
        float angle = branchIndex * (360f / 5); 
        float radians = angle * Mathf.Deg2Rad;
        float xOffset = Mathf.Cos(radians) * spreadRadius;
        float zOffset = Mathf.Sin(radians) * spreadRadius;

        return new Vector3(xOffset, 0f, zOffset);
    }

    private bool IsDirectionDownward(Vector3 direction)
    {
        // Verifica si la dirección de la rama está inclinada hacia abajo
        return direction.y < -0.5f; // Puedes ajustar el umbral según sea necesario
    }


    private void EnsureMinimumBranches(System.Random rng)
    {
        if (branches.Count == 0 && trunkDefiner.hasBranches)
        {
            int randomTrunkIndex = rng.Next(trunkDefiner.trunkCenters.Count);
            int randomHeightIndex = rng.Next(trunkDefiner.trunkCenters[randomTrunkIndex].Count);
            Vector3 fallbackPoint = trunkDefiner.trunkCenters[randomTrunkIndex][randomHeightIndex];

            float minHeight = trunkDefiner.trunkCenters[randomTrunkIndex][0].y;
            float maxHeight = trunkDefiner.trunkCenters[randomTrunkIndex][^1].y;
            float randomHeight = (float)(rng.NextDouble() * (maxHeight - minHeight - 1f) + minHeight + 1f);

            fallbackPoint.y = randomHeight;

            Vector3 fallbackDirection = Vector3.up;

            GenerateBranch(fallbackPoint, fallbackDirection, 0);
        }
    }

    private Vector3 FindValidBranchDirection(Vector3 startPoint, System.Random rng, float minAngleDifference)
    {
        float minAngle = 0f;
        float maxAngle = 360f;

        for (int attempt = 0; attempt < 10; attempt++)
        {
            float angle = (float)rng.NextDouble() * (maxAngle - minAngle) + minAngle;
            Vector3 direction = GenerateDirectionFromAngle(angle);

            if (IsDirectionValid(startPoint, direction, minAngleDifference))
            {
                return direction;
            }
        }
        return Vector3.forward;
    }

    private Vector3 GenerateDirectionFromAngle(float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(radians), -1f, Mathf.Sin(radians)).normalized;
    }

    private bool IsDirectionValid(Vector3 startPoint, Vector3 direction, float minAngleDifference)
    {
        foreach (var branch in branches)
        {
            Vector3 branchDirection = branch.transform.forward;
            float angle = Vector3.Angle(branchDirection, direction);

            if (angle < minAngleDifference)
            {
                return false;
            }
        }

        return true;
    }
}


