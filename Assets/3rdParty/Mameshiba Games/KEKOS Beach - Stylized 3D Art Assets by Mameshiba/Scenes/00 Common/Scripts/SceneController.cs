using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace MameshibaGames.KekosBeach.Common
{
    public class SceneController : MonoBehaviour
    {
        [SerializeField]
        private GameObject[] objectsToActivate;

        [SerializeField]
        private GameObject[] objectsToDeactivate;

        [SerializeField]
        private GameObject directionalLight;
        
        [SerializeField]
        private GameObject directionalLightNight;

        [SerializeField]
        private Color activeColor;

        [SerializeField]
        private Color deactivateColor;
        
        [Serializable]
        public class SkyColor
        {
            public Color topColor = Color.white;
            public Color middleColor = Color.white;
            public Color bottomColor = Color.white;
        }

        [SerializeField]
        private SkyColor onSky;
        
        [SerializeField]
        private SkyColor offSky;
        
        [SerializeField]
        private Color onFog;
        
        [SerializeField]
        private Color offFog;
        
        private bool _cameraMainMode = true;
        private bool _lightMainMode = true;
        #if UNITY_2022_1_OR_NEWER
        private Cubemap _customReflection;
        #elif UNITY_2021_1_OR_NEWER
        private Texture _customReflection;
        #else
        private Cubemap _customReflection;
        #endif
        
        private static readonly int _TopColor = Shader.PropertyToID("_TopColor");
        private static readonly int _MidColor = Shader.PropertyToID("_MidColor");
        private static readonly int _BottomColor = Shader.PropertyToID("_BottomColor");

        private List<Light> _objectsLights = new List<Light>();

        private void Awake()
        {
            _objectsLights = FindObjectsOfType<Light>().ToList();
            _objectsLights.Remove(directionalLight.GetComponent<Light>());
            _objectsLights.Remove(directionalLightNight.GetComponent<Light>());
            
            if (RenderSettings.skybox != null)
                RenderSettings.skybox = new Material(RenderSettings.skybox);

            _customReflection = RenderSettings.customReflection;
            
            ChangeCamera(_cameraMainMode);
            ChangeLight(_lightMainMode);
        }

        private void Update()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            bool changeCameraMode = Input.GetKeyDown(KeyCode.Alpha1);
            bool changeLight = Input.GetKeyDown(KeyCode.Alpha2);
#elif ENABLE_INPUT_SYSTEM
            bool changeCameraMode = Keyboard.current.digit1Key.wasPressedThisFrame;
            bool changeLight = Keyboard.current.digit2Key.wasPressedThisFrame;
#endif
            
            if (changeCameraMode)
            {
                _cameraMainMode = !_cameraMainMode;
                ChangeCamera(_cameraMainMode);
            }

            if (changeLight)
            {
                _lightMainMode = !_lightMainMode;
                ChangeLight(_lightMainMode);
            }
        }

        private void ChangeCamera(bool newValue)
        {
            foreach (GameObject obj in objectsToActivate)
            {
                obj.SetActive(newValue);
            }

            foreach (GameObject obj in objectsToDeactivate)
            {
                obj.SetActive(!newValue);
            }
        }

        private void ChangeLight(bool active)
        {
            _objectsLights.ForEach(x => x.enabled = !active);

            if (RenderSettings.fog)
                RenderSettings.fogColor = active ? onFog : offFog;
            
            directionalLight.SetActive(active);
            directionalLightNight.SetActive(!active);

            RenderSettings.ambientLight = active ? activeColor : deactivateColor;
            RenderSettings.customReflection = active ? _customReflection : null;
            if (RenderSettings.skybox != null)
            {
                RenderSettings.skybox.SetColor(_TopColor, active ? onSky.topColor : offSky.topColor);
                RenderSettings.skybox.SetColor(_MidColor, active ? onSky.middleColor : offSky.middleColor);
                RenderSettings.skybox.SetColor(_BottomColor, active ? onSky.bottomColor : offSky.bottomColor);
            }
        }
    }
}