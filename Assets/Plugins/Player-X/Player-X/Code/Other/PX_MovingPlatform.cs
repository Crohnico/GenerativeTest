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
    public class PX_MovingPlatform : MonoBehaviour
    {
    	[Header("Player-X [Moving Platform]")]
    	
    	[Space]
    	
    	[Header("- Player")]
    	public GameObject playerX;
    	public GameObject playerRoot;
        
    	
        void OnTriggerEnter(Collider other)
        {
            if(other.gameObject == playerRoot)
            {
                playerX.transform.parent = this.gameObject.transform;
            }
        }
        
        void OnTriggerExit(Collider other)
        {
            if(other.gameObject == playerRoot)
            {
                playerX.transform.parent = null;
            }
        }
    }
}
