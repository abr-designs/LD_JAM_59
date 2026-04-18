using Prototypes.Alex.Interactables;

namespace Prototypes.Alex
{
    public class PlayerFlagHoist : FlagHoister, IInteractable
    {
        private static PlayerFlagInventory s_playerFlagInventory;

        protected override void Start()
        {
            base.Start();
            
            if(s_playerFlagInventory == null)
                s_playerFlagInventory  = FindAnyObjectByType<PlayerFlagInventory>();
        }
        public void OnInteract()
        {
            //FIXME Need an input cooldown here to prevent taking down flags prematurely
            HoistFlags(s_playerFlagInventory.holdingFlags);
            s_playerFlagInventory.DropAllFlags();
        }
    }
}