using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{

    [SerializeField] GameManager gameManager;
    [SerializeField] private AudioMixer audioMixer;

    [SerializeField] GameObject onlineButton;
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject optionsMenu;
    [SerializeField] GameObject fullscreenToogle;
    [SerializeField] GameObject loadingScreen;

    public void onPlayOnlineSelected()
    {
        loadingScreen.SetActive(true);
        gameManager.ChangePlayMode(GameManager.PlayMode.ONLINE);
        
        Debug.Log("Play Online");
        Debug.Log("Start Matchmaking");

    }
    public void onPlayOfflineSelected()
    {
        loadingScreen.SetActive(true);
        gameManager.ChangePlayMode(GameManager.PlayMode.OFFLINE);
        Debug.Log("Play Offline");        
    
    }
    public void onTutorialSelected()
    {
        loadingScreen.SetActive(true);
        gameManager.ChangePlayMode(GameManager.PlayMode.TRAINING);     

        Debug.Log("Tutorial");
    }
    public void onConfigurationSelected()
    {
        Debug.Log("Configuration");
        mainMenu.SetActive(false);
        optionsMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(fullscreenToogle);

    } 
    public void onExitSelected()
    {
        Debug.Log("Exit");
        Application.Quit();
    }

    public void onFullscreenSelected(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

  
    public void OnSetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        Debug.Log("Quality changed.");
    }

    public void OnSFXAudioSlider(float value)
    {
        if(value == 0f) audioMixer.SetFloat("SFXVolume", -80);
        else audioMixer.SetFloat("SFXVolume", 20f * Mathf.Log10(value));

    } 
    
    public void OnMusicAudioSlider(float value)
    {
        if (value == 0f) audioMixer.SetFloat("MusicVolume", -80);
        else audioMixer.SetFloat("MusicVolume", 20f * Mathf.Log10(value));
    }

    public void OnReturnToMainMenu()
    {
        mainMenu.SetActive(true);
        optionsMenu.SetActive(false);
        EventSystem.current.SetSelectedGameObject(onlineButton);
    }

}
