using UnityEngine;

namespace MameshibaGames.KekosBeach.Common
{
    public class MakeAllRigidbodiesKinematic : MonoBehaviour
    {
        private void Awake()
        {
            foreach (Rigidbody rigid in FindObjectsOfType<Rigidbody>())
            {
                rigid.isKinematic = true;
            }
        }
    }
}