//------------
//... PLayer-X
//... V2.0.1
//... © TheFamousMouse™
//--------------------
//... Support email:
//... thefamousmouse.developer@gmail.com
//--------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;
using PlayerX;

namespace PlayerX
{
	public class PX_CameraOrbit : MonoBehaviour
	{
		[Header("Player-X [Camera Orbit]")]
		
		[Space]
		
		[Header("- Properties")]
		public Transform orbitPoint;
		public float offset = 10f;
		public float rotateSensitivity = 12f;
	    public float minAngle = -70f;
	    public float maxAngle = 5f;
		public float zoomMin = 70;
		public float zoomMax = 100;
		public float zoomSpeed = 10f;
		public float zoomSmoothness = 2f;
		
		//- Hidden Variables
		
		//...
		float 
		delta_followSpeed,
		delta_rotationSpeed,
		currentX,
		currentY,
		zoom;
		
		//...
	    Vector3 
		distanceOffset,
		smoothedPosition,
		cameraStartOffset,
		followVectors;
		
		//...
		Quaternion 
		rotation;
	    
	    //... Quick Setup
	    void Start()
	    {
			//... Cursor
	        Cursor.lockState = CursorLockMode.Locked;
	        Cursor.visible = false;
			
			//... Record Offset
			cameraStartOffset = transform.position;
			
			zoom = this.GetComponent<Camera>().fieldOfView;
			
			//... Set initial rotation
			rotation = transform.rotation;
	    }

	    
	    //... Gather and set data
	    void Update()
	    {
			//... Mouse output
			var	mouse_Inputs = Mouse.current.delta.ReadValue();
			var	mouseScroll_Input = Mouse.current.scroll.ReadValue();
			
			//... Input X & Y Rotation/Sensitivity
			currentX = currentX + (mouse_Inputs.x * Time.deltaTime) * rotateSensitivity;
			currentY = currentY + (mouse_Inputs.y * Time.deltaTime) * rotateSensitivity;
			
			//... Limits
			currentY = Mathf.Clamp(currentY, minAngle, maxAngle);
			
			if(mouseScroll_Input.y != 0 && zoom >= zoomMin && zoom <= zoomMax)
			{
				var zoomSpeedNormalized = mouseScroll_Input.normalized;
				
				var newZoomSpeed = zoomSpeedNormalized.y * -zoomSpeed;
				
				zoom = this.GetComponent<Camera>().fieldOfView + newZoomSpeed;
			}
			
			if(zoom < zoomMin)
			{
				zoom = zoomMin;
			}
			
			if(zoom > zoomMax)
			{
				zoom = zoomMax;
			}
			
			this.GetComponent<Camera>().fieldOfView = Mathf.Lerp(this.GetComponent<Camera>().fieldOfView, zoom, zoomSmoothness * Time.deltaTime);
	    }
		
		//... Apply data
	    void FixedUpdate()
	    {
			//... Convert to delta
			delta_followSpeed = 10 * Time.fixedDeltaTime;
			
			//... Check if offset is inverted
			if(offset < 0)
			{
				rotation = Quaternion.Euler(-currentY, currentX, 0);
			}
			
			else
			{
				rotation = Quaternion.Euler(currentY, currentX, 0);
			}
			
			//... Follow player
			distanceOffset = new Vector3(0, 0, offset);
			smoothedPosition = Vector3.Lerp(transform.position, orbitPoint.position + (rotation * distanceOffset), delta_followSpeed);
			transform.position = smoothedPosition;
			
			//... Look at player
			transform.LookAt(orbitPoint.position);
	    }
	}
}
