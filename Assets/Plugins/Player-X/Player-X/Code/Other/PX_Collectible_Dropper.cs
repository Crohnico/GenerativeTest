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
	public class PX_Collectible_Dropper : MonoBehaviour
	{
		[Header("Player-X [Collectible Dropper]")]
		
		[Space]
		
		[Header("- Collectible Drop Dependencies")]
		public PX_Dependencies dependencies;
		
		[Header("- Collectible References")]
	    public PX_Score score;
	    public Transform collectPlayer;
		public GameObject collectible;
		public Transform collectibleContainer;
		public Transform particleContainer;
		
		[Header("- Audio")]
		public GameObject soundSource;
		public Transform audioContainer;
		public AudioClip dropSound;
		
		bool droppedCollectable;
		
		
		
		void LateUpdate()
		{
			//... Drop collectible when this player dies
			if(!dependencies.state.isAlive && !droppedCollectable)
			{
				droppedCollectable = true;
				
				//... Collectible drop
				var drop = Instantiate(collectible, dependencies.player.rootPhysics.transform.position, Quaternion.identity);
				
				//... Assign collectible properties
				drop.GetComponent<PX_Collectible>().score = score;
				drop.GetComponent<PX_Collectible>().player = collectPlayer;
				drop.GetComponent<PX_Collectible>().particleContainer = particleContainer;
				drop.GetComponent<PX_Collectible>().audioContainer = audioContainer;
				drop.transform.parent = collectibleContainer;
				
				//... Audio
				if(soundSource != null)
				{
					var sound = Instantiate(soundSource, drop.transform.position, Quaternion.identity).GetComponent<AudioSource>();
					sound.clip = dropSound;
					sound.transform.parent = audioContainer;
					sound.Play();
				}
			}
		}
	}
}
