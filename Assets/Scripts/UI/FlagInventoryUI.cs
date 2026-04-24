using System.Collections.Generic;
using Prototypes.Alex.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Animations;

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
            PlayerFlagInventory.OnMaxFlagsReached += OnMaxFlagsReached;
        }

        private void OnDisable()
        {
            PlayerFlagInventory.OnHeldFlagsChanged -= OnHeldFlagsChanged;
            PlayerFlagInventory.OnMaxFlagsReached -= OnMaxFlagsReached;
        }
        
        private void OnHeldFlagsChanged(List<FLAG> heldFlags)
        {
            var previousCount = activeFlags.Count;
            
            for (int i = activeFlags.Count - 1; i >= 0 ; i--)
            {
                Destroy(activeFlags[i]);
            }
            activeFlags.Clear();

            if (heldFlags == null || heldFlags.Count == 0)
                return;

            for (var i = 0; i < heldFlags.Count; i++)
            {
                var flag = heldFlags[i];
                var flagUI = Instantiate(flagUIElementPrefab, flagUIContainer, false);
                flagUI.sprite = flag.GetSprite();
                activeFlags.Add(flagUI.gameObject);

                if (i == previousCount)
                    flagUI.GetComponent<TransformAnimator>()?.Play();
            }
        }

        private void OnMaxFlagsReached()
        {
            if (activeFlags == null || activeFlags.Count == 0)
                return;

            foreach (var activeFlag in activeFlags)
            {
                if (activeFlag == null)
                    continue;
                
                activeFlag.GetComponent<TransformAnimator>()?.Play();
            }
        }
    }
}