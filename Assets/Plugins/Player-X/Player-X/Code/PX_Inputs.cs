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

        [HideInInspector]
        public Vector2
        mouse_Inputs, key_Inputs;

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


        void Update()
        {
            if (dependencies.networkObject)
            {
                mouseLeft_input = Mouse.current.leftButton.isPressed;
                mouseRight_input = Mouse.current.rightButton.isPressed;

                mouse_Inputs = Mouse.current.delta.ReadValue();

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

                if (keyForward_Input)
                {
                    key_Inputs.y = 1;
                }

                else if (keyBackward_Input)
                {
                    key_Inputs.y = -1;
                }

                else
                {
                    key_Inputs.y = 0;
                }


                if (keyLeft_Input)
                {
                    key_Inputs.x = -1;
                }

                else if (keyRight_Input)
                {
                    key_Inputs.x = 1;
                }

                else
                {
                    key_Inputs.x = 0;
                }
            }
        }
    }
}
