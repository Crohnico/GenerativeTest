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
	public class PX_Camera : MonoBehaviour
	{
		[Header("Player-X [Camera]")]
		
		[Space]
		
		[Header("- Dependencies")]
		public PX_Dependencies dependencies;
		
	    [Header("- Camera")]
		public Camera followCamera;

		[Header("- Camera Stabilizing")]
		public Transform stabilizer;
		public Transform stabilizeTarget;
		public float stabilizeFrequency = 0.001f;
	    
	    [Header("- Follow Properties")]
		public Transform followPoint;
	    public float followSpeed = 14f;
		public float offset = 10f;
		
		[Header("- Follow Only (When Rotation Disabled)")]
		public bool followX = true;
		public bool followY = true;
		public bool followZ = true;
	    
	    [Header("- Rotation Properties")]
		public bool cameraRotation = false;
	    public float rotateSensitivity = 12f;
	    public float minAngle = -70f;
	    public float maxAngle = 5f;
	    
	    
	    //- Hidden Variables
		
		//...
		float 
		delta_followSpeed,
		delta_rotationSpeed,
		currentX,
		currentY;
		
		//...
	    Vector3 
		distanceOffset,
		smoothedPosition,
		cameraStartOffset,
		followVectors;
		
		//...
		Quaternion 
		rotation;

		//...
		public PX_Stabilizer 
		stabilizing = new PX_Stabilizer();
		
		
		
		//... Quick Setup
	    void Start()
	    {
			//... Cursor
	        Cursor.lockState = CursorLockMode.Locked;
	        Cursor.visible = false;
			
			//... Record Offset
			cameraStartOffset = followCamera.transform.position;
			
			//... Set initial rotation
			rotation = followCamera.transform.rotation;
	    }

		//... Gather and set data
	    void Update()
	    {
			//... Input X & Y Rotation/Sensitivity
			currentX = currentX + (dependencies.inputs.mouse_Inputs.x * Time.deltaTime) * rotateSensitivity;
			
			if(!dependencies.controller.reachingRight && !dependencies.controller.reachingLeft)
			{
				currentY = currentY + (dependencies.inputs.mouse_Inputs.y * Time.deltaTime) * rotateSensitivity;
			}
			
			//... Limits
			currentY = Mathf.Clamp(currentY, minAngle, maxAngle);
	    }
		
		
	    
	    //... Apply data
	    void FixedUpdate()
	    {
			if(dependencies.state.isAlive)
			{
				//... Convert to delta
				delta_followSpeed = followSpeed * Time.fixedDeltaTime;
				


				//... Calculate Stabilization
		        stabilizing.frequency = stabilizeFrequency;

		        Vector3 stabilizedPos = stabilizing.Step(Time.time, stabilizeTarget.position);
		        stabilizer.position = stabilizedPos;
				


				//... With Camera Rotation
				if(cameraRotation)
				{
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
					smoothedPosition = Vector3.Lerp(followCamera.transform.position, followPoint.position + (rotation * distanceOffset), delta_followSpeed);
					followCamera.transform.position = smoothedPosition;
					
					//... Look at player
					followCamera.transform.LookAt(followPoint.position);
				}
				
				
				//... Follow in one or all Vectors with NO Camera Rotation 
				else
				{
					//... Follow
					
					//... X
					if(followX)
					{
						float followVectorsX = Mathf.Lerp(followCamera.transform.position.x, followPoint.position.x + cameraStartOffset.x, delta_followSpeed);
						followVectors = new Vector3(followVectorsX, followVectors.y, followVectors.z);
					}
					
					//... No X
					else
					{
						followVectors = new Vector3(cameraStartOffset.x, followVectors.y, followVectors.z);
					}
					
					
					//... Y
					if(followY)
					{
						float followVectorsY = Mathf.Lerp(followCamera.transform.position.y, followPoint.position.y + cameraStartOffset.y, delta_followSpeed);
						followVectors = new Vector3(followVectors.x, followVectorsY, followVectors.z);
					}
					
					//... No Y
					else
					{
						followVectors = new Vector3(followVectors.x, cameraStartOffset.y, followVectors.z);
					}
					
					
					//... Z
					if(followZ)
					{
						float followVectorsZ = Mathf.Lerp(followCamera.transform.position.z, followPoint.position.z + offset, delta_followSpeed);
						followVectors = new Vector3(followVectors.x, followVectors.y, followVectorsZ);
					}
					
					//... No Z
					else
					{
						followVectors = new Vector3(followVectors.x, followVectors.y, offset);
					}
					
					//... Apply follow vectors
					followCamera.transform.position = followVectors;
				}
			}
	    }
	}
}
