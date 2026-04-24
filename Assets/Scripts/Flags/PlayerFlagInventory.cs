using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Prototypes.Alex
{
    public class PlayerFlagInventory : MonoBehaviour
    {
        public static event Action<List<FLAG>> OnHeldFlagsChanged;
        public static event Action OnMaxFlagsReached;
        
        public bool IsFull => holdingFlags.Count >= maxFlags;

        [SerializeField, Min(1)]
        private int maxFlags = 3;
        [ReadOnly]
        public List<FLAG> holdingFlags = new();

        public void HoldFlag(FLAG flag)
        {
            if (holdingFlags.Count == maxFlags)
            {
                OnMaxFlagsReached?.Invoke();
                return;
            }
            
            holdingFlags.Add(flag);
            OnHeldFlagsChanged?.Invoke(holdingFlags);
        }
        
        public void DropAllFlags()
        {
            holdingFlags.Clear();
            OnHeldFlagsChanged?.Invoke(holdingFlags);
        }
        
    }
}