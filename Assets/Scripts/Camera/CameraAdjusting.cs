using Cinemachine;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class CameraAdjusting : MonoBehaviour
{
    [SerializeField] GameObject playerOne;
    [SerializeField] GameObject playerTwo;
    GameManager gameManager;
    //private Animator animator;

    //[SerializeField] CinemachineVirtualCamera cam1;
    //[SerializeField] CinemachineVirtualCamera cam2;
    //private enum CameraState {FIRST_CAMERA,SECOND_CAMERA,PRESENTATION_CAMERA}
    //private CameraState cameraState;

    bool isSetUp;

    private void Awake()
    {
        //animator = GetComponent<Animator>();
        isSetUp = false;
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        gameManager.mainCam = gameObject.GetComponent<CinemachineVirtualCamera>();
    }
    public void SetPlayerInCamera()
    {
        playerOne = GameObject.FindGameObjectWithTag("PlayerOne");
        playerTwo = GameObject.FindGameObjectWithTag("PlayerTwo");

        gameObject.GetComponent<CinemachineVirtualCamera>().Follow = playerOne.transform;
        GetComponent<CinemachineVirtualCamera>().LookAt = playerTwo.transform;

        

        //cam2.GetComponent<CinemachineVirtualCamera>().Follow = playerTwo.transform;
        //cam2.GetComponent<CinemachineVirtualCamera>().LookAt = playerOne.transform;

        isSetUp = true;
        print("Cámara iniciada.");
    }

    //private void SwitchState()
    //{
    //    switch (cameraState)
    //    {
    //        case CameraState.FIRST_CAMERA:
    //            animator.Play("PlayerCameraOne");
    //            break;
    //        case CameraState.SECOND_CAMERA:
    //            animator.Play("PlayerCameraTwo");
    //            break;
    //        case CameraState.PRESENTATION_CAMERA:
    //            animator.Play("Presentation");
    //            break;            
    //    }
       
    //}
    void LateUpdate()
    {
        

        if(isSetUp)
        {
            
            if ((Vector3.Distance(playerOne.transform.position, transform.position) < Vector3.Distance(playerTwo.transform.position, transform.position)) && GetComponent<CinemachineVirtualCamera>().Follow != playerOne.transform)
            {
                GetComponent<CinemachineVirtualCamera>().Follow = playerOne.transform;
                GetComponent<CinemachineVirtualCamera>().LookAt = playerTwo.transform;
                //cameraState = CameraState.FIRST_CAMERA;
                //SwitchState();
                print("Cambio cámara.");
            }
            if ((Vector3.Distance(playerOne.transform.position, transform.position) >= Vector3.Distance(playerTwo.transform.position, transform.position)) && GetComponent<CinemachineVirtualCamera>().Follow != playerTwo.transform)
            {
                GetComponent<CinemachineVirtualCamera>().Follow = playerTwo.transform;
                GetComponent<CinemachineVirtualCamera>().LookAt = playerOne.transform;
                //cameraState = CameraState.SECOND_CAMERA;
                //SwitchState();
                print("Cambio cámara.");
            }

        }
    }
}
