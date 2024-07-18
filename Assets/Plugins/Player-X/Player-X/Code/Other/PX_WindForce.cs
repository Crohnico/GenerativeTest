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
	public class PX_WindForce : MonoBehaviour
	{
		[Header("Player-X [Wind Force]")]
		
		[Space]
		
		[Header("- Wind")]
		public float windForce;
		
		
		
		//... Apply force to Rigidbodies in trigger zone
	    void OnTriggerStay(Collider col)
		{
			if(col.GetComponent<Rigidbody>())
			{
				col.GetComponent<Rigidbody>().AddForce(Vector3.up * col.GetComponent<Rigidbody>().mass * windForce, ForceMode.Force);
			}
		}
	}
}
