using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem;
#endif

namespace MameshibaGames.KekosBeach.Common
{
    public class ControlsHandler : MonoBehaviour
    {
        [SerializeField] 
        private SimpleCharacterController simpleCharacterController;

        private void Awake()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER && UNITY_EDITOR
            StandaloneInputModule standaloneInputModule = FindObjectOfType<StandaloneInputModule>();
            if (standaloneInputModule != null)
            {
                GameObject standAloneGameObject = standaloneInputModule.gameObject;
                Destroy(standaloneInputModule);
                InputSystemUIInputModule inputSystem = standAloneGameObject.AddComponent<InputSystemUIInputModule>();
                inputSystem.enabled = false;
                inputSystem.enabled = true;
            }
#endif
        }

        private void Update()
        {
            CheckInputs();
        }

        private void CheckInputs()
        {
            // Old input backends are enabled.
#if ENABLE_LEGACY_INPUT_MANAGER
            simpleCharacterController.jump = Input.GetKeyDown(KeyCode.Space);
            simpleCharacterController.left = Input.GetKey(KeyCode.A);
            simpleCharacterController.right = Input.GetKey(KeyCode.D);
            simpleCharacterController.up = Input.GetKey(KeyCode.W);
            simpleCharacterController.down = Input.GetKey(KeyCode.S);
            simpleCharacterController.sprint = Input.GetKey(KeyCode.LeftShift);

            // New input system backends are enabled.
#elif ENABLE_INPUT_SYSTEM
            simpleCharacterController.jump = Keyboard.current.spaceKey.wasPressedThisFrame;
            simpleCharacterController.left = Keyboard.current.aKey.isPressed;
            simpleCharacterController.right = Keyboard.current.dKey.isPressed;
            simpleCharacterController.up = Keyboard.current.wKey.isPressed;
            simpleCharacterController.down = Keyboard.current.sKey.isPressed;
            simpleCharacterController.sprint = Keyboard.current.leftShiftKey.isPressed;
#endif
        }
    }
}
