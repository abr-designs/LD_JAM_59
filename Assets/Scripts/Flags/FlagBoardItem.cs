using System;
using TMPro;
using UnityEngine;

namespace Prototypes.Alex
{
    [RequireComponent(typeof(Collider))]
    public class FlagBoardItem : MonoBehaviour
    {
        [SerializeField]
        private FlagPickup flagPickup;
        [SerializeField]
        private TMP_Text flagText;
        [SerializeField]
        private SpriteRenderer flagSprite;
        
        private Collider m_collider;

        public void Setup(bool interactable, FlagDefinition flagDefinition)
        {
            if(m_collider == null && !interactable)
            {
                m_collider = GetComponent<Collider>();
                m_collider.enabled = false;
            }
            
            flagPickup.Setup(flagDefinition);
            flagText.text = flagDefinition.description;
            flagSprite.sprite = flagDefinition.sprite;
        }

    }
}