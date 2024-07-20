using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace MameshibaGames.KekosBeach.Common.Editor
{
    [CustomEditor(typeof(Patrol))]
    public class PatrolEditor : UnityEditor.Editor
    {
        private bool _tryToSnapPoints;
        private const string _MamePatrolSnapKey = "MAME_PATROL_SNAP";

        private void OnEnable()
        {
            _tryToSnapPoints = EditorPrefs.GetBool(_MamePatrolSnapKey, true);
        }

        private void OnDisable()
        {
            EditorPrefs.SetBool(_MamePatrolSnapKey, _tryToSnapPoints);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(20);
            GUILayout.Label("EDITOR =======");
            _tryToSnapPoints = GUILayout.Toggle(_tryToSnapPoints, "Try to snap points");
        }

        private void OnSceneGUI()
        {
            Patrol patrol = (Patrol)target;
            
            Handles.color = Color.blue;

            float sphereSize = 0.1f;
            Vector3 lineDisplacement = new Vector3(0, sphereSize, sphereSize);
            Vector3[] patrolPoints = patrol.patrolPoints;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                Handles.zTest = CompareFunction.Less;
                Vector3 patrolPoint = patrolPoints[i];
                if (i > 0)
                    Handles.DrawLine(patrolPoint + lineDisplacement, patrolPoints[i-1] + lineDisplacement);
                if (i == patrolPoints.Length - 1)
                    Handles.DrawLine(patrolPoint + lineDisplacement, patrolPoints[0] + lineDisplacement);

                Handles.zTest = CompareFunction.Always;
                Handles.Label(patrolPoint + lineDisplacement + lineDisplacement, (i+1).ToString(), new GUIStyle());
                
                EditorGUI.BeginChangeCheck();
                Vector3 newPosition = Handles.PositionHandle(patrolPoints[i], Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(patrol, "Patrol Points Change");

                    if (_tryToSnapPoints)
                    {
                        Ray ray = new Ray(newPosition + Vector3.up * 0.1f, Vector3.down);
                        if (Physics.Raycast(ray, out RaycastHit hit))
                            newPosition = hit.point;
                    }

                    patrolPoints[i] = newPosition;
                }
            }
        }
    }
}
