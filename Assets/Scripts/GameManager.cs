using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.InputSystem;
using Cinemachine;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public enum PlayMode
    {
        ONLINE, OFFLINE, TRAINING
    }
    [SerializeField] public PlayMode activePlayMode;


    [Header("Network Variables")]
    [SerializeField] int defaultFavor;
    [SerializeField] public int playerOneFavor;
    [SerializeField] public int playerTwoFavor;
    [SerializeField] int playerOneLifes;
    [SerializeField] int playerTwoLifes;
    [SerializeField] public int playerOneMagic;
    [SerializeField] public int playerTwoMagic;
    [SerializeField] int magicLimit = 100;

    [SerializeField] float fightTime;

    [SerializeField] int playerOneCharacterId;
    [SerializeField] int playerTwoCharacterId;

    [Header("Canvas Settings")]
    

    [SerializeField] GameObject playerOneMagicBar;
    [SerializeField] GameObject playerTwoMagicBar;

    [SerializeField] GameObject balance;
    [SerializeField] GameObject RbutonCanvasOne;
    [SerializeField] GameObject RbutonCanvasTwo;
    [SerializeField] GameObject gameplayCanvas;
    [SerializeField] GameObject victoryPanel;
    [SerializeField] GameObject[] lifesArrayOne;
    [SerializeField] GameObject[] lifesArrayTwo;

    [SerializeField] public GameObject[] charactersArrayOffline;
    [SerializeField] public GameObject[] charactersArrayOnline;

    [SerializeField] public CinemachineTargetGroup targetGroupCam;
    [SerializeField] public CinemachineVirtualCamera mainCam;


    GameObject playerOne;
    GameObject playerTwo;
   

    bool canDie = true;

    Gamepad playerOneGamepad;
    Gamepad playerTwoGamepad;

    //Pause
    [SerializeField] bool canPause;
    public bool gamePaused = false;
    [SerializeField] GameObject pausePanels;
    [SerializeField] GameObject goToMainScreenButton;
    [SerializeField] GameObject loadingScreen;

    public static GameManager instance;

    [Header("Network Manager")]
    [SerializeField] private GameObject networkManager;
    [SerializeField] private GameObject serverManager;
    [SerializeField] private GameObject approvalManager;


    bool trainingSetUp = false;
    private void Awake()
    {
        if (instance == null) instance = this;
        //DontDestroyOnLoad(transform.gameObject);



        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        //UpdateCanvas();
        //Luego poner character id
        if (activePlayMode == PlayMode.ONLINE)
        {
            GetComponent<PlayerInputManager>().playerPrefab = charactersArrayOnline[0];
        }
        else
        {
            GetComponent<PlayerInputManager>().playerPrefab = charactersArrayOffline[0];
        }


        if(playerOneMagicBar != null && playerTwoMagicBar != null)
        {
            playerOneMagicBar.GetComponent<Image>().fillAmount = playerOneMagic / 100;
            playerTwoMagicBar.GetComponent<Image>().fillAmount = playerTwoMagic / 100;
        }
     
        //activePlayMode = PlayMode.ONLINE;
    }

    public void OnPause()
    {
        if (canPause && !gamePaused)
        {
            gamePaused = true;
            Time.timeScale = 0f;
            pausePanels.SetActive(true);
            
            
        }
    }

    public void GoToMainMenu()
    {
        loadingScreen.SetActive(true);
        SceneManager.LoadScene("TitleScreen");
    }

    public void ChangePlayMode(PlayMode playMode)
    {
        activePlayMode = playMode;

        if (activePlayMode == PlayMode.ONLINE)
        {
            GetComponent<PlayerInputManager>().enabled = false;
            networkManager.SetActive(true);
            serverManager.SetActive(true);
            approvalManager.SetActive(true);
            SceneManager.LoadScene("CameraSolved");

        }
        else if (activePlayMode == PlayMode.OFFLINE)
        {
            GetComponent<PlayerInputManager>().enabled = true;
            networkManager.SetActive(false);
            serverManager.SetActive(false);
            approvalManager.SetActive(false);
            GetComponent<PlayerInputManager>().playerPrefab = charactersArrayOffline[0];
            SceneManager.LoadScene("OfflineRing");
            canPause = true;
        }
        else
        {
            GetComponent<PlayerInputManager>().enabled = false;
            networkManager.SetActive(false);
            serverManager.SetActive(false);
            approvalManager.SetActive(false);
            SceneManager.LoadScene("TrainingMode");
            canPause = true;
        }

        
    }
    private void Update()
    {
        if (playerOne != null && playerTwo != null)
        {
            if (Vector3.Distance(playerOne.transform.position, mainCam.gameObject.transform.position) < Vector3.Distance(playerTwo.transform.position, mainCam.gameObject.transform.position))
            {
                if (mainCam.Follow == playerTwo.transform)
                {
                    mainCam.Follow = playerOne.transform;
                    mainCam.LookAt = playerTwo.transform;
                }
            }
            else
            {
                if (mainCam.Follow == playerOne.transform)
                {
                    mainCam.Follow = playerTwo.transform;
                    mainCam.LookAt = playerOne.transform;
                }
            }
        }

        if(activePlayMode == PlayMode.TRAINING && !trainingSetUp)
        {
            trainingSetUp = true;
            playerOne = GameObject.FindGameObjectWithTag("Player");
            playerOne.GetComponentInChildren<PlayerController>().playerOne = true;
            playerOne.tag = "PlayerOne";

            try
            {
                InputSystem.SetDeviceUsage(Gamepad.all[0], "Player1");
                playerTwoGamepad = InputSystem.GetDevice<Gamepad>(new InternedString("Player2"));
                
            }
            catch
            {
                InputSystem.SetDeviceUsage(Keyboard.current, "Player1");
            }

            playerTwo = GameObject.FindGameObjectWithTag("Training");
            playerTwo.GetComponentInChildren<PlayerController>().playerOne = false;
            playerTwo.tag = "PlayerTwo";
            playerOne.GetComponentInChildren<PlayerController>().enemy = playerTwo;
            playerTwo.GetComponentInChildren<PlayerController>().enemy = playerOne;
            playerOne.GetComponentInChildren<PlayerController>().enemyController = playerTwo.GetComponentInChildren<PlayerController>();
            playerTwo.GetComponentInChildren<PlayerController>().enemyController = playerOne.GetComponentInChildren<PlayerController>();
                     
            mainCam.Follow = playerOne.transform;
            mainCam.LookAt = playerTwo.transform;

            gameplayCanvas.SetActive(true);
        }


    }

    public void RumblePulse(float lowFrequency, float highFrequency, float duration, bool playerOne)
    {
        if (playerOne)
        {
            if (playerOneGamepad != null)
            {
                playerOneGamepad.SetMotorSpeeds(lowFrequency, highFrequency);
                StartCoroutine(StopRumble(duration, playerOne));
            }             

        }
        else
        {
            if (playerTwoGamepad != null)
            {
                playerTwoGamepad.SetMotorSpeeds(lowFrequency, highFrequency);
                StartCoroutine(StopRumble(duration, playerOne));
            }
                
        }
    }

    private IEnumerator StopRumble(float duration, bool playerOne)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (playerOne)
        {
            playerOneGamepad.SetMotorSpeeds(0f, 0f);
        }
        else
        {
            playerTwoGamepad.SetMotorSpeeds(0f, 0f);
        }

        print("Para de vibrar.");
    }

    public void ChangeFavor(int favorDifference)
    {
        print("Favor changed");

        playerOneFavor += favorDifference;
        if (playerOneFavor > 100) playerOneFavor = 100;
        else if (playerOneFavor < 0) playerOneFavor = 0;
        playerTwoFavor -= favorDifference;
        if (playerTwoFavor > 100) playerTwoFavor = 100;
        else if (playerTwoFavor < 0) playerTwoFavor = 0;

        //UpdateCanvas();
        balance.GetComponent<BalanceFavor>().SetBalanceAngle();
    }

    internal void SetGameplayUI()
    {
        gameplayCanvas.SetActive(true);
    }

    public void ChangeMagic(int magicAmount, bool playerOne)
    {
        if (playerOne)
        {
            print("Change magic");
            playerOneMagic += magicAmount;

            if (playerOneMagic > magicLimit) playerOneMagic = magicLimit;
            float magicToBar = (float)playerOneMagic / 100;
            playerOneMagicBar.GetComponent<Image>().fillAmount = magicToBar;
        }
        else
        {
            playerTwoMagic += magicAmount;

            if (playerTwoMagic > magicLimit) playerTwoMagic = magicLimit;
            float magicToBar = (float)playerTwoMagic / 100;
            playerTwoMagicBar.GetComponent<Image>().fillAmount = magicToBar;
        }
        print("Magic changed: ");
        //UpdateCanvas();
    }
    public void ReduceLife(bool playerOne)
    {
        if (canDie && activePlayMode != PlayMode.TRAINING)
        {
            if (playerOne)
            {
                print("Reduzco vida");
                playerOneLifes--;
                lifesArrayOne[playerOneLifes].SetActive(false);

            }


            else
            {
                print("Reduzco vida");
                playerTwoLifes--;
                lifesArrayTwo[playerTwoLifes].SetActive(false);
            }


            playerOneFavor = defaultFavor;
            playerTwoFavor = defaultFavor;

            balance.GetComponent<BalanceFavor>().SetBalanceAngle();
            //UpdateCanvas();

            print("Se quitó una vida.");
            canDie = false;
            StartCoroutine("DeathCooldown");

        }

        if (playerOneLifes <= 0 || playerTwoLifes <= 0) GameEnd();
    }

    IEnumerator DeathCooldown()
    {
        yield return new WaitForSeconds(1);
        canDie = true;
    }

    private void GameEnd()
    {
        if(playerOneLifes < playerTwoLifes)
        {
            print("Player two won");
            
            victoryPanel.GetComponentInChildren<TextMeshProUGUI>().text = "Player two won!";
           
        }
        else
        {
            print("Player one won");
            
        }

        victoryPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(goToMainScreenButton);
        playerOne.SetActive(false);
        playerTwo.SetActive(false);
        gameplayCanvas.SetActive(false);
       
    }

    public void ReturnToTitle()
    {
        SceneManager.LoadScene("TitleScreen");
    }

    


    void OnPlayerJoined(PlayerInput playerInput)
    {
        if(activePlayMode == PlayMode.OFFLINE)
        {
            //Local multiplayer
            if (playerInput.playerIndex == 0)
            {
                playerOne = GameObject.FindGameObjectWithTag("Player");
                playerOne.GetComponentInChildren<PlayerController>().playerOne = true;
                playerOne.tag = "PlayerOne";
                GetComponent<PlayerInputManager>().playerPrefab = charactersArrayOffline[playerTwoCharacterId];
                try
                {
                    InputSystem.SetDeviceUsage(Gamepad.all[0], "Player1");
                    playerTwoGamepad = InputSystem.GetDevice<Gamepad>(new InternedString("Player2"));
                    RbutonCanvasOne.SetActive(false);
                }
                catch
                {
                    InputSystem.SetDeviceUsage(Keyboard.current, "Player1");
                }

            }
            else
            {
                playerTwo = GameObject.FindGameObjectWithTag("Player");
                playerTwo.GetComponentInChildren<PlayerController>().playerOne = false;
                playerTwo.tag = "PlayerTwo";
                playerOne.GetComponentInChildren<PlayerController>().enemy = playerTwo;
                playerTwo.GetComponentInChildren<PlayerController>().enemy = playerOne;
                playerOne.GetComponentInChildren<PlayerController>().enemyController = playerTwo.GetComponentInChildren<PlayerController>();
                playerTwo.GetComponentInChildren<PlayerController>().enemyController = playerOne.GetComponentInChildren<PlayerController>();
                RbutonCanvasTwo.SetActive(false);


                try
                {
                    InputSystem.SetDeviceUsage(Gamepad.all[1], "Player2");
                    playerTwoGamepad = InputSystem.GetDevice<Gamepad>(new InternedString("Player2"));
                }
                catch
                {
                    InputSystem.SetDeviceUsage(Keyboard.current, "Player2");
                }
                mainCam.Follow = playerOne.transform;
                mainCam.LookAt = playerTwo.transform;

                gameplayCanvas.SetActive(true);
            }

        }

    }

    public void OnTimerFinished()
    {
        if(playerOneFavor < playerTwoFavor)
        {
            playerOneLifes = 0;
            
        }
        else
        {
            playerTwoLifes = 0;
            
        }
        GameEnd();
    }

   
}
