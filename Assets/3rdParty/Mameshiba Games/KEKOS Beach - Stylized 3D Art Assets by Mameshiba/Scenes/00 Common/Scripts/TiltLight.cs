using UnityEngine;
using Random = UnityEngine.Random;

namespace MameshibaGames.KekosBeach.Common
{
    [RequireComponent(typeof(Light))]
    public class TiltLight : MonoBehaviour
    {
        [SerializeField] private float minIntensity;
        [SerializeField] private float maxIntensity;
        [SerializeField] private float tiltSpeed;

        private float _offset;
        private Light _light;

        private void Awake()
        {
            _light = GetComponent<Light>();
            _offset = Random.Range(0, 100f);
        }

        private void Update()
        {
            _offset += Time.deltaTime * tiltSpeed;
            float perlinValue = Mathf.PerlinNoise(_offset, _offset);
            _light.intensity = Remap(perlinValue, 0, 1, minIntensity, maxIntensity);
        }

        private float Remap(float value, float from1, float to1, float from2, float to2) => 
            (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
