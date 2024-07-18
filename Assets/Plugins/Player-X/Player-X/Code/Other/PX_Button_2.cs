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
    public class PX_Button_2 : MonoBehaviour
    {
    	[Header("Player-X [Button]")]
    	
    	[Space]
    	
    	[Header("- References")]
    	public PX_Dependencies playerToPush;
    	public GameObject[] lasers;
        public AudioSource soundSource;
    	
    	//- Hidden Variables
    	
        bool isReverseButton;
    	bool pressed;
    	
    	
        
        void OnCollisionEnter(Collision other)
        {
        	//... Disable
            if(!isReverseButton && !pressed)
            {
                if(other.gameObject == playerToPush.controller.grabDetectLeft.gameObject || other.gameObject ==  playerToPush.controller.grabDetectRight.gameObject)
                {
                    if(playerToPush.inputs.mouseLeft_input || playerToPush.inputs.mouseRight_input)
                    {
    					pressed = true;
    					
    					//... Switch off lasers
                        foreach(GameObject laser in lasers)
    					{
    						laser.SetActive(false);
    					}
                        
                        //... Audio
                        soundSource.Stop();
                        
                        if(!soundSource.isPlaying)
                        {
                            soundSource.Play();
                        }
    					
    					isReverseButton = true;
                    }
                }
            }
            
            //... Enable
            else if(isReverseButton && !pressed)
            {
                if(other.gameObject == playerToPush.controller.grabDetectLeft.gameObject || other.gameObject ==  playerToPush.controller.grabDetectRight.gameObject)
                {
                    if(playerToPush.inputs.mouseLeft_input || playerToPush.inputs.mouseRight_input)
                    {
    					pressed = true;
    					
    					//... Switch on lasers
                        foreach(GameObject laser in lasers)
    					{
    						laser.SetActive(true);
    					}

    					//... Audio
                        soundSource.Stop();
                        
                        if(!soundSource.isPlaying)
                        {
                            soundSource.Play();
                        }
    					
    					isReverseButton = false;
                    }
                }
            }
        }
    	
    	
    	void OnCollisionExit(Collision other)
        {
        	//... Reset
            if(pressed)
            {
                if(other.gameObject == playerToPush.controller.grabDetectLeft.gameObject || other.gameObject ==  playerToPush.controller.grabDetectRight.gameObject)
                {
                    if(!playerToPush.inputs.mouseLeft_input || !playerToPush.inputs.mouseRight_input)
                    {
    					pressed = false;
                    }
                }
            }
    	}
        
    }
}
