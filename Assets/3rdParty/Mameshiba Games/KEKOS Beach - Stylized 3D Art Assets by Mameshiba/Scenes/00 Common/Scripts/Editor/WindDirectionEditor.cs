using UnityEditor;
using UnityEngine;

namespace MameshibaGames.KekosBeach.Common.Editor
{
    [CustomEditor(typeof(WindDirection))]
    public class WindDirectionEditor : UnityEditor.Editor
    {
        private readonly Gradient _windForceGradientColor = new Gradient
        {
            alphaKeys = new[]
            {
                new GradientAlphaKey(0.5f, 0),
                new GradientAlphaKey(0.5f, 1),
            },
            colorKeys = new[]
            {
                new GradientColorKey(Color.red, 1),
                new GradientColorKey(new Color(1f, 0.65f, 0f), 0.75f),
                new GradientColorKey(Color.green, 0.25f),
            }
        };

        private Tool _lastTool = Tool.None;
        private float _internalWindForce;
        private GUIStyle _labelGUIStyle;

        private void OnEnable()
        {
            WindDirection windDirection = (WindDirection)target;
            _internalWindForce = Remap(windDirection.windForce, 0, 10, 1, 11);
            _lastTool = Tools.current;
            Tools.current = Tool.None;
        }

        private void OnDisable()
        {
            Tools.current = _lastTool;
        }

        private void OnSceneGUI()
        {
            WindDirection windDirection = (WindDirection)target;
            Handles.color = Color.red;

            Vector3 position = windDirection.transform.position;
            float handleSize = HandleUtility.GetHandleSize(position) * windDirection.editorScaleFactor;
            float addFactor = 1;

            float sizeFactor = 5;

            Handles.color = _windForceGradientColor.Evaluate(windDirection.windForce / 10);
            Handles.DrawSolidDisc(position, Vector3.up, ((windDirection.windForce / sizeFactor) + addFactor) * handleSize);

            Handles.color = new Color(0.82f, 0.82f, 0.82f);
            Handles.DrawSolidDisc(position, Vector3.up, addFactor * handleSize);

            Handles.color = Color.red;
            Handles.DrawWireDisc(position, Vector3.up, ((10 / sizeFactor) + addFactor) * handleSize);
            Handles.color = Color.green;
            Handles.DrawWireDisc(position, Vector3.up, (addFactor * handleSize));

            Handles.color = Color.blue;

            Quaternion rotation = Quaternion.Euler(0, windDirection.windDirection + 180, 0);

            EditorGUI.BeginChangeCheck();

            float scale = Handles.ScaleValueHandle(_internalWindForce, position, rotation,
                Remap(windDirection.windForce, 0, 10, 6, 18) * handleSize, Handles.ArrowHandleCap, 0);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(windDirection, "Wind Direction Change");
                _internalWindForce = Mathf.Clamp(scale, 1, 11);
                float realScale = Remap(_internalWindForce, 1, 11, 0, 10);
                windDirection.windForce = realScale;
                windDirection.ProcessChange();
            }
            
            _labelGUIStyle ??= new GUIStyle("label")
            {
                normal =
                {
                    textColor = Color.black
                }
            };

            Handles.Label(position,
                $"Wind\nDirection : {windDirection.windDirection:F0}\nForce: {windDirection.windForce:F1}", _labelGUIStyle);

            EditorGUI.BeginChangeCheck();
            Handles.color = Color.blue;
            Quaternion newRotation = Handles.Disc(Quaternion.Euler(0, windDirection.windDirection, 0), position, Vector3.up,
                0.5f * handleSize, false, 0);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(windDirection, "Wind Direction Change");

                windDirection.windDirection = newRotation.eulerAngles.y % 360;
                windDirection.ProcessChange();
            }
        }

        private static float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
    }
}