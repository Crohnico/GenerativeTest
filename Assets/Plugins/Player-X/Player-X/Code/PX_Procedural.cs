//------------
//... PLayer-X
//... V2.0.1
//... © TheFamousMouse™
//--------------------
//... Support email:
//... thefamousmouse.developer@gmail.com
//--------------------------------------

using UnityEngine;
using PlayerX;

namespace PlayerX
{
	public class PX_Procedural : MonoBehaviour
	{
		[Header("Player-X [Procedural]")]
		
		[Space]
		
		[Header("- Procedural Dependencies")]
		public PX_Dependencies dependencies;
		
		
		//... Walk
		[Header("Walk")]
		public float stepDuration = 0.27f;
		public float stepHeight = 0.18f;
		
		//- Hidden walk variables
		[HideInInspector]
		public float
		stepDurationRecord,
		stepHeightRecord;
		
		//...
		float 
		stepLeftTimer,
		stepRightTimer;
		
		//...
		bool
	    walk,
		StepRight, 
		StepLeft,
		stoppedRunning;
		
		
		//... Step Prediction
		[Header("- Step Prediction")]
		public bool enableStepPrediction;
		public float offBalanceLeanDistance = 0.25f;
		public float pushUpLeanDistance = 0.3f;
		public Transform centerMassPoint;
		public Transform centerCheckPoint;
		
		
		[Header("- Revert")]
		public float restorePoseSpeed = 20f;
		
		
		//... Other Hidden Variables
		
		//...
		[HideInInspector]
		public Quaternion rootTarget, headTarget,
		bodyLowerTarget, bodyUpperTarget,
		armRightTarget, handRightTarget,
		armLeftTarget, handLeftTarget,
		legRightTarget, footRightTarget,
		legLeftTarget, footLeftTarget;
		
		//...
		[HideInInspector]
		public Quaternion
		bendTargetRotation, 
		targetRotationLeft, 
		targetRotationRight;
		
		//...
		Vector3
		centerOfMassPoint;
		
		//...
		bool 
		offBalance = false;
		
		
		
		void Start()
		{
			Records();
		}
		
		void Update()
		{
			CenterOfMass();
		}
		
	    void FixedUpdate()
	    {
			if(dependencies.state.isAlive && !dependencies.state.isKnockedOut)
			{
				StepPrediction();
				Balance();
				Walk();
				Run();
				
				Jumping();
				InAir();
				
				RestorePlayerPose();
			}
	    }
		
		
		
		//... Records ...
		void Records()
		{
			//... Record prefered step info
			stepDurationRecord = stepDuration;
			stepHeightRecord = stepHeight;
			
			//... Body target rotation
			rootTarget = dependencies.player.rootJoint.targetRotation;
			bodyLowerTarget = dependencies.player.bodyLowerJoint.targetRotation;
			bodyUpperTarget = dependencies.player.bodyUpperJoint.targetRotation;
			bendTargetRotation = dependencies.player.bodyUpperJoint.targetRotation;
			headTarget = dependencies.player.headJoint.targetRotation;
			
			//... Arms target rotation
			armLeftTarget = dependencies.player.armLeftJoint.targetRotation;
			handLeftTarget = dependencies.player.handLeftJoint.targetRotation;
			armRightTarget = dependencies.player.armRightJoint.targetRotation;
			handRightTarget = dependencies.player.handRightJoint.targetRotation;
			
			//... legs target rotation
	        legLeftTarget = dependencies.player.legLeftJoint.targetRotation;
	        footLeftTarget = dependencies.player.footLeftJoint.targetRotation;
			legRightTarget = dependencies.player.legRightJoint.targetRotation;
	        footRightTarget = dependencies.player.footRightJoint.targetRotation;
		}
		
		
		
		//... Step Prediction ...
		void StepPrediction()
		{
			//... Reset variables if not walking
			if(!walk && !offBalance)
	        {
	            StepRight = false;
	            StepLeft = false;
	            stepRightTimer = 0;
	            stepLeftTimer = 0;
	        }

	        //... Walk with input
			if(dependencies.inputs.key_Inputs != Vector2.zero)
	        {
	            walk = true;
	        }
			
			//... Walk with step prediction
	        else if(enableStepPrediction && offBalance && dependencies.inputs.key_Inputs == Vector2.zero && Vector3.Distance(centerMassPoint.position, 
			new Vector3(centerCheckPoint.position.x, centerMassPoint.position.y, centerCheckPoint.position.z)) >= offBalanceLeanDistance)
	        {
	            walk = true;
	        }
			
			//... Stop walk
	        else
	        {
				walk = false;
	        }
			
			//... Simple AI walk example
			if(dependencies.inputs.simpleAI && dependencies.controller.moveDir != Vector3.zero)
			{
				walk = true;
			}
		}
		
		
		
