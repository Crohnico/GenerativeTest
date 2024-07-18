//------------
//... PLayer-X
//... V2.0.1
//... © TheFamousMouse™
//--------------------
//... Support email:
//... thefamousmouse.developer@gmail.com
//--------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PlayerX;

namespace PlayerX
{
	public class PX_Controller : MonoBehaviour
	{
		[Header("Player-X [Controller]")]
		
		[Space]
		
		[Header("- Controller Dependencies")]
		public PX_Dependencies dependencies;
		
		
		//... Move
		[Header("- Move")]
		public bool enableMove = true;
		public bool velocityMode = true;
		public float moveVelocity = 5.5f;
		public float runVelocity = 10f;
		public float maximumVelocity = 10f;
		public float accelerationSmoothness = 3f;
		public float deccelerationSmoothness = 0.5f;
		
		//... Hidden Move Variables
		[HideInInspector]
	    public Vector3 
		moveDir, 
		moveDirAcceleration, 
		moveDirRot,
		camDir;
		
		[HideInInspector]
		public bool 
		running = false;
		
		
		//... Rotation
		[Header("- Rotation")]
		public bool enableRotate = true;
		public float turnSmoothness = 15f;
		
		//... Hidden Rotation Variables
		Vector3 rotDir;
		Quaternion rotation;
		
		
		//... Jump
		[Header("- Jump")]
		public bool enableJump = true;
		public float jumpVelocity = 5f;
		public float maximumJumpVelocity = 8f;
		public float jumpDuration = 0.2f;
		
		//... Hidden Jump Variables
		[HideInInspector]
		public bool 
		jumping, jumped;
		
		float jumpTimer;
		
		//... Kneel
		[Header("- Kneel")]
		public bool enableKneel = true;
		
		//- Hidden Kneel Variables
		[HideInInspector]
		public bool kneeling = false;
		
		
		//... Head Tracking
		[Header("- Head Tracking")]
		public bool enableHeadTracking = true;
		public Transform headTracker;
		public float headTrackDistance = 5f;
		public float lookAtSpeed = 15;
		public List<Transform> 
		headTrackContainer = new List<Transform>();
		
		//- Hidden Head Track Variables
		[HideInInspector]
		public bool headTracking = false;
		
		[HideInInspector]
		public Transform trackObject;
		
		//...
		Quaternion headTrackInversedRot;
		
		
		//... Reach
		[Header("- Reach")]
		public bool enableReach = true;
		public float reachSensitivity = 15f;
		
		//- Hidden Reach Variables
		[HideInInspector]
		public bool
		reachingLeft = false, 
		reachingRight = false,
		reaching = false;
		
		
		//... Grab
		[Header("- Grab")]
		public bool enableGrab = true;
		public float grabForce = 0.25f;
		public PX_GrabDetector grabDetectLeft;
		public PX_GrabDetector grabDetectRight;
		
		//...
		bool
		grabbingLeft = false,
		grabbingRight = false;
		
		//...
		[HideInInspector]
		public ConfigurableJoint 
		grabLeft, grabRight;
		
		//...
		Rigidbody 
		otherPlayerPartGrabLeft, 
		otherPlayerPartGrabRight;
		
		Rigidbody []
		grabLeftOtherPlayerPhysics,
		grabRightOtherPlayerPhysics;
		
		
		//... Punch
		[Header("Punch")]
		public bool enablePunch = true;
		public float punchForce = 14f;
		
		//- Hidden Punch Variables
		[HideInInspector]
		public bool 
		punchingRight,
		punchingLeft,
		punching;
		
		//...
		float 
		punchRightRamp,
		punchLeftRamp;
		
		
		//... Kick
		[Header("Kick")]
		public bool enableKick = true;
		public float kickForce = 20f;
		public float supportForce = 30f;
		public Rigidbody leftShoe;
		public Rigidbody rightShoe;
		
		//- Hidden Punch Variables
		[HideInInspector]
		public bool 
		kickingRight,
		kickingLeft,
		kicking;
		
		//...
		float 
		kickRightRamp,
		kickLeftRamp;
		
		
		
		//... Initial Setups
		void Start()
		{
			SetupHeadTrack();
		}
		
		
		//... Non-Physics Functions
		void Update()
		{
			Direction();
		}
		
		
		//... Physics Functions
		void FixedUpdate()
		{
			if(dependencies.state.isAlive && dependencies.state.active && !dependencies.state.isKnockedOut)
			{
				Move();
				Rotate();
				Jump();
				Kneel();
			}
			
			if(dependencies.state.isAlive && !dependencies.state.isKnockedOut)
			{
				HeadTracking();
				
				Reach();
				
				Punch();
				Kick();
			}
			
			Grab();
		}
		
		
		
		//... Direction ...
		void Direction()
		{
			//... Direction based on camera angle
			if(dependencies.playerCamera.cameraRotation && !dependencies.inputs.simpleAI)
			{
				moveDir = (dependencies.playerCamera.followCamera.transform.forward * dependencies.inputs.key_Inputs.y 
				+ dependencies.playerCamera.followCamera.transform.right * dependencies.inputs.key_Inputs.x);
				
				moveDir.y = 0f;
				moveDir = moveDir.normalized;
			}
			
			//... Direction based on world
			else if(!dependencies.inputs.simpleAI)
			{
				moveDir = Vector3.forward * -dependencies.playerCamera.offset * dependencies.inputs.key_Inputs.y + Vector3.right * -dependencies.playerCamera.offset * dependencies.inputs.key_Inputs.x;
				moveDir.y = 0f;
				moveDir = moveDir.normalized;
			}
			
			//... Acceleration
			if(moveDir != Vector3.zero)
			{
				moveDirAcceleration = Vector3.Slerp(moveDirAcceleration, moveDir, accelerationSmoothness * Time.deltaTime);
				rotDir = moveDirAcceleration;
			}
			
			//... decceleration
			else if(moveDir == Vector3.zero && dependencies.state.Grounded())
			{
				moveDirAcceleration = Vector3.Slerp(moveDirAcceleration, Vector3.zero, deccelerationSmoothness * Time.deltaTime);
			}
		}
		
		
		
