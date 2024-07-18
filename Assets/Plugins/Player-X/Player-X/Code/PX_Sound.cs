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
	public class PX_Sound : MonoBehaviour
	{
		[Header("Player-X [Sound]")]
		
		[Space]
		
		[Header("- Sound Dependencies")]
	    public PX_Dependencies dependencies;
		
		[Header("- Source Instance")]
		public GameObject soundSource;
		public Transform audioContainer;
		
		[Header("- Player Sounds")]
		public AudioClip[] jumpSounds;
		public AudioClip[] attackSounds;
		public AudioClip[] knockoutSound;
		public AudioClip[] fleshImpactSounds;
		public AudioClip[] dismemberSounds;
		
		[Header("- Weapon Sounds")]
		public AudioClip equipSound;
		public AudioClip dropSound;
		
		//- Hidden Variables
		
		[HideInInspector]
		public AudioClip soundToPlay;
		[HideInInspector]
		public Vector3 soundPoint;
		
		
		//... Play Audio
		public void PlayAudio()
		{
			//... Create and set audio source data
			var soundSourceInstance = Instantiate(soundSource, soundPoint, Quaternion.identity).GetComponent<AudioSource>();
			soundSourceInstance.gameObject.transform.parent = audioContainer;
			soundSourceInstance.clip = soundToPlay;
			
			//... Play
			if(soundSourceInstance.clip != null)
			{
				soundSourceInstance.PlayOneShot(soundToPlay);
			}
		}
	}
}
