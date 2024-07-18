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
	public class PX_GrabDetector : MonoBehaviour
	{
		[Header("Player-X [Grab Detector]")]
		
		[Space]
		
		[Header("- Grab Dependencies")]
		public PX_Dependencies dependencies;
		
		[Header("- Grab Detection Layers")]
		public LayerMask grabLayers;
		
		//- Hidden Variables
		
		[HideInInspector]
		public Rigidbody 
		detectedObject;
		
		//...
		[HideInInspector]
		public ConfigurableJoint 
		connectedJointLeft, 
		connectedJointRight;
		
		//...
		[HideInInspector]
		public bool 
		collisionDetected;
		
		
		
		//... Collision Detection
		void OnCollisionEnter(Collision col)
		{
			//... Check layer
			if(!collisionDetected && grabLayers == (grabLayers | (1 << col.gameObject.layer)) && col.gameObject.transform.root.gameObject != this.gameObject.transform.root.gameObject)
			{
				//... Check physics body
				if(col.gameObject.GetComponent<Rigidbody>() != null)
				{
					//... Assign detected object for controller script to use
					collisionDetected = true;
					detectedObject = col.gameObject.GetComponent<Rigidbody>();
				}
			}
		}
		
		
		
		//... Revert if not grabbing
		void LateUpdate()
		{
			if(detectedObject != null && connectedJointLeft == null && !dependencies.inputs.mouseLeft_input)
			{
				collisionDetected = false;
				detectedObject = null;
			}
			
			if(detectedObject != null && connectedJointRight == null && !dependencies.inputs.mouseRight_input)
			{
				collisionDetected = false;
				detectedObject = null;
			}
		}
	}
}