		//... Move ...
		void Move()
		{
			if(enableMove)
			{
				if(!kicking)
				{
					//... Move by velocity (Less physics based but more responsive)
					if(velocityMode)
					{
						//... Calculate desired velocity
						Vector3 desiredMoveVelocity = moveDirAcceleration * moveVelocity;
						Vector3 desiredRunVelocity = moveDirAcceleration * runVelocity;

						//... Clamp the magnitude of desired move velocity
						if(desiredMoveVelocity.magnitude > maximumVelocity)
						{
							desiredMoveVelocity = desiredMoveVelocity.normalized * maximumVelocity;
							desiredMoveVelocity.y = 0f;
						}
						
						//... Clamp the magnitude of desired run velocity
						if(desiredRunVelocity.magnitude > maximumVelocity)
						{
							desiredRunVelocity = desiredRunVelocity.normalized * maximumVelocity;
							desiredRunVelocity.y = 0f;
						}
						
						
						//... Apply move velocity
						if(!dependencies.inputs.keyRun_Input && dependencies.state.Grounded() && moveDir != Vector3.zero)
						{
							if(running)
							{
								running = false;
							}
							
							dependencies.player.rootPhysics.AddForce(desiredMoveVelocity - new Vector3(dependencies.player.rootPhysics.velocity.x, 
							0f, dependencies.player.rootPhysics.velocity.z), ForceMode.VelocityChange);
							
							dependencies.procedural.stepDuration = dependencies.procedural.stepDurationRecord;
							dependencies.procedural.stepHeight = dependencies.procedural.stepHeightRecord;
						}
						
						//... Apply move in air velocity
						else if(!dependencies.state.Grounded() && moveDir != Vector3.zero)
						{
							if(running)
							{
								running = false;
							}
							
							var inAirVelocity = desiredMoveVelocity * 1.2f;
							
							dependencies.player.rootPhysics.AddForce(inAirVelocity - new Vector3(dependencies.player.rootPhysics.velocity.x, 
							0f, dependencies.player.rootPhysics.velocity.z), ForceMode.VelocityChange);
							
							dependencies.procedural.stepDuration = dependencies.procedural.stepDurationRecord;
							dependencies.procedural.stepHeight = dependencies.procedural.stepHeightRecord;
						}
						
						//... Apply run velocity
						else if(dependencies.inputs.keyRun_Input && dependencies.state.Grounded() && moveDir != Vector3.zero)
						{
							running = true;
							
							dependencies.player.rootPhysics.AddForce(desiredRunVelocity - new Vector3(dependencies.player.rootPhysics.velocity.x, 
							0f, dependencies.player.rootPhysics.velocity.z), ForceMode.VelocityChange);
							
							dependencies.procedural.stepDuration = dependencies.procedural.stepDurationRecord * 0.4f;
							dependencies.procedural.stepHeight = dependencies.procedural.stepHeightRecord * 3f;
						}
						
						else if(!dependencies.inputs.keyRun_Input && running)
						{
							running = false;
						}
					}
					
					//... Move by force (More physics based but less responsive)
					else
					{
						//... Calculate desired velocity
						Vector3 desiredMoveVelocity = moveDir * moveVelocity;
						Vector3 desiredRunVelocity = moveDir * runVelocity;

						//... Clamp the magnitude of desired move velocity
						if(desiredMoveVelocity.magnitude > maximumVelocity)
						{
							desiredMoveVelocity = desiredMoveVelocity.normalized * maximumVelocity;
							desiredMoveVelocity.y = 0f;
						}
						
						//... Clamp the magnitude of desired run velocity
						if(desiredRunVelocity.magnitude > maximumVelocity)
						{
							desiredRunVelocity = desiredRunVelocity.normalized * maximumVelocity;
							desiredRunVelocity.y = 0f;
						}
						
						
						//... Apply move velocity
						if(!dependencies.inputs.keyRun_Input && dependencies.state.Grounded() && moveDir != Vector3.zero)
						{
							if(running)
							{
								running = false;
							}
							
							dependencies.player.rootPhysics.AddForce(desiredMoveVelocity * 10f - new Vector3(dependencies.player.rootPhysics.velocity.x, 
							0f, dependencies.player.rootPhysics.velocity.z), ForceMode.Acceleration);
							
							dependencies.procedural.stepDuration = dependencies.procedural.stepDurationRecord;
							dependencies.procedural.stepHeight = dependencies.procedural.stepHeightRecord;
						}
						
						//... Apply move in air velocity
						else if(!dependencies.state.Grounded() && moveDir != Vector3.zero)
						{
							if(running)
							{
								running = false;
							}
							
							var inAirVelocity = desiredMoveVelocity * 1.2f;
							
							dependencies.player.rootPhysics.AddForce(inAirVelocity * 10f - new Vector3(dependencies.player.rootPhysics.velocity.x, 
							0f, dependencies.player.rootPhysics.velocity.z), ForceMode.Acceleration);
							
							dependencies.procedural.stepDuration = dependencies.procedural.stepDurationRecord;
							dependencies.procedural.stepHeight = dependencies.procedural.stepHeightRecord;
						}
						
						//... Apply run velocity
						else if(dependencies.inputs.keyRun_Input && dependencies.state.Grounded() && moveDir != Vector3.zero)
						{
							running = true;
							
							dependencies.player.rootPhysics.AddForce(desiredRunVelocity * 10f - new Vector3(dependencies.player.rootPhysics.velocity.x, 
							0f, dependencies.player.rootPhysics.velocity.z), ForceMode.Acceleration);
							
							dependencies.procedural.stepDuration = dependencies.procedural.stepDurationRecord * 0.4f;
							dependencies.procedural.stepHeight = dependencies.procedural.stepHeightRecord * 3f;
						}
						
						else if(!dependencies.inputs.keyRun_Input && running)
						{
							running = false;
						}
					}
				}
			}
		}
		
		
		
