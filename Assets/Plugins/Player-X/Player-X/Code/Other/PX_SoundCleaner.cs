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
	public class PX_SoundCleaner : MonoBehaviour
	{
		AudioSource sound;
		
		void Start()
		{
			sound = GetComponent<AudioSource>();
		}
		
	    void LateUpdate()
	    {
			//... Clean up audio source if no longer playing
	        if(!sound.isPlaying)
			{
				Destroy(this.gameObject);
			}
	    }
	}
}
