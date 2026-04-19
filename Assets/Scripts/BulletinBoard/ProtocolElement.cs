using Prototypes.Alex;
using Prototypes.Alex.Days;
using Prototypes.Alex.Utilities;
using UnityEngine;

public class ProtocolElement : MonoBehaviour
{
    [SerializeField] 
    private SpriteRenderer shipFlagRenderer;
    [SerializeField] 
    private SpriteRenderer cargoFlagRenderer;

    public void Setup(RuleData ruleData)
    {
        Setup(ruleData.shipType, ruleData.cargoTypes);
    }
    public void Setup(FLAG shipType, FLAG cargoType)
    {
        shipFlagRenderer.sprite = shipType.GetSprite();
        cargoFlagRenderer.sprite = cargoType.GetSprite();
    }
}
