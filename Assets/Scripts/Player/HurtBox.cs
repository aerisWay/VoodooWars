using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HurtBox : MonoBehaviour
{
    GameManager gameManager;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private NetworkPlayerController networkPlayerController;

    private void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        if(gameManager.activePlayMode == GameManager.PlayMode.ONLINE)
        {
            networkPlayerController = GetComponentInParent<NetworkPlayerController>();
        }
        else
        {
            playerController = GetComponentInParent<PlayerController>();
        }
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HitBox"))
        {
            if (gameManager.activePlayMode == GameManager.PlayMode.ONLINE)
            {
                networkPlayerController.ReceiveAttack();
            }
            else
            {
                playerController.ReceiveAttack();
            }
                
            print("Ay!");
        }
    }
}
