using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartUI : MonoBehaviour
{
    Quaternion originalRotation;


    private void Awake()
    {
        originalRotation = transform.localRotation;
    }

    private void Update()
    {
        transform.localRotation = Quaternion.identity;
        transform.rotation = Quaternion.identity;
    }
}
