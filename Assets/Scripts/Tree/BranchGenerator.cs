using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class BranchGenerator
{
    private GameObject branchPrefab;
    private float chancePertrunk;
    private int minLenght;
    private int maxLenght;

    private float branchXSize = .2f;
    private float branchYSize = .2f;
    private float branchZSize = .2f;


    [System.Serializable]
    public class BranchPart
    {
        public Vector3 id;
        public List<Vector3> corners = new List<Vector3>();
    }
    [System.Serializable]
    public class Branch 
    {
        public Vector3 growDir;
        public List<BranchPart> branch = new List<BranchPart>();
    }


    public List<Branch> branches = new List<Branch>();

    public List<BranchPart> rightBranch = new List<BranchPart>();
    public List<BranchPart> leftBranch = new List<BranchPart>();
    public List<BranchPart> forwardBranch = new List<BranchPart>();
    public List<BranchPart> backwardBranch = new List<BranchPart>();


    private float separatorX;
    private float separatorY;
    private float separatorZ;

    public List<Vector3> endPoints = new List<Vector3>();
    private Action<List<Vector3>> _callback;
    public List<Vector3> cloudPoints = new List<Vector3>();

    private Vector3 startLocalPos;
    List<Vector3> positions = new List<Vector3>();

    public BranchGenerator(Vector3 origin, Vector3 size, float sX, float sY, float sZ, float chancePertrunk, int minleght, int maxlenght)
    {
        //branchPrefab = prefab;
        branchXSize = size.x;
        branchYSize = size.y;
        branchZSize = size.z;

        branches.Clear();
        cloudPoints.Clear();
        startLocalPos = origin;

        separatorX = sX;
        separatorY = sY;
        separatorZ = sZ;
        this.chancePertrunk = chancePertrunk;
        minLenght = minleght;
        maxLenght = maxlenght;

        GenerateBranch();
    }


    void GenerateBranch()
    {
        float dice = Random.Range(0f, 1f);
        if (dice < chancePertrunk)
            GenerateBranch(rightBranch, 1, 0, 0);

        dice = Random.Range(0f, 1f);
        if (dice < chancePertrunk)
            GenerateBranch(leftBranch, -1, 0, 0);

        dice = Random.Range(0f, 1f);
        if (dice < chancePertrunk)
            GenerateBranch(forwardBranch, 0, 0, 1);

        dice = Random.Range(0f, 1f);
        if (dice < chancePertrunk)
            GenerateBranch(backwardBranch, 0, 0, -1);


        GenerateBranches();
    }

    void GenerateBranch(List<BranchPart> save, int initX, int initY, int initZ)
    {
        int lastX = initX;
        int lastY = initY;
        int lastZ = initZ;

        int branchLenght = UnityEngine.Random.Range(minLenght, maxLenght);
        for (int i = 0; i < branchLenght; i++)
        {
            BranchPart branchPart = new BranchPart();
            branchPart.id = new Vector3(lastX, lastY, lastZ);
            save.Add(branchPart);

            lastX += ((initX == 0) ? Random.Range(-1, 2) : initX);
            lastY += ((initY == 0) ? Random.Range(0, 2) : initY);
            lastZ += ((initZ == 0) ? Random.Range(-1, 2) : initZ);

            if (i == branchLenght - 1)
                positions.Add(branchPart.id);
        }

        Branch branch = new Branch();
        branch.branch = save;
        branch.growDir = new Vector3(initX, initY, initZ);
        branches.Add(branch);
    }

    void GenerateBranches()
    {
        InstantiateBranch(rightBranch, separatorX, separatorY, separatorX / 2, Vector3.right);
        InstantiateBranch(leftBranch, separatorX, separatorY, separatorX / 2, -Vector3.right);

        InstantiateBranch(forwardBranch, separatorZ / 2, separatorY, separatorZ, Vector3.forward);
        InstantiateBranch(backwardBranch, separatorZ / 2, separatorY, separatorZ, -Vector3.forward);
    }


    private void InstantiateBranch(List<BranchPart> branch, float xOffset, float yOffset, float zOffset, Vector3 spreadDirection)
    {
        foreach (var branchPart in branch)
        {
            Vector3 origin = new Vector3(((float)branchPart.id.x * xOffset), (float)branchPart.id.y * yOffset, ((float)branchPart.id.z * zOffset));
            Vector3 resultPosition = origin + startLocalPos;

            if (positions.Contains(branchPart.id))
            {
                endPoints.Add(resultPosition);
                AddPoints(resultPosition, true, spreadDirection, ref branchPart.corners);
            }
            else 
            {
                AddPoints(resultPosition, false, spreadDirection,ref branchPart.corners);
            }

        }
    }

    void AddPoints(Vector3 origin, bool fullBox, Vector3 spreadDirection, ref List<Vector3> positions)
    {
        int[] signs = { -1, 1 };

        foreach (int xSign in signs)
        {
            foreach (int ySign in signs)
            {
                foreach (int zSign in signs)
                {
                    // Si no es fullBox, filtramos las esquinas opuestas a la dirección de crecimiento
                    if (!fullBox)
                    {
                        if ((spreadDirection.x > 0 && xSign > 0) || (spreadDirection.x < 0 && xSign < 0) ||
                            (spreadDirection.y > 0 && ySign > 0) || (spreadDirection.y < 0 && ySign < 0) ||
                            (spreadDirection.z > 0 && zSign > 0) || (spreadDirection.z < 0 && zSign < 0))
                        {
                            continue;
                        }
                    }

                    Vector3 corner = origin + new Vector3(branchXSize * xSign, branchYSize * ySign, branchZSize * zSign);
                    positions.Add(corner);
                }
            }
        }
    }

}
