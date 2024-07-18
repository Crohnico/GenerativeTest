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
	public class PX_SeverAudio : MonoBehaviour
	{
		[Header("Player-X [Sever Audio]")]
		
		[Space]
		
		[Header("- Audio")]
		public PX_Sound soundPlayer;
		
	    void Awake()
	    {
	        if(soundPlayer.soundSource != null)
			{
				soundPlayer.soundToPlay = soundPlayer.dismemberSounds[Random.Range(0, soundPlayer.dismemberSounds.Length)];
				soundPlayer.soundPoint = transform.position;
				soundPlayer.PlayAudio();
			}
	    }
	}
}
