using UnityEngine;


//-------------------------------------------------------------
    //--APR Player
    //--MultiBoxButton
//-------------------------------------------------------------


namespace ARP.Examples.Dependancies.Scripts
{
    public class MultiBoxButton : MonoBehaviour
    {
        public MultiBoxDoorButton controller;
    
        private bool pressed = false;
    
        void OnTriggerStay(Collider col)
        {
            if(!pressed)
            {
                pressed = true;
                controller.pressedButtons += 1;
            }
        }
    
        void OnTriggerExit(Collider col)
        {
            if(pressed)
            {
                pressed = false;
                controller.pressedButtons -= 1;
            }
        }
    }
}
