using System;
using System.Collections;
using Prototypes.Alex;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utilities;

public class MenuController : MonoBehaviour
{
    [SerializeField]
    private Canvas menuCanvas;
    private CanvasGroup m_menuCanvasGroup;
    [SerializeField]
    private Button tryAgainButton;
    [SerializeField]
    private Button quitButton;

    //Unity Functions
    //================================================================================================================//

    private void OnEnable()
    {
        GameFlowManager.OnGameOver += OnGameOver;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        tryAgainButton.onClick.AddListener(OnTryAgainPressed);
        quitButton.onClick.AddListener(OnQuitPressed);
        
        m_menuCanvasGroup = menuCanvas.GetComponent<CanvasGroup>();
        m_menuCanvasGroup.alpha = 0;
        menuCanvas.enabled = false;
    }

    private void OnDisable()
    {
        GameFlowManager.OnGameOver -= OnGameOver;
    }

    //Callbacks
    //================================================================================================================//

    private void OnGameOver()
    {
        menuCanvas.enabled = true;
        StartCoroutine(TweenCoroutine(1f));
        return;

        IEnumerator TweenCoroutine(float targetAlpha, float duration = 0.5f)
        {
            var startingAlpha = m_menuCanvasGroup.alpha;

            for (float t = 0; t < duration; t+=Time.deltaTime)
            {
                m_menuCanvasGroup.alpha = Mathf.Lerp(startingAlpha, targetAlpha, t / duration);
                
                yield return null;
            }
        }
    }

    private void OnTryAgainPressed()
    {
        //TODO reload the current scene
        ScreenFader.FadeOut(() =>
        {
            SceneManager.LoadScene(1);
        });
    }
    
    private void OnQuitPressed()
    {
        //TODO return to the menu scene
        ScreenFader.FadeOut(() =>
        {
            SceneManager.LoadScene(0);
        });
    }

}
