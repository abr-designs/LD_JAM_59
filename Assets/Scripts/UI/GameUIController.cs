using UnityEngine;
using UnityEngine.UI;
using Utilities.Animations;

namespace Prototypes.Alex.UI
{
    public class GameUIController : MonoBehaviour
    {
        [SerializeField]
        //false: Additive strike visibility
        //true: subtractive strike visibility
        private bool reverseStrikeVisibility;
        
        [SerializeField]
        private Image strikeImagePrefab;
        
        [SerializeField]
        private RectTransform strikeImageContainerRectTransform;
        
        [SerializeField]
        private TransformAnimator textAnimator;
        
        private Image[] m_strikeImages;
        private TransformAnimator[] m_animators;
        
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

            m_animators = new TransformAnimator[gameFlowManager.MaxStrikes + 1];
            SetupImages(gameFlowManager.MaxStrikes);
            
            m_animators[^1] = textAnimator;
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
                instance.gameObject.name = $"StrikeImage-instance_[{i + 1}]";
                m_strikeImages[i] = instance;
                
                m_animators[i] = m_strikeImages[i].GetComponent<TransformAnimator>();
            }
            
            SetVisibleStrikes(0);
        }

        private static void Animate(TransformAnimator[] animators)
        {
            foreach (var transformAnimator in animators)
            {
                if (transformAnimator == null)
                    continue;
                
                transformAnimator.Play();
            }
        }

        //Callbacks
        //================================================================================================================//
        
        private void SetVisibleStrikes(int strikeCount)
        {
            for (int i = 0; i < m_strikeImages.Length; i++)
            {
                bool visible = !reverseStrikeVisibility ? i < strikeCount : i < (m_strikeImages.Length - strikeCount);
                
                m_strikeImages[i].gameObject.SetActive(visible);
            }

            Animate(m_animators);
        }
    }
}