		//...Balance ...
		void Balance()
		{
			//... Find center between length
			var centerLength = dependencies.player.rootPhysics.transform.position - dependencies.player.headPhysics.transform.position;
			centerCheckPoint.position = dependencies.player.rootPhysics.transform.position - (0.5f * centerLength);
			
			
			//... Push self away from falling direction
			if(dependencies.controller.moveDir == Vector3.zero && dependencies.state.Grounded() && !dependencies.controller.jumping)
			{
				//... Increase step height and step duration to help push back balance
				if(offBalance && !dependencies.inputs.keyRun_Input)
				{
					if(centerMassPoint.localPosition.z > centerCheckPoint.localPosition.z)
					{
						stepDuration = stepDurationRecord * 0.7f;
						stepHeight = stepHeightRecord * 1.2f;
					}
					
					else if(centerMassPoint.localPosition.z < centerCheckPoint.localPosition.z)
					{
						stepDuration = stepDurationRecord * 0.7f;
						stepHeight = stepHeightRecord * 1.2f;
					}
				}
				
				if(!dependencies.inputs.keyRun_Input && !dependencies.controller.reaching && !dependencies.controller.kneeling && !dependencies.controller.punching)
				{
					//... Correct slight of balance with tilting
					if(Vector3.Distance(centerMassPoint.position, new Vector3(centerCheckPoint.position.x, centerMassPoint.position.y, centerCheckPoint.position.z)) >= offBalanceLeanDistance
					&& Vector3.Distance(centerMassPoint.position, new Vector3(centerCheckPoint.position.x, centerMassPoint.position.y, centerCheckPoint.position.z)) <= pushUpLeanDistance)
					{
						//... When falling backward
						if(centerMassPoint.localPosition.z > centerCheckPoint.localPosition.z)
						{
							//... Body tilt forward
							dependencies.player.bodyLowerJoint.targetRotation = Quaternion.Slerp(dependencies.player.bodyLowerJoint.targetRotation, Quaternion.Euler(-60f, 0f, 0f), 3f * Time.fixedDeltaTime);
							dependencies.player.bodyUpperJoint.targetRotation = Quaternion.Slerp(dependencies.player.bodyUpperJoint.targetRotation, Quaternion.Euler(-30f, 0f, 0f), 3f * Time.fixedDeltaTime);
							
							//... Head tilt forward
							dependencies.player.headJoint.targetRotation = Quaternion.Slerp(dependencies.player.headJoint.targetRotation, Quaternion.Euler(-50f, 0f, 0f), 5f * Time.fixedDeltaTime);
						}
						
						//... When falling forward
						else if(centerMassPoint.localPosition.z < centerCheckPoint.localPosition.z)
						{
							//... Body tilt backward
							dependencies.player.bodyLowerJoint.targetRotation = Quaternion.Slerp(dependencies.player.bodyLowerJoint.targetRotation, Quaternion.Euler(60f, 0f, 0f), 3f * Time.fixedDeltaTime);
							dependencies.player.bodyUpperJoint.targetRotation = Quaternion.Slerp(dependencies.player.bodyUpperJoint.targetRotation, Quaternion.Euler(30f, 0f, 0f), 3f * Time.fixedDeltaTime);
							
							//... Head tilt backward
							dependencies.player.headJoint.targetRotation = Quaternion.Slerp(dependencies.player.headJoint.targetRotation, Quaternion.Euler(50f, 0f, 0f), 5f * Time.fixedDeltaTime);
						}
					}
					
					//... Correct the balance with push and bend
					else if(Vector3.Distance(centerMassPoint.position, new Vector3(centerCheckPoint.position.x, centerMassPoint.position.y, centerCheckPoint.position.z)) >= pushUpLeanDistance)
					{
						offBalance = true;
						
						//... When falling backward
						if(centerMassPoint.localPosition.z > centerCheckPoint.localPosition.z - 0.25f && !dependencies.controller.punching)
						{
							//... Body bend forward
							dependencies.player.bodyLowerJoint.targetRotation = Quaternion.Slerp(dependencies.player.bodyLowerJoint.targetRotation, Quaternion.Euler(-30f, 0f, 0f), 12f * Time.fixedDeltaTime);
							dependencies.player.bodyUpperJoint.targetRotation = Quaternion.Slerp(dependencies.player.bodyUpperJoint.targetRotation, Quaternion.Euler(-30f, 0f, 0f), 8f * Time.fixedDeltaTime);
							
							//... Head tilt backward
							dependencies.player.headJoint.targetRotation = Quaternion.Slerp(dependencies.player.headJoint.targetRotation, Quaternion.Euler(-50f, 0f, 0f), 8f * Time.fixedDeltaTime);
							
							if(!dependencies.controller.reaching && !dependencies.weapons.shootingLeft)
							{
								//... Arm left push forward
								dependencies.player.armLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.armLeftJoint.targetRotation, Quaternion.Inverse(Quaternion.Euler(90f, 15f, 5f)), 15f * Time.fixedDeltaTime);
								dependencies.player.handLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.handLeftJoint.targetRotation, Quaternion.Euler(15f, 0f, 0f), 10f * Time.fixedDeltaTime);
							}
							
							if(!dependencies.controller.reaching && !dependencies.weapons.shootingRight)
							{
								//... Arm right push forward
								dependencies.player.armRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.armRightJoint.targetRotation, Quaternion.Inverse(Quaternion.Euler(90f, -15f, -5f)), 15f * Time.fixedDeltaTime);
								dependencies.player.handRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.handRightJoint.targetRotation, Quaternion.Euler(15f, 0f, 0f), 10f * Time.fixedDeltaTime);
							}
						}
						
