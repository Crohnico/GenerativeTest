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
	public class PX_Inputs : MonoBehaviour
	{
		[Header("Player-X [Inputs]")]
		
		[Space]
		
		[Header("- Input Dependencies")]
		public PX_Dependencies dependencies;
		
		[Header("- AI")]
		public bool simpleAI = false;
		
		//- Hidden Variables
		
		//...
		[HideInInspector]
		public Vector2 
		mouse_Inputs, key_Inputs;
		
		//...
		[HideInInspector]
		public bool 
		mouseLeft_input, mouseRight_input, 
		keyLeft_Input, keyRight_Input, 
		keyForward_Input, keyBackward_Input, 
		keyJump_Input, keyRun_Input,
		keyLook_Input, keyKneel_Input, 
		keyPunchRight_Input, keyPunchLeft_Input, 
		keyKickRight_Input, keyKickLeft_Input,
		keyEquipLeft_Input, keyEquipRight_Input,
		velocityModeChange_Input,
		slowMotion_Input,
		restart_Input,
		exit_Input;
		
		//... Simple AI variables
		float actionTime;
		bool actionPerform;
		

		//... Calculate Input ...
	    void Update()
	    {
			//... Human Input
			//-
			
			if(!simpleAI)
			{
				//... Mouse inputs
				mouseLeft_input = Mouse.current.leftButton.isPressed;
				mouseRight_input = Mouse.current.rightButton.isPressed;
				
				//... Mouse output
				mouse_Inputs = Mouse.current.delta.ReadValue();
				
				//... Key inputs
				keyLeft_Input = Keyboard.current.aKey.isPressed;
				keyRight_Input = Keyboard.current.dKey.isPressed;
				keyForward_Input = Keyboard.current.wKey.isPressed;
				keyBackward_Input = Keyboard.current.sKey.isPressed;
				
				keyJump_Input = Keyboard.current.spaceKey.wasPressedThisFrame;
				keyRun_Input = Keyboard.current.leftShiftKey.isPressed;
				keyLook_Input = Keyboard.current.fKey.isPressed;
				keyKneel_Input = Keyboard.current.leftCtrlKey.isPressed;
				
				keyPunchLeft_Input = Keyboard.current.qKey.isPressed;
				keyPunchRight_Input = Keyboard.current.eKey.isPressed;
				keyKickLeft_Input = Keyboard.current.zKey.isPressed;
				keyKickRight_Input = Keyboard.current.cKey.isPressed;
				
				keyEquipLeft_Input = Keyboard.current.gKey.wasPressedThisFrame;
				keyEquipRight_Input = Keyboard.current.hKey.wasPressedThisFrame;
				
				slowMotion_Input = Keyboard.current.nKey.wasPressedThisFrame;
				
				velocityModeChange_Input = Keyboard.current.mKey.wasPressedThisFrame;
				
				restart_Input = Keyboard.current.rKey.wasPressedThisFrame;
				
				exit_Input = Keyboard.current.escapeKey.wasPressedThisFrame;
				
				
				//... Key Output values
				if(keyForward_Input)
				{
					key_Inputs.y = 1;
				}
				
				else if(keyBackward_Input)
				{
					key_Inputs.y = -1;
				}
				
				else
				{
					key_Inputs.y = 0;
				}
				
				
				if(keyLeft_Input)
				{
					key_Inputs.x = -1;
				}
				
				else if(keyRight_Input)
				{
					key_Inputs.x = 1;
				}
				
				else
				{
					key_Inputs.x = 0;
				}
			}
			
			
			//----------------------
			
			
			//... Simple AI example
			//-
			else
			{
				if(dependencies.state.isAlive && !dependencies.state.isKnockedOut && dependencies.controller.trackObject != null)
				{
					//... Simple AI action example
					if(dependencies.controller.trackObject.transform.root.GetComponent<PX_Dependencies>() && dependencies.controller.trackObject.transform.root.GetComponent<PX_Dependencies>().state.isAlive
					&& !dependencies.controller.trackObject.transform.root.GetComponent<PX_Dependencies>().state.isKnockedOut)
					{
						if(!actionPerform && Vector3.Distance(dependencies.player.rootPhysics.transform.position, dependencies.controller.trackObject.position) < 3f)
						{
							actionPerform = true;
							
							//... Pick a random number that will dictate the action
							var actionRondom = Random.Range(0, 4);
							actionTime = Random.Range(0.5f, 1.5f);
							
							//... Punch left
							if(actionRondom == 0)
							{
								keyPunchLeft_Input = true;
								Invoke(nameof(PunchLeft), actionTime);
							}
							
							//... Punch right
							else if(actionRondom == 1)
							{
								keyPunchRight_Input = true;
								Invoke(nameof(PunchRight), actionTime);
							}
							
							//... Kick left
							else if(actionRondom == 2)
							{
								keyKickLeft_Input = true;
								Invoke(nameof(KickLeft), actionTime);
							}
							
							//... Kick right
							else if(actionRondom == 3)
							{
								keyKickRight_Input = true;
								Invoke(nameof(KickRight), actionTime);
							}
						}
						
						//... Simple AI move & direction example
						if(Vector3.Distance(dependencies.player.rootPhysics.transform.position, dependencies.controller.trackObject.position) > 3f 
						&& Vector3.Distance(dependencies.player.rootPhysics.transform.position, dependencies.controller.trackObject.position) < dependencies.controller.headTrackDistance)
						{
							dependencies.controller.moveDir = (dependencies.player.rootPhysics.transform.forward);
							
							dependencies.controller.moveDir.y = 0f;
							dependencies.controller.moveDir = dependencies.controller.moveDir.normalized;
							
							key_Inputs.y = 1;
						}
						
						//... Stop moving the Ai
						else
						{
							dependencies.controller.moveDir = Vector3.zero;
							key_Inputs.y = 0;
						}
					}
					
					//... Stop moving the Ai
					else
					{
						dependencies.controller.moveDir = Vector3.zero;
						key_Inputs.y = 0;
					}
					
					//... Simple AI weapon check
					if(!dependencies.weapons.simpleAIWeaponCheck)
					{
						if(!dependencies.weapons.weaponAssignedLeft && !dependencies.weapons.weaponAssignedRight)
						{
							dependencies.weapons.simpleAIWeaponCheck = true;
							Invoke(nameof(AttemptWeaponPick), Random.Range(0.1f, 0.3f));
						}
					}
				}
			}
	    }
		
		//... Simple AI Functions
		//... AI Release left punch
		void PunchLeft()
		{
			keyPunchLeft_Input = false;
			Invoke(nameof(actionCoolDown), actionTime);
		}
		
		//... AI Release right punch
		void PunchRight()
		{
			keyPunchRight_Input = false;
			Invoke(nameof(actionCoolDown), actionTime);
		}
		
		//... AI Release left kick
		void KickLeft()
		{
			keyKickLeft_Input = false;
			Invoke(nameof(actionCoolDown), actionTime);
		}
		
		//... AI Release right kick
		void KickRight()
		{
			keyKickRight_Input = false;
			Invoke(nameof(actionCoolDown), actionTime);
		}
		
		//... AI Attempt to pickup weapon
		void AttemptWeaponPick()
		{
			//... AI Weapon left hand pickup
			if(!dependencies.weapons.weaponAssignedLeft)
			{
				keyEquipLeft_Input = true;
				Invoke(nameof(EquipedLeft), Time.smoothDeltaTime);
			}
			
			//... AI Weapon right hand pickup
			else if(!dependencies.weapons.weaponAssignedRight)
			{
				keyEquipRight_Input = true;
				Invoke(nameof(EquipedRight), Time.smoothDeltaTime);
			}
			
			dependencies.weapons.simpleAIWeaponCheck = false;
		}
		
		//... AI Release equip weapon left
		void EquipedLeft()
		{
			keyEquipLeft_Input = false;
		}
		
		//... AI Release equip weapon right
		void EquipedRight()
		{
			keyEquipRight_Input = false;
		}
		
		//... AI Allow next action
		void actionCoolDown()
		{
			actionPerform = false;
		}
	}
}
