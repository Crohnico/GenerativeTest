//------------
//... PLayer-X
//... V2.0.1
//... © TheFamousMouse™
//--------------------
//... Support email:
//... thefamousmouse.developer@gmail.com
//--------------------------------------

using System.Collections.Generic;
using UnityEngine;
using PlayerX;

namespace PlayerX
{
	public class PX_Player : MonoBehaviour
	{
		[Header("Player-X [Player]")]
		
		[Space]
		
		[Header("- Player Container")]
		public Rigidbody playerContainer;
		
		[Header("- Player Root")]
	    public Rigidbody rootPhysics;
		public ConfigurableJoint rootJoint;
		
		public float rootSpring = 60000f;
		public float rootDamper = 1000f;
		
		[Header("- Player Body Lower")]
		public Rigidbody bodyLowerPhysics;
		public ConfigurableJoint bodyLowerJoint;
		
		public float bodyLowerSpring = 15000f;
		public float bodyLowerDamper = 1000f;
		
		[Header("- Player Body Upper")]
		public Rigidbody bodyUpperPhysics;
		public ConfigurableJoint bodyUpperJoint;
		
		public float bodyUpperSpring = 10000f;
		public float bodyUpperDamper = 1000f;
		
		[Header("-  Player Head")]
		public Rigidbody headPhysics;
		public ConfigurableJoint headJoint;
		
		public float headSpring = 500f;
		public float headDamper = 100f;
		
		[Header("- Player Arm Left")]
		public Rigidbody armLeftPhysics;
		public ConfigurableJoint armLeftJoint;
		
		public float armLeftSpring = 5000f;
		public float armLeftDamper = 500f;
		
		[Header("- Player Arm Right")]
		public Rigidbody armRightPhysics;
		public ConfigurableJoint armRightJoint;
		
		public float armRightSpring = 5000f;
		public float armRightDamper = 500f;
		
		[Header("- Player Hand Left")]
		public Rigidbody handLeftPhysics;
		public ConfigurableJoint handLeftJoint;
		
		public float handLeftSpring = 5000f;
		public float handLeftDamper = 500f;
		
		[Header("- Player Hand Right")]
		public Rigidbody handRightPhysics;
		public ConfigurableJoint handRightJoint;
		
		public float handRightSpring = 5000f;
		public float handRightDamper = 500f;
		
		[Header("- Player Leg Left")]
		public Rigidbody legLeftPhysics;
		public ConfigurableJoint legLeftJoint;
		
		public float legLeftSpring = 30000f;
		public float legLeftDamper = 1000f;
		
		[Header("- Player Leg Right")]
		public Rigidbody legRightPhysics;
		public ConfigurableJoint legRightJoint;
		
		public float legRightSpring = 30000f;
		public float legRightDamper = 1000f;
		
		[Header("- Player Foot Left")]
		public Rigidbody footLeftPhysics;
		public ConfigurableJoint footLeftJoint;
		
		public float footLeftSpring = 30000f;
		public float footLeftDamper = 1000f;
		
		[Header("- Player Foot Right")]
		public Rigidbody footRightPhysics;
		public ConfigurableJoint footRightJoint;
		
		public float footRightSpring = 30000f;
		public float footRightDamper = 1000f;
		
		[Header("- Other")]
		public float armsPoseSpring = 30000f;
		public float armsPoseDamper = 1000f;
		
		public float armsRelaxSpring = 500f;
		public float armsRelaxDamper = 100f;
		
		public float bodyPoseSpring = 30000f;
		public float bodyPoseDamper = 1000f;
		
		public float grabSpring = 1500f;
		public float grabDamper = 250f;
		
		public float ragdollSpring = 500f;
		public float ragdollDamper = 50f;
		
		public float noSpring = 0f;
		public float noDamper = 0f;
		
		
		
		//- Hidden Variables
		
		[HideInInspector]
		public JointDrive 
		noJointDrive,
		rootJointDrive,
		bodyLowerJointDrive,
		bodyUpperJointDrive,
		headJointDrive,
		armLeftJointDrive,
		armRightJointDrive,
		handLeftJointDrive,
		handRightJointDrive,
		legLeftJointDrive,
		legRightJointDrive,
		footLeftJointDrive,
		footRightJointDrive,
		armsPoseJointDrive,
		armsRelaxJointDrive,
		bodyPoseJointDrive,
		grabJointDrive,
		ragdollJointDrive;
		
		[HideInInspector]
		public ConfigurableJoint[] 
		playerJoints;
		
		List<ConfigurableJoint> 
		jointContainer = new List<ConfigurableJoint>();
		
		
		
