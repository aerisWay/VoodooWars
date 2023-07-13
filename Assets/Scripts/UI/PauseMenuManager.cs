using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private GameManager gM;

    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject mainPausePanels;
    [SerializeField] GameObject optionsPanel;

    [SerializeField] GameObject resumeButton;
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject optionsMenu;
    [SerializeField] GameObject fullscreenToogle;
    [SerializeField] GameObject loadingScreen;

  
    public void onResumeSelected()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        gM.gamePaused = false;

    }
    public void onOptionsSelected()
    {
        mainPausePanels.SetActive(false);
        optionsPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(fullscreenToogle);
    }
        
    public void onExitSelected()
    {
        loadingScreen.SetActive(true);
        SceneManager.LoadScene("TitleScreen");
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
        if (value == 0f) audioMixer.SetFloat("SFXVolume", -80);
        else audioMixer.SetFloat("SFXVolume", 20f * Mathf.Log10(value));

    }

    public void OnMusicAudioSlider(float value)
    {
        if (value == 0f) audioMixer.SetFloat("MusicVolume", -80);
        else audioMixer.SetFloat("MusicVolume", 20f * Mathf.Log10(value));
    }

   public void OnReturnToMainPauseMenu()
    {
        mainPausePanels.SetActive(true);
        optionsPanel.SetActive(false);
     
        EventSystem.current.SetSelectedGameObject(resumeButton);
    }

}
