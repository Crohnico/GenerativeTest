//------------
//... PLayer-X
//... V2.0.1
//... © TheFamousMouse™
//--------------------
//... Support email:
//... thefamousmouse.developer@gmail.com
//--------------------------------------

using UnityEngine.InputSystem;
using UnityEngine;
using PlayerX;

namespace PlayerX
{
	public class PX_SlowMotion : MonoBehaviour
	{
		[Header("Player-X [Slow Motion]")]
		
		[Space]
		
		[Header("- Slow Motion Dependencies")]
		public PX_Dependencies dependencies;
		
		[Header("- Properties")]
		public float slowMotionSpeed = 0.3f;
		public bool noInputMotion;
		
		
		
	    void LateUpdate()
	    {
			if(noInputMotion)
			{
				//... Slow motion when there is no input
				if(dependencies.controller.moveDir != Vector3.zero || dependencies.inputs.mouseLeft_input || dependencies.inputs.mouseRight_input || dependencies.controller.jumping || dependencies.inputs.keyKickLeft_Input || dependencies.inputs.keyKickRight_Input || dependencies.inputs.keyPunchLeft_Input || dependencies.inputs.keyPunchRight_Input)
				{
					if(Time.timeScale != 1)
					{
						Time.timeScale = 1;
					}
					
					var fovSmooth = Mathf.Lerp(dependencies.playerCamera.followCamera.fieldOfView, 70f, 6f * Time.deltaTime);
					dependencies.playerCamera.followCamera.fieldOfView = fovSmooth;
				}
				
				else
				{
					if(Time.timeScale != slowMotionSpeed)
					{
						Time.timeScale = slowMotionSpeed;
					}
					
					var fovSmooth = Mathf.Lerp(dependencies.playerCamera.followCamera.fieldOfView, 40f, 3f * Time.deltaTime);
					dependencies.playerCamera.followCamera.fieldOfView = fovSmooth;
				}
			}
			
			else
			{
				//... Slow motion upon specific key
				if(dependencies.inputs.slowMotion_Input)
				{
					if(Time.timeScale != 1)
					{
						Time.timeScale = 1;
					}
					
					else if(Time.timeScale != slowMotionSpeed)
					{
						Time.timeScale = slowMotionSpeed;
					}
				}
				
				if(Time.timeScale == 1)
				{
					var fovSmooth = Mathf.Lerp(dependencies.playerCamera.followCamera.fieldOfView, 70f, 6f * Time.deltaTime);
					dependencies.playerCamera.followCamera.fieldOfView = fovSmooth;
				}
				
				if(Time.timeScale == slowMotionSpeed)
				{
					var fovSmooth = Mathf.Lerp(dependencies.playerCamera.followCamera.fieldOfView, 40f, 3f * Time.deltaTime);
					dependencies.playerCamera.followCamera.fieldOfView = fovSmooth;
				}
			}
	    }
	}
}