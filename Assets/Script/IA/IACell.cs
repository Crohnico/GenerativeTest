
using System;
using System.Collections.Generic;
using UnityEngine;


public enum IACellType
{
    Ground,
    Water,
    Tree01,
    Tree02,
    Exit,
    OpenTreeGroup,
    Dirt,
    Border,
    RiverBed,
    None
}

public class IACell
{
    public Vector3 Position { get; private set; }
    public IACellType Type { get; set; }
    public bool IsWalkable;
    public int GridX { get; set; }
    public int GridZ { get; set; }
    public float GCost { get; set; }
    public float HCost { get; set; }
    public float FCost { get { return GCost + HCost; } }
    public float height;
    public IACell Parent { get; set; }

    private bool isOccupped = false;
    public bool isSeashore = false;
    public bool isEatable = false;
    public string owner = "";

    public Action OnCellIn;
    public Action OnCellOut;
    public bool IsOccupped
    {
        get { return isOccupped; }
        set
        {
            if (isOccupped != value)
            {
                isOccupped = value;

                ParseChangeState(isOccupped);
            }
        }
    }

    public NPCCore ocupedAnimal = null;
    private bool isOnSight;

    public bool IsOnSight
    {
        get { return isOnSight; }
        set
        {
            if (isOnSight != value)
            {
                isOnSight = value;
            }
        }
    }

    public List<(int, int)> neighborCoordinates = new List<(int, int)>();
    public IACell(Vector3 position, IACellType type, bool walkable = true)
    {
        Position = position;
        GridX = Mathf.RoundToInt(position.x);
        GridZ = Mathf.RoundToInt(position.z);
        Type = type;
        IsWalkable = walkable;
        neighborCoordinates = new List<(int, int)>();
    }

    private void ParseChangeState(bool newState)
    {         
        if (newState) OnCellIn?.Invoke();
        else OnCellOut?.Invoke();
    }

    public void UpdateOcupant(NPCCore npc, bool leave) 
    {
        if (leave)
            ocupedAnimal = (npc == ocupedAnimal) ? null : ocupedAnimal;
        else
            ocupedAnimal = npc;
    }
}