		//... Setup Joint Drives ...
	    void Awake()
	    {
			//... No Joint Drive
			noJointDrive.positionSpring = noSpring;
			noJointDrive.positionDamper = noDamper;
			noJointDrive.maximumForce = Mathf.Infinity;
			
			
			
			//- Body
			
			//... Root
			rootJointDrive.positionSpring = rootSpring;
			rootJointDrive.positionDamper = rootDamper;
			rootJointDrive.maximumForce = Mathf.Infinity;
			
			//... Body Lower
			bodyLowerJointDrive.positionSpring = bodyLowerSpring;
			bodyLowerJointDrive.positionDamper = bodyUpperDamper;
			bodyLowerJointDrive.maximumForce = Mathf.Infinity;
			
			//... Body Upper
			bodyUpperJointDrive.positionSpring = bodyUpperSpring;
			bodyUpperJointDrive.positionDamper = bodyUpperDamper;
			bodyUpperJointDrive.maximumForce = Mathf.Infinity;
			
			//... Head
			headJointDrive.positionSpring = headSpring;
			headJointDrive.positionDamper = headDamper;
			headJointDrive.maximumForce = Mathf.Infinity;
			
			
			
			//- Arms
			
			//... Arm Left
			armLeftJointDrive.positionSpring = armLeftSpring;
			armLeftJointDrive.positionDamper = armLeftDamper;
			armLeftJointDrive.maximumForce = Mathf.Infinity;
			
			//... Arm Right
			armRightJointDrive.positionSpring = armRightSpring;
			armRightJointDrive.positionDamper = armRightDamper;
			armRightJointDrive.maximumForce = Mathf.Infinity;
			
			//... Hand Left
			handLeftJointDrive.positionSpring = handLeftSpring;
			handLeftJointDrive.positionDamper = handLeftDamper;
			handLeftJointDrive.maximumForce = Mathf.Infinity;
			
			//... Hand Right
			handRightJointDrive.positionSpring = handRightSpring;
			handRightJointDrive.positionDamper = handRightDamper;
			handRightJointDrive.maximumForce = Mathf.Infinity;
			
			
			
			//- Legs
			
			//... Leg Left
			legLeftJointDrive.positionSpring = legLeftSpring;
			legLeftJointDrive.positionDamper = legLeftDamper;
			legLeftJointDrive.maximumForce = Mathf.Infinity;
			
			//... Leg Right
			legRightJointDrive.positionSpring = legRightSpring;
			legRightJointDrive.positionDamper = legRightDamper;
			legRightJointDrive.maximumForce = Mathf.Infinity;
			
			//... Foot Left
			footLeftJointDrive.positionSpring = footLeftSpring;
			footLeftJointDrive.positionDamper = footLeftDamper;
			footLeftJointDrive.maximumForce = Mathf.Infinity;
			
			//... Foot Right
			footRightJointDrive.positionSpring = footRightSpring;
			footRightJointDrive.positionDamper = footRightDamper;
			footRightJointDrive.maximumForce = Mathf.Infinity;
			
			
			
			//- Other
			
			//... Reaching/Bending Body
			bodyPoseJointDrive.positionSpring = bodyPoseSpring;
			bodyPoseJointDrive.positionDamper = bodyPoseDamper;
			bodyPoseJointDrive.maximumForce = Mathf.Infinity;
			
			//... Reaching Arms
			armsPoseJointDrive.positionSpring = armsPoseSpring;
			armsPoseJointDrive.positionDamper = armsPoseDamper;
			armsPoseJointDrive.maximumForce = Mathf.Infinity;
			
			//... Relax Arms
			armsRelaxJointDrive.positionSpring = armsRelaxSpring;
			armsRelaxJointDrive.positionDamper = armsRelaxDamper;
			armsRelaxJointDrive.maximumForce = Mathf.Infinity;
			
			//... Ragdoll
			grabJointDrive.positionSpring = grabSpring;
			grabJointDrive.positionDamper = grabDamper;
			grabJointDrive.maximumForce = Mathf.Infinity;
			
			//... Ragdoll
			ragdollJointDrive.positionSpring = ragdollSpring;
			ragdollJointDrive.positionDamper = ragdollDamper;
			ragdollJointDrive.maximumForce = Mathf.Infinity;
			
			
			
			//... Apply respective joint drives ...
			
			//... Root
			rootJoint.slerpDrive = rootJointDrive;
			
			//... Lower Body
			bodyLowerJoint.slerpDrive = bodyLowerJointDrive;
			
			//... Upper Body
			bodyUpperJoint.slerpDrive = bodyUpperJointDrive;
			
			//... Head
			headJoint.slerpDrive = headJointDrive;
			
			//... Arm Left
			armLeftJoint.slerpDrive = armLeftJointDrive;
			
			//... Arm Right
			armRightJoint.slerpDrive = armRightJointDrive;
			
			//... Left Hand
			handLeftJoint.slerpDrive = handLeftJointDrive;
			
			//... Right Hand
			handRightJoint.slerpDrive = handRightJointDrive;
			
			//... Leg Left
			legLeftJoint.slerpDrive = legLeftJointDrive;
			
			//... Leg Right
			legRightJoint.slerpDrive = legRightJointDrive;
			
			//... Foot Left
			footLeftJoint.slerpDrive = footLeftJointDrive;
			
			//... Foot Right
			footRightJoint.slerpDrive = footRightJointDrive;
			
			
			//... Store all player joints in a container
			jointContainer.Add(rootJoint);
			jointContainer.Add(bodyLowerJoint);
			jointContainer.Add(bodyUpperJoint);
			jointContainer.Add(headJoint);
			jointContainer.Add(armLeftJoint);
			jointContainer.Add(armRightJoint);
			jointContainer.Add(handLeftJoint);
			jointContainer.Add(handRightJoint);
			jointContainer.Add(legLeftJoint);
			jointContainer.Add(legRightJoint);
			jointContainer.Add(footLeftJoint);
			jointContainer.Add(footRightJoint);
			
			playerJoints = new ConfigurableJoint[jointContainer.Count];
			for (int i = 0; i < playerJoints.Length; i++)
			{
				playerJoints[i] = jointContainer[i];
				
				//... Player joint indexes for reference
				
				//...(0) Root
				//...(1) Body Lower
				//...(2) Body Upper
				//...(3) Head
				//...(4) Arm left
				//...(5) Arm Right
				//...(6) Hand Left
				//...(7) Hand Right
				//...(8) Leg left
				//...(9) Leg Right
				//...(10) Foot left
				//...(11) Foot Right
			}
	    }
	}
}
