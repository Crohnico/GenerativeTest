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
	public class PX_SeveredOutBound : MonoBehaviour
	{
		[Header("Player-X [Severed Out Bound]")]
		
		[Space]
		
		[Header("- Severed Dependencies")]
		public PX_Dependencies dependencies;
		
		[Header("- Severed Bound")]
		public float outOfBoundsHeight = -20;
		
		//- Hidden Variables
		bool checkedTrigger = false;
		
		
	    void LateUpdate()
	    {
			return;
			//... Disable dismembered part when individually out of bound
	        if(transform.position.y < outOfBoundsHeight && !checkedTrigger)
	        {
				if(this.gameObject.transform.parent.gameObject == dependencies.dismember.dismemberContainer.gameObject)
				{
					this.gameObject.SetActive(false);
				}
			}
	    }
	}
}
