using UnityEngine;
using UnityEngine.UI;


//-------------------------------------------------------------
    //--APR Player
    //--FPSCounter
//-------------------------------------------------------------


namespace ARP.Examples.Dependancies.Scripts
{
    public class FPSCounter : MonoBehaviour
    {
        private Text fpsText;
    
        float frameCount = 0f, dt = 0f, fps = 0f, updateRate = 4f;
    
        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        
            fpsText = this.GetComponent<Text>();
        }

        void Update()
        {
            frameCount++;
            dt += Time.deltaTime;
            if (dt > 1.0f/updateRate)
            {
                fps = frameCount / dt ;
                frameCount = 0f;
                dt -= 1.0f/updateRate;
            
                fps = Mathf.RoundToInt(fps);
                fpsText.text = "50 APR Players - FPS: " + fps.ToString();
            }
        }
    }
}
