using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PausePanel : MonoBehaviour
{
    [SerializeField] GameObject resumeButton;
    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(resumeButton);
        print("Despierto");
    }
}
