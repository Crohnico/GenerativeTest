﻿using System.Collections;
using UnityEngine;


//-------------------------------------------------------------
//--APR Player
//--APRController (Main Player Controller)
//----------------------------------


namespace ARP.APR.Scripts
{
    public class APRController : MonoBehaviour
    {
        public GameObject
            Root, Body, Head,
            UpperRightArm, LowerRightArm,
            UpperLeftArm, LowerLeftArm,
            UpperRightLeg, LowerRightLeg,
            UpperLeftLeg, LowerLeftLeg,
            RightFoot, LeftFoot;

        public Rigidbody RightHand, LeftHand;

        public Transform COMP;

        [Header("Player Input Axis")]
        public string forwardBackward = "Vertical";
        public string leftRight = "Horizontal";
        public string jump = "Jump";
        public string reachLeft = "Fire1";
        public string reachRight = "Fire2";


        [Header("The Layer Only This Player Is On")]
        //Player layer name
        public string thisPlayerLayer = "Player_1";

        [Header("Movement Properties")]
        //Player properties
        public bool forwardIsCameraDirection = true;
        //Movement
        public float moveSpeed = 10f;
        public float turnSpeed = 6f;
        public float jumpForce = 18f;

        [Header("Balance Properties")]
        //Balance
        public bool autoGetUpWhenPossible = true;
        public bool useStepPrediction = true;
        public float balanceHeight = 2.5f;
        public float balanceStrength = 5000f;
        public float coreStrength = 1500f;
        public float limbStrength = 500f;
        //Walking
        public float StepDuration = 0.2f;
        public float StepHeight = 1.7f;
        public float FeetMountForce = 25f;

        [Header("Reach Properties")]
        //Reach
        public float reachSensitivity = 25f;
        public float armReachStiffness = 2000f;


        private float
            timer, Step_R_timer, Step_L_timer,
            MouseYAxisArms, MouseXAxisArms, MouseYAxisBody;

        private bool
            WalkForward, WalkBackward,
            StepRight, StepLeft, Alert_Leg_Right,
            Alert_Leg_Left, balanced = true, GettingUp,
            ResetPose, isRagdoll, isKeyDown, moveAxisUsed,
            jumpAxisUsed, reachLeftAxisUsed, reachRightAxisUsed;

        [HideInInspector]
        public bool
            jumping, isJumping, inAir,
            punchingRight, punchingLeft;

        private Camera cam;
        private Vector3 Direction;
        private Vector3 CenterOfMassPoint;

        private GameObject[] APR_Parts;


        JointDrive
            BalanceOn, PoseOn, CoreStiffness, ReachStiffness, DriveOff;


        Quaternion
            HeadTarget, BodyTarget,
            UpperRightArmTarget, LowerRightArmTarget,
            UpperLeftArmTarget, LowerLeftArmTarget,
            UpperRightLegTarget, LowerRightLegTarget,
            UpperLeftLegTarget, LowerLeftLegTarget;

        [Header("Player Editor Debug Mode")]
        public bool editorDebugMode;



        void Awake()
        {
            PlayerSetup();
        }

        void Update()
        {
            if (!inAir)
            {
                PlayerMovement();

            }


            if (balanced && useStepPrediction)
            {
                StepPrediction();
                CenterOfMass();
            }

            if (!useStepPrediction)
            {
                ResetWalkCycle();
            }

            GroundCheck();
            CenterOfMass();
        }

        void FixedUpdate()
        {
            Walking();

            PlayerRotation();
            ResetPlayerPose();

            PlayerGetUpJumping();
        }

