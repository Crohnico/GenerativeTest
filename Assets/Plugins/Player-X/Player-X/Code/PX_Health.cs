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
	public class PX_Health : MonoBehaviour
	{
		[Header("Player-X [Health]")]
		
		[Header("- Health Dependencies")]
		public PX_Dependencies dependencies;
		
		[Header("- Health")]
	    public float playerHealth = 100f;
		
		
		
		void LateUpdate()
		{
			//... Kill player if health depleted
			if(playerHealth <= 0f)
			{
				dependencies.state.isAlive = false;
				dependencies.state.RagdollMode();
			}
		}
	}
}
