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
	public class PX_Shell : MonoBehaviour
	{
		[Header("Player-X [Shell]")]
		
		[Header("- Shell Properties")]
		public float shellDuration = 3f;
		public float fadeSpeed = 0.2f;
		public Rigidbody shellPhysics;
		
		[Header("- Audio")]
		public AudioSource shellAudioSource;
		public AudioClip[] shellSounds;
		
		//- Hidden Variables
		
		bool audioRest;
		bool fading = false;
		float audioTimer = 0f;
		
		
		
	    //... Apply variation to stains
	    void Start()
	    {
			shellPhysics.AddForce(new Vector3(Random.Range(-1, 1), 1, Random.Range(-1, 1)) * 5f, ForceMode.Impulse);
			
			shellPhysics.transform.Rotate(new Vector3(shellPhysics.rotation.x + Random.Range(-90, 90), 
			shellPhysics.rotation.y + Random.Range(-90, 90), 
			shellPhysics.rotation.z + Random.Range(-90, 90)));
			
	        Invoke(nameof(StartFade), shellDuration);
	    }
		
		
		//... StartFade ...
		void StartFade()
		{
			fading = true;
		}
		
		
		
	    //... Reduce scale, fade until destroyed
	    void Update()
	    {
	        if(fading)
			{
				transform.localScale -= new Vector3(fadeSpeed, fadeSpeed / 2, fadeSpeed) * Time.deltaTime;
				
				if(transform.localScale.x <= 0)
				{
					Destroy(this.gameObject);
				}
			}
			
			//... Audio rest
			if(audioRest)
			{
				audioTimer += Time.deltaTime;
			}
			
			if(audioTimer > 1f)
			{
				audioRest = false;
			}
	    }
		
		//... Shell sounds
		void OnCollisionEnter()
		{
			if(shellAudioSource != null)
			{
				if(!audioRest)
				{
					shellAudioSource.clip = shellSounds[Random.Range(0, shellSounds.Length)];
					shellAudioSource.Play();
					
					audioRest = true;
				}
			}
		}
	}
}
