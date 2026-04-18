using GameInput;
using UnityEngine;

namespace Prototypes.Alex.Interactables
{
    public class LookSelector : MonoBehaviour
    {
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

        void TrySelect()
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
            {
                Debug.Log("Hit: " + hit.collider.name);

                // Option 1: Interface-based interaction
                var interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.OnInteract();
                }
            }
        }
    }
}