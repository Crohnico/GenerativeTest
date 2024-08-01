using UnityEngine;


//-------------------------------------------------------------
    //--APR Player
    //--Object Out Of Bounds
//-------------------------------------------------------------



namespace ARP.Examples.Dependancies.Scripts
{
    public class ObjectOutOfBound : MonoBehaviour
    {
        public Transform objResetPoint;
        private Rigidbody rb;

        void OnTriggerEnter(Collider col)
        {
            if(col.gameObject.layer != LayerMask.NameToLayer("Player_1"))
            {
                if(col.gameObject.GetComponent<Rigidbody>() != null)
                {
                    rb = col.gameObject.GetComponent<Rigidbody>();
                    rb.transform.position = objResetPoint.position;
                }
            }
        }
    }
}