		//... Rotate ...
		void Rotate()
		{
			if(enableRotate)
			{
				//... Human input rotation
				if(!dependencies.inputs.simpleAI)
				{
					//... Record camera direction
					camDir = dependencies.playerCamera.followCamera.transform.forward;
					camDir.y = 0f;
					
					//... Rotate player in move direction
					if(moveDir.magnitude > 0f && dependencies.playerCamera.cameraRotation && !dependencies.weapons.shootingLeft && !dependencies.weapons.shootingRight)
					{
						rotation = Quaternion.LookRotation(moveDir, Vector3.up);
						dependencies.player.rootJoint.targetRotation = Quaternion.Slerp(dependencies.player.rootJoint.targetRotation, Quaternion.Inverse(rotation), turnSmoothness * Time.fixedDeltaTime);
					}
					
					//... Look in camera direction
					else if(dependencies.weapons.shootingLeft || dependencies.weapons.shootingRight)
					{
						rotation = Quaternion.LookRotation(camDir);
						dependencies.player.rootJoint.targetRotation = Quaternion.Slerp(dependencies.player.rootJoint.targetRotation, Quaternion.Inverse(rotation), turnSmoothness * Time.fixedDeltaTime);
					}
					
					//... Rotate player towards other player
					else if(headTracking  && dependencies.playerCamera.cameraRotation && dependencies.state.balanced && Vector3.Distance(dependencies.player.rootPhysics.transform.position, trackObject.position) > 1.5f)
					{
						if(!grabbingLeft && !grabbingRight && !dependencies.weapons.shootingLeft && !dependencies.weapons.shootingRight)
						{
							//... Look at other player
							var headTrackDir = headTracker.transform.forward;
							headTrackDir.y = 0f;
								
							rotation = Quaternion.LookRotation(headTrackDir);
							dependencies.player.rootJoint.targetRotation = Quaternion.Slerp(dependencies.player.rootJoint.targetRotation, Quaternion.Inverse(rotation), turnSmoothness * Time.fixedDeltaTime);
						}
					}
				}
				
				//... Rotate player with mouse input (if camera rotation is disabled & Human input)
				if(!dependencies.playerCamera.cameraRotation && !dependencies.inputs.simpleAI)
				{
					//... Spin player
					if(dependencies.inputs.keyLook_Input)
					{
						dependencies.player.rootJoint.targetRotation = Quaternion.Euler(dependencies.player.rootJoint.targetRotation.eulerAngles 
						+ new Vector3(0f, turnSmoothness * 1.1f * dependencies.inputs.mouse_Inputs.x * Time.fixedDeltaTime, 0f));
					}
					
					//... Rotate player in move direction
					else if(moveDir.magnitude > 0f && !dependencies.inputs.keyLook_Input)
					{
						rotation = Quaternion.LookRotation(moveDir, Vector3.up);
						dependencies.player.rootJoint.targetRotation = Quaternion.Slerp(dependencies.player.rootJoint.targetRotation, Quaternion.Inverse(rotation), turnSmoothness * Time.fixedDeltaTime);
					}
					
					//... Look at other player
					else if(headTracking && !grabbingLeft && !grabbingRight && dependencies.state.balanced)
					{
						var headTrackDir = headTracker.transform.forward;
						headTrackDir.y = 0f;
							
						rotation = Quaternion.LookRotation(headTrackDir);
						dependencies.player.rootJoint.targetRotation = Quaternion.Slerp(dependencies.player.rootJoint.targetRotation, Quaternion.Inverse(rotation), turnSmoothness * Time.fixedDeltaTime);
					}
				}
				
				//... Simple AI rotation example
				if(dependencies.inputs.simpleAI && headTracking && Vector3.Distance(dependencies.player.rootPhysics.transform.position, trackObject.position) > 1.5f)
				{
					//... Look at player
					var headTrackDir = headTracker.transform.forward;
					headTrackDir.y = 0f;
							
					rotation = Quaternion.LookRotation(headTrackDir);
					dependencies.player.rootJoint.targetRotation = Quaternion.Slerp(dependencies.player.rootJoint.targetRotation, Quaternion.Inverse(rotation), turnSmoothness * Time.fixedDeltaTime);
				}
			}
		}
		
		//... Jump ...
		void Jump()
		{
			if(enableJump)
			{
				//... Jump if grounded
				if(dependencies.state.Grounded())
				{
					if(dependencies.inputs.keyJump_Input && !jumping)
					{
						jumping = true;
						jumped = true;
						
						//... Audio
						if(dependencies.sound.soundSource != null)
						{
							dependencies.sound.soundToPlay = dependencies.sound.jumpSounds[Random.Range(0, dependencies.sound.jumpSounds.Length)];
							dependencies.sound.soundPoint = dependencies.player.headPhysics.transform.position;
							dependencies.sound.PlayAudio();
						}
					}
				}
				
				//... Apply up velocity
				if(jumping)
				{
					jumpTimer += 1 * Time.fixedDeltaTime;
					
					dependencies.player.rootPhysics.AddForce(Vector3.up * jumpVelocity, ForceMode.VelocityChange);
					
					if(dependencies.player.rootPhysics.velocity.y > maximumJumpVelocity)
					{
						dependencies.player.rootPhysics.velocity = new Vector3(dependencies.player.rootPhysics.velocity.x, maximumJumpVelocity, dependencies.player.rootPhysics.velocity.z);
					}
					
					if(dependencies.state.isKnockedOut)
					{
						jumping = false;
						jumpTimer = 0f;
					}
				}
				
				//... Reset after jump duration
				if(jumpTimer > jumpDuration)
				{
					jumping = false;
					jumpTimer = 0f;
				}
			}
		}
		
		
		
