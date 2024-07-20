using UnityEngine;

namespace MameshibaGames.KekosBeach.Common
{
    public class BuoyancyObject : MonoBehaviour
    {
        [SerializeField]
        private float underwaterDrag = 1;

        [SerializeField]
        private float underwaterAngularDrag = 1;

        [SerializeField]
        private float airDrag;

        [SerializeField]
        private float airAngularDrag = 0.05f;

        [SerializeField]
        private float floatingPower = 3;

        [SerializeField]
        private float waterHeight;
        
        [SerializeField]
        private float waterHeightAmplitude ;

        public Vector3[] floatPositions;

        private Rigidbody _rigidbody;
        private bool _underwater;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }


        private void FixedUpdate()
        {
            bool allInAir = true;
            
            float waterHeightVariance = Mathf.Sin(Time.time - 0.05f) * waterHeightAmplitude;
            foreach (Vector3 floatPosition in floatPositions)
            {
                Vector3 floatGlobalPosition = transform.TransformPoint(floatPosition);
                float difference = floatGlobalPosition.y - (waterHeight + waterHeightVariance);

                if (difference < 0)
                {
                    _rigidbody.AddForceAtPosition(Vector3.up * (floatingPower * Mathf.Abs(difference)), floatGlobalPosition,
                        ForceMode.Force);
                    allInAir = false;
                }
            }
            
            if (allInAir && _underwater)
            {
                _underwater = false;
                SwitchState(_underwater);
            }
            else if (!allInAir && !_underwater)
            {
                _underwater = true;
                SwitchState(_underwater);
            }
        }

        private void SwitchState(bool isUnderWater)
        {
            if (isUnderWater)
            {
                _rigidbody.drag = underwaterDrag;
                _rigidbody.angularDrag = underwaterAngularDrag;
            }
            else
            {
                _rigidbody.drag = airDrag;
                _rigidbody.angularDrag = airAngularDrag;
            }
        }
    }
}
