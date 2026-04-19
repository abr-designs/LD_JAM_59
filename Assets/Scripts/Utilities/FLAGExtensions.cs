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
    }
}