						//... When falling forward
						else if(centerMassPoint.localPosition.z < centerCheckPoint.localPosition.z && !dependencies.controller.punching)
						{
							//... Body bend forward
							dependencies.player.bodyLowerJoint.targetRotation = Quaternion.Slerp(dependencies.player.bodyLowerJoint.targetRotation, Quaternion.Euler(-60f, 0f, 0f), 12f * Time.fixedDeltaTime);
							dependencies.player.bodyUpperJoint.targetRotation = Quaternion.Slerp(dependencies.player.bodyUpperJoint.targetRotation, Quaternion.Euler(-30f, 0f, 0f), 8f * Time.fixedDeltaTime);
							
							//... Head tilt forward
							dependencies.player.headJoint.targetRotation = Quaternion.Slerp(dependencies.player.headJoint.targetRotation, Quaternion.Euler(50f, 0f, 0f), 8f * Time.fixedDeltaTime);
							
							if(!dependencies.controller.reaching && !dependencies.weapons.shootingLeft)
							{
								//... Arm left push backward
								dependencies.player.armLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.armLeftJoint.targetRotation, Quaternion.Inverse(Quaternion.Euler(-90f, 288f, 0f)), 15f * Time.fixedDeltaTime);
								dependencies.player.handLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.handLeftJoint.targetRotation, Quaternion.Euler(10f, 0f, 0f), 10f * Time.fixedDeltaTime);
							}
							
