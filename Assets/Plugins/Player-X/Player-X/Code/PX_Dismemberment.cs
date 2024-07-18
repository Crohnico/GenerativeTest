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
	public class PX_Dismemberment : MonoBehaviour
	{
		[Header("Player-X [Dismemberment]")]
		
		[Space]
		
		[Header("- Dismember Dependencies")]
		public PX_Dependencies dependencies;
		
		[Header("- Properties")]
		public float dismemberForce = 5f;
		public float dismemberRequiredForce = 350f;
		public Transform dismemberContainer;
		
		
		[Header("- Body Lower Dismember")]
		public GameObject bodyLowerSelfWound;
		public GameObject bodyLowerOtherWound;
		
		[Header("- Body Upper Dismember")]
		public GameObject bodyUpperSelfWound;
		public GameObject bodyUpperOtherWound;
		
		[Header("- Head Dismember")]
		public GameObject headSelfWound;
		public GameObject headOtherWound;
		
		[Header("- Arm Left Dismember")]
		public GameObject armLeftSelfWound;
		public GameObject armLeftOtherWound;
		
		[Header("- Arm Right Dismember")]
		public GameObject armRightSelfWound;
		public GameObject armRightOtherWound;
		
		[Header("- Hand Left Dismember")]
		public GameObject handLeftSelfWound;
		public GameObject handLeftOtherWound;
		
		[Header("- Hand Right Dismember")]
		public GameObject handRightSelfWound;
		public GameObject handRightOtherWound;
		
		[Header("- Leg left Dismember")]
		public GameObject legLeftSelfWound;
		public GameObject legLeftOtherWound;
		
		[Header("- Leg Right Dismember")]
		public GameObject legRightSelfWound;
		public GameObject legRightOtherWound;
		
		[Header("- Foot Left Dismember")]
		public GameObject footLeftSelfWound;
		public GameObject footLeftOtherWound;
		
		[Header("- Foot Right Dismember")]
		public GameObject footRightSelfWound;
		public GameObject footRightOtherWound;
		
		
		public void BodyLowerSever()
		{
			//... Disable player
			dependencies.state.isAlive = false;
			dependencies.state.RagdollMode();
			
			//... Remove impact detector
			Destroy(dependencies.player.bodyLowerPhysics.GetComponent<PX_ImpactDetect>());
			
			//... Release joint drive
			dependencies.player.bodyLowerJointDrive = dependencies.player.noJointDrive;
			dependencies.player.bodyLowerJoint.slerpDrive = dependencies.player.noJointDrive;
			
			
			//... Release extensions joint drive
			//... Upper body
			dependencies.player.bodyUpperJointDrive = dependencies.player.noJointDrive;
			dependencies.player.bodyUpperJoint.slerpDrive = dependencies.player.noJointDrive;
			
			//... Head
			dependencies.player.headJointDrive = dependencies.player.noJointDrive;
			dependencies.player.headJoint.slerpDrive = dependencies.player.noJointDrive;
			
			//... Arm Left
			dependencies.player.armLeftJointDrive = dependencies.player.noJointDrive;
			dependencies.player.armLeftJoint.slerpDrive = dependencies.player.noJointDrive;
			
			//... Hand Left
			dependencies.player.handLeftJointDrive = dependencies.player.noJointDrive;
			dependencies.player.handLeftJoint.slerpDrive = dependencies.player.noJointDrive;
			
			//... Arm Right
			dependencies.player.armRightJointDrive = dependencies.player.noJointDrive;
			dependencies.player.armRightJoint.slerpDrive = dependencies.player.noJointDrive;
			
			//... Hand Right
			dependencies.player.handRightJointDrive = dependencies.player.noJointDrive;
			dependencies.player.handRightJoint.slerpDrive = dependencies.player.noJointDrive;
			
			
			
			//... Set joint free
			dependencies.player.bodyLowerJoint.xMotion = ConfigurableJointMotion.Free;
			dependencies.player.bodyLowerJoint.yMotion = ConfigurableJointMotion.Free;
			dependencies.player.bodyLowerJoint.zMotion = ConfigurableJointMotion.Free;
			
			dependencies.player.bodyLowerJoint.angularXMotion = ConfigurableJointMotion.Free;
			dependencies.player.bodyLowerJoint.angularYMotion = ConfigurableJointMotion.Free;
			dependencies.player.bodyLowerJoint.angularZMotion = ConfigurableJointMotion.Free;
			
			
			//... Remove connected body
			dependencies.player.bodyLowerJoint.connectedBody = null;
			
			
			//... Move from player tree to container
			dependencies.player.bodyLowerPhysics.transform.parent = dismemberContainer;
			
			//... Move Body Upper with
			dependencies.player.bodyUpperPhysics.transform.parent = dismemberContainer;
			
			//... Move head with
			dependencies.player.headPhysics.transform.parent = dismemberContainer;
			
			
			//... Move left arm with
			dependencies.player.armLeftPhysics.transform.parent = dismemberContainer;
			//... Move left hand with
			dependencies.player.handLeftPhysics.transform.parent = dismemberContainer;
			
			//... Move right arm with
			dependencies.player.armRightPhysics.transform.parent = dismemberContainer;
			//... Move right hand with
			dependencies.player.handRightPhysics.transform.parent = dismemberContainer;
			
			
			//... Activate wounds
			bodyLowerSelfWound.SetActive(true);
			bodyLowerOtherWound.SetActive(true);
			
			
			//... Remove KO stars if were created before death
			if(dependencies.state.knockedOutStars != null)
			{
				Destroy(dependencies.state.knockedOutStars);
			}
		}
		
		public void BodyUpperSever()
		{
			//... Disable player
			dependencies.state.isAlive = false;
			dependencies.state.RagdollMode();
			
			//... Remove impact detector
			Destroy(dependencies.player.bodyUpperPhysics.GetComponent<PX_ImpactDetect>());
			
			//... Release joint drive
			dependencies.player.bodyUpperJointDrive = dependencies.player.noJointDrive;
			dependencies.player.bodyUpperJoint.slerpDrive = dependencies.player.noJointDrive;
			
			//... Release extensions joint drive
			//... Head
			dependencies.player.headJointDrive = dependencies.player.noJointDrive;
			dependencies.player.headJoint.slerpDrive = dependencies.player.noJointDrive;
			
			//... Arm Left
			dependencies.player.armLeftJointDrive = dependencies.player.noJointDrive;
			dependencies.player.armLeftJoint.slerpDrive = dependencies.player.noJointDrive;
			
			//... Hand Left
			dependencies.player.handLeftJointDrive = dependencies.player.noJointDrive;
			dependencies.player.handLeftJoint.slerpDrive = dependencies.player.noJointDrive;
			
			//... Arm Right
			dependencies.player.armRightJointDrive = dependencies.player.noJointDrive;
			dependencies.player.armRightJoint.slerpDrive = dependencies.player.noJointDrive;
			
			//... Hand Right
			dependencies.player.handRightJointDrive = dependencies.player.noJointDrive;
			dependencies.player.handRightJoint.slerpDrive = dependencies.player.noJointDrive;
			
			
			//... Set joint free
			dependencies.player.bodyUpperJoint.xMotion = ConfigurableJointMotion.Free;
			dependencies.player.bodyUpperJoint.yMotion = ConfigurableJointMotion.Free;
			dependencies.player.bodyUpperJoint.zMotion = ConfigurableJointMotion.Free;
			
			dependencies.player.bodyUpperJoint.angularXMotion = ConfigurableJointMotion.Free;
			dependencies.player.bodyUpperJoint.angularYMotion = ConfigurableJointMotion.Free;
			dependencies.player.bodyUpperJoint.angularZMotion = ConfigurableJointMotion.Free;
			
			
			//... Remove connected body
			dependencies.player.bodyUpperJoint.connectedBody = null;
			
			
			//... Move from player tree to container
			dependencies.player.bodyUpperPhysics.transform.parent = dismemberContainer;
			
			//... Move head with
			dependencies.player.headPhysics.transform.parent = dismemberContainer;
			
			
			//... Move left arm with
			dependencies.player.armLeftPhysics.transform.parent = dismemberContainer;
			//... Move left hand with
			dependencies.player.handLeftPhysics.transform.parent = dismemberContainer;
			
			//... Move right arm with
			dependencies.player.armRightPhysics.transform.parent = dismemberContainer;
			//... Move right hand with
			dependencies.player.handRightPhysics.transform.parent = dismemberContainer;
			
			//... Activate wounds
			bodyUpperSelfWound.SetActive(true);
			bodyUpperOtherWound.SetActive(true);
			
			
			//... Remove KO stars if were created before death
			if(dependencies.state.knockedOutStars != null)
			{
				Destroy(dependencies.state.knockedOutStars);
			}
		}
		
		public void HeadSever()
		{
			//... Disable player
			dependencies.state.isAlive = false;
			dependencies.state.RagdollMode();
			
			//... Remove impact detector
			Destroy(dependencies.player.headPhysics.GetComponent<PX_ImpactDetect>());
			
			//... Release joint drive
			dependencies.player.headJointDrive = dependencies.player.noJointDrive;
			dependencies.player.headJoint.slerpDrive = dependencies.player.noJointDrive;
			
			
			//... Set joint free
			dependencies.player.headJoint.xMotion = ConfigurableJointMotion.Free;
			dependencies.player.headJoint.yMotion = ConfigurableJointMotion.Free;
			dependencies.player.headJoint.zMotion = ConfigurableJointMotion.Free;
			
			dependencies.player.headJoint.angularXMotion = ConfigurableJointMotion.Free;
			dependencies.player.headJoint.angularYMotion = ConfigurableJointMotion.Free;
			dependencies.player.headJoint.angularZMotion = ConfigurableJointMotion.Free;
			
			
			//... Remove connected body
			dependencies.player.headJoint.connectedBody = null;
			
			
			//... Move from player tree to container
			dependencies.player.headPhysics.transform.parent = dismemberContainer;
			
			//... Activate wounds
			headSelfWound.SetActive(true);
			headOtherWound.SetActive(true);
			
			
			//... Remove KO stars if were created before death
			if(dependencies.state.knockedOutStars != null)
			{
				Destroy(dependencies.state.knockedOutStars);
			}
		}
		
		public void ArmLeftSever()
		{
			//... Remove impact detector
			Destroy(dependencies.player.armLeftPhysics.GetComponent<PX_ImpactDetect>());
			
			//... Release joint drive
			dependencies.player.armLeftJointDrive = dependencies.player.noJointDrive;
			dependencies.player.armLeftJoint.slerpDrive = dependencies.player.noJointDrive;
			
			//... Release extension drives too
			dependencies.player.handLeftJointDrive = dependencies.player.noJointDrive;
			dependencies.player.handLeftJoint.slerpDrive = dependencies.player.noJointDrive;
			
			
			//... Set joint free
			dependencies.player.armLeftJoint.xMotion = ConfigurableJointMotion.Free;
			dependencies.player.armLeftJoint.yMotion = ConfigurableJointMotion.Free;
			dependencies.player.armLeftJoint.zMotion = ConfigurableJointMotion.Free;
			
			dependencies.player.armLeftJoint.angularXMotion = ConfigurableJointMotion.Free;
			dependencies.player.armLeftJoint.angularYMotion = ConfigurableJointMotion.Free;
			dependencies.player.armLeftJoint.angularZMotion = ConfigurableJointMotion.Free;
			
			
			//... Remove connected body
			dependencies.player.armLeftJoint.connectedBody = null;
			
			
			//... Move from player tree to container
			dependencies.player.armLeftPhysics.transform.parent = dismemberContainer;
			//... Move hand with
			dependencies.player.handLeftPhysics.transform.parent = dismemberContainer;
			
			//... Activate wounds
			armLeftSelfWound.SetActive(true);
			armLeftOtherWound.SetActive(true);
		}
		
		public void ArmRightSever()
		{
			//... Remove impact detector
			Destroy(dependencies.player.armRightPhysics.GetComponent<PX_ImpactDetect>());
			
			//... Release joint drive
			dependencies.player.armRightJointDrive = dependencies.player.noJointDrive;
			dependencies.player.armRightJoint.slerpDrive = dependencies.player.noJointDrive;
			
			//... Release extension drives too
			dependencies.player.handRightJointDrive = dependencies.player.noJointDrive;
			dependencies.player.handRightJoint.slerpDrive = dependencies.player.noJointDrive;
			
			
			//... Set joint free
			dependencies.player.armRightJoint.xMotion = ConfigurableJointMotion.Free;
			dependencies.player.armRightJoint.yMotion = ConfigurableJointMotion.Free;
			dependencies.player.armRightJoint.zMotion = ConfigurableJointMotion.Free;
			
			dependencies.player.armRightJoint.angularXMotion = ConfigurableJointMotion.Free;
			dependencies.player.armRightJoint.angularYMotion = ConfigurableJointMotion.Free;
			dependencies.player.armRightJoint.angularZMotion = ConfigurableJointMotion.Free;
			
			
			//... Remove connected body
			dependencies.player.armRightJoint.connectedBody = null;
			
			
			//... Move from player tree to container
			dependencies.player.armRightPhysics.transform.parent = dismemberContainer;
			//... Move hand with
			dependencies.player.handRightPhysics.transform.parent = dismemberContainer;
			
			//... Activate wounds
			armRightSelfWound.SetActive(true);
			armRightOtherWound.SetActive(true);
		}
		
		public void HandLeftSever()
		{
			//... Remove impact detector
			Destroy(dependencies.player.handLeftPhysics.GetComponent<PX_ImpactDetect>());
			
			//... Release joint drive
			dependencies.player.handLeftJointDrive = dependencies.player.noJointDrive;
			dependencies.player.handLeftJoint.slerpDrive = dependencies.player.noJointDrive;
			
			
			//... Set joint free
			dependencies.player.handLeftJoint.xMotion = ConfigurableJointMotion.Free;
			dependencies.player.handLeftJoint.yMotion = ConfigurableJointMotion.Free;
			dependencies.player.handLeftJoint.zMotion = ConfigurableJointMotion.Free;
			
			dependencies.player.handLeftJoint.angularXMotion = ConfigurableJointMotion.Free;
			dependencies.player.handLeftJoint.angularYMotion = ConfigurableJointMotion.Free;
			dependencies.player.handLeftJoint.angularZMotion = ConfigurableJointMotion.Free;
			
			
			//... Remove connected body
			dependencies.player.handLeftJoint.connectedBody = null;
			
			
			//... Move from player tree to container
			dependencies.player.handLeftPhysics.transform.parent = dismemberContainer;
			
			//... Activate wounds
			handLeftSelfWound.SetActive(true);
			handLeftOtherWound.SetActive(true);
		}
		
		public void HandRightSever()
		{
			//... Remove impact detector
			Destroy(dependencies.player.handRightPhysics.GetComponent<PX_ImpactDetect>());
			
			//... Release joint drive
			dependencies.player.handRightJointDrive = dependencies.player.noJointDrive;
			dependencies.player.handRightJoint.slerpDrive = dependencies.player.noJointDrive;
			
			
			//... Set joint free
			dependencies.player.handRightJoint.xMotion = ConfigurableJointMotion.Free;
			dependencies.player.handRightJoint.yMotion = ConfigurableJointMotion.Free;
			dependencies.player.handRightJoint.zMotion = ConfigurableJointMotion.Free;
			
			dependencies.player.handRightJoint.angularXMotion = ConfigurableJointMotion.Free;
			dependencies.player.handRightJoint.angularYMotion = ConfigurableJointMotion.Free;
			dependencies.player.handRightJoint.angularZMotion = ConfigurableJointMotion.Free;
			
			
			//... Remove connected body
			dependencies.player.handRightJoint.connectedBody = null;
			
			
			//... Move from player tree to container
			dependencies.player.handRightPhysics.transform.parent = dismemberContainer;
			
			//... Activate wounds
			handRightSelfWound.SetActive(true);
			handRightOtherWound.SetActive(true);
		}
		
		public void LegLeftSever()
		{
			//... Remove impact detector
			Destroy(dependencies.player.legLeftPhysics.GetComponent<PX_ImpactDetect>());
			
			//... Release joint drive
			dependencies.player.legLeftJointDrive = dependencies.player.noJointDrive;
			dependencies.player.legLeftJoint.slerpDrive = dependencies.player.noJointDrive;
			
			//... Release extension drive too
			dependencies.player.footLeftJointDrive = dependencies.player.noJointDrive;
			dependencies.player.footLeftJoint.slerpDrive = dependencies.player.noJointDrive;
			
			
			//... Set joint free
			dependencies.player.legLeftJoint.xMotion = ConfigurableJointMotion.Free;
			dependencies.player.legLeftJoint.yMotion = ConfigurableJointMotion.Free;
			dependencies.player.legLeftJoint.zMotion = ConfigurableJointMotion.Free;
			
			dependencies.player.legLeftJoint.angularXMotion = ConfigurableJointMotion.Free;
			dependencies.player.legLeftJoint.angularYMotion = ConfigurableJointMotion.Free;
			dependencies.player.legLeftJoint.angularZMotion = ConfigurableJointMotion.Free;
			
			
			//... Remove connected body
			dependencies.player.legLeftJoint.connectedBody = null;
			
			
			//... Move from player tree to container
			dependencies.player.legLeftPhysics.transform.parent = dismemberContainer;
			//... Move foot with
			dependencies.player.footLeftPhysics.transform.parent = dismemberContainer;
			
			//... Activate wounds
			legLeftSelfWound.SetActive(true);
			legLeftOtherWound.SetActive(true);
		}
		
		public void LegRightSever()
		{
			//... Remove impact detector
			Destroy(dependencies.player.legRightPhysics.GetComponent<PX_ImpactDetect>());
			
			//... Release joint drive
			dependencies.player.legRightJointDrive = dependencies.player.noJointDrive;
			dependencies.player.legRightJoint.slerpDrive = dependencies.player.noJointDrive;
			
			//... Release extension drive too
			dependencies.player.footRightJointDrive = dependencies.player.noJointDrive;
			dependencies.player.footRightJoint.slerpDrive = dependencies.player.noJointDrive;
			
			
			//... Set joint free
			dependencies.player.legRightJoint.xMotion = ConfigurableJointMotion.Free;
			dependencies.player.legRightJoint.yMotion = ConfigurableJointMotion.Free;
			dependencies.player.legRightJoint.zMotion = ConfigurableJointMotion.Free;
			
			dependencies.player.legRightJoint.angularXMotion = ConfigurableJointMotion.Free;
			dependencies.player.legRightJoint.angularYMotion = ConfigurableJointMotion.Free;
			dependencies.player.legRightJoint.angularZMotion = ConfigurableJointMotion.Free;
			
			
			//... Remove connected body
			dependencies.player.legRightJoint.connectedBody = null;
			
			
			//... Move from player tree to container
			dependencies.player.legRightPhysics.transform.parent = dismemberContainer;
			//... Move foot with
			dependencies.player.footRightPhysics.transform.parent = dismemberContainer;
			
			//... Activate wounds
			legRightSelfWound.SetActive(true);
			legRightOtherWound.SetActive(true);
		}
		
		public void FootLeftSever()
		{
			//... Remove impact detector
			Destroy(dependencies.player.footLeftPhysics.GetComponent<PX_ImpactDetect>());
			
			//... Release joint drive
			dependencies.player.footLeftJointDrive = dependencies.player.noJointDrive;
			dependencies.player.footLeftJoint.slerpDrive = dependencies.player.noJointDrive;
			
			
			//... Set joint free
			dependencies.player.footLeftJoint.xMotion = ConfigurableJointMotion.Free;
			dependencies.player.footLeftJoint.yMotion = ConfigurableJointMotion.Free;
			dependencies.player.footLeftJoint.zMotion = ConfigurableJointMotion.Free;
			
			dependencies.player.footLeftJoint.angularXMotion = ConfigurableJointMotion.Free;
			dependencies.player.footLeftJoint.angularYMotion = ConfigurableJointMotion.Free;
			dependencies.player.footLeftJoint.angularZMotion = ConfigurableJointMotion.Free;
			
			
			//... Remove connected body
			dependencies.player.footLeftJoint.connectedBody = null;
			
			
			//... Move from player tree to container
			dependencies.player.footLeftPhysics.transform.parent = dismemberContainer;
			
			//... Activate wounds
			footLeftSelfWound.SetActive(true);
			footLeftOtherWound.SetActive(true);
		}
		
		public void FootRightSever()
		{
			//... Remove impact detector
			Destroy(dependencies.player.footRightPhysics.GetComponent<PX_ImpactDetect>());
			
			//... Release joint drive
			dependencies.player.footRightJointDrive = dependencies.player.noJointDrive;
			dependencies.player.footRightJoint.slerpDrive = dependencies.player.noJointDrive;
			
			
			//... Set joint free
			dependencies.player.footRightJoint.xMotion = ConfigurableJointMotion.Free;
			dependencies.player.footRightJoint.yMotion = ConfigurableJointMotion.Free;
			dependencies.player.footRightJoint.zMotion = ConfigurableJointMotion.Free;
			
			dependencies.player.footRightJoint.angularXMotion = ConfigurableJointMotion.Free;
			dependencies.player.footRightJoint.angularYMotion = ConfigurableJointMotion.Free;
			dependencies.player.footRightJoint.angularZMotion = ConfigurableJointMotion.Free;
			
			
			//... Remove connected body
			dependencies.player.footRightJoint.connectedBody = null;
			
			
			//... Move from player tree to container
			dependencies.player.footRightPhysics.transform.parent = dismemberContainer;
			
			//... Activate wounds
			footRightSelfWound.SetActive(true);
			footRightOtherWound.SetActive(true);
		}
		
		
		void LateUpdate()
		{
			//... Disable movement when both legs are dismembered
			if(dependencies.player.legLeftJoint.connectedBody == null && dependencies.player.legRightJoint.connectedBody == null)
			{
				if(dependencies.controller.enableMove && dependencies.controller.enableJump)
				{
					dependencies.controller.enableMove = false;
					dependencies.controller.enableJump = false;
				}
			}
			
			//... Kill player when both arms and legs are severed
			if(dependencies.player.legLeftJoint.connectedBody == null && dependencies.player.legRightJoint.connectedBody == null 
			&& dependencies.player.armLeftJoint.connectedBody == null && dependencies.player.armRightJoint.connectedBody == null 
			|| dependencies.player.legLeftJoint.connectedBody == null && dependencies.player.legRightJoint.connectedBody == null 
			&& dependencies.player.handLeftJoint.connectedBody == null && dependencies.player.handRightJoint.connectedBody == null
			|| dependencies.player.footLeftJoint.connectedBody == null && dependencies.player.footRightJoint.connectedBody == null 
			&& dependencies.player.handLeftJoint.connectedBody == null && dependencies.player.handRightJoint.connectedBody == null)
			{
				if(dependencies.state.isAlive)
				{
					dependencies.state.isAlive = false;
					dependencies.state.RagdollMode();
				}
			}
		}
	}
}
