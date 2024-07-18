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
	public class PX_Collectible : MonoBehaviour
	{
		[Header("Player-X [Collectible]")]
		
		[Space]
		
		[Header("- Properties")]
		public Vector3 startSize = new Vector3(0.67f, 0.67f, 0.67f);
		public float collectDistance = 3f;
		public float collectSpeed = 10f;
		public int collectScore = 1;
		
		[Header("- References")]
		public PX_Score score;
	    public Transform player;
		
		[Header("- Particle")]
		public GameObject collectedParticle;
		public Transform particleContainer;
		
		[Header("- Audio")]
		public GameObject soundSource;
		public AudioClip collectSound;
		public Transform audioContainer;
		
		
		void Awake()
		{
			transform.localScale = startSize;
		}

	    void Update()
	    {
			if(player != null)
			{
				if(player.root.gameObject.GetComponent<PX_State>() && player.root.gameObject.GetComponent<PX_State>().isAlive)
				{
					//... Check collect distance
					if(Vector3.Distance(transform.position, player.position) < collectDistance)
					{
						//... Move towards player
						var dir = (player.position - transform.position).normalized;
						transform.Translate(dir * (collectSpeed * Time.deltaTime), Space.World);
					}
					
					//... Collect
					if(Vector3.Distance(transform.position, player.position) < 0.3f)
					{
						//... Particle
						var particle = Instantiate(collectedParticle, transform.position, Quaternion.identity);
						particle.transform.parent = particleContainer;
						
						//... Audio
						if(soundSource != null)
						{
							var sound = Instantiate(soundSource, transform.position, Quaternion.identity).GetComponent<AudioSource>();
							sound.clip = collectSound;
							sound.transform.parent = audioContainer;
							sound.Play();
						}
						
						//... Add score
						score.AddScore(collectScore);
						
						//... Remove collectable
						Destroy(this.gameObject);
					}
				}
			}
	    }
	}
}
