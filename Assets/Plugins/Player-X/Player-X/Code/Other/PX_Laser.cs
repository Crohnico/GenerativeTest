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
	public class PX_Laser : MonoBehaviour
	{
		[Header("Player-X [Laser]")]
		
		[Space]
		
		[Header("- Laser")]
		public GameObject sparkParticle;
		public Transform particleContainer;
		public Transform audioContainer;
		public GameObject soundSource;
		public AudioClip[] sparkSounds;
		
		
		
		//... Detection
	    void OnTriggerEnter(Collider col)
		{
			var detectPoint = col.ClosestPoint(transform.position);
			
			//... Sever part
			if(col.gameObject.GetComponent<PX_ImpactDetect>())
			{
				col.gameObject.GetComponent<PX_ImpactDetect>().DismemberOnCommand();
			}
			
			//... Particle
			var Spark = Instantiate(sparkParticle, detectPoint, Quaternion.identity);
			Spark.transform.parent = particleContainer;
			
			
			//... Audio
			if(soundSource != null)
			{
				var sparkSound = Instantiate(soundSource, detectPoint, Quaternion.identity);
				sparkSound.gameObject.transform.parent = audioContainer;
				sparkSound.GetComponent<AudioSource>().clip = sparkSounds[Random.Range(0, sparkSounds.Length)];
				sparkSound.GetComponent<AudioSource>().Play();
			}
		}
	}
}
