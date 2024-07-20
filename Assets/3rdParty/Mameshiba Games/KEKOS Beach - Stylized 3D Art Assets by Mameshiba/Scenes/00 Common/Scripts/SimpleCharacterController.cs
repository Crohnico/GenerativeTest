using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MameshibaGames.KekosBeach.Common
{
    public class SimpleCharacterController : MonoBehaviour
    {
        [SerializeField]
        private Transform moveTransform;

        [SerializeField]
        private Animator animator;

        [SerializeField]
        private float walkSpeed;

        [SerializeField]
        private float runSpeed;
        
        [SerializeField]
        private float jumpForce;

        [SerializeField]
        private float jumpDelay;

        [SerializeField]
        private LayerMask groundLayer;
        
        [SerializeField]
        private float turnSpeed;
        
        [SerializeField]
        private Camera characterCamera;

        private Rigidbody _rigidbody;
        private Vector3 _targetForward;
        private Vector3 _previousMoveVector;
        private Vector3 _originForward;
        private float _turnLerp;
        private float _speed;
        private float _horizontalInput;
        private float _verticalInput;
        private bool _jumping;
        private bool _goingDown;
        private Coroutine _jumpCoroutine;
        
        private bool InFloor
        {
            get
            {
                List<Collider> overlapSphere = Physics.OverlapSphere(_rigidbody.position, 0.1f, groundLayer).ToList();
                overlapSphere.RemoveAll(x => x.gameObject == gameObject);
                return overlapSphere.Count > 0 && !_jumping;
            }
        }

        private static readonly int _Walk = Animator.StringToHash("Walk");
        private static readonly int _Run = Animator.StringToHash("Run");
        private static readonly int _Idle = Animator.StringToHash("Idle");
        
        [HideInInspector]
        public bool jump, sprint, left, right, up, down;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            Movement();
            Jump();
        }

        private void FixedUpdate()
        {
            PerformPhysicMovement();
        }

        private void Movement()
        {
            ProcessInputs();
            UpdateSpeedAndPerformAnimation();
            RotateCharacter();

            void ProcessInputs()
            {
                _horizontalInput = 0;
                _verticalInput = 0;
                if (right)
                    _horizontalInput = 1;
                else if (left)
                    _horizontalInput = -1;
                if (up)
                    _verticalInput = 1;
                else if (down)
                    _verticalInput = -1;
            }
            void UpdateSpeedAndPerformAnimation()
            {
                _speed = 0;
                
                if (_horizontalInput != 0 || _verticalInput != 0)
                {
                    if (sprint)
                    {
                        _speed = runSpeed;
                        if (InFloor)
                        {
                            animator.SetTrigger(_Run);
                            animator.ResetTrigger(_Idle);
                            animator.ResetTrigger(_Walk);
                        }
                    }
                    else
                    {
                        _speed = walkSpeed;
                        if (InFloor)
                        {
                            animator.SetTrigger(_Walk);
                            animator.ResetTrigger(_Idle);
                            animator.ResetTrigger(_Run);
                        }
                    }
                }
                else
                {
                    if (InFloor)
                    {
                        animator.SetTrigger(_Idle);
                        animator.ResetTrigger(_Walk);
                        animator.ResetTrigger(_Run);
                    }
                }
            }
            void RotateCharacter()
            {
                if (_speed > 0)
                {
                    Vector3 moveVector = new Vector3(_horizontalInput, 0, _verticalInput);
                    Vector3 previousMoveVector = moveVector;
                    
                    moveVector = Quaternion.AngleAxis(characterCamera.transform.rotation.eulerAngles.y, Vector3.up) * moveVector;
                    
                    if (previousMoveVector != _previousMoveVector)
                    {
                        _turnLerp = 0;
                        _originForward = moveTransform.forward;
                    }

                    _previousMoveVector = previousMoveVector;
                    _targetForward = moveVector;
                    if (moveTransform.forward != _targetForward)
                    {
                        moveTransform.forward = Vector3.Lerp(_originForward, _targetForward, _turnLerp);
                        _turnLerp += Time.deltaTime * turnSpeed;
                    }
                }
            }
        }

        private void PerformPhysicMovement()
        {
            if (_speed > 0 && !_jumping)
            {
                Vector3 moveVector = new Vector3(_horizontalInput, 0, _verticalInput);
                moveVector = Quaternion.AngleAxis(characterCamera.transform.rotation.eulerAngles.y, Vector3.up) * moveVector;
                _rigidbody.MovePosition(_rigidbody.position + (moveVector * (_speed * Time.deltaTime)));
            }
        }

        private void Jump()
        {
            if (InFloor)
                _goingDown = false;
            
            if (!InFloor && _rigidbody.velocity.y < -0.01f && !_goingDown)
            {
                _jumping = false;
                _goingDown = true;
                animator.ResetTrigger(_Idle);
                animator.ResetTrigger(_Walk);
                animator.ResetTrigger(_Run);
                animator.CrossFade("InPlace_Jump_InAir", 0.2f);
                if (_jumpCoroutine != null)
                    StopCoroutine(_jumpCoroutine);
            }

            if (jump && InFloor)
            {
                _jumping = true;
                animator.ResetTrigger(_Idle);
                animator.ResetTrigger(_Walk);
                animator.ResetTrigger(_Run);
                animator.CrossFadeInFixedTime("InPlace_Jump_Start", 0.1f);
                if (_jumpCoroutine != null)
                    StopCoroutine(_jumpCoroutine);
                _jumpCoroutine = StartCoroutine(PerformJump());
            }
        }

        private IEnumerator PerformJump()
        {
            yield return new WaitForSeconds(jumpDelay);
            _rigidbody.AddForce(0, jumpForce, 0);
            yield return new WaitForSeconds(0.1f);
            _jumping = false;
        }
    }
}