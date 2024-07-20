using UnityEngine;

namespace MameshibaGames.KekosBeach.Common
{
    [RequireComponent(typeof(Animation))]
    public class RandomizeAnimation : MonoBehaviour
    {
        private Animation _myAnimation;

        private void OnEnable()
        {
            _myAnimation = GetComponent<Animation>();
            _myAnimation.Stop();
            Invoke(nameof(StartAnimation), Random.Range(0, _myAnimation.clip.length));
        }

        private void StartAnimation()
        {
            _myAnimation.Play();
        }
    }
}