using Prototypes.Alex;
using Prototypes.Alex.Interactables;
using UnityEngine;
using Utilities.Animations;

public class TrashCanInteractable : MonoBehaviour, IInteractable
{
    private static FPSHandsController s_fpsHandsController;
    private static PlayerFlagInventory s_playerFlagInventory;
    
    [SerializeField]
    private TransformAnimator transformAnimator;
    
    private void Start()
    {
        if(s_playerFlagInventory == null)
            s_playerFlagInventory = FindAnyObjectByType<PlayerFlagInventory>();
        
        if (s_fpsHandsController == null)
            s_fpsHandsController = FindAnyObjectByType<FPSHandsController>();
    }

    public void OnInteract()
    {
        s_playerFlagInventory.DropAllFlags();
        s_fpsHandsController.SetState(FPSHandsController.HandState.Dropping);
        transformAnimator.Play();
    }
}
