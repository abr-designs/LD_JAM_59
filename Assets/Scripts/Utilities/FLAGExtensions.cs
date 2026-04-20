using UnityEngine;

namespace Prototypes.Alex.Utilities
{
    public static class FLAGExtensions
    {
        private static GameFlowManager s_gameFlowManager;

        public static FlagDefinition GetFlagDefinition(this FLAG flagType)
        {
            if (s_gameFlowManager == null)
                s_gameFlowManager = UnityEngine.Object.FindAnyObjectByType<GameFlowManager>();

            return s_gameFlowManager.GetFlagDefinition(flagType);
        }

        public static Sprite GetSprite(this FLAG flagType)
        {
            return flagType.GetFlagDefinition().sprite;
        }
        public static string GetDescription(this FLAG flagType)
        {
            return flagType.GetFlagDefinition().description;
        }

        public static Texture2D GetTexture(this FLAG flagType)
        {
            return flagType.GetFlagDefinition().texture;
        }

        public static bool IsDock(this FLAG flagType)
        {
            return flagType is FLAG.MOVE_TO_A or FLAG.MOVE_TO_B or FLAG.MOVE_TO_C;
        }
        
        public static bool IsShip(this FLAG flagType)
        {
            return flagType is FLAG.SHIP_YELLOW or FLAG.SHIP_BLUE or FLAG.SHIP_GREEN;
        }
        
        public static bool IsCargo(this FLAG flagType)
        {
            return flagType is FLAG.CARGO_GOODS or FLAG.CARGO_PASSENGER;
        }
    }
}