using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    TextMeshProUGUI timerText;
    [SerializeField] int timeRemaining;
    GameManager gM;
    float floatTimer;
    bool timerUp;

    private void Awake()
    {
        timerText = GetComponent<TextMeshProUGUI>();
        gM = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        timerText.text = timeRemaining.ToString();
        floatTimer = 0;
        timerUp = true;
    }

    private void Update()
    {
        if (timerUp)
        {
            floatTimer += Time.deltaTime;

            if(floatTimer > 1f)
            {
                floatTimer -= 1f;
                timeRemaining -= 1;
                timerText.text = timeRemaining.ToString();
            }
            
            if(timeRemaining <= 0)
            {
                timerUp = false;
                gM.OnTimerFinished();
            }
        }
    }
}
