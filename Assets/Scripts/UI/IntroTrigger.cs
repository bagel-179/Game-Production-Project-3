using Unity.Cinemachine;
using System.Collections;
using UnityEngine;
using Unity.VisualScripting;

public class IntroTrigger : MonoBehaviour
{
    [Header("Camera Settings")]
    public GameObject introCameraObj;
    public CinemachineCamera introCamera; 
    public CinemachineCamera playerCamera;
    public CinemachineCamera playerTowerCamera;

    [Header("Trigger Settings")]
    public GameObject player;
    public float triggerRange = 25f; 
    private bool introPlayed = false;
    private PlayerMovement playerMovementScript;

    [Header("UI Settings")]
    public GameObject introUI;

    [Header("Camera Transition Settings")]
    public float transitionSpeed = 2f;
    private Transform introCameraTransform;
    public float positionThreshold = 1f;
    public float rotationThreshold = 2f;

    private CinemachineCamera lastActiveCamera;

    private void Start()
    {
        playerMovementScript = player.GetComponent<PlayerMovement>();
        introCameraTransform = introCamera.transform;

        introCameraObj.SetActive(false);

        // Find which Cinemachine camera is currently active
        lastActiveCamera = playerCamera.Priority > playerTowerCamera.Priority ? playerCamera : playerTowerCamera;
    }

    private void Update()
    {
        if (!introPlayed && Vector3.Distance(transform.position, player.transform.position) <= triggerRange)
        {
            StartCoroutine(StartIntro());
        }
    }

    private IEnumerator StartIntro()
    {
        introPlayed = true;
        playerMovementScript.enabled = false;

        // Ensure we are detecting the correct active camera
        lastActiveCamera = playerCamera.Priority > playerTowerCamera.Priority ? playerCamera : playerTowerCamera;

        // Temporarily increase introCamera priority
        introCameraObj.SetActive(true);

        introCamera.Priority = 100;

        yield return new WaitForSeconds(0.5f); // Allow time for camera transition

        // Wait until the camera reaches the target position & rotation
        while (!IsCameraInPosition(introCameraTransform))
        {
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);
        // Camera reached target, show UI
        introUI.SetActive(true);

        yield return new WaitForSeconds(4f);
        EndIntro();
    }

    private bool IsCameraInPosition(Transform targetTransform)
    {
        Transform mainCam = Camera.main.transform;

        float positionDiff = Vector3.Distance(mainCam.position, targetTransform.position);
        float rotationDiff = Quaternion.Angle(mainCam.rotation, targetTransform.rotation);

        return positionDiff < positionThreshold && rotationDiff < rotationThreshold;
    }

    private void EndIntro()
    {
        introUI.SetActive(false);
        introCameraObj.SetActive(false);

        introCamera.Priority = 1;

        playerMovementScript.enabled = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, triggerRange);
    }
}