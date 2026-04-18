using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Prototypes.Alex
{
    public class PlayerFlagInventory : MonoBehaviour
    {
        public static event Action<List<FLAG>> OnHeldFlagsChanged;
        
        [ReadOnly]
        public List<FLAG> holdingFlags = new();

        public void HoldFlag(FLAG flag)
        {
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