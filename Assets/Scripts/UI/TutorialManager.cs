using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] GameObject tutorialPanel;
    [SerializeField] TextMeshProUGUI tutorialText;
    [SerializeField] RawImage tutorialImage;
    [SerializeField] float timeBetweenMessages;

    [SerializeField] int tutorialtextIndex = 0;
    [SerializeField] int tutorialImageIndex = 0;


    [SerializeField] private string[] tutorialTextArray;
    [SerializeField] private Texture[] spriteImageArray;

    [SerializeField] private PlayerController playerOneController;
    [SerializeField] private PlayerController trainingPlayerController;

    bool waitingForInput = false;

    private void Awake()
    {
        UpdateTutorialCanvas();
    }

    private void Update()
    {
        if (waitingForInput)
        {
            if(tutorialtextIndex == 2 && playerOneController.currentState == PlayerController.PlayerState.MOVEMENT) InputCompleted();
            if (tutorialtextIndex == 3 && playerOneController.inAir) InputCompleted();            
            if (tutorialtextIndex == 4 && playerOneController.currentState == PlayerController.PlayerState.FAST_ATACK) InputCompleted();
            if (tutorialtextIndex == 7 && playerOneController.currentState == PlayerController.PlayerState.SLOW_ATACK) InputCompleted();
            if (tutorialtextIndex == 8 && playerOneController.currentState == PlayerController.PlayerState.DASH) InputCompleted();
            if (tutorialtextIndex == 9 && playerOneController.currentState == PlayerController.PlayerState.BLOCK) InputCompleted();
            if (tutorialtextIndex == 10 && playerOneController.currentState == PlayerController.PlayerState.PARRY) InputCompleted();
          

        }
    }
    private void UpdateTutorialCanvas()
    {
        tutorialText.text = tutorialTextArray[tutorialtextIndex];


        tutorialImage.texture = spriteImageArray[tutorialImageIndex];
        

        switch (tutorialtextIndex)
        {
            case 0:
            case 1:
            case 5:
            case 6:
            case 11:
            case 12:
                tutorialText.color = Color.white;
                tutorialImage.gameObject.SetActive(false);
                
                break;                           

            case 13:
                StartCoroutine("PassToNextMessage");
                
                break;

            default:
                WaitForInput();
                break;
        }

    }

    IEnumerator PassToNextMessage()
    {        
        yield return new WaitForSeconds(timeBetweenMessages);
        tutorialtextIndex++;
        UpdateTutorialCanvas();

        if (tutorialtextIndex == 13) tutorialPanel.SetActive(false);
    }

    private void WaitForInput()
    {
        tutorialText.color = Color.white;
        tutorialImage.gameObject.SetActive(true);
        waitingForInput = true;
    }
    
    private void InputCompleted()
    {
        waitingForInput = false;

        tutorialImageIndex++;

        tutorialText.color = Color.green;

        StartCoroutine("PassToNextMessage");
    }
}
