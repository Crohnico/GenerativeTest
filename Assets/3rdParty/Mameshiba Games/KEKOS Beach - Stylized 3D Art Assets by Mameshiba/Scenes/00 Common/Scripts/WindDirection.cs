using UnityEngine;
using UnityEngine.Events;

namespace MameshibaGames.KekosBeach.Common
{
    public class WindDirection : MonoBehaviour
    {
        public static readonly UnityEvent<Vector3, float> WindChange = new UnityEvent<Vector3, float>();

        [SerializeField]
        private bool affectToMaterials;

        public bool AffectToMaterials
        {
            get => affectToMaterials;
            set
            {
                affectToMaterials = value;
                ProcessChange();
            }
        }
        
        [SerializeField]
        private bool affectToObjects;
        
        public bool AffectToObjects
        {
            get => affectToObjects;
            set
            {
                affectToObjects = value;
                ProcessChange();
            }
        }

        [Range(0, 360)]
        public float windDirection;

        [Range(0, 10)]
        public float windForce;

        public float editorScaleFactor = 1;
        
        private static readonly int _WindDirection = Shader.PropertyToID("WindDirection");
        private static readonly int _WindForce = Shader.PropertyToID("WindForce");
        public Vector3 WindDirectionVector => new Vector3(-Mathf.Sin(windDirection / 60), 0, -Mathf.Cos(windDirection / 60));

        private void Start()
        {
            ProcessChange();
        }

        public void ChangeWindDirection(float newAngleDegrees)
        {
            windDirection = newAngleDegrees;
            ProcessChange();
        }

        public void ChangeWindForce(float newForce)
        {
            windForce = newForce;
            ProcessChange();
        }

        public void ProcessChange()
        {
            Shader.SetGlobalFloat(_WindDirection, AffectToMaterials ? windDirection : 0);
            Shader.SetGlobalFloat(_WindForce, AffectToMaterials ? windForce : 0);   

            WindChange?.Invoke(WindDirectionVector, affectToObjects ? windForce : 0);
        }

        private void OnValidate()
        {
            ProcessChange();
        }
    }
}