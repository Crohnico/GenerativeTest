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
	public class PX_ImpactDetect : MonoBehaviour
	{
		[Header("Player-X [Impact Detect]")]
		
		[Space]
		
		[Header("- Impact Dependencies")]
		public PX_Dependencies dependencies;
		
		//- Hidden Varaibles
		
		Rigidbody thisPhysics;
		bool dismembered = false;
		
		
		void Start()
		{
			thisPhysics = this.gameObject.GetComponent<Rigidbody>();
		}
		
		
		
		void OnCollisionEnter(Collision col)
		{
			var impactMagnitude = col.relativeVelocity.magnitude;
			
			//... Dismember
			if(dependencies.dismember != null && dependencies.dismember.enabled && 
			impactMagnitude >= dependencies.dismember.dismemberRequiredForce && col.gameObject.transform.root.gameObject != this.gameObject.transform.root.gameObject)
			{
				dismembered = true;
				
				//... React upon impact
				dependencies.state.impactedPart = thisPhysics;
				dependencies.state.impactDir = (this.gameObject.transform.position - col.transform.position).normalized;
				dependencies.state.impactForce = dependencies.dismember.dismemberForce;
				dependencies.state.Damaged();
			
				DismemberOnCommand();
			}
			
			//... Knock out if forces are weaker and impact is against the head
			else if(!dismembered && impactMagnitude >= dependencies.state.impactRequiredKo && col.gameObject.transform.root.gameObject != this.gameObject.transform.root.gameObject && 
			dependencies.state.isAlive && !dependencies.state.isKnockedOut && this.gameObject == dependencies.player.headPhysics.gameObject)
			{
				dependencies.state.impactDir = (this.gameObject.transform.position - col.transform.position).normalized;
				dependencies.state.KnockedOut();
				
				//... Knockout Audio
				if(dependencies.sound.soundSource != null)
				{
					dependencies.sound.soundToPlay = dependencies.sound.knockoutSound[Random.Range(0, dependencies.sound.knockoutSound.Length)];
					dependencies.sound.soundPoint = dependencies.player.headPhysics.transform.position;
					dependencies.sound.PlayAudio();
				}
			}
			
			//... Damage
			else if(!dismembered && impactMagnitude >= dependencies.state.reactRequiredForce && col.gameObject.transform.root.gameObject != this.gameObject.transform.root.gameObject)
			{
				//... React upon impact
				dependencies.state.impactedPart = thisPhysics;
				dependencies.state.impactDir = (this.gameObject.transform.position - col.transform.position).normalized;
				dependencies.state.impactForce = dependencies.state.reactForce;
				dependencies.state.Damaged();
				
				//... Impact Audio
				if(dependencies.sound.soundSource != null)
				{
					if(this.gameObject.transform.root.gameObject.GetComponent<PX_Dependencies>())
					{
						dependencies.sound.soundToPlay = dependencies.sound.fleshImpactSounds[Random.Range(0, dependencies.sound.fleshImpactSounds.Length)];
						dependencies.sound.soundPoint = transform.position;
						dependencies.sound.PlayAudio();
					}
				}
			}
		}
		
		public void DismemberOnCommand()
		{
			if(dependencies.dismember != null)
			{
				if(dependencies.dismember.enabled)
				{
					//... Body lower sever
					if(this.gameObject == dependencies.player.bodyLowerPhysics.gameObject)
					{
						//... Sever
						dependencies.dismember.BodyLowerSever();
					}
					
					//... Body upper sever
					if(this.gameObject == dependencies.player.bodyUpperPhysics.gameObject)
					{
						//... Sever
						dependencies.dismember.BodyUpperSever();
					}
					
					//... Head sever
					if(this.gameObject == dependencies.player.headPhysics.gameObject)
					{	
						//... off with his head!
						dependencies.dismember.HeadSever();
					}
					
					//... Arm left sever
					if(this.gameObject == dependencies.player.armLeftPhysics.gameObject)
					{
						//... Sever
						dependencies.dismember.ArmLeftSever();
					}
					
					//... Arm right sever
					if(this.gameObject == dependencies.player.armRightPhysics.gameObject)
					{
						//... Sever
						dependencies.dismember.ArmRightSever();
					}
					
					//... Hand left sever
					if(this.gameObject == dependencies.player.handLeftPhysics.gameObject)
					{
						//... Sever
						dependencies.dismember.HandLeftSever();
					}
					
					//... Hand right sever
					if(this.gameObject == dependencies.player.handRightPhysics.gameObject)
					{
						//... Sever
						dependencies.dismember.HandRightSever();
					}
					
					//... Leg left sever
					if(this.gameObject == dependencies.player.legLeftPhysics.gameObject)
					{
						//... Sever
						dependencies.dismember.LegLeftSever();
					}
					
					//... Leg right sever
					if(this.gameObject == dependencies.player.legRightPhysics.gameObject)
					{
						//... Sever
						dependencies.dismember.LegRightSever();
					}
					
					//... Foot left sever
					if(this.gameObject == dependencies.player.footLeftPhysics.gameObject)
					{
						//... Sever
						dependencies.dismember.FootLeftSever();
					}
					
					//... Foot right sever
					if(this.gameObject == dependencies.player.footRightPhysics.gameObject)
					{
						//... Sever
						dependencies.dismember.FootRightSever();
					}
				}
			}
		}
	}
}
