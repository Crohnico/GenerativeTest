using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeData
{
    public float height;
    public float width;
    public float bend;
    public float twist;
    public float branchWidth;
    public float branchDensity;
    public float branchLenght;
    public float branchDeformity;
    public float leavesDensity;

    public TreeData(float height, float width, float bend, float twist, float branchWidth, float branchDensity, float branchLenght, float branchDeformity, float leavesDensity)
    {
        this.height = height;
        this.width = width;
        this.bend = bend;
        this.twist = twist;
        this.branchWidth = branchWidth;
        this.branchDensity = branchDensity;
        this.branchLenght = branchLenght;
        this.branchDeformity = branchDeformity;
        this.leavesDensity = leavesDensity;
    }
}
