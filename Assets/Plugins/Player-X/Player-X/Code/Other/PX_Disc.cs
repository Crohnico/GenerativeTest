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
	public class PX_Disc : MonoBehaviour
	{
		[Header("Player-X [Disc]")]
		
		[Space]
		
		[Header("- Disc Properties")]
		public float repelForce = 50f;
		public GameObject sparkParticle;
		public Transform particleContainer;
		public Transform audioContainer;
		public GameObject soundSource;
		public AudioClip[] sparkSounds;
		
		
		
		//... Detection
	    void OnCollisionEnter(Collision col)
		{
			//... Sever part
			if(col.gameObject.GetComponent<PX_ImpactDetect>())
			{
				col.gameObject.GetComponent<PX_ImpactDetect>().DismemberOnCommand();
			}
			
			if(col.gameObject.GetComponent<Rigidbody>())
			{
				//... Repel Direction
				var impactDir = (col.transform.position - this.gameObject.transform.position).normalized;
				
				//... Repel Force
				col.gameObject.GetComponent<Rigidbody>().AddForce(impactDir * repelForce, ForceMode.Impulse);
				
				//... Particle
				var spark = Instantiate(sparkParticle, col.contacts[0].point, Quaternion.LookRotation(col.contacts[0].normal));
				spark.transform.parent = particleContainer;
				
				//... Audio
				if(soundSource != null)
				{
					var sparkSound = Instantiate(soundSource, col.contacts[0].point, Quaternion.identity);
					sparkSound.gameObject.transform.parent = audioContainer;
					sparkSound.GetComponent<AudioSource>().clip = sparkSounds[Random.Range(0, sparkSounds.Length)];
					sparkSound.GetComponent<AudioSource>().Play();
				}
			}
		}
	}
}
