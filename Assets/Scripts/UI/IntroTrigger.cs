using Unity.Cinemachine;
using System.Collections;
using UnityEngine;
using Unity.VisualScripting;

public class IntroTrigger : MonoBehaviour
{
    [Header("Camera Settings")]
    public GameObject introCameraObj;
    public CinemachineCamera introCamera; 
    public CinemachineCamera freeLookCamera;
    public CinemachineCamera towerCamera;

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

        lastActiveCamera = freeLookCamera.Priority > towerCamera.Priority ? freeLookCamera : towerCamera;
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

        lastActiveCamera = freeLookCamera.Priority > towerCamera.Priority ? freeLookCamera : towerCamera;

        introCameraObj.SetActive(true);

        introCamera.Priority = 100;

        yield return new WaitForSeconds(0.5f); 

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