using UnityEngine;

namespace Prototypes.Alex
{
    public enum FLAG_TYPE
    {
        NONE,
        TARGET,
        ACTION,
        DESCRIPTOR,
        OTHER
    }

    public enum FLAG
    {
        ACKNOWLEDGE_RESPONDING,
        UNCLEAR,
        CARGO_PASSENGER,
        CARGO_GOODS,
        HELLO,
        SHIP_BLUE,
        SHIP_YELLOW,
        SHIP_GREEN,
        DENIED_ENTRY,
        STOP,
        MOVE_TO
    }

    [CreateAssetMenu(fileName = "Flag", menuName = "ScriptableObjects/New Flag", order = 1)]
    public class FlagDefinition : ScriptableObject
    {
        public FLAG_TYPE type;
        public FLAG flag;
        public Sprite sprite;
        public Texture2D texture;
        public string description;
    }
}