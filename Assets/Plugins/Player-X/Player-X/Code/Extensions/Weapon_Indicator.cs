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
	public class Weapon_Indicator : MonoBehaviour
	{
	    [Header("Player-X [Weapon Indicator]")]
		
		[Space]
		
		[Header("Properties")]
		public Transform cameraToFace;

		
	    void Update()
	    {
	        transform.LookAt(cameraToFace.position);
	    }
	}
}
