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
			

			
			//... Knock out if forces are weaker and impact is against the head
			/*else if(!dismembered && impactMagnitude >= dependencies.state.impactRequiredKo && col.gameObject.transform.root.gameObject != this.gameObject.transform.root.gameObject && 
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
			}*/
			

		}
		
		
	}
}