        void PlayerSetup()
        {
            cam = Camera.main;

            BalanceOn = new JointDrive();
            BalanceOn.positionSpring = balanceStrength;
            BalanceOn.positionDamper = 0;
            BalanceOn.maximumForce = Mathf.Infinity;

            PoseOn = new JointDrive();
            PoseOn.positionSpring = limbStrength;
            PoseOn.positionDamper = 0;
            PoseOn.maximumForce = Mathf.Infinity;

            CoreStiffness = new JointDrive();
            CoreStiffness.positionSpring = coreStrength;
            CoreStiffness.positionDamper = 0;
            CoreStiffness.maximumForce = Mathf.Infinity;

            ReachStiffness = new JointDrive();
            ReachStiffness.positionSpring = armReachStiffness;
            ReachStiffness.positionDamper = 0;
            ReachStiffness.maximumForce = Mathf.Infinity;

            DriveOff = new JointDrive();
            DriveOff.positionSpring = 25;
            DriveOff.positionDamper = 0;
            DriveOff.maximumForce = Mathf.Infinity;


            APR_Parts = new GameObject[]
            {
                Root,
                Body,
                Head,
                UpperRightArm,
                LowerRightArm,
                UpperLeftArm,
                LowerLeftArm,
                UpperRightLeg,
                LowerRightLeg,
                UpperLeftLeg,
                LowerLeftLeg,
                RightFoot,
                LeftFoot
            };

            BodyTarget = APR_Parts[1].GetComponent<ConfigurableJoint>().targetRotation;
            HeadTarget = APR_Parts[2].GetComponent<ConfigurableJoint>().targetRotation;
            UpperRightArmTarget = APR_Parts[3].GetComponent<ConfigurableJoint>().targetRotation;
            LowerRightArmTarget = APR_Parts[4].GetComponent<ConfigurableJoint>().targetRotation;
            UpperLeftArmTarget = APR_Parts[5].GetComponent<ConfigurableJoint>().targetRotation;
            LowerLeftArmTarget = APR_Parts[6].GetComponent<ConfigurableJoint>().targetRotation;
            UpperRightLegTarget = APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation;
            LowerRightLegTarget = APR_Parts[8].GetComponent<ConfigurableJoint>().targetRotation;
            UpperLeftLegTarget = APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation;
            LowerLeftLegTarget = APR_Parts[10].GetComponent<ConfigurableJoint>().targetRotation;
        }


