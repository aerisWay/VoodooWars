using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitCollision : MonoBehaviour
{

    [SerializeField] PlayerController playerC;
    [SerializeField] GameManager gM;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            print("Da al enemigo");
           
            if (playerC.chargingSlowAttack && playerC.enemyTriggered)
            {
                //use charge.timer
                other.gameObject.GetComponent<Rigidbody>().AddForceAtPosition((transform.forward * 5 + transform.up * 2)*gM.playerOneFavor / 20 , gameObject.transform.position, ForceMode.Impulse);
                gM.ChangeFavor(5);
                Debug.Log("Impacté lento");
                playerC.enemyTriggered = false;

            }

            if(playerC.basicAtacking && playerC.enemyTriggered)
            {
                other.gameObject.GetComponent<Rigidbody>().AddForceAtPosition((transform.forward * 5 + transform.up * 2) * gM.playerOneFavor / 20, gameObject.transform.position, ForceMode.Impulse);
                gM.ChangeFavor(2);
                Debug.Log("Impacté rápido");
                playerC.enemyTriggered = false;
            }

        }


    }

}