		//... Kneel ...
		void Kneel()
		{
			if(enableKneel)
			{
				if(dependencies.inputs.keyKneel_Input)
				{
					kneeling = true;
					
					//... Bend Legs
					dependencies.player.legLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.legLeftJoint.targetRotation, Quaternion.Euler(100f, 0f, 0f), 8f * Time.fixedDeltaTime);
					dependencies.player.footLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.footLeftJoint.targetRotation,  Quaternion.Euler(-130f, 0f, 0f), 8f * Time.fixedDeltaTime);
					
					dependencies.player.legRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.legRightJoint.targetRotation,  Quaternion.Euler(100f, 0f, 0f), 8f * Time.fixedDeltaTime);
					dependencies.player.footRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.footRightJoint.targetRotation,  Quaternion.Euler(-130f, 0f, 0f), 8f * Time.fixedDeltaTime);
				}
				
				else if(kneeling)
				{
					kneeling = false;
				}
			}
		}
		
		
		
		//... Setup Head Track ...
		void SetupHeadTrack()
		{
			//... Store head tracker start local rotation
			headTrackInversedRot = Quaternion.Inverse(headTracker.localRotation);
		}
		
		
		
		//... Head Tracking ...
		void HeadTracking()
		{
			if(enableHeadTracking)
			{
				//... Keep head tracker at head pposition
				headTracker.position = dependencies.player.headPhysics.transform.position;
				
				if(headTrackContainer.Count != 0)
				{
					//... Find closest target
					trackObject = headTrackContainer.OrderBy(t=> (t.position - dependencies.player.headPhysics.transform.position).sqrMagnitude).FirstOrDefault();
					
					//... Look at object when close enough
					if(Vector3.Distance(dependencies.player.headPhysics.transform.position, trackObject.position) < headTrackDistance && trackObject.gameObject.activeSelf)
					{
						headTracking = true;
						
						//... Look at target
						headTracker.transform.LookAt(trackObject.position);
						dependencies.player.headJoint.targetRotation = Quaternion.Slerp(dependencies.player.headJoint.targetRotation, Quaternion.Inverse(headTracker.localRotation) * headTrackInversedRot, lookAtSpeed * 10f * Time.fixedDeltaTime);
					}
						
					else
					{
						headTracking = false;
					}
				}
				
				else
				{
					headTracking = false;
				}
				
				
				//... Remove disabled track objects from order list
				if(headTrackContainer.Count != 0)
				{
					foreach(Transform trackPoint in headTrackContainer)
					{
						if(!trackPoint.gameObject.activeSelf && headTrackContainer.Contains(trackPoint))
						{
							headTrackContainer.Remove(trackPoint);
							break;
						}
					}
				}
			}
		}
		
		
		
		//... Player Reach ...
		void Reach()
		{
			if(enableReach)
			{
				//... Reach left arm
				if(dependencies.inputs.mouseLeft_input && !dependencies.weapons.weaponAssignedLeft)
				{
					if(dependencies.player.armLeftJoint.connectedBody != null && dependencies.player.handLeftJoint.connectedBody != null)
					{
						if(!reachingLeft)
						{
							reachingLeft = true;
							reaching = true;
							
							//... Set stronger joint drives
							dependencies.player.bodyLowerJoint.slerpDrive = dependencies.player.bodyPoseJointDrive;
							dependencies.player.bodyUpperJoint.slerpDrive = dependencies.player.bodyPoseJointDrive;
							
							dependencies.player.armLeftJoint.slerpDrive = dependencies.player.armsPoseJointDrive;
							dependencies.player.handLeftJoint.slerpDrive = dependencies.player.armsPoseJointDrive;
							
							dependencies.procedural.targetRotationLeft = Quaternion.Inverse(Quaternion.Euler(new Vector3(-90f, 280f, 0f)));
						}
						
						//... Rotate arms with input
						dependencies.procedural.targetRotationLeft *= Quaternion.Euler(new Vector3(1f, 0f, 0f) * dependencies.inputs.mouse_Inputs.y * reachSensitivity * Time.fixedDeltaTime);
						dependencies.player.armLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.armLeftJoint.targetRotation, dependencies.procedural.targetRotationLeft, 10f * Time.fixedDeltaTime);
						dependencies.player.handLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.handLeftJoint.targetRotation, Quaternion.Euler(15f, 0f, 0f), 10f * Time.fixedDeltaTime);
					}
				}
				
				//... Rest left arm
				else
				{
					if(reachingLeft)
					{
						reachingLeft = false;
						reaching = false;
						
						//... Reset joint drives
						dependencies.player.bodyLowerJoint.slerpDrive = dependencies.player.bodyLowerJointDrive;
						dependencies.player.bodyUpperJoint.slerpDrive = dependencies.player.bodyUpperJointDrive;
						
						dependencies.player.armLeftJoint.slerpDrive = dependencies.player.armLeftJointDrive;
						dependencies.player.handLeftJoint.slerpDrive = dependencies.player.handLeftJointDrive;
					}
				}
				
				//... Reach right arm
				if(dependencies.inputs.mouseRight_input && !dependencies.weapons.weaponAssignedRight)
				{
					if(dependencies.player.armRightJoint.connectedBody != null && dependencies.player.handRightJoint.connectedBody != null)
					{
						if(!reachingRight)
						{
							reachingRight = true;
							reaching = true;
							
							//... Set stronger joint drives
							dependencies.player.bodyLowerJoint.slerpDrive = dependencies.player.bodyPoseJointDrive;
							dependencies.player.bodyUpperJoint.slerpDrive = dependencies.player.bodyPoseJointDrive;
							
							dependencies.player.armRightJoint.slerpDrive = dependencies.player.armsPoseJointDrive;
							dependencies.player.handRightJoint.slerpDrive = dependencies.player.armsPoseJointDrive;
							
							dependencies.procedural.targetRotationRight = Quaternion.Inverse(Quaternion.Euler(new Vector3(-90f, 80f, 0f)));
						}
						
						//... Rotate arms with input
						dependencies.procedural.targetRotationRight *= Quaternion.Euler(new Vector3(1f, 0f, 0f) * dependencies.inputs.mouse_Inputs.y * reachSensitivity * Time.fixedDeltaTime);
						dependencies.player.armRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.armRightJoint.targetRotation, dependencies.procedural.targetRotationRight, 10f * Time.fixedDeltaTime);
						dependencies.player.handRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.handRightJoint.targetRotation, Quaternion.Euler(15f, 0f, 0f), 10f * Time.fixedDeltaTime);
					}
				}
				
				//... Rest right arm
				else
				{
					if(reachingRight)
					{
						reachingRight = false;
						reaching = false;
						
						//... Reset joint drives
						dependencies.player.bodyLowerJoint.slerpDrive = dependencies.player.bodyLowerJointDrive;
						dependencies.player.bodyUpperJoint.slerpDrive = dependencies.player.bodyUpperJointDrive;
						
						dependencies.player.armRightJoint.slerpDrive = dependencies.player.armRightJointDrive;
						dependencies.player.handRightJoint.slerpDrive = dependencies.player.handRightJointDrive;
					}
				}
				
				
				
				//... Bend Body & Legs
				if(reaching)
				{
					//... Body bend
					dependencies.procedural.bendTargetRotation *= Quaternion.Euler(new Vector3(1f, 0f, 0f) * dependencies.inputs.mouse_Inputs.y * reachSensitivity * Time.fixedDeltaTime);
					dependencies.player.bodyLowerJoint.targetRotation = Quaternion.Slerp(dependencies.player.bodyLowerJoint.targetRotation, dependencies.procedural.bendTargetRotation, 8f * Time.fixedDeltaTime);
					dependencies.player.bodyUpperJoint.targetRotation = Quaternion.Slerp(dependencies.player.bodyUpperJoint.targetRotation, dependencies.procedural.bendTargetRotation, 8f * Time.fixedDeltaTime);
				}
				
				//... Rest body
				else if(!reaching && !punching)
				{
					dependencies.procedural.bendTargetRotation = dependencies.procedural.bodyUpperTarget;
				}
			}
		}
		
		
		
		//... Grab ...
		void Grab()
		{
			if(enableGrab)
			{
				//... Grab left hand
				if(dependencies.state.Grounded() && reachingLeft && !dependencies.weapons.weaponAssignedLeft)
				{
					if(grabDetectLeft.detectedObject != null && dependencies.player.handLeftJoint.connectedBody != null)
					{
						if(grabDetectLeft.collisionDetected && !grabbingLeft)
						{
							grabbingLeft = true;
							
							//... Create joint connection
							grabDetectLeft.connectedJointLeft = grabDetectLeft.gameObject.AddComponent<ConfigurableJoint>();
							grabDetectLeft.connectedJointLeft.connectedBody = grabDetectLeft.detectedObject;
							
							//... Set joint properties
							grabDetectLeft.connectedJointLeft.xMotion = ConfigurableJointMotion.Locked;
							grabDetectLeft.connectedJointLeft.yMotion = ConfigurableJointMotion.Locked;
							grabDetectLeft.connectedJointLeft.zMotion = ConfigurableJointMotion.Locked;
							
							grabDetectLeft.connectedJointLeft.rotationDriveMode = RotationDriveMode.Slerp;
							grabDetectLeft.connectedJointLeft.slerpDrive = dependencies.player.grabJointDrive;
							
							//... Check if other player was grabbed
							if(grabDetectLeft.detectedObject.transform.root.gameObject.GetComponent<PX_Dependencies>() != null)
							{
								otherPlayerPartGrabLeft = grabDetectLeft.detectedObject.GetComponent<Rigidbody>();
								
								//... Collect all other player parts
								grabLeftOtherPlayerPhysics = grabDetectLeft.detectedObject.transform.root.transform.Find("Player_Container").GetComponentsInChildren<Rigidbody>();
								
								//... Off balance the other player
								otherPlayerPartGrabLeft.transform.root.gameObject.GetComponent<PX_Dependencies>().state.isBeingPickedUp = true;
								
								//... Allow root to rotate freely
								grabDetectLeft.detectedObject.transform.root.transform.Find("Player_Container/Root").GetComponent<ConfigurableJoint>().slerpDrive = dependencies.player.noJointDrive;
							}
						}
						
						if(grabbingLeft)
						{
							if(otherPlayerPartGrabLeft != null && grabLeftOtherPlayerPhysics != null)
							{
								//... Help lift other players
								otherPlayerPartGrabLeft.AddForce(Vector3.up * grabForce, ForceMode.VelocityChange);
								
								foreach(Rigidbody part in grabLeftOtherPlayerPhysics)
								{
									if(part.GetComponent<PX_ImpactDetect>())
									{
										part.AddForce(Vector3.up * (grabForce / 3), ForceMode.VelocityChange);
									}
								}
								
								//... Mount player to ground when lifting other players
								dependencies.player.footLeftPhysics.AddForce(-Vector3.up * grabForce * 2f, ForceMode.VelocityChange);
								dependencies.player.footRightPhysics.AddForce(-Vector3.up * grabForce * 2f, ForceMode.VelocityChange);
							}
						}
					}
				}
				
				//... Release left hand
				if(grabbingLeft)
				{
					if(!dependencies.inputs.mouseLeft_input || dependencies.state.isKnockedOut || !dependencies.state.isAlive || dependencies.weapons.weaponAssignedLeft)
					{
						grabbingLeft = false;
						
						if(grabDetectLeft.connectedJointLeft != null)
						{
							grabDetectLeft.detectedObject = null;
							grabDetectLeft.collisionDetected = false;
							Destroy(grabDetectLeft.connectedJointLeft);
							grabDetectLeft.connectedJointLeft = null;
						}
					}
				}
				
				//... Reset other player grabbed with left hand
				if(!grabbingLeft)
				{
					if(otherPlayerPartGrabLeft != null && grabLeftOtherPlayerPhysics != null)
					{
						//... Allow other player to get up again
						if(otherPlayerPartGrabLeft.transform.root.gameObject.GetComponent<PX_Dependencies>() != null)
						{
							otherPlayerPartGrabLeft.transform.root.gameObject.GetComponent<PX_Dependencies>().state.isBeingPickedUp = false;
						}
						
						otherPlayerPartGrabLeft = null;
						grabLeftOtherPlayerPhysics = null;
					}
				}
				
				
				
				//... Grab right hand
				if(dependencies.state.Grounded() && reachingRight && !dependencies.weapons.weaponAssignedRight)
				{
					if(grabDetectRight.detectedObject != null && dependencies.player.handRightJoint.connectedBody != null)
					{
						if(grabDetectRight.collisionDetected && !grabbingRight)
						{
							grabbingRight = true;
							
							//... Create joint connection
							grabDetectRight.connectedJointRight = grabDetectRight.gameObject.AddComponent<ConfigurableJoint>();
							grabDetectRight.connectedJointRight.connectedBody = grabDetectRight.detectedObject;
							
							//... Set joint properties
							grabDetectRight.connectedJointRight.xMotion = ConfigurableJointMotion.Locked;
							grabDetectRight.connectedJointRight.yMotion = ConfigurableJointMotion.Locked;
							grabDetectRight.connectedJointRight.zMotion = ConfigurableJointMotion.Locked;
							
							grabDetectRight.connectedJointRight.rotationDriveMode = RotationDriveMode.Slerp;
							grabDetectRight.connectedJointRight.slerpDrive = dependencies.player.grabJointDrive;
							
							//... Check if other player was grabbed
							if(grabDetectRight.detectedObject.transform.root.gameObject.GetComponent<PX_Dependencies>() != null)
							{
								otherPlayerPartGrabRight = grabDetectRight.detectedObject.GetComponent<Rigidbody>();
								
								//... Collect all other player parts
								grabRightOtherPlayerPhysics = grabDetectRight.detectedObject.transform.root.transform.Find("Player_Container").GetComponentsInChildren<Rigidbody>();
								
								//... Off balance the other player
								otherPlayerPartGrabRight.transform.root.gameObject.GetComponent<PX_Dependencies>().state.isBeingPickedUp = true;
								
								//... Allow root to rotate freely
								grabDetectRight.detectedObject.transform.root.transform.Find("Player_Container/Root").GetComponent<ConfigurableJoint>().slerpDrive = dependencies.player.noJointDrive;
							}
						}
						
						if(grabbingRight)
						{
							if(otherPlayerPartGrabRight != null && grabRightOtherPlayerPhysics != null)
							{
								//... Help lift other players
								otherPlayerPartGrabRight.AddForce(Vector3.up * grabForce, ForceMode.VelocityChange);
								
								foreach(Rigidbody part in grabRightOtherPlayerPhysics)
								{
									if(part.GetComponent<PX_ImpactDetect>())
									{
										part.AddForce(Vector3.up * (grabForce / 3), ForceMode.VelocityChange);
									}
								}
								
								//... Mount player to ground when lifting other players
								dependencies.player.footLeftPhysics.AddForce(-Vector3.up * grabForce * 2f, ForceMode.VelocityChange);
								dependencies.player.footRightPhysics.AddForce(-Vector3.up * grabForce * 2f, ForceMode.VelocityChange);
							}
						}
					}
				}
				
				//... Release right hand
				if(grabbingRight)
				{
					if(!dependencies.inputs.mouseRight_input || dependencies.state.isKnockedOut || !dependencies.state.isAlive || dependencies.weapons.weaponAssignedRight)
					{
						
						grabbingRight = false;
						
						if(grabDetectRight.connectedJointRight != null)
						{
							grabDetectRight.detectedObject = null;
							grabDetectRight.collisionDetected = false;
							Destroy(grabDetectRight.connectedJointRight);
							grabDetectRight.connectedJointRight = null;
						}
					}
				}
				
				//... Reset other player grabbed with right hand
				if(!grabbingRight)
				{
					if(otherPlayerPartGrabRight != null && grabRightOtherPlayerPhysics != null)
					{
						//... Allow other player to get up again
						if(otherPlayerPartGrabRight.transform.root.gameObject.GetComponent<PX_Dependencies>() != null)
						{
							otherPlayerPartGrabRight.transform.root.gameObject.GetComponent<PX_Dependencies>().state.isBeingPickedUp = false;
						}
						
						otherPlayerPartGrabRight = null;
						grabRightOtherPlayerPhysics = null;
					}
				}
				
				
				//... Keep other player off balance when grabbed with both hands
				if(grabbingLeft && otherPlayerPartGrabLeft != null && otherPlayerPartGrabLeft.transform.root.gameObject.GetComponent<PX_Dependencies>()
				&& grabbingRight && otherPlayerPartGrabRight != null && otherPlayerPartGrabRight.transform.root.gameObject.GetComponent<PX_Dependencies>())
				{
					otherPlayerPartGrabLeft.transform.root.gameObject.GetComponent<PX_Dependencies>().state.isBeingPickedUp = true;
				}
			}
		}
		
		
		
		//... Punch ...
	    void Punch()
	    {
			if(enablePunch)
			{
				//... Punch left
				if(dependencies.inputs.keyPunchLeft_Input)
				{
					if(dependencies.player.armLeftJoint.connectedBody != null && dependencies.player.handLeftJoint.connectedBody != null && !dependencies.weapons.shootingLeft)
					{
						punching = true;
						
						if(!punchingLeft)
						{
							punchingLeft = true;
							
							//... Set stronger joint drives
							dependencies.player.armLeftJoint.slerpDrive = dependencies.player.armsPoseJointDrive;
							dependencies.player.handLeftJoint.slerpDrive = dependencies.player.armsPoseJointDrive;
						}
						
						//... Ramp punch force to allow soft or hard punches
						punchLeftRamp = Mathf.Lerp(punchLeftRamp, dependencies.weapons.equipLeft.attackForce * 100f, 2.5f * Time.fixedDeltaTime);
						
						//... Twist body
						dependencies.player.bodyUpperJoint.targetRotation = Quaternion.Slerp(dependencies.player.bodyUpperJoint.targetRotation, Quaternion.Euler(0f, 30f, 0f), 20 * Time.fixedDeltaTime);
						
						//... Left hand punch pull back pose
						dependencies.player.armLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.armLeftJoint.targetRotation, Quaternion.Euler(30f, 15f, 0f), 15 * Time.fixedDeltaTime);
						dependencies.player.handLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.handLeftJoint.targetRotation, Quaternion.Euler(120f, 0f, 0f), 15 * Time.fixedDeltaTime);
					}
				}
				
				if(punchingLeft && !dependencies.inputs.keyPunchLeft_Input)
				{
					punchingLeft = false;
					
					//... Release arms from joint drive
					dependencies.player.armLeftJoint.slerpDrive = dependencies.player.noJointDrive;
					dependencies.player.handLeftJoint.slerpDrive = dependencies.player.noJointDrive;
					

					//... Aim if target assigned
					if(headTracking)
					{
						var punchDir = (trackObject.position - dependencies.weapons.equipLeft.attackPoint.position).normalized;
								
						//... Left hand punch force in target direction
						dependencies.weapons.equipLeft.weaponPhysics.AddForceAtPosition(punchDir * punchLeftRamp, dependencies.weapons.equipLeft.attackPoint.position, ForceMode.Impulse);
					}
					
					//... Punch forward
					else
					{
						//... Left hand punch force forward direction
						dependencies.weapons.equipLeft.weaponPhysics.AddForceAtPosition(dependencies.player.rootPhysics.transform.forward * punchLeftRamp, dependencies.weapons.equipLeft.attackPoint.position, ForceMode.Impulse);
					}
					
					punching = false;
					punchLeftRamp = 0f;
					
					//... Audio
					if(dependencies.sound.soundSource != null)
					{
						dependencies.sound.soundToPlay = dependencies.weapons.equipLeft.attackSounds[Random.Range(0, dependencies.weapons.equipLeft.attackSounds.Length)];
						dependencies.sound.soundPoint = dependencies.weapons.equipLeft.transform.position;
						dependencies.sound.PlayAudio();
					}
					
					//... Restore joint drive
					Invoke(nameof(ResetPunchLeft), 0.2f);
				}
				
				
				
				//... Punch right
				if(dependencies.inputs.keyPunchRight_Input)
				{
					if(dependencies.player.armRightJoint.connectedBody != null && dependencies.player.handRightJoint.connectedBody != null && !dependencies.weapons.shootingRight)
					{
						punching = true;
						
						if(!punchingRight)
						{
							punchingRight = true;
							
							//... Set stronger joint drives
							dependencies.player.armRightJoint.slerpDrive = dependencies.player.armsPoseJointDrive;
							dependencies.player.handRightJoint.slerpDrive = dependencies.player.armsPoseJointDrive;
						}
						
						//... Ramp punch force to allow soft or hard punches
						punchRightRamp = Mathf.Lerp(punchRightRamp, dependencies.weapons.equipRight.attackForce * 100f, 2.5f * Time.fixedDeltaTime);
						
						//... Twist body
						dependencies.player.bodyUpperJoint.targetRotation = Quaternion.Slerp(dependencies.player.bodyUpperJoint.targetRotation, Quaternion.Euler(0f, -30f, 0f), 20 * Time.fixedDeltaTime);
						
						//... Right hand punch pull back pose
						dependencies.player.armRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.armRightJoint.targetRotation, Quaternion.Euler(30f, -15f, 0f), 15 * Time.fixedDeltaTime);
						dependencies.player.handRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.handRightJoint.targetRotation, Quaternion.Euler(120f, 0f, 0f), 15 * Time.fixedDeltaTime);
					}
				}
				
				if(punchingRight && !dependencies.inputs.keyPunchRight_Input)
				{
					punchingRight = false;
					
					//... Release arms from joint drive
					dependencies.player.armRightJoint.slerpDrive = dependencies.player.noJointDrive;
					dependencies.player.handRightJoint.slerpDrive = dependencies.player.noJointDrive;
					
					//... Aim if target assigned
					if(headTracking)
					{
						var punchDir = (trackObject.position - dependencies.weapons.equipRight.attackPoint.position).normalized;
								
						//... Right hand punch force target direction
						dependencies.weapons.equipRight.weaponPhysics.AddForceAtPosition(punchDir * punchRightRamp, dependencies.weapons.equipRight.attackPoint.position, ForceMode.Impulse);
					}
							
					//... Punch forward
					else
					{
						//... Right hand punch force forward direction
						dependencies.weapons.equipRight.weaponPhysics.AddForceAtPosition(dependencies.player.rootPhysics.transform.forward * punchRightRamp, dependencies.weapons.equipRight.attackPoint.position, ForceMode.Impulse);
					}
					
					punching = false;
					punchRightRamp = 0f;
					
					//... Audio
					if(dependencies.sound.soundSource != null)
					{
						dependencies.sound.soundToPlay = dependencies.weapons.equipRight.attackSounds[Random.Range(0, dependencies.weapons.equipRight.attackSounds.Length)];
						dependencies.sound.soundPoint = dependencies.weapons.equipRight.transform.position;
						dependencies.sound.PlayAudio();
					}
					
					//... Restore joint drive
					Invoke(nameof(ResetPunchRight), 0.2f);
				}
			}
	    }
		
		
		
		//... Reset Punch ...
		void ResetPunchLeft()
		{
			if(!dependencies.inputs.keyPunchLeft_Input)
			{
				dependencies.player.armLeftJoint.slerpDrive = dependencies.player.armLeftJointDrive;
				dependencies.player.handLeftJoint.slerpDrive = dependencies.player.handLeftJointDrive;
			}
		}
		
		
		void ResetPunchRight()
		{
			if(!dependencies.inputs.keyPunchRight_Input)
			{
				dependencies.player.armRightJoint.slerpDrive = dependencies.player.armRightJointDrive;
				dependencies.player.handRightJoint.slerpDrive = dependencies.player.handRightJointDrive;
			}
		}
		
		
		
		//... Kick ...
	    void Kick()
	    {
			if(enableKick)
			{
				//... Kick left
				if((dependencies.inputs.keyKickLeft_Input && !kickingRight && dependencies.state.Grounded()) || (dependencies.inputs.keyKickLeft_Input && !dependencies.state.Grounded()))
				{
					if(dependencies.player.legLeftJoint.connectedBody != null && dependencies.player.footLeftJoint.connectedBody != null)
					{
						kicking = true;
						
						if(!kickingLeft)
						{
							kickingLeft = true;
						}
						
						//... Ramp kick force to allow soft or hard punches
						kickLeftRamp = Mathf.Lerp(kickLeftRamp, kickForce * 100f, 2.5f * Time.fixedDeltaTime);
						
						//... Left leg kick pull back pose
						dependencies.player.legLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.legLeftJoint.targetRotation, Quaternion.Euler(120f, 0f, 0f), 15f * Time.fixedDeltaTime);
						dependencies.player.footLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.footLeftJoint.targetRotation, Quaternion.Euler(-90f, 0f, 0f), 12f * Time.fixedDeltaTime);
						
						if(dependencies.state.Grounded() && dependencies.player.legRightJoint.connectedBody != null)
						{
							//... Support player balance
							dependencies.player.rootPhysics.AddForce(Vector3.up * supportForce * 100f, ForceMode.Force);
						}
					}
				}
				
				if(kickingLeft && !dependencies.inputs.keyKickLeft_Input)
				{
					kickingLeft = false;
					
					//... Release legs from joint drive
					dependencies.player.legLeftJoint.slerpDrive = dependencies.player.noJointDrive;
					dependencies.player.footLeftJoint.slerpDrive = dependencies.player.noJointDrive;

					//... Aim if target assigned
					if(headTracking && !dependencies.state.Grounded())
					{
						var kickDir = (trackObject.position - leftShoe.transform.position).normalized;
						
						//... Left leg kick force in target direction
						leftShoe.AddForce(kickDir * kickLeftRamp, ForceMode.VelocityChange);
						
						if(dependencies.state.Grounded() && dependencies.player.footLeftPhysics.gameObject.activeSelf && dependencies.player.footRightPhysics.gameObject.activeSelf)
						{
							dependencies.player.rootPhysics.AddForce(Vector3.up / 3f * kickLeftRamp, ForceMode.Impulse);
						}
					}
					
					//... Kick forward
					else
					{
						//... Left leg kick forward direction
						leftShoe.AddForce(dependencies.player.rootPhysics.transform.forward * kickLeftRamp, ForceMode.VelocityChange);
						
						//... Left leg kick up direction
						if(dependencies.state.Grounded() && dependencies.player.legLeftJoint.connectedBody != null)
						{
							dependencies.player.rootPhysics.AddForce(Vector3.up / 3f * kickLeftRamp, ForceMode.Impulse);
						}
					}
					
					kicking = false;
					kickLeftRamp = 0;
					
					//... Audio
					if(dependencies.sound.soundSource != null)
					{
						dependencies.sound.soundToPlay = dependencies.sound.attackSounds[Random.Range(0, dependencies.sound.attackSounds.Length)];
						dependencies.sound.soundPoint = dependencies.player.footLeftPhysics.transform.position;
						dependencies.sound.PlayAudio();
					}
					
					//... Restore joint drive
					Invoke(nameof(ResetKickLeft), 0.3f);
				}
				
				
				
				//... Kick right
				if((dependencies.inputs.keyKickRight_Input && !kickingLeft && dependencies.state.Grounded()) || (dependencies.inputs.keyKickRight_Input && !dependencies.state.Grounded()))
				{
					if(dependencies.player.legRightJoint.connectedBody != null && dependencies.player.footRightJoint.connectedBody != null)
					{
						kicking = true;
						
						if(!kickingRight)
						{
							kickingRight = true;
						}
						
						//... Ramp kick force to allow soft or hard punches
						kickRightRamp = Mathf.Lerp(kickRightRamp, kickForce * 100f, 2.5f * Time.fixedDeltaTime);
						
						//... Set joint drive
						dependencies.player.legRightJoint.slerpDrive = dependencies.player.legRightJointDrive;
						dependencies.player.footRightJoint.slerpDrive = dependencies.player.footRightJointDrive;
						
						//... Right leg kick pull back pose
						dependencies.player.legRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.legRightJoint.targetRotation, Quaternion.Euler(120f, 0f, 0f), 15 * Time.fixedDeltaTime);
						dependencies.player.footRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.footRightJoint.targetRotation, Quaternion.Euler(-90f, 0f, 0f), 12 * Time.fixedDeltaTime);
						
						if(dependencies.state.Grounded() && dependencies.player.footLeftPhysics.gameObject.activeSelf && dependencies.player.footRightPhysics.gameObject.activeSelf)
						{
							//... Support player balance
							dependencies.player.rootPhysics.AddForce(Vector3.up * supportForce * 100f, ForceMode.Force);
						}
					}
				}
				
				if(kickingRight && !dependencies.inputs.keyKickRight_Input)
				{
					kickingRight = false;
					
					//... Release legs from joint drive
					dependencies.player.legRightJoint.slerpDrive = dependencies.player.noJointDrive;
					dependencies.player.footRightJoint.slerpDrive = dependencies.player.noJointDrive;
					
					//... Aim if target assigned
					if(headTracking && !dependencies.state.Grounded())
					{
						var kickDir = (trackObject.position - rightShoe.transform.position).normalized;
						
						//... Right leg kick force target direction
						rightShoe.AddForce(kickDir * kickRightRamp, ForceMode.VelocityChange);
						
						if(dependencies.state.Grounded())
						{
							dependencies.player.rootPhysics.AddForce(Vector3.up / 3f * kickRightRamp, ForceMode.Impulse);
						}
					}
							
					//... Kick forward
					else
					{
						//... Right leg kick force forward direction
						rightShoe.AddForce(dependencies.player.rootPhysics.transform.forward * kickRightRamp, ForceMode.VelocityChange);
						
						//... Right leg kick force up direction
						if(dependencies.state.Grounded())
						{
							dependencies.player.rootPhysics.AddForce(Vector3.up / 3f * kickRightRamp, ForceMode.Impulse);
						}
					}
					
					kicking = false;
					kickRightRamp = 0;
					
					//... Audio
					if(dependencies.sound.soundSource != null)
					{
						dependencies.sound.soundToPlay = dependencies.sound.attackSounds[Random.Range(0, dependencies.sound.attackSounds.Length)];
						dependencies.sound.soundPoint = dependencies.player.footRightPhysics.transform.position;
						dependencies.sound.PlayAudio();
					}
					
					//... Restore joint drive
					Invoke(nameof(ResetKickRight), 0.3f);
				}
			}
	    }
		
		//... Reset Kick ...
		void ResetKickLeft()
		{
			dependencies.player.legLeftJoint.slerpDrive = dependencies.player.legLeftJointDrive;
			dependencies.player.footLeftJoint.slerpDrive = dependencies.player.footLeftJointDrive;
			
			dependencies.player.legLeftJoint.targetRotation = dependencies.procedural.legLeftTarget;
			dependencies.player.footLeftJoint.targetRotation = dependencies.procedural.footLeftTarget;
		}
		
		void ResetKickRight()
		{
			dependencies.player.legRightJoint.slerpDrive = dependencies.player.legRightJointDrive;
			dependencies.player.footRightJoint.slerpDrive = dependencies.player.footRightJointDrive;
			
			dependencies.player.legRightJoint.targetRotation = dependencies.procedural.legRightTarget;
			dependencies.player.footRightJoint.targetRotation = dependencies.procedural.footRightTarget;
		}
	}
}
