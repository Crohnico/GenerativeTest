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
	public class PX_OutOfBounds : MonoBehaviour
	{
		[Header("Player-X [Out Of Bounds]")]
		
		[Space]
		
		[Header("- Dependencies")]
		public PX_Dependencies dependencies;
	    
		[Header("- Bound Properties")]
		public Transform resetPoint;
		public bool maintainVelocity = false;
		public float outOfBoundsHeight = -20f;
	    
		[Header("- Camera Reset Properties")]
		public bool cameraFollowsThisPlayer = true;
		public bool instantCameraUpdate = true;
		
		[Header("- Particle Properties")]
		public Transform particleContainer;
		public Transform particlePoint;
		public GameObject resetParticle;
		
		[Header("- Audio")]
		public AudioSource resetAudioSource;
	    
		
		//- Hidden Variables
	    
	    //...
	    bool 
		checkedTrigger, resetCompleted;
		
	    //...
	    Rigidbody[] 
		playerPhysics;
	    
	    //...
	    Vector3 
		storedVelocity, 
		storedCameraOffset;
	    
		
	    
		//... Reset position when out of bound
	    void FixedUpdate()
	    {
	        if(dependencies.state.isAlive && dependencies.player.rootPhysics.transform.position.y < outOfBoundsHeight && !checkedTrigger)
	        {
	                checkedTrigger = true;

					
					//... Store all player Rigidbodies to freeze ragdoll physics
	                playerPhysics = dependencies.player.playerContainer.GetComponentsInChildren<Rigidbody>();
	                
					
	                //... freeze physics and store velocity
	                foreach(Rigidbody physics in playerPhysics)
	                {
						storedVelocity = physics.velocity;
	                    physics.isKinematic = true;
	                }
	                
					
	                //... Record camera current offset
					if(instantCameraUpdate && cameraFollowsThisPlayer)
					{
						storedCameraOffset = new Vector3(dependencies.playerCamera.followCamera.transform.position.x 
						- dependencies.player.rootPhysics.transform.position.x, dependencies.playerCamera.followCamera.transform.position.y 
						- dependencies.player.rootPhysics.transform.position.y, dependencies.playerCamera.followCamera.transform.position.z 
						- dependencies.player.rootPhysics.transform.position.z);

						//... Reset stabilizer with new
						dependencies.playerCamera.stabilizing = new PX_Stabilizer();
						dependencies.playerCamera.stabilizer.position = dependencies.playerCamera.stabilizeTarget.position;
					}
					
					
					//... Set new position to reset point
					dependencies.player.playerContainer.transform.position = resetPoint.position;
					
					resetCompleted = true;
	        }
			
			else if(!dependencies.state.isAlive && dependencies.inputs.simpleAI && dependencies.player.rootPhysics.transform.position.y < outOfBoundsHeight && !checkedTrigger)
			{
				dependencies.player.playerContainer.transform.root.gameObject.SetActive(false);
			}
	    }
		
		
		
		//... Restore after reset
		void LateUpdate()
		{
			if(checkedTrigger && resetCompleted)
			{
				//... Re-activate physics and apply stored velocity
				foreach(Rigidbody physics in playerPhysics)
				{
					physics.isKinematic = false;
								
					if(maintainVelocity)
					{
						physics.velocity = storedVelocity;
					}
				}
	                    
	                    
	            //... Apply camera offset to new snap position
	            if(instantCameraUpdate && cameraFollowsThisPlayer)
	            {
	                dependencies.playerCamera.followCamera.transform.position = resetPoint.position + storedCameraOffset;
	            }
					
					
				//... Reset particle
				var particle = Instantiate(resetParticle, particlePoint.position, Quaternion.identity);
				particle.transform.parent = particleContainer;
				
				//... Audio
				if(resetAudioSource != null)
				{
					resetAudioSource.Play();
				}
					
				resetCompleted = false;
				checkedTrigger = false;
			}
		}
	}
}
