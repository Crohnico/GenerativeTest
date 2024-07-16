using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class Herbivore : NPCCore
{
    protected override void OnEnable()
    {
        base.OnEnable();
        RandomiceStats();
    }

    protected void RandomiceStats()
    {
        hungerLost = Random.Range(0.001f, 0.01f);
        thirstLost = Random.Range(0.001f, 0.01f);
        sleepLost = Random.Range(0.001f, 0.01f);

        hungerGain = Random.Range(0.1f, 0.2f);
        thirstGain = Random.Range(0.1f, 0.2f);
        sleepGain = Random.Range(0.04f, 0.08f);

        hungerThreshold = Random.Range(0.1f, 0.5f);
        thirstThreshold = Random.Range(0.1f, 0.5f);
        sleepThreshold = Random.Range(0.1f, 0.5f);
    }
    protected override void DrinkBehaviour()
    {
        base.DrinkBehaviour();
    }
    protected override void EatBehaviour()
    {
        isEating = false;
        eatSource = vision.FirstOrDefault(n => n.isEatable);

        if (familyLeader != null && familyLeader.eatSource != null && eatSource != null &&
            Vector3.Distance(eatSource.Position, familyLeader.eatSource.Position) > 15)
            eatSource = null;

        if (eatSource == null)
        {
            WanderBehaviour();
            return;
        }

        if (currentCell != eatSource)
        {
            List<Vector3> path = AStar.FindPath(currentCell, eatSource);

            if (path.Count > 0)
                WanderTillCell(path, velocity);
        }
        else
        {
            isEating = true;
            isWandering = false;
        }
    }
    protected override void SleepBehaviour()
    {
        isSleeping = true;
        isWandering = false;
    }
    protected override void WanderBehaviour()
    {
        isWandering = true;

        if (familyLeader != null && Vector3.Distance(transform.position, familyLeader.transform.position) > 10)
        {
            List<Vector3> path = AStar.FindPath(currentCell, familyLeader.currentCell);
            if (path.Count > 0)
                WanderTillCell(path, velocity);
        }
        else
        {
            WanderOption(velocity);
        }
    }



#if UNITY_EDITOR
    // Llamar a DrawGizmosSelected para dibujar los gizmos en el editor
    private void OnDrawGizmos()
    {
        return;
        if (vision.Count <= 0) return;

        foreach (var cell in vision)
        {
            Color c = Color.red;

            if (cell.IsWalkable)
            {
                c = (cell.isSeashore) ? Color.blue : Color.green;          
            }

            if (cell.isEatable)
                c = Color.yellow;

            Gizmos.color = c;

            Vector3 position = cell.Position + Vector3.up * cell.height;
            Gizmos.DrawCube(position, Vector3.one * .5f);
        }
    }
#endif

}

#if UNITY_EDITOR

[CustomEditor(typeof(Herbivore))]
public class HerbivoreEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Herbivore core = (Herbivore)target;
        if (GUILayout.Button("Asignar Sprite"))
        {
           // core.SetUp();
        }
    }
}
#endif
