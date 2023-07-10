using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalanceFavor : MonoBehaviour
{
    float startingZRotation = 0;
    [SerializeField] float minAngle = -30;
    [SerializeField] float maxAngle = 30;

    GameManager gM;

    private void Awake()
    {
        gM = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    public void SetBalanceAngle()
    {
        float newAngle = (gM.playerOneFavor * 60 / 100) - 30;

        print("Nuevo angulo: " + newAngle);

        Quaternion newRotation = Quaternion.Euler(0f, 0f, newAngle);

        float rotationSpeed = Mathf.Abs(newAngle - transform.rotation.eulerAngles.z) / 1f;

        StartCoroutine(RotateProgressive(newRotation, rotationSpeed));


       
        //transform.rotation = Quaternion.Slerp(transform.rotation, newRotation)
    }

    IEnumerator RotateProgressive(Quaternion targetRotation, float rotationSpeed)
    {
        float elapsedTime = 0f;
        Quaternion startRotation = transform.rotation;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime;

            // Perform the rotation interpolation using Lerp
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsedTime * rotationSpeed);

            yield return null;
        }

        // Ensure the object reaches the exact target rotation
        transform.rotation = targetRotation;
    }
}
