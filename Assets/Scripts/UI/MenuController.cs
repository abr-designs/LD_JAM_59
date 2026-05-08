using System;
using Prototypes.Alex;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utilities;

public class MenuController : MonoBehaviour
{
    [SerializeField]
    private Canvas menuCanvas;
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
