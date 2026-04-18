using System;
using NaughtyAttributes;
using Prototypes.Alex.Interactables;
using UnityEngine;

namespace Prototypes.Alex
{
    public class FlagPickup : MonoBehaviour, IInteractable
    {
        private static PlayerFlagInventory s_playerFlagInventory;

        [SerializeField, ReadOnly]
        private FLAG flagType;
        
        private SpriteRenderer spriteRenderer;

        private void Start()
        {
            if(s_playerFlagInventory == null)
                s_playerFlagInventory  = FindAnyObjectByType<PlayerFlagInventory>();
        }

        public void Setup(FlagDefinition flagDefinition)
        {
            flagType = flagDefinition.flag;
        }

        public void OnInteract()
        {
            s_playerFlagInventory.HoldFlag(flagType);
        }
    }
}