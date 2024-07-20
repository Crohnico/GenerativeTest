using System.Collections;
using UnityEngine;

namespace MameshibaGames.KekosBeach.Common
{
    public class Patrol : MonoBehaviour
    {
        public Vector3[] patrolPoints;

        [SerializeField]
        private bool snapToFloor = true;
        
        [SerializeField] 
        private float speed;

        [SerializeField]
        private float rotationTime = 0.5f;

        private int _currentIndex;
        private Vector3 _nextPosition;
        private Vector3 _originPosition;
        private float _lerpValue;
        private Transform _transform;
        private Coroutine _changeRotationCoroutine;

        private void Awake()
        {
            _transform = transform;
            _currentIndex = 1;
            _transform.position = patrolPoints[0];
            _originPosition = _transform.position;
            _nextPosition = patrolPoints[_currentIndex];

            _transform.rotation = Quaternion.LookRotation(_nextPosition - _transform.position, Vector3.up);
        }

        private void Update()
        {
            if (_lerpValue >= 1)
            {
                _currentIndex++;
                _originPosition = _nextPosition;
                _nextPosition = patrolPoints[_currentIndex % patrolPoints.Length];
                _lerpValue = 0;
                
                if (_changeRotationCoroutine != null)
                    StopCoroutine(_changeRotationCoroutine);
                
                _changeRotationCoroutine = StartCoroutine(ChangeRotation(Quaternion.LookRotation(_nextPosition - _transform.position, Vector3.up)));
            }

            _lerpValue += (Time.deltaTime * speed) / Vector3.Distance(_originPosition, _nextPosition);
            _transform.position = Vector3.Lerp(_originPosition, _nextPosition, _lerpValue);

            if (snapToFloor && Physics.Raycast(_transform.position + Vector3.up, Vector3.down, out RaycastHit hit))
            {
                Vector3 position = _transform.position;
                position = new Vector3(position.x, hit.point.y, position.z);
                _transform.position = position;
            
            }
        }

        private IEnumerator ChangeRotation(Quaternion lookRotation)
        {
            float t = 0.0f;
            float duration = rotationTime;
            Quaternion start = _transform.rotation;
            Quaternion end = lookRotation;

            while (t < duration)
            {
                t += Time.deltaTime;
                _transform.rotation = Quaternion.Lerp(start, end, t / duration);
                yield return null;
            }
            
            _transform.rotation = end;
        }

        private void Reset()
        {
            Vector3 position = transform.position;
            patrolPoints = new[]
            {
                position,
                position - Vector3.right
            };
        }
    }
}