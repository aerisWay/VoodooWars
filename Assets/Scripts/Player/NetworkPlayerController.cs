using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;
using UnityEngine.InputSystem;
using JetBrains.Annotations;
using Unity.VisualScripting;
using System;
using System.Globalization;
using TMPro;
using UnityEngine.Networking.Types;

public class NetworkPlayerController : NetworkBehaviour
{
    public enum PlayerState
    {
        FAST_ATACK, SLOW_ATACK, BLOCK, PARRY, DASH, VOODOO_POW, MOVEMENT, IDLE, JOKE, HIT, STUNNED, KNOCKED_UP
    }

    [Header("State machine")]
    public PlayerState currentState;
    [SerializeField] bool canChangeState = true;  

    [Header("Server variables")]
    public PlayerState playerOneState;
    public PlayerState playerTwoState;     

    [Header("Game State")]
    [SerializeField] Transform playerOneSpawn;
    [SerializeField] Transform playerTwoSpawn;
    [SerializeField] GameManager gameManager;
    [SerializeField] NetworkManager networkManager;
    [SerializeField] public bool playerOne;
    [SerializeField] public NetworkPlayerController enemyController;
    [SerializeField] public GameObject enemy;



    [Header("Camera Movement")]
    [SerializeField] GameObject cam;
    [SerializeField] private Transform playerCameraPosition;
    [SerializeField] CinemachineFreeLook freeLookCam;

    [Header("Movement")]
    [SerializeField] private bool canMove;
    [SerializeField] private bool isRunning = false;
    [SerializeField] Vector3 inputDir;
    [SerializeField] private Transform orientation;

    [SerializeField] private float rotationSpeed;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Rigidbody playerRb;
    [SerializeField] private float playerSpeed;
    [SerializeField] private float cameraSensitivity;
    [SerializeField] private float baseJumpForce;
    [SerializeField] private float minJumpForce;
    [SerializeField] private float jumpForceContinuous;

    [SerializeField] private int maxJumps;
    [SerializeField] private bool inAir = false;
    [SerializeField] private bool canCancelJump = false;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float jumpMaxTime;
    private float jumpTime;
    private bool isJumping = false;         // Flag to indicate if the jump input is being held
    [SerializeField] private float jumpForceMultiplier = 0f; // Multiplier for the jump force based on the time holding the jump input
    private bool falling = false;
    [SerializeField] private float currentJumps;

    [Space]
    [Header("Dash")]
    [SerializeField] float dashSpeed;
    [SerializeField] float dashDelay;
    [SerializeField] float dashDuration;
    [SerializeField] float dashDistanceToTarget;
    [SerializeField] float dashCooldown;
    [SerializeField] int dashMagicCost = 10;
    private bool isDashing = false;
    [SerializeField] private bool canDash = true;


    [Space]
    [Header("Atack")]
    [SerializeField] LayerMask enemyLayer;
    public bool enemyTriggered = false;
    [SerializeField] Transform spawnPoint;

    [SerializeField] BoxCollider[] hitPoints = new BoxCollider[4];
    public bool chargingSlowAttack = false;
    public float currentChargedAttackForce = 0f;
    public float chargeTimer = 0f;
    [SerializeField] float maxChargeTime;
    [SerializeField] private int basicAttackFavorGain = 5;
    [SerializeField] private int basicAttackMagicGain = 5;
    [SerializeField] private int strongAttackBaseFavorGain = 10;
    [SerializeField] private int strongAttackBaseMagicGain = 10;

    [Space]
    [Header("Blocks")]
    private bool isParrying = false;


    private Vector2 movementInput = Vector2.zero;
    private bool jumped = false;
    public bool basicAtacking = false;
    public bool slowAtacking = false;
    private bool taunt = false;
    [SerializeField] private bool parry = false;
    [SerializeField] private bool block = false;
    private bool dash = false;
    private bool focus = false;
    private bool voodooPow = false;
    private bool voodooUlt = false;

    bool canBasicAttack;
    bool canSlowAttack;
    bool canTaunt;
    bool canParry;
    bool canDashing;
    bool canVoodooPow;


    //mouse input for camera

    [Header("Particles and VFX")]
    [SerializeField] ParticleSystem basicAtackSlashParticle;
    [SerializeField] ParticleSystem basicAtackImpactParticle;

    [SerializeField] ParticleSystem slowAtackSlashParticle;
    [SerializeField] ParticleSystem slowAtackImpactParticle;
    [SerializeField] ParticleSystem chargeSlowAttackParticle;


    [SerializeField] ParticleSystem shieldEffectParticle;
    [SerializeField] ParticleSystem blockEffectParticle;

    [SerializeField] ParticleSystem parryStartedParticle;
    [SerializeField] ParticleSystem parryDoneParticle;
    [SerializeField] ParticleSystem parryStunnedParticle;

    [SerializeField] ParticleSystem startRunAnimationParticle;
    [SerializeField] ParticleSystem endRunAnimationParticle;
    [SerializeField] ParticleSystem runningParticle;

    [SerializeField] ParticleSystem dashParticle;
    [SerializeField] ParticleSystem dashImpactParticle;

    [SerializeField] ParticleSystem voodooPowExplosionParticle;

    [SerializeField] ParticleSystem idleStateParticle;

    [SerializeField] ParticleSystem jokeParticle;

    [SerializeField] ParticleSystem flyingKnockUpParticle;
    [SerializeField] ParticleSystem groundHitParticle;



    [Space]
    [Header("Sound effects")]
    public AudioSource playerAudio;
    public AudioSource playerStepsAudio;
    [SerializeField] AudioClip footStepsSound;
    [SerializeField] AudioClip basicAttackSlashSound;
    [SerializeField] AudioClip basicAttackImpactSound;

    [SerializeField] AudioClip slowAttackSlashSound;
    [SerializeField] AudioClip slowAttackImpactSound;
    [SerializeField] AudioClip slowAttackChargeSound;

