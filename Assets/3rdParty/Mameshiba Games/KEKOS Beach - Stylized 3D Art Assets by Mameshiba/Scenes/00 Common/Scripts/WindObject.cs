using UnityEngine;

namespace MameshibaGames.KekosBeach.Common
{
    [RequireComponent(typeof(Rigidbody))]
    public class WindObject : MonoBehaviour
    {
        [SerializeField]
        private float windScale = 1;

        private Vector3 _windDirection;
        private float _windForce;

        private Rigidbody _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            WindDirection.WindChange.AddListener(WindChange);
        }

        private void WindChange(Vector3 newDirection, float newForce)
        {
            _windDirection = newDirection;
            _windForce = newForce;
        }

        private void FixedUpdate()
        {
            _rigidbody.AddForce(_windDirection.normalized * (_windForce * windScale), ForceMode.Force);
        }
    }
}