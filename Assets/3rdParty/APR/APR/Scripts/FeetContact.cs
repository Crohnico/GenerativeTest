using UnityEngine;


//-------------------------------------------------------------
    //--APR Player
    //--Feet Contact
//-------------------------------------------------------------


namespace ARP.APR.Scripts
{
    public class FeetContact : MonoBehaviour
    {
        public APRController APR_Player;
	
        //Alert APR player when feet colliders enter ground object layer
        void OnCollisionEnter(Collision col)
        {
            if(!APR_Player.isJumping && APR_Player.inAir)
            {
                if(col.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    APR_Player.PlayerLanded();
                }
            }
        }
    }
}
