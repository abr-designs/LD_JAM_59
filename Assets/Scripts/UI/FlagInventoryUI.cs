using System;
using System.Collections.Generic;
using Prototypes.Alex.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Prototypes.Alex.UI
{
    public class FlagInventoryUI : MonoBehaviour
    {
        [SerializeField]
        private RectTransform flagUIContainer;
        [SerializeField]
        private Image flagUIElementPrefab;
        
        private List<GameObject> activeFlags = new();
        
        private void OnEnable()
        {
            PlayerFlagInventory.OnHeldFlagsChanged += OnHeldFlagsChanged;
        }

        private void OnDisable()
        {
            PlayerFlagInventory.OnHeldFlagsChanged -= OnHeldFlagsChanged;
        }
        
        private void OnHeldFlagsChanged(List<FLAG> heldFlags)
        {
            for (int i = activeFlags.Count - 1; i >= 0 ; i--)
            {
                Destroy(activeFlags[i]);
            }
            activeFlags.Clear();

            if (heldFlags == null || heldFlags.Count == 0)
                return;

            foreach (var flag in heldFlags)
            {
                var flagUI = Instantiate(flagUIElementPrefab, flagUIContainer, false);
                flagUI.sprite = flag.GetSprite();
                activeFlags.Add(flagUI.gameObject);
            }
        }
    }
}