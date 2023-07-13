using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonOnSelect : MonoBehaviour, ISelectHandler
{
    [SerializeField] AudioSource selectedAudioSource;
    public void OnSelect(BaseEventData eventData)
    {
        selectedAudioSource.Play();
    }
}
