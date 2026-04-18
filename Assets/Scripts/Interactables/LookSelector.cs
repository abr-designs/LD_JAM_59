using GameInput;
using UnityEngine;

namespace Prototypes.Alex.Interactables
{
    public class LookSelector : MonoBehaviour
    {
        public Transform cameraTransform;
        public float interactDistance = 5f;
        public LayerMask interactLayer;

        private void OnEnable()
        {
            GameInputDelegator.OnLeftClick += OnLeftClick;
        }

        private void OnDisable()
        {
            GameInputDelegator.OnLeftClick -= OnLeftClick;
        }
    
        private void OnLeftClick(bool pressed)
        {
            if (!pressed)
                return;
        
            TrySelect();
        }

        private void TrySelect()
        {
            var ray = new Ray(cameraTransform.position, cameraTransform.forward);

            if (!Physics.Raycast(ray, out var hit, interactDistance, interactLayer)) 
                return;
            Debug.Log("Hit: " + hit.collider.name);

            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.OnInteract();
            }
        }
    }
}