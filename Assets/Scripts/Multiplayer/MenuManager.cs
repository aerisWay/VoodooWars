using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{

    [SerializeField] GameManager gameManager;

    public void onPlayOnlineSelected()
    {
        gameManager.ChangePlayMode(GameManager.PlayMode.ONLINE);
        
        Debug.Log("Play Online");
        Debug.Log("Start Matchmaking");

    }
    public void onPlayOfflineSelected()
    {       
        gameManager.ChangePlayMode(GameManager.PlayMode.OFFLINE);
        Debug.Log("Play Offline");        
    
    }
    public void onTutorialSelected()
    {
        gameManager.ChangePlayMode(GameManager.PlayMode.TRAINING);
        Debug.Log("Tutorial");
    }
    public void onConfigurationSelected()
    {
        Debug.Log("Configuration");
    } 
    public void onExitSelected()
    {
        Debug.Log("Exit");
        Application.Quit();
    }




}
