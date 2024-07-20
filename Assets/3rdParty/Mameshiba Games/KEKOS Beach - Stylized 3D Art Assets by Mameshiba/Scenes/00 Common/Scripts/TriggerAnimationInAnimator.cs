using UnityEngine;

namespace MameshibaGames.KekosBeach.Common
{
    [RequireComponent(typeof(Animator))]
    public class TriggerAnimationInAnimator : MonoBehaviour
    {
        [SerializeField]
        private string triggerName;

        private void OnEnable()
        {
            GetComponent<Animator>().SetTrigger(triggerName);
        }
    }
}
