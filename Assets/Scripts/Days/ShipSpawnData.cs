using System;
using UnityEngine;

namespace Prototypes.Alex.Days
{
    [Serializable]
    public class ShipSpawnData
    {
        internal float SpawnWeight => spawnWeight;
        internal FLAG ShipType => shipType;
        internal FLAG CargoType => cargoType;
        
        [SerializeField, Min(0f)]
        private float spawnWeight;
        [SerializeField]
        private FLAG shipType;
        [SerializeField]
        private FLAG cargoType;
    }
}