							if(!dependencies.controller.reaching && !dependencies.weapons.shootingRight)
							{
								//... Arm right push backward
								dependencies.player.armRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.armRightJoint.targetRotation, Quaternion.Inverse(Quaternion.Euler(-90f, 88f, 0f)), 15f * Time.fixedDeltaTime);
								dependencies.player.handRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.handRightJoint.targetRotation, Quaternion.Euler(10f, 0f, 0f), 10f * Time.fixedDeltaTime);
							}
						}
					}
					
					//... restore step variables when balanced
					else if(offBalance && !dependencies.controller.running)
					{
						stepDuration = stepDurationRecord;
						stepHeight = stepHeightRecord;
							
						offBalance = false;
					}
				}
			}
			
			else
			{
				offBalance = false;
			}
		}
		
		
		
		//... Walk ...
		void Walk()
		{
	        if(dependencies.state.Grounded() && !dependencies.controller.jumping && !dependencies.controller.kicking)
	        {
				//... Check which leg to walk with
	            if(walk)
				{
	                //... Right leg
					if(dependencies.player.footRightPhysics.transform.localPosition.z < dependencies.player.footLeftPhysics.transform.localPosition.z && !StepLeft)
					{
						StepRight = true;
					}
	                
	                //... Left leg
					if(dependencies.player.footRightPhysics.transform.localPosition.z > dependencies.player.footLeftPhysics.transform.localPosition.z && !StepRight)
					{
						StepLeft = true;
					}
				}
				
				
				//... Step right
				if(StepRight)
				{
					stepRightTimer += Time.fixedDeltaTime;
				
					//... Walk right leg simulation
					if(walk)
					{
						dependencies.player.legRightJoint.targetRotation = new Quaternion(dependencies.player.legRightJoint.targetRotation.x + 0.09f * stepHeight, dependencies.player.legRightJoint.targetRotation.y, dependencies.player.legRightJoint.targetRotation.z, dependencies.player.legRightJoint.targetRotation.w);
						dependencies.player.footRightJoint.targetRotation = new Quaternion(dependencies.player.footRightJoint.targetRotation.x + 0.06f * stepHeight * 2, dependencies.player.footRightJoint.targetRotation.y, dependencies.player.footRightJoint.targetRotation.z, dependencies.player.footRightJoint.targetRotation.w);
							
						dependencies.player.legLeftJoint.targetRotation = new Quaternion(dependencies.player.legLeftJoint.targetRotation.x - 0.12f * stepHeight / 2, dependencies.player.legLeftJoint.targetRotation.y, dependencies.player.legLeftJoint.targetRotation.z, dependencies.player.legLeftJoint.targetRotation.w);
					}
	                
	                
					//... Step right duration
					if(stepRightTimer > stepDuration)
					{
						stepRightTimer = 0;
						StepRight = false;
						
						if(walk)
						{
							StepLeft = true;
						}
					}
				}
				
				//... Reset right leg to start rotation
				else
				{
					if(walk)
					{
						if(!dependencies.inputs.keyRun_Input)
						{
							//... Reset legs (Walk)
							dependencies.player.legRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.legRightJoint.targetRotation, legRightTarget.normalized * Quaternion.Euler(-20f, 0f, -10f), 8f * Time.fixedDeltaTime);
							dependencies.player.footRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.footRightJoint.targetRotation, footRightTarget.normalized * Quaternion.Euler(0f, 0f, -10f), 18f * Time.fixedDeltaTime);
						}
						
						else
						{
							//... Reset legs (Run)
							dependencies.player.legRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.legRightJoint.targetRotation, Quaternion.Euler(-20f, 0f, -10f), 8f * Time.fixedDeltaTime);
							dependencies.player.footRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.footRightJoint.targetRotation, Quaternion.Euler(0f, 0f, -10f), 18f * Time.fixedDeltaTime);
						}	
					}
				}
	            
	            
	            //... Step left
				if(StepLeft)
				{
					stepLeftTimer += Time.fixedDeltaTime;
	                
	                //... Walk left leg simulation
					if(walk)
					{
						dependencies.player.legLeftJoint.targetRotation = new Quaternion(dependencies.player.legLeftJoint.targetRotation.x + 0.09f * stepHeight, dependencies.player.legLeftJoint.targetRotation.y, dependencies.player.legLeftJoint.targetRotation.z, dependencies.player.legLeftJoint.targetRotation.w);
						dependencies.player.footLeftJoint.targetRotation = new Quaternion(dependencies.player.footLeftJoint.targetRotation.x + 0.06f * stepHeight * 2, dependencies.player.footLeftJoint.targetRotation.y, dependencies.player.footLeftJoint.targetRotation.z, dependencies.player.footLeftJoint.targetRotation.w);
							
						dependencies.player.legRightJoint.targetRotation = new Quaternion(dependencies.player.legRightJoint.targetRotation.x - 0.12f * stepHeight / 2, dependencies.player.legRightJoint.targetRotation.y, dependencies.player.legRightJoint.targetRotation.z, dependencies.player.legRightJoint.targetRotation.w);
					}
				
				
					//... Step left duration
					if(stepLeftTimer > stepDuration)
					{
						stepLeftTimer = 0;
						StepLeft = false;
						
						if(walk)
						{
							StepRight = true;
						}
					}
					
				}
				
				//... Reset left leg to start rotation
				else
				{
					if(walk)
					{
						if(!dependencies.inputs.keyRun_Input)
						{
							//... Reset legs (Walk)
							dependencies.player.legLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.legLeftJoint.targetRotation, legLeftTarget.normalized * Quaternion.Euler(-20f, 0f, 10f), 8f * Time.fixedDeltaTime);
							dependencies.player.footLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.footLeftJoint.targetRotation, footLeftTarget.normalized * Quaternion.Euler(0f, 0f, 10f), 18f * Time.fixedDeltaTime);
						}
						
						else
						{
							//... Reset legs (Run)
							dependencies.player.legLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.legLeftJoint.targetRotation, Quaternion.Euler(-20f, 0f, 10f), 8f * Time.fixedDeltaTime);
							dependencies.player.footLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.footLeftJoint.targetRotation, Quaternion.Euler(0f, 0f, 10f), 18f * Time.fixedDeltaTime);
						}
					}
				}
			}
		}
		
		
		
		//... Run ...
		void Run()
		{
			if(walk)
			{
				//... Run pose
				if(dependencies.inputs.keyRun_Input)
				{
					stoppedRunning = false;
					
					//... Body bend forward run pose
					dependencies.player.bodyLowerJoint.targetRotation = Quaternion.Slerp(dependencies.player.bodyLowerJoint.targetRotation, Quaternion.Euler(-30f, 0f, 0f), 15f * Time.fixedDeltaTime);
					dependencies.player.bodyUpperJoint.targetRotation = Quaternion.Slerp(dependencies.player.bodyUpperJoint.targetRotation, Quaternion.Euler(-60f, 0f, 0f), 8f * Time.fixedDeltaTime);
					
					//... Head tilt backward run pose
					dependencies.player.headJoint.targetRotation = Quaternion.Slerp(dependencies.player.headJoint.targetRotation, Quaternion.Euler(-15f, 0f, 0f), 8f * Time.fixedDeltaTime);
					
					if(!dependencies.controller.reaching && !dependencies.controller.punching && !dependencies.weapons.shootingLeft)
					{
						//... Check if still a connected joint before applying pose drive
						if(dependencies.player.armLeftJoint.connectedBody != null && dependencies.player.handLeftJoint.connectedBody != null)
						{
							dependencies.player.armLeftJoint.slerpDrive = dependencies.player.armsPoseJointDrive;
							dependencies.player.handLeftJoint.slerpDrive = dependencies.player.armsPoseJointDrive;
						}
						
						//... Arm left extend backward run pose
						dependencies.player.armLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.armLeftJoint.targetRotation, Quaternion.Euler(-60f, 0f, 0f), 8f * Time.fixedDeltaTime);
						dependencies.player.handLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.handLeftJoint.targetRotation, Quaternion.Euler(0f, 0f, 0f), 5f * Time.fixedDeltaTime);
					}
					
					if(!dependencies.controller.reaching && !dependencies.controller.punching && !dependencies.weapons.shootingRight)
					{
						//... Check if still a connected joint before applying pose drive
						if(dependencies.player.armRightJoint.connectedBody != null && dependencies.player.handRightJoint.connectedBody != null)
						{
							dependencies.player.armRightJoint.slerpDrive = dependencies.player.armsPoseJointDrive;
							dependencies.player.handRightJoint.slerpDrive = dependencies.player.armsPoseJointDrive;
						}
						
						//... Arm right extend backward run pose
						dependencies.player.armRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.armRightJoint.targetRotation, Quaternion.Euler(-60f,0f,0f), 8f * Time.fixedDeltaTime);
						dependencies.player.handRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.handRightJoint.targetRotation, Quaternion.Euler(0f, 0f, 0f), 5f * Time.fixedDeltaTime);
					}
				}
				
				if(!dependencies.inputs.keyRun_Input && !stoppedRunning)
				{
					stoppedRunning = true;
					
					//... Revert arm joint drives
					dependencies.player.armLeftJoint.slerpDrive = dependencies.player.armLeftJointDrive;
					dependencies.player.handLeftJoint.slerpDrive = dependencies.player.handLeftJointDrive;
						
					dependencies.player.armRightJoint.slerpDrive = dependencies.player.armRightJointDrive;
					dependencies.player.handRightJoint.slerpDrive = dependencies.player.handRightJointDrive;
				}
			}
		}
		
		
		
		//... Jumping ...
		void Jumping()
		{
			if(dependencies.controller.jumping)
			{
				//... Body pose jump
				dependencies.player.bodyLowerJoint.targetRotation = Quaternion.Slerp(dependencies.player.bodyLowerJoint.targetRotation, Quaternion.Euler(60f, 0f, 0f), 18f * Time.fixedDeltaTime);
				dependencies.player.bodyUpperJoint.targetRotation = Quaternion.Slerp(dependencies.player.bodyUpperJoint.targetRotation, Quaternion.Euler(30f, 0f, 0f), 10f * Time.fixedDeltaTime);
				
				if(!dependencies.controller.reaching && !dependencies.controller.punching)
				{
					//... Arm left pose jump
					if(!dependencies.weapons.shootingLeft)
					{
						dependencies.player.armLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.armLeftJoint.targetRotation, Quaternion.Euler(-60f, 0f, 15f), 10f * Time.fixedDeltaTime);
						dependencies.player.handLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.handLeftJoint.targetRotation, Quaternion.Euler(60f, 0f, 0f), 8f * Time.fixedDeltaTime);
					}
					
					//... Arm right pose jump
					if(!dependencies.weapons.shootingRight)
					{
						dependencies.player.armRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.armRightJoint.targetRotation, Quaternion.Euler(-60f, 0f, -15f), 10f * Time.fixedDeltaTime);
						dependencies.player.handRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.handRightJoint.targetRotation, Quaternion.Euler(60f, 0f, 0f), 8f * Time.fixedDeltaTime);
					}
				}
				
				if(!dependencies.controller.kicking)
				{
					//... Legs pose
					dependencies.player.legLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.legLeftJoint.targetRotation, legLeftTarget * Quaternion.Euler(-30f, 0f, 0f), 20f * Time.fixedDeltaTime);
					dependencies.player.footLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.footLeftJoint.targetRotation, Quaternion.Euler(-120f, 0f, 0f), 18f * Time.fixedDeltaTime);
					
					dependencies.player.legRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.legRightJoint.targetRotation, legRightTarget * Quaternion.Euler(-30f, 0f, 0f), 20f * Time.fixedDeltaTime);
					dependencies.player.footRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.footRightJoint.targetRotation, Quaternion.Euler(-120f, 0f, 0f), 18f * Time.fixedDeltaTime);
				}
			}
		}
		
		
		
		//... In Air ...
		void InAir()
		{
			if(!dependencies.state.Grounded() && !dependencies.controller.jumping)
			{
				if(!offBalance && !dependencies.controller.reaching && !dependencies.controller.punching)
				{
					//... Arm left pose in air
					if(!dependencies.weapons.shootingLeft)
					{
						dependencies.player.armLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.armLeftJoint.targetRotation, Quaternion.Inverse(Quaternion.Euler(0f, -60f, 0f)), 8f * Time.fixedDeltaTime);
						dependencies.player.handLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.handLeftJoint.targetRotation, Quaternion.Euler(60f, 0f, 0f), 5f * Time.fixedDeltaTime);
					}
					
					//... Arm right pose in air
					if(!dependencies.weapons.shootingRight)
					{
						dependencies.player.armRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.armRightJoint.targetRotation, Quaternion.Inverse(Quaternion.Euler(0f, 60f, 0f)), 8f * Time.fixedDeltaTime);
						dependencies.player.handRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.handRightJoint.targetRotation, Quaternion.Euler(60f, 0f, 0f), 5f * Time.fixedDeltaTime);
					}
				}
				
				if(dependencies.player.rootPhysics.velocity.y > 0)
				{
					//... Body pose in air
					dependencies.player.bodyLowerJoint.targetRotation = Quaternion.Slerp(dependencies.player.bodyLowerJoint.targetRotation, Quaternion.Euler(-15f, 0f, 0f), 8f * Time.fixedDeltaTime);
					dependencies.player.bodyUpperJoint.targetRotation = Quaternion.Slerp(dependencies.player.bodyUpperJoint.targetRotation, Quaternion.Euler(-15f, 0f, 0f), 8f * Time.fixedDeltaTime);
					
					if(!dependencies.controller.kicking)
					{
						//... Legs pose in air
						dependencies.player.legLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.legLeftJoint.targetRotation, Quaternion.Euler(90f, 30f, 30f), 15f * Time.fixedDeltaTime);
						dependencies.player.footLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.footLeftJoint.targetRotation, Quaternion.Euler(-80f, -90f, 90f), 8f * Time.fixedDeltaTime);
						
						dependencies.player.legRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.legRightJoint.targetRotation, Quaternion.Euler(90f, -30f, -30f), 15f * Time.fixedDeltaTime);
						dependencies.player.footRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.footRightJoint.targetRotation, Quaternion.Euler(-80f, 90f, -90f), 8f * Time.fixedDeltaTime);
					}
				}
				
				else if(dependencies.player.rootPhysics.velocity.y < 0)
				{		
					//... Body pose in air
					dependencies.player.bodyLowerJoint.targetRotation = Quaternion.Slerp(dependencies.player.bodyLowerJoint.targetRotation, Quaternion.Euler(-30f, 0f, 0f), 8f * Time.fixedDeltaTime);
					dependencies.player.bodyUpperJoint.targetRotation = Quaternion.Slerp(dependencies.player.bodyUpperJoint.targetRotation, Quaternion.Euler(-30f, 0f, 0f), 8f * Time.fixedDeltaTime);
					
					if(!dependencies.controller.kicking)
					{
						//... Legs pose in air
						dependencies.player.legLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.legLeftJoint.targetRotation, legLeftTarget.normalized * Quaternion.Euler(90f, 5f, 0f), 15f * Time.fixedDeltaTime);
						dependencies.player.footLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.footLeftJoint.targetRotation, footLeftTarget.normalized * Quaternion.Euler(-90f, 0f, 0f), 8f * Time.fixedDeltaTime);
						
						dependencies.player.legRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.legRightJoint.targetRotation, legRightTarget.normalized * Quaternion.Euler(90f, -5f, 0f), 15f * Time.fixedDeltaTime);
						dependencies.player.footRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.footRightJoint.targetRotation, footRightTarget.normalized * Quaternion.Euler(-90f, 0f, 0f), 8f * Time.fixedDeltaTime);
					}
				}
			}
		}
		
		
		
		//... Restore Player Pose...
		void RestorePlayerPose()
		{
			//... Body restore
			if(dependencies.state.active && dependencies.state.Grounded() && !dependencies.controller.running && !dependencies.controller.jumping && !offBalance && !dependencies.controller.reaching && !dependencies.controller.punching)
			{
				dependencies.player.bodyLowerJoint.targetRotation = Quaternion.Slerp(dependencies.player.bodyLowerJoint.targetRotation, bodyLowerTarget, restorePoseSpeed * Time.fixedDeltaTime);
				dependencies.player.bodyUpperJoint.targetRotation = Quaternion.Slerp(dependencies.player.bodyUpperJoint.targetRotation, bodyUpperTarget, restorePoseSpeed * Time.fixedDeltaTime);
			}
			
			//... Head Restore
			if(dependencies.state.active && dependencies.state.Grounded() && !dependencies.controller.headTracking && !offBalance && !dependencies.controller.running)
			{
				dependencies.player.headJoint.targetRotation = Quaternion.Slerp(dependencies.player.headJoint.targetRotation, headTarget, restorePoseSpeed * Time.fixedDeltaTime);
			}
			
			//... Left arm restore
			if(dependencies.state.active && dependencies.state.Grounded() && !dependencies.controller.running && !dependencies.controller.jumping && !offBalance && !dependencies.controller.reachingLeft && !dependencies.controller.punching && !dependencies.weapons.shootingLeft)
			{
				dependencies.player.armLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.armLeftJoint.targetRotation, armLeftTarget, restorePoseSpeed * Time.fixedDeltaTime);
				dependencies.player.handLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.handLeftJoint.targetRotation, handLeftTarget, restorePoseSpeed * Time.fixedDeltaTime);
			}
			
			//... Right arm restore
			if(dependencies.state.active && dependencies.state.Grounded() && !dependencies.controller.running && !dependencies.controller.jumping && !offBalance && !dependencies.controller.reachingRight && !dependencies.controller.punching && !dependencies.weapons.shootingRight)
			{
				dependencies.player.armRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.armRightJoint.targetRotation, armRightTarget, restorePoseSpeed * Time.fixedDeltaTime);
				dependencies.player.handRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.handRightJoint.targetRotation, handRightTarget, restorePoseSpeed * Time.fixedDeltaTime);
			}
			
			//... Left leg restore
			if(dependencies.state.active && !dependencies.state.isKnockedOut && dependencies.state.Grounded() && !walk && !dependencies.controller.kneeling && !dependencies.controller.jumping && !dependencies.controller.kicking)
			{
				dependencies.player.legLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.legLeftJoint.targetRotation, legLeftTarget, restorePoseSpeed * Time.fixedDeltaTime);
				dependencies.player.footLeftJoint.targetRotation = Quaternion.Slerp(dependencies.player.footLeftJoint.targetRotation, footLeftTarget, restorePoseSpeed * Time.fixedDeltaTime);
			}
			
			//... Right leg restore
			if(dependencies.state.active && !dependencies.state.isKnockedOut && dependencies.state.Grounded() && !walk && !dependencies.controller.kneeling && !dependencies.controller.jumping && !dependencies.controller.kicking)
			{
				dependencies.player.legRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.legRightJoint.targetRotation, legRightTarget, restorePoseSpeed * Time.fixedDeltaTime);
				dependencies.player.footRightJoint.targetRotation = Quaternion.Slerp(dependencies.player.footRightJoint.targetRotation, footRightTarget, restorePoseSpeed * Time.fixedDeltaTime);
			}
		}
		
		
		
		//... Center of mass ...
		void CenterOfMass()
		{		
			if(enableStepPrediction)
			{
				//... Calculate parts based on whether or not dismembered
				//.......................................................
				
				//... Root cannot be dismembered so keeps its own value
				var rootMass = dependencies.player.rootPhysics.mass;
				var rootPos = dependencies.player.rootPhysics.transform.position;
				
				
				//... Body Lower
				var bodyLowerMass = 0f;
				var bodyLowerPos = Vector3.zero;
				
				if(dependencies.player.bodyLowerJoint.connectedBody != null)
				{
					bodyLowerMass = dependencies.player.bodyLowerPhysics.mass;
					bodyLowerPos = dependencies.player.bodyLowerPhysics.transform.position;
				}
				
				else
				{
					bodyLowerMass = 0f;
					bodyLowerPos = centerMassPoint.position;
				}
				
				
				//... Body Upper
				var bodyUpperMass = 0f;
				var bodyUpperPos = Vector3.zero;
				
				if(dependencies.player.bodyUpperJoint.connectedBody != null)
				{
					bodyUpperMass = dependencies.player.bodyUpperPhysics.mass;
					bodyUpperPos = dependencies.player.bodyUpperPhysics.transform.position;
				}
				
				else
				{
					bodyUpperMass = 0f;
					bodyUpperPos = centerMassPoint.position;
				}
				
				
				//... Head
				var headMass = 0f;
				var headPos = Vector3.zero;
				
				if(dependencies.player.headJoint.connectedBody != null)
				{
					headMass = dependencies.player.headPhysics.mass;
					headPos = dependencies.player.headPhysics.transform.position;
				}
				
				else
				{
					headMass = 0f;
					headPos = centerMassPoint.position;
				}
				
				
				//... Arm Left
				var armLeftMass = 0f;
				var armLeftPos = Vector3.zero;
				
				if(dependencies.player.armLeftJoint.connectedBody != null)
				{
					armLeftMass = dependencies.player.armLeftPhysics.mass;
					armLeftPos = dependencies.player.armLeftPhysics.transform.position;
				}
				
				else
				{
					armLeftMass = 0f;
					armLeftPos = centerMassPoint.position;
				}
				
				
				//... Arm Right
				var armRightMass = 0f;
				var armRightPos = Vector3.zero;
				
				if(dependencies.player.armRightJoint.connectedBody != null)
				{
					armRightMass = dependencies.player.armRightPhysics.mass;
					armRightPos = dependencies.player.armRightPhysics.transform.position;
				}
				
				else
				{
					armRightMass = 0f;
					armRightPos = centerMassPoint.position;
				}
				
				
				//... Hand Left
				var handLeftMass = 0f;
				var handLeftPos = Vector3.zero;
				
				if(dependencies.player.handLeftJoint.connectedBody != null && dependencies.player.armLeftJoint.connectedBody != null)
				{
					handLeftMass = dependencies.player.handLeftPhysics.mass;
					handLeftPos = dependencies.player.handLeftPhysics.transform.position;
				}
				
				else
				{
					handLeftMass = 0f;
					handLeftPos = centerMassPoint.position;
				}
				
				
				//... Hand Right
				var handRightMass = 0f;
				var handRightPos = Vector3.zero;
				
				if(dependencies.player.handRightJoint.connectedBody != null && dependencies.player.armRightJoint.connectedBody != null)
				{
					handRightMass = dependencies.player.handRightPhysics.mass;
					handRightPos = dependencies.player.handRightPhysics.transform.position;
				}
				
				else
				{
					handRightMass = 0f;
					handRightPos = centerMassPoint.position;
				}
				
				
				//... Leg Left
				var legLeftMass = 0f;
				var legLeftPos = Vector3.zero;
				
				if(dependencies.player.legLeftJoint.connectedBody != null)
				{
					legLeftMass = dependencies.player.legLeftPhysics.mass;
					legLeftPos = dependencies.player.legLeftPhysics.transform.position;
				}
				
				else
				{
					legLeftMass = 0f;
					legLeftPos = centerMassPoint.position;
					
					enableStepPrediction = false;
				}
				
				
				//... Leg Right
				var legRightMass = 0f;
				var legRightPos = Vector3.zero;
				
				if(dependencies.player.legRightJoint.connectedBody != null)
				{
					legRightMass = dependencies.player.legRightPhysics.mass;
					legRightPos = dependencies.player.legRightPhysics.transform.position;
				}
				
				else
				{
					legRightMass = 0f;
					legRightPos = centerMassPoint.position;
					
					enableStepPrediction = false;
				}
				
				
				//... Foot Left
				var footLeftMass = 0f;
				var footLeftPos = Vector3.zero;
				
				if(dependencies.player.footLeftJoint.connectedBody != null && dependencies.player.legLeftJoint.connectedBody != null)
				{
					footLeftMass = dependencies.player.footLeftPhysics.mass;
					footLeftPos = dependencies.player.footLeftPhysics.transform.position;
				}
				
				else
				{
					footLeftMass = 0f;
					footLeftPos = centerMassPoint.position;
					
					enableStepPrediction = false;
				}
				
				
				//... Foot Right
				var footRightMass = 0f;
				var footRightPos = Vector3.zero;
				
				if(dependencies.player.footRightJoint.connectedBody != null && dependencies.player.legRightJoint.connectedBody != null)
				{
					footRightMass = dependencies.player.footRightPhysics.mass;
					footRightPos = dependencies.player.footRightPhysics.transform.position;
				}
				
				else
				{
					footRightMass = 0f;
					footRightPos = centerMassPoint.position;
					
					enableStepPrediction = false;
				}
				
				
				//... Center mass of all parts
				centerOfMassPoint =
				
				(rootMass * rootPos + 
	            bodyLowerMass * bodyLowerPos +
	            bodyUpperMass * bodyUpperPos +
	            headMass * headPos +
	            armLeftMass * armLeftPos +
	            armRightMass * armRightPos +
	            handLeftMass * handLeftPos +
				handRightMass * handRightPos +
				legLeftMass * legLeftPos +
				legRightMass * legRightPos +
				footLeftMass * footLeftPos +
				footRightMass * footRightPos) 
	            
	            /
				
	            (rootMass + bodyLowerMass +
	            bodyUpperMass + headMass +
	            armLeftMass + armRightMass +
	            handLeftMass + handRightMass +
				legLeftMass + legRightMass +
				footLeftMass + footRightMass);
				
				centerMassPoint.position = centerOfMassPoint;
			}
		}
		
		
		
		//... Debug Info ...
		void OnDrawGizmos()
		{
			//... Center Of Mass Debug ...
			if(enableStepPrediction)
			{
				//... Center of mass position sphere
				Gizmos.color = Color.cyan;
				Gizmos.DrawWireSphere(centerOfMassPoint, 0.3f);
			}
			
			//... Center check position sphere
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(centerCheckPoint.position, 0.1f);
		}
	}
}