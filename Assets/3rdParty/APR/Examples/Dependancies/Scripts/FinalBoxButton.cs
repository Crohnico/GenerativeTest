using UnityEngine;


//-------------------------------------------------------------
    //--APR Player
    //--FinalBoxButton
//-------------------------------------------------------------


namespace ARP.Examples.Dependancies.Scripts
{
    public class FinalBoxButton : MonoBehaviour
    {
        public string CorrectBox = "finalbox";
        public GameObject billboard;
    
        private bool correct;
    
    
        void OnTriggerStay(Collider col)
        {
            if(!correct && col.gameObject.name == CorrectBox)
            {
                correct = true;
            }
        }
    
        void OnTriggerExit(Collider col)
        {
            if(correct && col.gameObject.name == CorrectBox)
            {
                correct = false;
            }
        }

    
        void Update()
        {
            if(correct)
            {
                billboard.SetActive(true);
            }
            
            else if(!correct)
            {
                billboard.SetActive(false);
            }
        }
    }
}
