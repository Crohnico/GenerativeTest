using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace MameshibaGames.KekosBeach.Common
{
	[RequireComponent(typeof(Camera))]
	public class OrbitalCamera : MonoBehaviour
	{
		[SerializeField] 
		private Transform focus;
		
		[SerializeField]
		[Range(1f, 20f)] 
		private float distance = 5f;
		
		[SerializeField]
		[Range(-3f, 3f)] 
		private float height;

		[SerializeField]
		[Range(1f, 360f)] 
		private float rotationSpeed = 90f;

		[SerializeField]
		[Range(-89f, 89f)] 
		private float minVerticalAngle = -45f, maxVerticalAngle = 45f;

		[SerializeField] 
		private LayerMask obstructionMask = -1;

		private bool _moveOnFocus = true;

		private Camera _regularCamera;

		private Vector2 _orbitAngles = new Vector2(45f, 0f);

		private bool _focused;

		private Vector3 CameraHalfExtends
		{
			get
			{
				Vector3 halfExtends;
				halfExtends.y = _regularCamera.nearClipPlane * Mathf.Tan(0.5f * Mathf.Deg2Rad * _regularCamera.fieldOfView);
				halfExtends.x = halfExtends.y * _regularCamera.aspect;
				halfExtends.z = 0f;
				return halfExtends;
			}
		}

		private void OnValidate()
		{
			if (maxVerticalAngle < minVerticalAngle)
			{
				maxVerticalAngle = minVerticalAngle;
			}
		}

		private void Awake()
		{
			_regularCamera = GetComponent<Camera>();
			transform.localRotation = Quaternion.Euler(_orbitAngles);
		}

		private void LateUpdate()
		{
			Quaternion lookRotation;
			if (ManualRotation())
			{
				ConstrainAngles();
				lookRotation = Quaternion.Euler(_orbitAngles);
			}
			else
			{
				lookRotation = transform.localRotation;
			}

			Vector3 lookDirection = lookRotation * Vector3.forward;
			Vector3 focusPosition = focus.position + Vector3.up * height;
			Vector3 lookPosition = focusPosition - lookDirection * distance;

			Vector3 rectOffset = lookDirection * _regularCamera.nearClipPlane;
			Vector3 rectPosition = lookPosition + rectOffset;
			Vector3 castFrom = focusPosition;
			Vector3 castLine = rectPosition - castFrom;
			float castDistance = castLine.magnitude;
			Vector3 castDirection = castLine / castDistance;

			if (Physics.BoxCast(castFrom, CameraHalfExtends, castDirection, out RaycastHit hit, lookRotation, castDistance,
				    obstructionMask))
			{
				rectPosition = castFrom + castDirection * hit.distance;
				lookPosition = (rectPosition + Vector3.up*0.1f) - rectOffset;
			}

			transform.SetPositionAndRotation(lookPosition, lookRotation);
		}

		private bool ManualRotation()
		{
			if (_moveOnFocus && !_focused) return false;

#if ENABLE_LEGACY_INPUT_MANAGER
			Vector2 input = new Vector2(-Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X"));
			float speed = rotationSpeed;
#elif ENABLE_INPUT_SYSTEM
			Vector2 input = new Vector2(-Mouse.current.delta.y.ReadValue(), Mouse.current.delta.x.ReadValue());
			float speed = rotationSpeed / 10;
#endif

			const float e = 0.001f;
			if (input.x < -e || input.x > e || input.y < -e || input.y > e)
			{
				_orbitAngles += speed * Time.unscaledDeltaTime * input;
				return true;
			}

			return false;
		}

		private void ConstrainAngles()
		{
			_orbitAngles.x = Mathf.Clamp(_orbitAngles.x, minVerticalAngle, maxVerticalAngle);

			if (_orbitAngles.y < 0f)
			{
				_orbitAngles.y += 360f;
			}
			else if (_orbitAngles.y >= 360f)
			{
				_orbitAngles.y -= 360f;
			}
		}

		private void OnEnable()
		{
			_focused = true;
			SetCursorState(_focused);
		}

		private void OnDisable()
		{
			_focused = false;
			SetCursorState(_focused);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
}
