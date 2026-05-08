using UnityEngine;
using UnityEngine.UI;

namespace Prototypes.Alex.UI
{
    public class GameUIController : MonoBehaviour
    {
        [SerializeField]
        private bool reverseStrikeVisibility;
        
        [SerializeField]
        private Image strikeImagePrefab;
        
        [SerializeField]
        private RectTransform strikeImageContainerRectTransform;
        
        private Image[] m_strikeImages;
        
        //Unity Functions
        //================================================================================================================//

        private void OnEnable()
        {
            GameFlowManager.OnStrikesChanged += SetVisibleStrikes;
        }

        private void Start()
        {
            var gameFlowManager = FindAnyObjectByType<GameFlowManager>();
            if(gameFlowManager.MaxStrikes <= 0)
            {
                this.enabled = false;
                return;
            }

            SetupImages(gameFlowManager.MaxStrikes);
        }

        private void OnDisable()
        {
            GameFlowManager.OnStrikesChanged -= SetVisibleStrikes;
        }

        //================================================================================================================//

        private void SetupImages(int count)
        {
            m_strikeImages = new Image[count];
            for (int i = 0; i < count; i++)
            {
                var instance = Instantiate(strikeImagePrefab, strikeImageContainerRectTransform);
                m_strikeImages[i] = instance;
                instance.gameObject.SetActive(!reverseStrikeVisibility);
            }
        }

        //Callbacks
        //================================================================================================================//
        
        private void SetVisibleStrikes(int visibleCount)
        {
            for (int i = 0; i < m_strikeImages.Length; i++)
            {
                m_strikeImages[i].gameObject.SetActive(i < visibleCount && !reverseStrikeVisibility);
            }
        }
    }
}