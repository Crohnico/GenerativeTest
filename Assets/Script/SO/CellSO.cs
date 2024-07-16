using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(fileName = "Cell Scriptable", menuName = "Map/Cell")]
public class CellSO : ScriptableObject
{
    [SerializeField]
    protected IACellType type;
    [SerializeField]
    protected GameObject[] prefab;
    [SerializeField]
    protected bool isWalkable;
    [SerializeField]
    protected Color testColor;
}




