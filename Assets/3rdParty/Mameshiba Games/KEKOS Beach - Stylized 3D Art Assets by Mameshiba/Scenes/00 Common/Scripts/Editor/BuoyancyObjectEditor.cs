using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace MameshibaGames.KekosBeach.Common.Editor
{
    [CustomEditor(typeof(BuoyancyObject))]
    public class BuoyancyObjectEditor : UnityEditor.Editor
    {
        private bool _drawGizmos;
        private const string _MameBuoyancyPrefKey = "MAME_BUOYANCY_HELP";

        private void OnEnable()
        {
            _drawGizmos = EditorPrefs.GetBool(_MameBuoyancyPrefKey, true);
        }

        private void OnDisable()
        {
            EditorPrefs.SetBool(_MameBuoyancyPrefKey, _drawGizmos);
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(20);
            GUILayout.Label("EDITOR =======");
            _drawGizmos = GUILayout.Toggle(_drawGizmos, "DrawHelpGizmos");
        }
        
        private void OnSceneGUI()
        {
            if (!_drawGizmos) return;
            
            BuoyancyObject buoyancy = (BuoyancyObject)target;
            
            Vector3[] floatPoints = buoyancy.floatPositions;
            for (int i = 0; i < floatPoints.Length; i++)
            {
                Handles.zTest = CompareFunction.Always;
                
                EditorGUI.BeginChangeCheck();
                Vector3 newPosition = Handles.PositionHandle(buoyancy.transform.TransformPoint(floatPoints[i]), Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(buoyancy, "Float Points Change");
                    floatPoints[i] = buoyancy.transform.InverseTransformPoint(newPosition);
                }
            }
        }
    }
}
