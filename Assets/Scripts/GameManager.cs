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
    [SerializeField] TextMeshProUGUI textPlayerOneFavor;
    [SerializeField] TextMeshProUGUI textPlayerTwoFavor;
    [SerializeField] TextMeshProUGUI textPlayerOneLifes;
    [SerializeField] TextMeshProUGUI textPlayerTwoLifes;
    [SerializeField] TextMeshProUGUI textPlayerOneMagic;
    [SerializeField] TextMeshProUGUI textPlayerTwoMagic;


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

    public static GameManager instance;

    [Header("Network Manager")]
    [SerializeField] private GameObject networkManager;
    [SerializeField] private GameObject serverManager;
    [SerializeField] private GameObject approvalManager;

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
       
        //activePlayMode = PlayMode.ONLINE;
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
        }
        else
        {
            GetComponent<PlayerInputManager>().enabled = false;
            networkManager.SetActive(false);
            serverManager.SetActive(false);
            approvalManager.SetActive(false);
            SceneManager.LoadScene("TrainingMode");
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

    public void ChangeMagic(int magicAmount, bool playerOne)
    {
        if (playerOne)
        {
            playerOneMagic += magicAmount;

            if (playerOneMagic > magicLimit) playerOneMagic = magicLimit;
        }
        else
        {
            playerTwoMagic += magicAmount;

            if (playerTwoMagic > magicLimit) playerTwoMagic = magicLimit;
        }
        print("Magic changed: ");
        //UpdateCanvas();
    }
    public void ReduceLife(bool playerOne)
    {
        if (canDie)
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
        playerOne.SetActive(false);
        playerTwo.SetActive(false);
        gameplayCanvas.SetActive(false);
       
    }

    public void ReturnToTitle()
    {
        SceneManager.LoadScene("TitleScreen");
    }

    private void UpdateCanvas()
    {
        textPlayerOneFavor.text = "ALLY FAVOR: " + playerOneFavor;
        textPlayerTwoFavor.text = "ENEMY FAVOR: " + playerTwoFavor;
        textPlayerOneLifes.text = "ALLY LIFES: " + playerOneLifes;
        textPlayerTwoLifes.text = "ENEMY LIFES: " + playerTwoLifes;
        textPlayerOneMagic.text = "ALLY MAGIC: " + playerOneMagic;
        textPlayerTwoMagic.text = "ENEMY MAGIC: " + playerTwoMagic;
    }


    void OnPlayerJoined(PlayerInput playerInput)
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
