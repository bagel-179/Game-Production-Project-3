using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Camera Targets")]
    public Transform player;
    public CinemachineCamera playerCamera;
    public CinemachineCamera towerCamera;
    public GameObject towerCam;

    [Header("Settings")]
    public float rotationSpeed = 10f;
    public float transitionSpeed = 2f;
    public int highPriority = 20;
    public int lowPriority = 10;

    private bool isTransitioning = false;
    private Transform currentView;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;


        playerCamera.Priority = highPriority;
        towerCamera.Priority = lowPriority;
    }

    private void FixedUpdate()
    {
        HandleCameraSwitch();
        RotatePlayer();
    }

    private void HandleCameraSwitch()
    {
        if (Input.GetMouseButton(1) || Input.GetButtonDown("SecondView"))
        {
            towerCamera.Priority = highPriority;
            playerCamera.Priority = lowPriority;
        }
        else 
        {
            towerCamera.Priority = lowPriority;
            playerCamera.Priority = highPriority;
        }
    }

    private void RotatePlayer()
    {
        CinemachineCamera activeCamera = playerCamera.Priority > towerCamera.Priority ? playerCamera : towerCamera;
        Transform activeCamTransform = activeCamera.transform;

        Vector3 cameraForward = activeCamTransform.forward;
        cameraForward.y = 0;
        if (cameraForward != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
            player.rotation = Quaternion.Slerp(player.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}