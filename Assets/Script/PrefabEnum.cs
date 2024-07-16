using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PrefabEnum 
{
    public CellGenerator cellSO;

    [Range(0, 1f)]
    public float percentThreshold;

    public float resultantThreshold;

    [Range(0f, 1f)]
    public float density;



}