    [SerializeField] AudioClip shieldSound;
    [SerializeField] AudioClip shieldImpactSound;

    [SerializeField] AudioClip parryStartedSound;
    [SerializeField] AudioClip parryDoneSound;
    [SerializeField] AudioClip stunnedSound;

    [SerializeField] AudioClip dashStartSound;
    [SerializeField] AudioClip dashImpactSound;

    [SerializeField] AudioClip voodooPowerSound;
    [SerializeField] AudioClip idleSound;

    [SerializeField] AudioClip jokeSound;
    [SerializeField] AudioClip groundHitSound;


    
    public override void OnNetworkSpawn()
    {
        print("Soy un nuevo jugador! Mi id es: " + NetworkObjectId);
        
        
        currentState = PlayerState.IDLE;
        canMove = true;
        Physics.gravity = new Vector3(0, -20.0F, 0);
        cam = GameObject.FindGameObjectWithTag("MainCamera");
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        networkManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManager>();
             
        playerOneSpawn = GameObject.FindGameObjectWithTag("PlayerOneSpawn").transform;
        playerTwoSpawn = GameObject.FindGameObjectWithTag("PlayerTwoSpawn").transform;
        print(NetworkObjectId);


        if (NetworkObjectId == 1)
        {
            
            playerOne = true;
            gameObject.tag = "PlayerOne";
            //networkManager.AddNetworkPrefab(gameManager.charactersArray[1]);
        }
        if(NetworkObjectId == 2)
        {
            playerOne = false;
            gameObject.tag = "PlayerTwo";
        }        
        if (playerOne)
        {
            transform.parent.transform.position = GameObject.FindGameObjectWithTag("PlayerOneSpawn").transform.position;
            spawnPoint = playerOneSpawn;
            

            //Cambiar prefab en connection approval

        }

        else
        {        
            transform.parent.transform.position = GameObject.FindGameObjectWithTag("PlayerTwoSpawn").transform.position;
            spawnPoint = playerTwoSpawn;

            enemy = GameObject.FindGameObjectWithTag("PlayerOne");
            enemyController = enemy.GetComponentInChildren<NetworkPlayerController>();
            enemyController.enemy = GameObject.FindGameObjectWithTag("PlayerTwo");
            enemyController.enemyController = GetComponent<NetworkPlayerController>();
            gameManager.mainCam.GetComponent<CameraAdjusting>().SetPlayerInCamera();
                    
        }
        GetComponent<PlayerInput>().neverAutoSwitchControlSchemes = false;
        canBasicAttack = true;
        canSlowAttack = true;
        canTaunt = true;
        canParry = true;
        canDashing = true;
        canVoodooPow = true;
        gameManager.SetGameplayUI();

        base.OnNetworkSpawn();
    }


