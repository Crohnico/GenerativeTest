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
	public class PX_Trigger : MonoBehaviour
	{
		[Header("Player-X [Trigger]")]
		
		[Space]
		
		[Header("- Player")]
		public Transform playerX;

		[Header("- Goal")]
	    public GameObject EndCollectable;
		
		[Header("- Particle")]
	    public GameObject confetti;
		public Transform particleContainer;
		
		[Header("- Audio")]
		public AudioSource endSound;
		
		bool reachedEnd;
		
		
	    void OnTriggerEnter(Collider other)
	    {
	        if(other.gameObject == playerX.gameObject && !reachedEnd)
	        {
				reachedEnd = true;
				
	            var particle = Instantiate(confetti, transform.position, Quaternion.identity);
				particle.transform.parent = particleContainer;

				Destroy(EndCollectable);
				
				endSound.Play();
	        }
	    }
	}
}