        void GroundCheck()
        {
            Ray ray = new Ray(APR_Parts[0].transform.position, -APR_Parts[0].transform.up);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, balanceHeight, 1 << LayerMask.NameToLayer("Ground")) && !inAir && !isJumping && !reachRightAxisUsed && !reachLeftAxisUsed)
            {
                if (!balanced && APR_Parts[0].GetComponent<Rigidbody>().velocity.magnitude < 1f)
                {
                    if (autoGetUpWhenPossible)
                    {
                        balanced = true;
                    }
                }
            }
            else if (!Physics.Raycast(ray, out hit, balanceHeight, 1 << LayerMask.NameToLayer("Ground")))
            {
                if (balanced)
                {
                    balanced = false;
                }
            }
            if (balanced && isRagdoll)
            {
                DeactivateRagdoll();
            }
            else if (!balanced && !isRagdoll)
            {
                ActivateRagdoll();
            }
        }

        void StepPrediction()
        {
            if (!WalkForward && !WalkBackward)
            {
                StepRight = false;
                StepLeft = false;
                Step_R_timer = 0;
                Step_L_timer = 0;
                Alert_Leg_Right = false;
                Alert_Leg_Left = false;
            }

            if (COMP.position.z < APR_Parts[11].transform.position.z && COMP.position.z < APR_Parts[12].transform.position.z)
            {
                WalkBackward = true;
            }
            else
            {
                if (!isKeyDown)
                {
                    WalkBackward = false;
                }
            }

            if (COMP.position.z > APR_Parts[11].transform.position.z && COMP.position.z > APR_Parts[12].transform.position.z)
            {
                WalkForward = true;
            }
            else
            {
                if (!isKeyDown)
                {
                    WalkForward = false;
                }
            }
        }

        void ResetWalkCycle()
        {
            if (!WalkForward && !WalkBackward)
            {
                StepRight = false;
                StepLeft = false;
                Step_R_timer = 0;
                Step_L_timer = 0;
                Alert_Leg_Right = false;
                Alert_Leg_Left = false;
            }
        }

        void PlayerMovement()
        {
            if (forwardIsCameraDirection)
            {
                Direction = APR_Parts[0].transform.rotation * new Vector3(Input.GetAxisRaw(leftRight), 0.0f, Input.GetAxisRaw(forwardBackward));
                Direction.y = 0f;
                APR_Parts[0].transform.GetComponent<Rigidbody>().velocity = Vector3.Lerp(APR_Parts[0].transform.GetComponent<Rigidbody>().velocity, (Direction * moveSpeed) + new Vector3(0, APR_Parts[0].transform.GetComponent<Rigidbody>().velocity.y, 0), 0.8f);

                if (Input.GetAxisRaw(leftRight) != 0 || Input.GetAxisRaw(forwardBackward) != 0 && balanced)
                {
                    if (!WalkForward && !moveAxisUsed)
                    {
                        WalkForward = true;
                        moveAxisUsed = true;
                        isKeyDown = true;
                    }
                }

                else if (Input.GetAxisRaw(leftRight) == 0 && Input.GetAxisRaw(forwardBackward) == 0)
                {
                    if (WalkForward && moveAxisUsed)
                    {
                        WalkForward = false;
                        moveAxisUsed = false;
                        isKeyDown = false;
                    }
                }

            }
            else
            {
                if (Input.GetAxisRaw(forwardBackward) != 0)
                {
                    var v3 = APR_Parts[0].GetComponent<Rigidbody>().transform.forward * (Input.GetAxisRaw(forwardBackward) * moveSpeed);
                    v3.y = APR_Parts[0].GetComponent<Rigidbody>().velocity.y;
                    APR_Parts[0].GetComponent<Rigidbody>().velocity = v3;
                }


                if (Input.GetAxisRaw(forwardBackward) > 0)
                {
                    if (!WalkForward && !moveAxisUsed)
                    {
                        WalkBackward = false;
                        WalkForward = true;
                        moveAxisUsed = true;
                        isKeyDown = true;

                        if (isRagdoll)
                        {
                            APR_Parts[7].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
                            APR_Parts[7].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
                            APR_Parts[8].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
                            APR_Parts[8].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
                            APR_Parts[9].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
                            APR_Parts[9].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
                            APR_Parts[10].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
                            APR_Parts[10].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
                            APR_Parts[11].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
                            APR_Parts[11].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
                            APR_Parts[12].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
                            APR_Parts[12].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
                        }
                    }
                }

                else if (Input.GetAxisRaw(forwardBackward) < 0)
                {
                    if (!WalkBackward && !moveAxisUsed)
                    {
                        WalkForward = false;
                        WalkBackward = true;
                        moveAxisUsed = true;
                        isKeyDown = true;

                        if (isRagdoll)
                        {
                            APR_Parts[7].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
                            APR_Parts[7].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
                            APR_Parts[8].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
                            APR_Parts[8].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
                            APR_Parts[9].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
                            APR_Parts[9].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
                            APR_Parts[10].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
                            APR_Parts[10].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
                            APR_Parts[11].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
                            APR_Parts[11].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
                            APR_Parts[12].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
                            APR_Parts[12].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
                        }
                    }
                }

                else if (Input.GetAxisRaw(forwardBackward) == 0)
                {
                    if (WalkForward || WalkBackward && moveAxisUsed)
                    {
                        WalkForward = false;
                        WalkBackward = false;
                        moveAxisUsed = false;
                        isKeyDown = false;

                        if (isRagdoll)
                        {
                            APR_Parts[7].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
                            APR_Parts[7].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
                            APR_Parts[8].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
                            APR_Parts[8].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
                            APR_Parts[9].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
                            APR_Parts[9].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
                            APR_Parts[10].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
                            APR_Parts[10].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
                            APR_Parts[11].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
                            APR_Parts[11].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
                            APR_Parts[12].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
                            APR_Parts[12].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
                        }
                    }
                }
            }
        }


        void PlayerRotation()
        {
            if (forwardIsCameraDirection)
            {
                var lookPos = cam.transform.forward;
                lookPos.y = 0;
                var rotation = Quaternion.LookRotation(lookPos);
                APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation = Quaternion.Slerp(APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation, Quaternion.Inverse(rotation), Time.deltaTime * turnSpeed);
            }

            else
            {
                if (Input.GetAxisRaw(leftRight) != 0)
                {
                    APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation = Quaternion.Lerp(APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation, new Quaternion(APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation.x, APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation.y - (Input.GetAxisRaw(leftRight) * turnSpeed), APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation.z, APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation.w), 6 * Time.fixedDeltaTime);
                }

                if (APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation.y < -0.98f)
                {
                    APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation.x, 0.98f, APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation.z, APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation.w);
                }

                else if (APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation.y > 0.98f)
                {
                    APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation.x, -0.98f, APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation.z, APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation.w);
                }
            }
        }


        void PlayerGetUpJumping()
        {
            if (Input.GetAxis(jump) > 0)
            {
                if (!jumpAxisUsed)
                {
                    if (balanced && !inAir)
                    {
                        jumping = true;
                    }

                    else if (!balanced)
                    {
                        DeactivateRagdoll();
                    }
                }

                jumpAxisUsed = true;
            }

            else
            {
                jumpAxisUsed = false;
            }


            if (jumping)
            {
                isJumping = true;

                var v3 = APR_Parts[0].GetComponent<Rigidbody>().transform.up * jumpForce;
                v3.x = APR_Parts[0].GetComponent<Rigidbody>().velocity.x;
                v3.z = APR_Parts[0].GetComponent<Rigidbody>().velocity.z;
                APR_Parts[0].GetComponent<Rigidbody>().velocity = v3;
            }

            if (isJumping)
            {
                timer = timer + Time.fixedDeltaTime;

                if (timer > 0.2f)
                {
                    timer = 0.0f;
                    jumping = false;
                    isJumping = false;
                    inAir = true;
                }
            }
        }

        public void PlayerLanded()
        {
            if (inAir && !isJumping && !jumping)
            {
                inAir = false;
                ResetPose = true;
            }
        }

        void Walking()
        {
            if (!inAir)
            {
                if (WalkForward)
                {
                    if (APR_Parts[11].transform.position.z < APR_Parts[12].transform.position.z && !StepLeft && !Alert_Leg_Right)
                    {
                        StepRight = true;
                        Alert_Leg_Right = true;
                        Alert_Leg_Left = true;
                    }

                    if (APR_Parts[11].transform.position.z > APR_Parts[12].transform.position.z && !StepRight && !Alert_Leg_Left)
                    {
                        StepLeft = true;
                        Alert_Leg_Left = true;
                        Alert_Leg_Right = true;
                    }
                }

                if (WalkBackward)
                {
                    if (APR_Parts[11].transform.position.z > APR_Parts[12].transform.position.z && !StepLeft && !Alert_Leg_Right)
                    {
                        StepRight = true;
                        Alert_Leg_Right = true;
                        Alert_Leg_Left = true;
                    }

                    if (APR_Parts[11].transform.position.z < APR_Parts[12].transform.position.z && !StepRight && !Alert_Leg_Left)
                    {
                        StepLeft = true;
                        Alert_Leg_Left = true;
                        Alert_Leg_Right = true;
                    }
                }

                if (StepRight)
                {
                    Step_R_timer += Time.fixedDeltaTime;

                    APR_Parts[11].GetComponent<Rigidbody>().AddForce(-Vector3.up * FeetMountForce * Time.deltaTime, ForceMode.Impulse);

                    if (WalkForward)
                    {
                        APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.x + 0.09f * StepHeight, APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.y, APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.z, APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.w);
                        APR_Parts[8].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(APR_Parts[8].GetComponent<ConfigurableJoint>().targetRotation.x - 0.09f * StepHeight * 2, APR_Parts[8].GetComponent<ConfigurableJoint>().targetRotation.y, APR_Parts[8].GetComponent<ConfigurableJoint>().targetRotation.z, APR_Parts[8].GetComponent<ConfigurableJoint>().targetRotation.w);

                        APR_Parts[9].GetComponent<ConfigurableJoint>().GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.x - 0.12f * StepHeight / 2, APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.y, APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.z, APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.w);
                    }

                    if (WalkBackward)
                    {
                        APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.x - 0.00f * StepHeight, APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.y, APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.z, APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.w);
                        APR_Parts[8].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(APR_Parts[8].GetComponent<ConfigurableJoint>().targetRotation.x - 0.07f * StepHeight * 2, APR_Parts[8].GetComponent<ConfigurableJoint>().targetRotation.y, APR_Parts[8].GetComponent<ConfigurableJoint>().targetRotation.z, APR_Parts[8].GetComponent<ConfigurableJoint>().targetRotation.w);

                        APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.x + 0.02f * StepHeight / 2, APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.y, APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.z, APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.w);
                    }

                    if (Step_R_timer > StepDuration)
                    {
                        Step_R_timer = 0;
                        StepRight = false;

                        if (WalkForward || WalkBackward)
                        {
                            StepLeft = true;
                        }
                    }

                }
                else
                {
                    APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation = Quaternion.Lerp(APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation, UpperRightLegTarget, (8f) * Time.fixedDeltaTime);
                    APR_Parts[8].GetComponent<ConfigurableJoint>().targetRotation = Quaternion.Lerp(APR_Parts[8].GetComponent<ConfigurableJoint>().targetRotation, LowerRightLegTarget, (17f) * Time.fixedDeltaTime);

                    APR_Parts[11].GetComponent<Rigidbody>().AddForce(-Vector3.up * FeetMountForce * Time.deltaTime, ForceMode.Impulse);
                    APR_Parts[12].GetComponent<Rigidbody>().AddForce(-Vector3.up * FeetMountForce * Time.deltaTime, ForceMode.Impulse);
                }

                if (StepLeft)
                {
                    Step_L_timer += Time.fixedDeltaTime;

                    APR_Parts[12].GetComponent<Rigidbody>().AddForce(-Vector3.up * FeetMountForce * Time.deltaTime, ForceMode.Impulse);

                    if (WalkForward)
                    {
                        APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.x + 0.09f * StepHeight, APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.y, APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.z, APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.w);
                        APR_Parts[10].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(APR_Parts[10].GetComponent<ConfigurableJoint>().targetRotation.x - 0.09f * StepHeight * 2, APR_Parts[10].GetComponent<ConfigurableJoint>().targetRotation.y, APR_Parts[10].GetComponent<ConfigurableJoint>().targetRotation.z, APR_Parts[10].GetComponent<ConfigurableJoint>().targetRotation.w);

                        APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.x - 0.12f * StepHeight / 2, APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.y, APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.z, APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.w);
                    }

                    if (WalkBackward)
                    {
                        APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.x - 0.00f * StepHeight, APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.y, APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.z, APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.w);
                        APR_Parts[10].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(APR_Parts[10].GetComponent<ConfigurableJoint>().targetRotation.x - 0.07f * StepHeight * 2, APR_Parts[10].GetComponent<ConfigurableJoint>().targetRotation.y, APR_Parts[10].GetComponent<ConfigurableJoint>().targetRotation.z, APR_Parts[10].GetComponent<ConfigurableJoint>().targetRotation.w);

                        APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.x + 0.02f * StepHeight / 2, APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.y, APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.z, APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.w);
                    }

                    if (Step_L_timer > StepDuration)
                    {
                        Step_L_timer = 0;
                        StepLeft = false;

                        if (WalkForward || WalkBackward)
                        {
                            StepRight = true;
                        }
                    }
                }
                else
                {
                    APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation = Quaternion.Lerp(APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation, UpperLeftLegTarget, (7f) * Time.fixedDeltaTime);
                    APR_Parts[10].GetComponent<ConfigurableJoint>().targetRotation = Quaternion.Lerp(APR_Parts[10].GetComponent<ConfigurableJoint>().targetRotation, LowerLeftLegTarget, (18f) * Time.fixedDeltaTime);

                    APR_Parts[11].GetComponent<Rigidbody>().AddForce(-Vector3.up * FeetMountForce * Time.deltaTime, ForceMode.Impulse);
                    APR_Parts[12].GetComponent<Rigidbody>().AddForce(-Vector3.up * FeetMountForce * Time.deltaTime, ForceMode.Impulse);
                }
            }
        }

        public void ActivateRagdoll()
        {
            isRagdoll = true;
            balanced = false;


            APR_Parts[0].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
            APR_Parts[0].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;

            APR_Parts[2].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
            APR_Parts[2].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;

            if (!reachRightAxisUsed)
            {
                APR_Parts[3].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
                APR_Parts[3].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
                APR_Parts[4].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
                APR_Parts[4].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
            }

            if (!reachLeftAxisUsed)
            {
                APR_Parts[5].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
                APR_Parts[5].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
                APR_Parts[6].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
                APR_Parts[6].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
            }

            APR_Parts[7].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
            APR_Parts[7].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
            APR_Parts[8].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
            APR_Parts[8].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
            APR_Parts[9].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
            APR_Parts[9].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
            APR_Parts[10].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
            APR_Parts[10].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
            APR_Parts[11].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
            APR_Parts[11].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
            APR_Parts[12].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
            APR_Parts[12].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
        }

        void DeactivateRagdoll()
        {
            isRagdoll = false;
            balanced = true;

            APR_Parts[0].GetComponent<ConfigurableJoint>().angularXDrive = BalanceOn;
            APR_Parts[0].GetComponent<ConfigurableJoint>().angularYZDrive = BalanceOn;

            APR_Parts[2].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
            APR_Parts[2].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;

            if (!reachRightAxisUsed)
            {
                APR_Parts[3].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
                APR_Parts[3].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
                APR_Parts[4].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
                APR_Parts[4].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
            }

            if (!reachLeftAxisUsed)
            {
                APR_Parts[5].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
                APR_Parts[5].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
                APR_Parts[6].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
                APR_Parts[6].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
            }

            APR_Parts[7].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
            APR_Parts[7].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
            APR_Parts[8].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
            APR_Parts[8].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
            APR_Parts[9].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
            APR_Parts[9].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
            APR_Parts[10].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
            APR_Parts[10].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
            APR_Parts[11].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
            APR_Parts[11].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
            APR_Parts[12].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
            APR_Parts[12].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;

            ResetPose = true;
        }

        void ResetPlayerPose()
        {
            if (ResetPose && !jumping)
            {
                APR_Parts[1].GetComponent<ConfigurableJoint>().targetRotation = BodyTarget;
                APR_Parts[3].GetComponent<ConfigurableJoint>().targetRotation = UpperRightArmTarget;
                APR_Parts[4].GetComponent<ConfigurableJoint>().targetRotation = LowerRightArmTarget;
                APR_Parts[5].GetComponent<ConfigurableJoint>().targetRotation = UpperLeftArmTarget;
                APR_Parts[6].GetComponent<ConfigurableJoint>().targetRotation = LowerLeftArmTarget;

                MouseYAxisArms = 0;

                ResetPose = false;
            }
        }

        void CenterOfMass()
        {
            CenterOfMassPoint =

                (APR_Parts[0].GetComponent<Rigidbody>().mass * APR_Parts[0].transform.position +
                 APR_Parts[1].GetComponent<Rigidbody>().mass * APR_Parts[1].transform.position +
                 APR_Parts[2].GetComponent<Rigidbody>().mass * APR_Parts[2].transform.position +
                 APR_Parts[3].GetComponent<Rigidbody>().mass * APR_Parts[3].transform.position +
                 APR_Parts[4].GetComponent<Rigidbody>().mass * APR_Parts[4].transform.position +
                 APR_Parts[5].GetComponent<Rigidbody>().mass * APR_Parts[5].transform.position +
                 APR_Parts[6].GetComponent<Rigidbody>().mass * APR_Parts[6].transform.position +
                 APR_Parts[7].GetComponent<Rigidbody>().mass * APR_Parts[7].transform.position +
                 APR_Parts[8].GetComponent<Rigidbody>().mass * APR_Parts[8].transform.position +
                 APR_Parts[9].GetComponent<Rigidbody>().mass * APR_Parts[9].transform.position +
                 APR_Parts[10].GetComponent<Rigidbody>().mass * APR_Parts[10].transform.position +
                 APR_Parts[11].GetComponent<Rigidbody>().mass * APR_Parts[11].transform.position +
                 APR_Parts[12].GetComponent<Rigidbody>().mass * APR_Parts[12].transform.position)

                /

                (APR_Parts[0].GetComponent<Rigidbody>().mass + APR_Parts[1].GetComponent<Rigidbody>().mass +
                 APR_Parts[2].GetComponent<Rigidbody>().mass + APR_Parts[3].GetComponent<Rigidbody>().mass +
                 APR_Parts[4].GetComponent<Rigidbody>().mass + APR_Parts[5].GetComponent<Rigidbody>().mass +
                 APR_Parts[6].GetComponent<Rigidbody>().mass + APR_Parts[7].GetComponent<Rigidbody>().mass +
                 APR_Parts[8].GetComponent<Rigidbody>().mass + APR_Parts[9].GetComponent<Rigidbody>().mass +
                 APR_Parts[10].GetComponent<Rigidbody>().mass + APR_Parts[11].GetComponent<Rigidbody>().mass +
                 APR_Parts[12].GetComponent<Rigidbody>().mass);

            COMP.position = CenterOfMassPoint;
        }
    }
}
