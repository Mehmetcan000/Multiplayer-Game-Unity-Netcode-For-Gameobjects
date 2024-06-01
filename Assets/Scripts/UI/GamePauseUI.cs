using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GamePauseUI : MonoBehaviour
{

    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button optionsButton;


    private void Awake()
    {
        resumeButton.onClick.AddListener(() =>
        {
            KitchenGameManager.Instance.TogglePauseGame();
        });
        optionsButton.onClick.AddListener(() =>
        {
            OptionsUI.Instance.Show();
        });
        mainMenuButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            Loader.Load((Loader.Scene.MainMenuScene));
        });
    }

    private void Start()
    {
        KitchenGameManager.Instance.OnLocalGamePaused += KithcenLocalGameManagerOnLocalGamePaused;
        KitchenGameManager.Instance.OnLocalGameUnPaused += KithcenLocalGameManagerOnLocalGameUnPaused;
        Hide();
    }

    private void KithcenLocalGameManagerOnLocalGameUnPaused(object sender, EventArgs e)
    {
        Hide();
    }

    private void KithcenLocalGameManagerOnLocalGamePaused(object sender, EventArgs e)
    {
        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
    
}
