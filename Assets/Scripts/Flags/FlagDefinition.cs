using UnityEngine;

namespace Prototypes.Alex
{




    [CreateAssetMenu(fileName = "Flag", menuName = "ScriptableObjects/New Flag", order = 1)]
    public class FlagDefinition : ScriptableObject
    {
        public FLAG_TYPE type;
        public FLAG flag;
        public Sprite sprite;
        public string description;
    }
}