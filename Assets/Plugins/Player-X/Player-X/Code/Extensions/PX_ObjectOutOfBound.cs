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
	public class PX_ObjectOutOfBound : MonoBehaviour
	{
		[Header("Player-X [Object Out Of Bounds]")]
		
		[Space]
		
		[Header("~ Bound Properties")]
		public float outOfBoundsHeight = -20f;
	    public Transform objectResetPoint;
		
		[Header("~ Particle Properties")]
		public Transform particlePoint;
		public GameObject resetParticle;
		
		//- Hidden Variables
		
		//...
		bool checkedTrigger;

		
	    void Update()
	    {
			//... Check out of bounds
			if(objectResetPoint != null)
			{
				if(transform.position.y < outOfBoundsHeight && !checkedTrigger)
				{
					//... Reset object position
					checkedTrigger = true;
					transform.position = objectResetPoint.position;
					
					
					//... Reset particle
					Instantiate(resetParticle, particlePoint.position, Quaternion.identity);
					
					checkedTrigger = false;
				}
			}
	    }
	}
}
