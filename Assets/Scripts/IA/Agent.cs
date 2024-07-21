using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Agent : MonoBehaviour
{
    public Color pathColor = Color.green;
    public List<Vector3> pathToDraw;
    public GameObject target;

    public void FindPath()
    {
        // Obtener la celda de inicio (0, 0)
        IACell startCell  = Grid.Instance.GetGridCell((int)transform.position.x, (int)transform.position.z);
        IACell targetCell = Grid.Instance.GetGridCell((int)target.transform.position.x, (int)target.transform.position.z);

        Debug.Log($"Start is null? {startCell != null}" );
        Debug.Log($"targetCell is null? {targetCell != null}");

        List<Vector3> path = AStar.FindPath(startCell, targetCell);
        Debug.Log($"path es nulo? {path == null}");

        if (path != null)
            Debug.Log(path.Count);
        // Dibujar el camino encontrado con gizmos
        pathToDraw = path;
     //   DrawPath(path);
    }

#if UNITY_EDITOR
    // Llamar a DrawGizmosSelected para dibujar los gizmos en el editor
    private void OnDrawGizmos()
    {
        if (pathToDraw != null)
        {
            DrawPath(pathToDraw);
        }
    }
#endif

    // Método para dibujar el camino con gizmos
    private void DrawPath(List<Vector3> path)
    {
        if (path == null || path.Count == 0)
        {
            return;
        }

        for (int i = 0; i < path.Count - 1; i++)
        {
            Gizmos.color = pathColor;
            Gizmos.DrawCube(path[i], Vector3.one * .5f); // Dibujar un cubo (cuadrado en el plano XY) en la posición del punto
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Agent))]
public class AgentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Agent agent = (Agent)target;

        if (GUILayout.Button("Buscar y Pintar Camino"))
        {
            agent.FindPath();
        }
    }
}
#endif
