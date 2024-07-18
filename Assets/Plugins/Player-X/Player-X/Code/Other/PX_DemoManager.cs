//------------
//... PLayer-X
//... V2.0.1
//... © TheFamousMouse™
//--------------------
//... Support email:
//... thefamousmouse.developer@gmail.com
//--------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using PlayerX;

namespace PlayerX
{
	public class PX_DemoManager : MonoBehaviour
	{
		public PX_Dependencies dependencies;
		
		public bool canChangeMoveMode = true;
		public bool canRestart = true;
		public bool canExitToMenu = true;
		
	    void LateUpdate()
	    {
			//... If player input found
			if(dependencies != null)
			{
				//... Movement type toggle
				if(dependencies.inputs.velocityModeChange_Input && canChangeMoveMode)
				{
					if(!dependencies.controller.velocityMode)
					{
						dependencies.controller.velocityMode = true;
					}
				
					else
					{
						dependencies.controller.velocityMode = false;
					}
				}
				
				//... Restart Scene
				if(dependencies.inputs.restart_Input && canRestart)
				{
					SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
				}
				
				//... Return to menu
				if(dependencies.inputs.exit_Input && canExitToMenu)
				{
					SceneManager.LoadScene(0);
				}
			}
			
			//... If no player inputs found
			else
			{
				//... Return to menu
				if(Keyboard.current.nKey.wasPressedThisFrame && canExitToMenu)
				{
					if(Time.timeScale != 1)
					{
						Time.timeScale = 1;
					}
					
					else if(Time.timeScale != 0.3f)
					{
						Time.timeScale = 0.3f;
					}
				}
				
				//... Restart Scene
				if(Keyboard.current.rKey.wasPressedThisFrame && canRestart)
				{
					SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
				}
				
				//... Return to menu
				if(Keyboard.current.escapeKey.wasPressedThisFrame && canExitToMenu)
				{
					SceneManager.LoadScene(0);
				}
			}
	    }
	}
}
