using System;
using GameInput;
using UnityEngine;
using UnityEngine.UI;

namespace Prototypes.Alex
{
    public class LookingGlass : MonoBehaviour
    {
        public Camera mainCamera;
        public Image lookingGlassSilouhette;
        public float zoomedFOV;
        public float normalFOV;
        
        private bool isLooking;

        private void OnEnable()
        {
            GameInputDelegator.OnRightClick += OnRightClick;
        }

        private void Start()
        {
            lookingGlassSilouhette.enabled = false;
        }

        private void OnDisable()
        {
            GameInputDelegator.OnRightClick -= OnRightClick;
        }
        
        private void OnRightClick(bool pressed)
        {
            isLooking = pressed;
            
            lookingGlassSilouhette.enabled = pressed;
            mainCamera.fieldOfView = pressed ? zoomedFOV : normalFOV;
        }
    }
}