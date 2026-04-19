using UnityEngine;

namespace Prototypes.Alex
{
    public class FlagBoard : MonoBehaviour
    {
        [SerializeField]
        private bool interactable;
        
        [SerializeField]
        private FlagDefinition[]  flagDefinitions;
        
        [SerializeField]
        private FlagBoardItem flagBoardItemPrefab;

        [SerializeField] private Vector2 spacing;

        [SerializeField]
        private int maxItemsPerColumn = 5;

        [SerializeField]
        private float itemScale = 1f;

        [SerializeField]
        private Vector3 startPositionOffset;

        private void Start()
        {
            for (int i = 0; i < flagDefinitions.Length; i++)
            {
                int column = i / maxItemsPerColumn;
                int row = i % maxItemsPerColumn;

                Vector3 localPosition = new Vector3(column * spacing.x, -row * spacing.y, 0f);

                FlagBoardItem item = Instantiate(flagBoardItemPrefab, transform);
                item.gameObject.name = $"{flagDefinitions[i].name}_{nameof(FlagBoardItem)}_instance";
                item.transform.localPosition = localPosition + startPositionOffset;
                item.transform.localScale = flagBoardItemPrefab.transform.localScale * itemScale;
                item.Setup(interactable, flagDefinitions[i]);
            }
        }
    }
}