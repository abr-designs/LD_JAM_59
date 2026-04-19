using Prototypes.Alex.Days;
using Prototypes.Alex.Utilities;
using UnityEngine;

public class ManifestElement : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer dockFlagRenderer;
    [SerializeField]
    private SpriteRenderer[] cargoFlagRenderers;

    public void Setup(DockRequirementData dockRequirementData)
    {
        ResetVisibility();

        var flagSprite = dockRequirementData.cargoType.GetSprite();
        for (int i = 0; i < dockRequirementData.cargoCount; i++)
        {
            cargoFlagRenderers[i].gameObject.SetActive(true);
            cargoFlagRenderers[i].sprite = flagSprite;
        }
        
    }

    private void ResetVisibility()
    {
        for (int i = 0; i < cargoFlagRenderers.Length; i++)
        {
            cargoFlagRenderers[i].gameObject.SetActive(false);
        }
    }
    
}
