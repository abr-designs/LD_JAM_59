using Audio;
using Audio.SoundFX;
using NaughtyAttributes;
using UnityEngine;

namespace Prototypes.Alex.Boats
{
    public class BaseBoat : MonoBehaviour
    {
        public FLAG CargoType => cargoType;
        public FLAG ShipType => shipType;
        public bool IsDocked => isDocked;
        
        [SerializeField, ReadOnly] 
        private FLAG shipType;
        [SerializeField, ReadOnly] 
        private FLAG cargoType;
        [SerializeField, ReadOnly] 
        private bool isDocked;
        
        public void Init(FLAG shipType, FLAG cargoType)
        {
            this.shipType = shipType;
            this.cargoType = cargoType;
            isDocked = false;
            
            //SFX.BELL_RING.PlaySound();
        }
    }
}