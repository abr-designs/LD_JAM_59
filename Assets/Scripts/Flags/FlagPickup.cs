using System;
using NaughtyAttributes;
using Prototypes.Alex.Interactables;
using UnityEngine;

namespace Prototypes.Alex
{
    public class FlagPickup : MonoBehaviour, IInteractable
    {
        private static FPSHandsController s_fpsHandsController;
        private static PlayerFlagInventory s_playerFlagInventory;

        [SerializeField, ReadOnly]
        private FLAG flagType;
        
        private SpriteRenderer spriteRenderer;

        private void Start()
        {
            if(s_playerFlagInventory == null)
                s_playerFlagInventory  = FindAnyObjectByType<PlayerFlagInventory>();
            
            if(s_fpsHandsController == null)
                s_fpsHandsController  = FindAnyObjectByType<FPSHandsController>();
        }

        public void Setup(FlagDefinition flagDefinition)
        {
            flagType = flagDefinition.flag;
        }

        public void OnInteract()
        {
            s_playerFlagInventory.HoldFlag(flagType);
            s_fpsHandsController.SetState(FPSHandsController.HandState.Grabbing);
        }
    }
}