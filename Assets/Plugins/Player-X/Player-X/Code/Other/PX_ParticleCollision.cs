//------------
//... PLayer-X
//... V2.0.1
//... © TheFamousMouse™
//--------------------
//... Support email:
//... thefamousmouse.developer@gmail.com
//--------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerX;

namespace PlayerX
{
	public class PX_ParticleCollision : MonoBehaviour
	{
		[Header("Player-X [Particle Collision]")]
		
		[Space]
		
		[Header("- Particle Stain")]
		public GameObject stain;
		
		//- Hidden Particle Variables
	    ParticleSystem particle;
		List<ParticleCollisionEvent> 
		collisionEvents = new List<ParticleCollisionEvent>();
		
		
		
		void Awake()
		{
			particle = this.gameObject.GetComponent<ParticleSystem>();
		}
		
		//... Spawn stain at particle collision points
		void OnParticleCollision(GameObject other)
		{
			int numCollisionEvents = particle.GetCollisionEvents(other, collisionEvents);
			
			int i = 0;
				
			while(i < numCollisionEvents)
			{
				Vector3 pos = collisionEvents[i].intersection;
				Vector3 rot = collisionEvents[i].normal;
				Instantiate(stain, pos - new Vector3(0,0.05f,0), Quaternion.FromToRotation(Vector3.up, rot));
				
				i++;
			}
		}
	}
}
