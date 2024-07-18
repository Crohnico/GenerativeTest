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
	public class PX_State : MonoBehaviour
	{
		[Header("Player-X [State]")]
		
		[Space]
		
		[Header("- State Dependencies")]
		public PX_Dependencies dependencies;
		
		
		[Header("- Ground Check")]
		public float groundCheckHeight = 1.8f;
		public float groundCheckRadius = 0.2f;
		public LayerMask groundLayer;
		
		//- Hidden ground check variables
		RaycastHit hitGround;
		
		
		[Header("- Balance")]
		public float balanceRecoverySpeed = 100f;
		
		//- Hidden balance variables
		[HideInInspector]
		public bool 
		active = true,
		balanced = true,
		isGrounded = true;
		
		[Header("- Impact")]
		public float reactForce = 150f;
		public float reactRequiredForce = 35f;
		
		[Header("- Knock Out")]
		public float impactRequiredKo = 150f;
		public float KnockReactForce = 120f;
		public float ragdollModeDuration = 2f;
		public float recoverySpeed = 30f;
		
		//- Hidden knocked out variables
		[HideInInspector]
		public bool isKnockedOut = false;
		
		//...
		[HideInInspector]
		public GameObject knockedOutStars;
		
		//...
		[HideInInspector]
		public Vector3 
		impactDir;
		
		//...
		[HideInInspector]
		public float springRamp;
		[HideInInspector]
		public float impactForce;
		float rootSpringRecord;
		float springRampTemp = 0f;
		float ragdollTimer = 0f;
		
		//...
		[HideInInspector]
		public Rigidbody impactedPart;
		
		//...
		[HideInInspector]
		public bool isAlive = true,
		isBeingPickedUp = false;
		
		
		[Header("- Impact & KO Particles")]
		public Transform particleContainer;
		public GameObject impactParticle;
		public GameObject knockedOutParticle;
		public GameObject koStarsParticle;
		
		[Header("- Death Screen")]
		public GameObject deathScreen;
		
		//... Debug ...
		Vector3 debugRaycastGroundPoint;
		Vector3 debugRaycastWallPoint;
		
		
		
		//... Functions
		void Start()
		{
			SpringSetup();
		}
		
		void FixedUpdate()
		{
			Balance();
			
			if(isAlive)
			{
				ActiveMode();
			}
		}
		
		void LateUpdate()
		{
			if(!isAlive)
			{
				DeathToStars();
				DeathScreen();
			}
		}
		
		
		
		//... Setup spring ...
		void SpringSetup()
		{
			rootSpringRecord = dependencies.player.rootJointDrive.positionSpring;
			springRamp = rootSpringRecord;
		}
		
		
		
		//... Grounded ...
		public bool Grounded()
		{
			if(Physics.SphereCast(dependencies.player.rootPhysics.transform.position, groundCheckRadius, -Vector3.up, out hitGround, groundCheckHeight, groundLayer))
			{
				return(true);
			}
			
			else
			{
				return(false);
			}
		}
		
		
		
		//... Balance ...
		void Balance()
		{
			isGrounded = Grounded();
			
			//... Check if other player grabbed this player with both hands
			if(isBeingPickedUp)
			{
				//... Keep off balance
				if(active)
				{
					active = false;
					balanced = false;
					springRamp = 0f;
				}
			}
			
			//... Grounded
			if(isGrounded)
			{
				//... Gain balance if grounded
				if(!active && !balanced && !isKnockedOut)
				{
					active = true;
				}
				
				//... Count till ragdoll mode duration if not active
				else if(!active && isKnockedOut && ragdollTimer < ragdollModeDuration)
				{
					ragdollTimer += 1f * Time.fixedDeltaTime;
				}
				
				//... Set back to active when ragdoll mode ends 
				else if(!active && isKnockedOut && ragdollTimer > ragdollModeDuration)
				{
					ragdollTimer = 0;
					active = true;
				}
			}
			
			//... Not Grounded
			else if(!isGrounded)
			{
				//... Lose balance when not grounded
				if(dependencies.player.rootPhysics.velocity.y < 0f)
				{
					dependencies.player.rootJoint.slerpDrive = dependencies.player.noJointDrive;
					
					springRamp = 0;
					active = false;
					balanced = false;
				}
			}
		}
		
		
		
		//... Damaged ...
		public void Damaged()
		{
			//... Apply impact force
			impactedPart.AddForce(impactDir * impactForce, ForceMode.Impulse);
			
			//... Hit particle
			var hitParticle = Instantiate(impactParticle, impactedPart.transform.position, Quaternion.identity);
			hitParticle.transform.parent = particleContainer;
		}
		
		
		
		//... Knocked Out ...
		public void KnockedOut()
		{
			//... Apply impact force
			dependencies.player.headPhysics.AddForce(impactDir * KnockReactForce, ForceMode.Impulse);
			dependencies.player.bodyUpperPhysics.AddForce(impactDir * (KnockReactForce / 2), ForceMode.Impulse);
			
			//... Blood particle
			var koParticle = Instantiate(knockedOutParticle, dependencies.player.headPhysics.transform.position, Quaternion.identity);
			koParticle.transform.parent = particleContainer;
			
			if(!isKnockedOut)
			{
				//... Remove KO Stars if any already
				if(knockedOutStars != null)
				{
					Destroy(knockedOutStars);
				}
				
				//... Create and set KO stars
				if(isAlive)
				{
					knockedOutStars = Instantiate(koStarsParticle, dependencies.player.headPhysics.transform.position, Quaternion.identity);
					knockedOutStars.transform.parent = dependencies.player.headPhysics.transform;
				}
			}
			
			RagdollMode();
		}
		
		
		
		//... Death To Stars ...
		void DeathToStars()
		{
			//... Remove KO Stars if knockedout before death
			if(knockedOutStars != null)
			{
				Destroy(knockedOutStars);
			}
		}
		
		
		
		//... Active Mode ...
		public void ActiveMode()
		{
			if(active)
			{
				//... Slowly restore root joint drive when grounded
				if(springRamp < rootSpringRecord)
				{
					//... Recover speed ramp if IS knocked out and NO input
					if(isKnockedOut && dependencies.inputs.key_Inputs == Vector2.zero)
					{
						springRamp += Mathf.Lerp(0f, recoverySpeed * 1000f, 1f * Time.fixedDeltaTime);
					}
					
					//... Recover speed ramp if NOT knocked out and NO input
					else if(!isKnockedOut && dependencies.inputs.key_Inputs == Vector2.zero)
					{
						springRamp += Mathf.Lerp(0f, balanceRecoverySpeed / 3 * 1000f, 1f * Time.fixedDeltaTime);
					}
					
					//... Recover speed ramp if NOT knocked out and RECEIVE input
					else
					{
						springRamp += Mathf.Lerp(0f, balanceRecoverySpeed * 1000f, 1f * Time.fixedDeltaTime);
					}
					
					springRampTemp = springRamp;
					
					//... Root stiffen
					dependencies.player.rootJointDrive.positionSpring = springRampTemp;
					dependencies.player.rootJointDrive.positionDamper = 500f;
					dependencies.player.rootJoint.slerpDrive = dependencies.player.rootJointDrive;
					
					if(isKnockedOut)
					{
						if(dependencies.player.legLeftJoint.gameObject.transform.parent.gameObject != dependencies.dismember.dismemberContainer.gameObject)
						{
							//... Upper legs stiffen - Left
							dependencies.player.legLeftJoint.slerpDrive = dependencies.player.rootJointDrive;
							dependencies.player.legLeftJoint.targetRotation = dependencies.procedural.legLeftTarget.normalized * Quaternion.Euler(0, 10 ,30);
						}
						
						if(dependencies.player.footLeftJoint.gameObject.transform.parent.gameObject != dependencies.dismember.dismemberContainer.gameObject)
						{
							//... lower legs stiffen - Left
							dependencies.player.footLeftJoint.slerpDrive = dependencies.player.rootJointDrive;
							dependencies.player.footLeftJoint.targetRotation = dependencies.procedural.footLeftTarget.normalized * Quaternion.Euler(-100, 10 ,0);
						}
						
						if(dependencies.player.legRightJoint.gameObject.transform.parent.gameObject != dependencies.dismember.dismemberContainer.gameObject)
						{
							//... Upper legs stiffen - Right
							dependencies.player.legRightJoint.slerpDrive = dependencies.player.rootJointDrive;
							dependencies.player.legRightJoint.targetRotation = dependencies.procedural.legRightTarget.normalized * Quaternion.Euler(0, -10 ,-30);
						}
						
						if(dependencies.player.footRightJoint.gameObject.transform.parent.gameObject != dependencies.dismember.dismemberContainer.gameObject)
						{
							//... lower legs stiffen - Right
							dependencies.player.footRightJoint.slerpDrive = dependencies.player.rootJointDrive;
							dependencies.player.footRightJoint.targetRotation = dependencies.procedural.footRightTarget.normalized * Quaternion.Euler(-100, -10 ,0);
						}
					}
				}
						
				//... Set original root joint drive after transition
				if(springRamp > rootSpringRecord && !balanced)
				{
					dependencies.player.rootJointDrive.positionSpring = rootSpringRecord;
					dependencies.player.rootJointDrive.positionDamper = dependencies.player.rootDamper;
					
					//... Set each part to its respective joint drive
					dependencies.player.rootJoint.slerpDrive = dependencies.player.rootJointDrive;
					dependencies.player.bodyLowerJoint.slerpDrive = dependencies.player.bodyLowerJointDrive;
					dependencies.player.bodyUpperJoint.slerpDrive = dependencies.player.bodyUpperJointDrive;
					dependencies.player.headJoint.slerpDrive = dependencies.player.headJointDrive;
					dependencies.player.armLeftJoint.slerpDrive = dependencies.player.armLeftJointDrive;
					dependencies.player.armRightJoint.slerpDrive = dependencies.player.armRightJointDrive;
					dependencies.player.handLeftJoint.slerpDrive = dependencies.player.handLeftJointDrive;
					dependencies.player.handRightJoint.slerpDrive = dependencies.player.handRightJointDrive;
					dependencies.player.legLeftJoint.slerpDrive = dependencies.player.legLeftJointDrive;
					dependencies.player.legRightJoint.slerpDrive = dependencies.player.legRightJointDrive;
					dependencies.player.footLeftJoint.slerpDrive = dependencies.player.footLeftJointDrive;
					dependencies.player.footRightJoint.slerpDrive = dependencies.player.footRightJointDrive;
							
					balanced = true;
					isKnockedOut = false;
					
					//... Remove KO stars
					Destroy(knockedOutStars);
				}
			}
		}
		
		
		//... Ragdoll Mode ...
		public void RagdollMode()
		{
			active = false;
			balanced = false;
			isKnockedOut = true;
			springRamp = 0;
			
			//... Set no joint drive around root for 360 player rotation during ragdoll
			dependencies.player.rootJoint.slerpDrive = dependencies.player.noJointDrive;
			
			//... Set other parts to ragdoll drive strength
			foreach(ConfigurableJoint joints in dependencies.player.playerJoints)
	        {
				if(joints != dependencies.player.playerJoints[0] && joints.gameObject.transform.parent.gameObject != dependencies.dismember.dismemberContainer.gameObject)
				{
					joints.slerpDrive = dependencies.player.ragdollJointDrive;
				}
	        }
		}
		
		
		
		void DeathScreen()
		{
			if(deathScreen != null && !deathScreen.activeSelf)
			{
				deathScreen.SetActive(true);
			}
		}
		
		
		
		//... Debug Info ...
		void OnDrawGizmos()
		{
			//... Grounded Debug ...
			//... Set spherecast position
			if(Grounded())
			{
				//... Track raycast hit
				debugRaycastGroundPoint = hitGround.point;
			}
			
			else
			{
				//... Set spherecast to groundCheckHeight if no hit detected
				debugRaycastGroundPoint = dependencies.player.rootPhysics.transform.position + -Vector3.up * groundCheckHeight;
			}
			
			//... Ground checker position
			Gizmos.color = Color.magenta;
	        Gizmos.DrawWireSphere(dependencies.player.rootPhysics.transform.position, 0.5f);
			
			//... Ground check ray
			Gizmos.color = Color.yellow;
	        Gizmos.DrawLine(dependencies.player.rootPhysics.transform.position, dependencies.player.rootPhysics.transform.position + -Vector3.up * groundCheckHeight);
			
			//... Ground check radius position
			if(Grounded())
			{
				Gizmos.color = Color.green;
			}
			
			else
			{
				Gizmos.color = Color.yellow;
			}
			
	        Gizmos.DrawWireSphere(debugRaycastGroundPoint, groundCheckRadius);
		}
	}
}