    #region Inputs
    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        jumped = context.action.triggered;
    }

    public void OnCameraMove(InputAction.CallbackContext context)
    {

        //cursorX = context.ReadValue<Vector2>().x;
        //cursorY = context.ReadValue<Vector2>().y;
    }

    public void OnBasicAtack(InputAction.CallbackContext context)
    {
        basicAtacking = context.action.triggered;

    }

    public void OnSlowAttack(InputAction.CallbackContext context)
    {
        slowAtacking = context.action.triggered;

    }

    public void OnTaunt(InputAction.CallbackContext context)
    {
        taunt = context.action.triggered;

    }
    public void OnDash(InputAction.CallbackContext context)
    {
        dash = context.action.triggered;

    }
    public void OnBlock(InputAction.CallbackContext context)
    {
        block = context.action.triggered;

    }
    public void OnParry(InputAction.CallbackContext context)
    {
        parry = context.action.triggered;

    }
    public void OnFocus(InputAction.CallbackContext context)
    {
        focus = context.action.triggered;
        print("Focus");
    }

    public void OnVoodooPow(InputAction.CallbackContext context)
    {
        voodooPow = context.action.triggered;

    }

    public void OnVoodooUlt(InputAction.CallbackContext context)
    {
        voodooUlt = context.action.triggered;
        print("Voodoo Ult");
    }

    #endregion
    

    [ServerRpc]
    public void TransitionToStateServerRpc(PlayerState newState)
    {
        if (!IsServer) return;
        currentState = newState;    
        // Call client RPC to update the state on other clients
        UpdateStateOnClientRpc(currentState);
    }

    [ClientRpc]
    private void UpdateStateOnClientRpc(PlayerState newState)
    {
        if (IsOwner || IsServer) return;     
        print("Actualizo cliente id: ." + NetworkObjectId);
        currentState = newState;
    }
   
    private void FixedUpdate()
    {
        if(IsServer) return;

        TakeInputs();
        FixJump();
       
    }
    private void TakeInputs()
    {
        
        MovePlayer();
        MoveCamera();
        Jump();
        BasicAtacking();
        SlowAtack();
        Parrying();
        Blocking();
        Dashing();
        Taunting();
        VoodooPow();
        //Focus();
    }

    private void VoodooPow()
    {
        if (IsOwner)
        {
            if (voodooPow && canChangeState && canVoodooPow)
            {
                voodooPowExplosionParticle.Play();
                canChangeState = false;
                canMove = false;
                currentState = PlayerState.VOODOO_POW;
                TransitionToStateServerRpc(currentState);
                playerAnimator.SetBool("VoodooPow", true);
                CustomBasicPower();
                print("Voodoo Pow Owner");
                canVoodooPow = false;
            }
            else
            {
                if (!voodooPow)
                {
                    if (playerAnimator.GetBool("VoodooPow") == true)
                    {
                        playerAnimator.SetBool("VoodooPow", false);

                    }
                    if (!canVoodooPow)
                    {
                        canVoodooPow = true;
                    }
                }
            }
        }
        else
        {
            if(currentState == PlayerState.VOODOO_POW)
            {
                voodooPowExplosionParticle.Play();
                canChangeState = false;
                canMove = false;               
                playerAnimator.SetBool("VoodooPow", true);
                CustomBasicPower();
                print("Voodoo Pow Not Owner");
                canVoodooPow = false;
            }
            else
            {
                if (playerAnimator.GetBool("VoodooPow") == true)
                {
                    playerAnimator.SetBool("VoodooPow", false);

                }
                if (!canVoodooPow)
                {
                    canVoodooPow = true;
                }
            }
           
        }
        
    }

    internal void ReceiveAttack()
    {
        Vector3 enemyPos = enemy.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(new Vector3(enemyPos.x, 0, enemyPos.z).normalized, Vector3.up);

        if (IsOwner)
        {
            if (isParrying)
            {
                enemyController.currentState = PlayerState.STUNNED;
                TransitionToStateServerRpc(currentState);
                if (enemyController.playerAnimator.GetBool("Stunned") == false)
                {
                    enemyController.canMove = false;
                    enemyController.canChangeState = false;
                    enemyController.playerAnimator.SetBool("Stunned", true);
                    parryStunnedParticle.Play();
                    parryDoneParticle.Play();
                    gameManager.RumblePulse(1.0f, 2.0f, 0.8f, playerOne);
                    gameManager.RumblePulse(1.0f, 2.0f, 0.8f, !playerOne);

                }
            }
            else if (currentState == PlayerState.BLOCK)
            {
                if (playerAnimator.GetBool("BlockedHit") == false)
                {
                    playerAnimator.SetBool("BlockedHit", true);
                    enemyController.blockEffectParticle.Play();
                }
            }
            else
            {
                canChangeState = false;
                canMove = false;
                print("Received");
                Vector3 relativePos = (transform.position - enemyController.gameObject.transform.position).normalized;
                if (enemyController.currentState == PlayerState.SLOW_ATACK)
                {
                    currentState = PlayerState.HIT;
                    TransitionToStateServerRpc(currentState);
                    if (playerAnimator.GetBool("Receiving") == false)
                    {
                        playerAnimator.SetBool("Receiving", true);
                        enemyController.slowAtackImpactParticle.Play();
                        playerAudio.clip = slowAttackImpactSound;
                        
                        playerAudio.Play();
                        gameManager.RumblePulse(1.0f, 2.0f, 0.8f, playerOne);

                    }

                    if (!playerOne)
                    {
                        gameManager.ChangeFavor(-strongAttackBaseFavorGain);
                    }
                    else
                    {
                        gameManager.ChangeFavor(strongAttackBaseFavorGain);
                    }

                    float totalDmgDone;

                    if (playerOne)
                    {
                        if (gameManager.playerOneFavor > 50)
                        {
                            totalDmgDone = strongAttackBaseFavorGain + (strongAttackBaseFavorGain * (gameManager.playerOneFavor - 50) / 100);
                        }
                        else
                        {
                            totalDmgDone = strongAttackBaseFavorGain - (strongAttackBaseFavorGain * (50 - gameManager.playerOneFavor) / 100);
                        }


                    }
                    else
                    {
                        if (gameManager.playerOneFavor > 50)
                        {
                            totalDmgDone = strongAttackBaseFavorGain + (strongAttackBaseFavorGain * (gameManager.playerTwoFavor - 50) / 100);
                        }
                        else
                        {
                            totalDmgDone = strongAttackBaseFavorGain - (strongAttackBaseFavorGain * (50 - gameManager.playerTwoFavor) / 100);
                        }
                    }

                    enemyController.gameManager.ChangeMagic((int)totalDmgDone, !playerOne);


                    relativePos.y = 0.8f;
                    print(relativePos);
                    GetComponent<Rigidbody>().AddForce((relativePos * totalDmgDone * enemyController.currentChargedAttackForce) * 0.1f, ForceMode.Impulse);
                    print(relativePos * totalDmgDone * enemyController.currentChargedAttackForce);
                    //Cuando esa fuerza llega a determinado punto, manda por los aires.
                }
                else if (enemyController.currentState == PlayerState.FAST_ATACK)
                {

                    currentState = PlayerState.HIT;
                    TransitionToStateServerRpc(currentState);

                    if (playerAnimator.GetBool("Receiving") == false)
                    {
                        playerAnimator.SetBool("Receiving", true);
                        if (enemyController.basicAtackImpactParticle.isPlaying) enemyController.basicAtackImpactParticle.Clear();
                        enemyController.basicAtackImpactParticle.Play();

                        playerAudio.clip = basicAttackImpactSound;
                        if (playerAudio.isPlaying) playerAudio.Stop();
                        playerAudio.Play();
                        print("Audio is playing: " + playerAudio.isPlaying);

                    }

                    if (!playerOne)
                    {
                        gameManager.ChangeFavor(-basicAttackFavorGain);
                    }
                    else
                    {
                        gameManager.ChangeFavor(basicAttackFavorGain);
                    }

                    float totalDmgDone;
                    if (playerOne)
                    {
                        if (gameManager.playerOneFavor > 50)
                        {
                            totalDmgDone = basicAttackFavorGain + (basicAttackFavorGain * (gameManager.playerOneFavor - 50) / 100);
                        }
                        else
                        {
                            totalDmgDone = basicAttackFavorGain - (basicAttackFavorGain * (50 - gameManager.playerOneFavor) / 100);
                        }
                    }
                    else
                    {
                        if (gameManager.playerOneFavor > 50)
                        {
                            totalDmgDone = basicAttackFavorGain + (strongAttackBaseFavorGain * (gameManager.playerTwoFavor - 50) / 100);
                        }
                        else
                        {
                            totalDmgDone = basicAttackFavorGain - (strongAttackBaseFavorGain * (50 - gameManager.playerTwoFavor) / 100);
                        }
                    }
                    enemyController.gameManager.ChangeMagic((int)totalDmgDone, !playerOne);
                    relativePos.y = 0f;
                    print(relativePos);
                    GetComponent<Rigidbody>().AddForce((relativePos * totalDmgDone) * 0.8f, ForceMode.Impulse);
                }

            }
        }
        else
        {
            if (isParrying)
            {                
                if (enemyController.playerAnimator.GetBool("Stunned") == false)
                {
                    enemyController.playerAnimator.SetBool("Stunned", true);
                    parryStunnedParticle.Play();
                    parryDoneParticle.Play();
                    gameManager.RumblePulse(1.0f, 2.0f, 0.8f, playerOne);
                    gameManager.RumblePulse(1.0f, 2.0f, 0.8f, !playerOne);
                }
            }
            else if (currentState == PlayerState.BLOCK)
            {
                if (playerAnimator.GetBool("BlockedHit") == false)
                {
                    playerAnimator.SetBool("BlockedHit", true);
                    enemyController.blockEffectParticle.Play();
                }
            }
            else
            {
                print("Received");
                Vector3 relativePos = (transform.position - enemyController.gameObject.transform.position).normalized;
                if (enemyController.currentState == PlayerState.SLOW_ATACK)
                {                   
                    if (playerAnimator.GetBool("Receiving") == false)
                    {
                        playerAnimator.SetBool("Receiving", true);
                        enemyController.slowAtackImpactParticle.Play();
                        playerAudio.clip = slowAttackImpactSound;
                        playerAudio.loop = false;
                        playerAudio.Play();
                        gameManager.RumblePulse(1.0f, 2.0f, 0.8f, playerOne);

                    }
                    if (!playerOne)
                    {
                        gameManager.ChangeFavor(-strongAttackBaseFavorGain);
                    }
                    else
                    {
                        gameManager.ChangeFavor(strongAttackBaseFavorGain);
                    }

                    //float totalDmgDone;

                    //if (playerOne)
                    //{
                    //    if (gameManager.playerOneFavor > 50)
                    //    {
                    //        totalDmgDone = strongAttackBaseFavorGain + (strongAttackBaseFavorGain * (gameManager.playerOneFavor - 50) / 100);
                    //    }
                    //    else
                    //    {
                    //        totalDmgDone = strongAttackBaseFavorGain - (strongAttackBaseFavorGain * (50 - gameManager.playerOneFavor) / 100);
                    //    }


                    //}
                    //else
                    //{
                    //    if (gameManager.playerOneFavor > 50)
                    //    {
                    //        totalDmgDone = strongAttackBaseFavorGain + (strongAttackBaseFavorGain * (gameManager.playerTwoFavor - 50) / 100);
                    //    }
                    //    else
                    //    {
                    //        totalDmgDone = strongAttackBaseFavorGain - (strongAttackBaseFavorGain * (50 - gameManager.playerTwoFavor) / 100);
                    //    }
                    //}

                    //enemyController.gameManager.ChangeMagic((int)totalDmgDone, !playerOne);


                    //relativePos.y = 1f;
                    //print(relativePos);
                    //GetComponent<Rigidbody>().AddForce((relativePos * totalDmgDone * enemyController.currentChargedAttackForce) * 0.2f, ForceMode.Impulse);
                    //print(relativePos * totalDmgDone * enemyController.currentChargedAttackForce);
                    //Cuando esa fuerza llega a determinado punto, manda por los aires.
                }
                else if (enemyController.currentState == PlayerState.FAST_ATACK)
                {                  
                    if (playerAnimator.GetBool("Receiving") == false)
                    {
                        playerAnimator.SetBool("Receiving", true);
                        enemyController.basicAtackImpactParticle.Play();
                        playerAudio.clip = basicAttackImpactSound;
                        playerAudio.loop = false;
                        playerAudio.Play();

                    }
                    if (!playerOne)
                    {
                        gameManager.ChangeFavor(-basicAttackFavorGain);
                    }
                    else
                    {
                        gameManager.ChangeFavor(basicAttackFavorGain);
                    }

                    //float totalDmgDone;
                    //if (playerOne)
                    //{
                    //    if (gameManager.playerOneFavor > 50)
                    //    {
                    //        totalDmgDone = basicAttackFavorGain + (basicAttackFavorGain * (gameManager.playerOneFavor - 50) / 100);
                    //    }
                    //    else
                    //    {
                    //        totalDmgDone = basicAttackFavorGain - (basicAttackFavorGain * (50 - gameManager.playerOneFavor) / 100);
                    //    }
                    //}
                    //else
                    //{
                    //    if (gameManager.playerOneFavor > 50)
                    //    {
                    //        totalDmgDone = basicAttackFavorGain + (strongAttackBaseFavorGain * (gameManager.playerTwoFavor - 50) / 100);
                    //    }
                    //    else
                    //    {
                    //        totalDmgDone = basicAttackFavorGain - (strongAttackBaseFavorGain * (50 - gameManager.playerTwoFavor) / 100);
                    //    }
                    //}
                    //enemyController.gameManager.ChangeMagic((int)totalDmgDone, !playerOne);
                    //relativePos.y = 0.2f;
                    //print(relativePos);
                    //GetComponent<Rigidbody>().AddForce(relativePos * totalDmgDone, ForceMode.Impulse);
                }

            }
        }

      
    }

    private void CustomBasicPower()
    {
        //Depende del personaje
    }

    private void Focus()
    {
        if (focus)
        {
            Vector3 direction = GameObject.FindGameObjectWithTag("Enemy").transform.position - freeLookCam.transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            if (Mathf.Abs(freeLookCam.m_XAxis.Value - targetRotation.x) < 100f)
            {

                freeLookCam.m_XAxis.Value = Quaternion.Lerp(Quaternion.Euler(0, freeLookCam.m_XAxis.Value, 0), Quaternion.Euler(0, targetRotation.x, 0), 5 * Time.deltaTime).eulerAngles.y;

            }
        }
    }

    public void ReturnToMove()
    {
        canMove = true;

        canChangeState = true;

        chargingSlowAttack = false;

        chargeTimer = 0f;
        //Se podría sustituir por un switch
        if (playerAnimator.GetBool("Receiving") == true)
        {
            playerAnimator.SetBool("Receiving", false);
        }

        if (playerAnimator.GetBool("VoodooPow") == true)
        {
            playerAnimator.SetBool("VoodooPow", false);

        }
        if (IsOwner)
        {
           
            if (playerAnimator.GetBool("BlockedHit") == true)
            {
                playerAnimator.SetBool("BlockedHit", false);
                currentState = PlayerState.BLOCK;
                TransitionToStateServerRpc(currentState);
                playerAnimator.Play("Block", 0, 1.0f);
            }
            else
            {
                if (movementInput != Vector2.zero && canMove)
                {
                    currentState = PlayerState.MOVEMENT;

                    playerAnimator.SetBool("Running", true);
                }

                else currentState = PlayerState.IDLE;
            }

            if (playerAnimator.GetBool("SlowAttack") == true)
            {
                playerAnimator.SetBool("SlowAttack", false);

            }

        }
        else
        {
            if (playerAnimator.GetBool("BlockedHit") == true)
            {
                playerAnimator.SetBool("BlockedHit", false);                
                playerAnimator.Play("Block", 0, 1.0f);
            }
            //Aquí podría ir el idle

            if (movementInput != Vector2.zero && canMove)
            {
                currentState = PlayerState.MOVEMENT;

                playerAnimator.SetBool("Running", true);
            }

            else currentState = PlayerState.IDLE;
        }

        if (playerAnimator.GetBool("Stunned") == true)
            {
                playerAnimator.SetBool("Stunned", false);
            }

       




    }

    private void Taunting()
    {
        if (IsOwner)
        {
            if (taunt && canChangeState && canTaunt)
            {
                jokeParticle.Play();
                canChangeState = false;
                canMove = false;
                currentState = PlayerState.JOKE;
                TransitionToStateServerRpc(currentState);
                playerAnimator.SetBool("Taunting", true);
                print("Taunt Owner");
                canTaunt = false;
            }
            else
            {
                if (!taunt)
                {
                    if (playerAnimator.GetBool("Taunting") == true)
                    {
                        playerAnimator.SetBool("Taunting", false);
                    }

                    if (!canTaunt) canTaunt = true;
                }

            }
        }
        else
        {
            if(currentState == PlayerState.JOKE)
            {
                jokeParticle.Play();
                canChangeState = false;
                canMove = false;
                currentState = PlayerState.JOKE;              
                playerAnimator.SetBool("Taunting", true);
                print("Taunt Not Owner");
                canTaunt = false;
            }
            else
            {
                if (playerAnimator.GetBool("Taunting") == true)
                {
                    playerAnimator.SetBool("Taunting", false);
                }

                if (!canTaunt) canTaunt = true;
            }
        }
      
    }

    #region Dash
    private void Dashing()
    {
        if (IsOwner)
        {
            
            if (dash && canDash && canChangeState && canDashing && (playerOne && (gameManager.playerOneMagic >= dashMagicCost) || (!playerOne && (gameManager.playerTwoMagic >= dashMagicCost))))
            {
                gameManager.ChangeMagic(-dashMagicCost, playerOne);
                canChangeState = false;
                canDash = false;
                canMove = false;
                canDashing = false;
                currentState = PlayerState.DASH;
                TransitionToStateServerRpc(currentState);
                playerAnimator.SetBool("Dashing", true);

                //Añadir un pequeño desfase en el dash, que no se produzca inmediato


                StartCoroutine("DashDelay");
                print("Dash Owner");

                //print(Vector3.Distance(GameObject.FindGameObjectWithTag("Enemy").transform.position, transform.position));

            }
            else
            {
                if (!dash && !canDashing) canDashing = true;
                if (!canDash)
                {
                    //print(Vector3.Distance(enemy.transform.position, transform.position));
                    if (Vector3.Distance(enemy.transform.position, transform.position) < dashDistanceToTarget && currentState == PlayerState.DASH)
                    {
                        dashImpactParticle.Play();
                        StopCoroutine("StopDash");
                        //print("Dash Stopped");
                        StartCoroutine("DashReset");
                        playerRb.velocity = Vector3.zero;
                        canMove = true;
                        canChangeState = true;
                        if (playerAnimator.GetBool("Dashing") == true)
                        {
                            playerAnimator.SetBool("Dashing", false);
                        }
                        currentState = PlayerState.IDLE;
                        TransitionToStateServerRpc(currentState);
                        print("Stop player: ");
                    }


                }
            }
        }
        else
        {
            if(currentState == PlayerState.DASH)
            {
                gameManager.ChangeMagic(-dashMagicCost, playerOne);
                canChangeState = false;
                canDash = false;
                canMove = false;
                canDashing = false;
                if (IsOwner)
                {
                    currentState = PlayerState.DASH;
                    TransitionToStateServerRpc(currentState);
                }
               
                playerAnimator.SetBool("Dashing", true);

                //Añadir un pequeño desfase en el dash, que no se produzca inmediato


                StartCoroutine("DashDelay");
                print("Dash Not Owner");
            }
            else
            {
                if (!dash && !canDashing) canDashing = true;
                if (playerAnimator.GetBool("Dashing") == true)
                {
                    playerAnimator.SetBool("Dashing", false);
                }
            }
        }
        
    }

    IEnumerator DashDelay()
    {
        Vector3 relativePos = enemy.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(new Vector3(relativePos.x, 0, relativePos.z).normalized, Vector3.up);
        if (inAir)
        {
            playerRb.velocity = Vector3.zero;
            playerRb.useGravity = false;
        }


        yield return new WaitForSeconds(dashDelay);

        relativePos = enemy.transform.position - transform.position;
        playerRb.velocity = (relativePos.normalized * dashSpeed);
        dashParticle.Play();
        playerAudio.loop = false;
        playerAudio.clip = dashStartSound;
        playerAudio.Play();


        if (!isDashing)
        {
            StartCoroutine("StopDash");
            isDashing = true;
        }
    }

    IEnumerator StopDash()
    {
        yield return new WaitForSeconds(dashDuration);
        playerRb.velocity = Vector3.zero;
        canMove = true;
        canChangeState = true;
        //currentState = PlayerState.IDLE;
        //print("Cambio a idle");
        StartCoroutine("DashReset");
        if (playerAnimator.GetBool("Dashing") == true)
        {
            playerAnimator.SetBool("Dashing", false);
        }
    }

    IEnumerator DashReset()
    {
        yield return new WaitForSeconds(dashCooldown);
        //print("Dash Reset");
        canDash = true;
        isDashing = false;

    }

    #endregion
    private void Blocking()
    {
        if (IsOwner)
        {
                        
            if (block && !inAir && canChangeState)
            {
                shieldEffectParticle.Play();
                canChangeState = false;
                Vector3 relativePos = enemy.transform.position - transform.position;
                transform.rotation = Quaternion.LookRotation(new Vector3(relativePos.x, 0, relativePos.z).normalized, Vector3.up);
                canMove = false;
                currentState = PlayerState.BLOCK;
                TransitionToStateServerRpc(currentState);
                playerAnimator.SetBool("Blocking", true);
                print("Block Owner");
            }
            else
            {

                if (!block)
                {
                    if (playerAnimator.GetBool("Blocking") == true)
                    {
                        shieldEffectParticle.Stop();
                        playerAnimator.SetBool("Blocking", false);
                        canMove = true;
                        canChangeState = true;
                        currentState = PlayerState.IDLE;
                        TransitionToStateServerRpc(currentState);
                        print("Cambio a idle");
                        print("Reset Blocking");
                    }
                }


            }
        }
        else
        {
            if(currentState == PlayerState.BLOCK)
            {
                shieldEffectParticle.Play();
                canChangeState = false;
                Vector3 relativePos = enemy.transform.position - transform.position;
                transform.rotation = Quaternion.LookRotation(new Vector3(relativePos.x, 0, relativePos.z).normalized, Vector3.up);
                canMove = false;
                currentState = PlayerState.BLOCK;               
                playerAnimator.SetBool("Blocking", true);
                print("Block Not Owner");
            }
            else
            {
                if (playerAnimator.GetBool("Blocking") == true)
                {
                    shieldEffectParticle.Stop();
                    playerAnimator.SetBool("Blocking", false);
                    canMove = true;
                    canChangeState = true;
                    currentState = PlayerState.IDLE;                  
                    print("Cambio a idle");
                    print("Reset Blocking");
                }
            }
        }

        
    }

    private void Parrying()
    {
        if (IsOwner)
        {
            if (parry && canChangeState && canParry)
            {
                canChangeState = false;
                Vector3 relativePos = enemy.transform.position - transform.position;
                transform.rotation = Quaternion.LookRotation(new Vector3(relativePos.x, 0, relativePos.z).normalized, Vector3.up);
                canMove = false;
                currentState = PlayerState.PARRY;
                TransitionToStateServerRpc(currentState);
                playerAnimator.SetBool("Parrying", true);
                print("Parry Owner");
                canParry = false;
            }
            else
            {
                if (!parry)
                {
                    if (!canParry) canParry = true;
                }

                if (canChangeState)
                {
                    if (playerAnimator.GetBool("Parrying") == true)
                    {
                        playerAnimator.SetBool("Parrying", false);
                    }
                }
            }

            if (isParrying && !parryStartedParticle.isPlaying)
            {
                parryStartedParticle.Play();
                playerAudio.loop = false;
                playerAudio.clip = parryStartedSound;
                playerAudio.Play();
            }
        }
        else
        {
            if(currentState == PlayerState.PARRY)
            {
                canChangeState = false;
                Vector3 relativePos = enemy.transform.position - transform.position;
                transform.rotation = Quaternion.LookRotation(new Vector3(relativePos.x, 0, relativePos.z).normalized, Vector3.up);
                canMove = false;
                currentState = PlayerState.PARRY;            
                playerAnimator.SetBool("Parrying", true);
                print("Parry Not Owner");
                canParry = false;
            }
            else
            {
                if (!canParry) canParry = true;
                if (canChangeState)
                {
                    if (playerAnimator.GetBool("Parrying") == true)
                    {
                        playerAnimator.SetBool("Parrying", false);
                    }
                }

                if (isParrying && !parryStartedParticle.isPlaying)
                {
                    parryStartedParticle.Play();
                    playerAudio.loop = false;
                    playerAudio.clip = parryStartedSound;
                    playerAudio.Play();
                }
            }
        }
       
    }

    private void ParryStarts()
    {
        isParrying = true;
    }

    private void ParryEnds()
    {
        isParrying = false;
    }


    private void BasicAtacking()
    {
        if (IsOwner)
        {
            if (basicAtacking && canChangeState && canBasicAttack)
            {
                currentState = PlayerState.FAST_ATACK;
                TransitionToStateServerRpc(currentState);
                basicAtackSlashParticle.Play();
                playerAudio.clip = basicAttackSlashSound;
                playerAudio.Play();

                canChangeState = false;
                canMove = false;

                Vector3 relativePos = enemy.transform.position - transform.position;
                transform.rotation = Quaternion.LookRotation(new Vector3(relativePos.x, 0, relativePos.z).normalized, Vector3.up);
                playerAnimator.SetBool("BasicAtack", true);
                print("Atacó Owner");
                canBasicAttack = false;
            }
            else
            {
                if (canChangeState)
                {
                    if (playerAnimator.GetBool("BasicAtack") == true)
                    {
                        playerAnimator.SetBool("BasicAtack", false);                       
                    }
                }
                if (!basicAtacking && !canBasicAttack) canBasicAttack = true;
               

            }
        }
        else
        {
            if(currentState == PlayerState.FAST_ATACK)
            {
                currentState = PlayerState.FAST_ATACK;                
                basicAtackSlashParticle.Play();
                playerAudio.clip = basicAttackSlashSound;
                playerAudio.Play();

                canChangeState = false;
                canMove = false;

                Vector3 relativePos = enemy.transform.position - transform.position;
                transform.rotation = Quaternion.LookRotation(new Vector3(relativePos.x, 0, relativePos.z).normalized, Vector3.up);
                playerAnimator.SetBool("BasicAtack", true);
                print("Atacó Not Owner");
                canBasicAttack = false;
            }
            else
            {
                if (canChangeState)
                {
                    if (playerAnimator.GetBool("BasicAtack") == true)
                    {
                        playerAnimator.SetBool("BasicAtack", false);                                     
                      
                    }
                }
                if (!basicAtacking && !canBasicAttack) canBasicAttack = true;
            }
        }
       
    }

    private void SlowAtack()
    {
        if (IsOwner)
        {
            if (slowAtacking && canChangeState && canSlowAttack)
            {
                chargeSlowAttackParticle.Play();
                currentChargedAttackForce = 0f;
                canChangeState = false;
                canMove = false;
                Vector3 relativePos = enemy.transform.position - transform.position;
                transform.rotation = Quaternion.LookRotation(new Vector3(relativePos.x, 0, relativePos.z).normalized, Vector3.up);
                chargingSlowAttack = true;
                currentState = PlayerState.SLOW_ATACK;
                playerAnimator.SetBool("SlowAttack", true);
                print("Ataque fuerte");
                canSlowAttack = false;
                TransitionToStateServerRpc(currentState);
            }
            else
            {
                if (!slowAtacking)
                {
                    if (chargeSlowAttackParticle.isPlaying) chargeSlowAttackParticle.Stop();


                    if (GetComponent<Animator>().speed == 0f && currentState == PlayerState.SLOW_ATACK)
                    {
                        GetComponent<Animator>().speed = 1f;
                        slowAtackSlashParticle.Play();
                    }

                    if (!chargingSlowAttack && !canSlowAttack) canSlowAttack = true;

                    currentChargedAttackForce = chargeTimer * 5;
                }

            }

            if (chargingSlowAttack)
            {
                if (chargeTimer < maxChargeTime)
                {
                    chargeTimer += Time.deltaTime;
                }
                else
                {
                    if (GetComponent<Animator>().speed == 0f)
                    {
                        GetComponent<Animator>().speed = 1f;

                        slowAtackSlashParticle.Play();
                        chargeSlowAttackParticle.Stop();
                        currentChargedAttackForce = chargeTimer * 5;

                    }
                }

            }



        }

        else
        {
            if(currentState == PlayerState.SLOW_ATACK)
            {
                if (slowAtacking && canChangeState && canSlowAttack)
                {
                    chargeSlowAttackParticle.Play();
                    currentChargedAttackForce = 0f;
                    canChangeState = false;
                    canMove = false;
                    Vector3 relativePos = enemy.transform.position - transform.position;
                    transform.rotation = Quaternion.LookRotation(new Vector3(relativePos.x, 0, relativePos.z).normalized, Vector3.up);
                    chargingSlowAttack = true;
                    currentState = PlayerState.SLOW_ATACK;
                    playerAnimator.SetBool("SlowAttack", true);
                    print("Ataque fuerte");
                    canSlowAttack = false;
                   
                }
                else
                {
                    if (!slowAtacking)
                    {
                        if (chargeSlowAttackParticle.isPlaying) chargeSlowAttackParticle.Stop();


                        if (GetComponent<Animator>().speed == 0f && currentState == PlayerState.SLOW_ATACK)
                        {
                            GetComponent<Animator>().speed = 1f;
                            slowAtackSlashParticle.Play();
                        }

                        if (!chargingSlowAttack && !canSlowAttack) canSlowAttack = true;

                        currentChargedAttackForce = chargeTimer * 5;
                    }

                }

                if (chargingSlowAttack)
                {
                    if (chargeTimer < maxChargeTime)
                    {
                        chargeTimer += Time.deltaTime;
                    }
                    else
                    {
                        if (GetComponent<Animator>().speed == 0f)
                        {
                            GetComponent<Animator>().speed = 1f;

                            slowAtackSlashParticle.Play();
                            chargeSlowAttackParticle.Stop();
                            currentChargedAttackForce = chargeTimer * 5;

                        }
                    }

                }
            }
        }


    }

    public void ChargeSlowAttack()
    {
        if (slowAtacking)
        {
            GetComponent<Animator>().speed = 0f;

        }
    }
    public void FastAttackDmg()
    {
        foreach (BoxCollider box in hitPoints)
        {
            box.enabled = true;
            StartCoroutine("DisableHitFast");
        }

        enemyTriggered = true;
    }
    public void SlowAttackDmg()
    {
        foreach (BoxCollider box in hitPoints)
        {
            box.enabled = true;
            StartCoroutine("DisableHitPoints");
        }

        enemyTriggered = true;
    }

    IEnumerator DisableHitFast()
    {
        for (int i = 0; i < 10; i++)
        {
            yield return null;
        }
        foreach (BoxCollider box in hitPoints)
        {
            box.enabled = false;
        }

        if (enemyTriggered)
            enemyTriggered = false;
    }
    IEnumerator DisableHitPoints()
    {
        for (int i = 0; i < 10; i++)
        {
            yield return null;
        }
        foreach (BoxCollider box in hitPoints)
        {
            box.enabled = false;
        }
        chargingSlowAttack = false;
        chargeTimer = 0f;

        if (enemyTriggered)
            enemyTriggered = false;
    }
    private void MoveCamera()
    {
        //xRot -= cursorY * cameraSensitivity;
        //transform.Rotate(0f, cursorX * cameraSensitivity, 0f);
        //playerCameraPosition.transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);


    }

    private void MovePlayer()
    {
        if (IsOwner)
        {
            Vector3 viewDir = transform.position - new Vector3(cam.transform.position.x, transform.position.y, cam.transform.position.z);
            orientation.forward = viewDir.normalized;
            orientation.transform.position = transform.position;

            inputDir = (orientation.forward * movementInput.y + orientation.right * movementInput.x).normalized;

            //print(playerRb.velocity.magnitude);
            if (movementInput != Vector2.zero && canMove)
            {
                currentState = PlayerState.MOVEMENT;
                TransitionToStateServerRpc(currentState);
                //transform.forward = inputDir.normalized;
                transform.forward = Vector3.Slerp(transform.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
                playerRb.velocity = new Vector3(inputDir.x * playerSpeed, playerRb.velocity.y, inputDir.z * playerSpeed);
                //print(playerRb.velocity);
                playerAnimator.SetBool("Running", true);
                playerAnimator.SetFloat("Speed", movementInput.magnitude);

                if (movementInput.magnitude >= 0.6f && !isRunning && !inAir)
                {
                    playerAudio.clip = footStepsSound;
                    playerAudio.volume = 0.2f;
                    playerAudio.loop = true;
                    playerAudio.Play();
                    isRunning = true;
                    startRunAnimationParticle.Play();
                    runningParticle.Play();


                }
                if ((movementInput.magnitude < 0.6f && isRunning) || (inAir && isRunning))
                {

                    playerAudio.Stop();
                    playerAudio.loop = false;
                    isRunning = false;

                    runningParticle.Stop();
                }





                //print(movementInput);
            }
            else
            {

                if ((movementInput.magnitude < 0.6f && isRunning) || !canMove)
                {
                    playerAudio.Stop();
                    isRunning = false;
                    runningParticle.Stop();
                }

                if (playerRb.velocity.magnitude <= 1f)
                {
                    if (playerAnimator.GetBool("Running") == true)
                    {
                        playerAnimator.SetBool("Running", false);

                        if (currentState == PlayerState.MOVEMENT)
                        {
                            currentState = PlayerState.IDLE;
                            TransitionToStateServerRpc(currentState);

                        }
                    }
                }

            }
        }
        else
        {
            if(currentState == PlayerState.MOVEMENT)
            {
                playerAnimator.SetBool("Running", true);
                playerAnimator.SetFloat("Speed", movementInput.magnitude);
                if (movementInput.magnitude >= 0.6f && !isRunning && !inAir)
                {
                    playerAudio.clip = footStepsSound;
                    playerAudio.volume = 0.2f;
                    playerAudio.loop = true;
                    playerAudio.Play();
                    isRunning = true;
                    startRunAnimationParticle.Play();
                    runningParticle.Play();
                }

                if ((movementInput.magnitude < 0.6f && isRunning) || (inAir && isRunning))
                {

                    playerAudio.Stop();
                    playerAudio.loop = false;
                    isRunning = false;

                    runningParticle.Stop();
                }
            }
            else
            {
                if ((movementInput.magnitude < 0.6f && isRunning) || !canMove)
                {
                    playerAudio.Stop();
                    isRunning = false;
                    runningParticle.Stop();
                }

                if (playerRb.velocity.magnitude <= 1f)
                {
                    if (playerAnimator.GetBool("Running") == true)
                    {
                        playerAnimator.SetBool("Running", false);

                        if (currentState == PlayerState.MOVEMENT)
                        {
                            currentState = PlayerState.IDLE;                           

                        }
                    }
                }
            }
        }
       
        
    }

    private void Jump()
    {

        if (jumped && currentJumps > 0 && !inAir && (currentState == PlayerState.IDLE || currentState == PlayerState.MOVEMENT))
        {
            inAir = true;
            startRunAnimationParticle.Play();
            jumpTime = 0f;
            playerRb.AddForce(Vector3.up * baseJumpForce, ForceMode.Impulse);
            playerRb.useGravity = false;

            print("Salté.");
            currentJumps--;
            playerAnimator.SetBool("Jumping", true);
        }

        if (!jumped && inAir && falling)
        {
            canCancelJump = true;
        }

        if (block && inAir && canCancelJump)
        {
            playerRb.AddForce(Vector3.down * baseJumpForce, ForceMode.Impulse);
            canCancelJump = false;
        }

        if (inAir)
        {
            if (playerRb.velocity.y > 0)
            {
                falling = false;
            }
            else
            {
                falling = true;
            }
        }
    }

    private void FixJump()
    {
        if (inAir)
        {
            jumpTime += Time.deltaTime;

            if (jumped && jumpTime < jumpMaxTime)
            {
                playerRb.AddForce(Vector3.up * jumpForceContinuous, ForceMode.Impulse);
            }
            else
            {
                if (currentState != PlayerState.DASH) playerRb.useGravity = true;
            }
        }
    }


    IEnumerator JumpReset()
    {
        yield return new WaitForSeconds(jumpCooldown);
        currentJumps = maxJumps;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor") && inAir)
        {

            StartCoroutine("JumpReset");
            playerAnimator.SetBool("Jumping", false);
            inAir = false;
            canCancelJump = false;
            //if (playerRb.velocity.y < -0.3f)
            groundHitParticle.Play();


        }
    }



    private void OnTriggerEnter(Collider other)
    {

        if ((other.gameObject.CompareTag("DeathFire")))
        {
            transform.position = spawnPoint.transform.position;
            
            gameManager.ReduceLife(playerOne);
        }
    }


}





