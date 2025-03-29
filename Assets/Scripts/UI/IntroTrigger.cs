using Unity.Cinemachine;
using System.Collections;
using UnityEngine;
using Unity.VisualScripting;

public class IntroTrigger : MonoBehaviour
{
    [Header("Camera Settings")]
    public GameObject introCamera; 
    public GameObject playerCamera;
    public GameObject playerTowerCamera;

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
    private Transform playerCameraTransform;
    private Transform playerTowerCameraTransform;
    public float positionThreshold = 1f;

    private GameObject lastActiveCamera; 

    private void Start()
    {
        playerMovementScript = player.GetComponent<PlayerMovement>();
        introCameraTransform = introCamera.transform;
        playerCameraTransform = playerCamera.transform;
        playerTowerCameraTransform = playerTowerCamera.transform;

        introCamera.SetActive(false);

        lastActiveCamera = playerCamera; 
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

        lastActiveCamera = playerCamera.activeSelf ? playerCamera : playerTowerCamera;

        introCamera.SetActive(true);
        playerCamera.SetActive(false);
        playerTowerCamera.SetActive(false);

        float elapsedTime = 0f;
        Vector3 startPosition = lastActiveCamera.transform.position;
        Quaternion startRotation = lastActiveCamera.transform.rotation;

        while (Vector3.Distance(lastActiveCamera.transform.position, introCameraTransform.position) > positionThreshold ||
               Quaternion.Angle(lastActiveCamera.transform.rotation, introCameraTransform.rotation) > positionThreshold)
        {
            elapsedTime += Time.deltaTime * transitionSpeed;
            float t = Mathf.Clamp01(elapsedTime);

            lastActiveCamera.transform.position = Vector3.Lerp(startPosition, introCameraTransform.position, t);
            lastActiveCamera.transform.rotation = Quaternion.Slerp(startRotation, introCameraTransform.rotation, t);

            yield return null;
        }

        lastActiveCamera.transform.position = introCameraTransform.position;
        lastActiveCamera.transform.rotation = introCameraTransform.rotation;

        introUI.SetActive(true);

        yield return new WaitForSeconds(4f);
        EndIntro();
    }

    private void EndIntro()
    {
        introUI.SetActive(false);
        introCamera.SetActive(false);

        playerCamera.SetActive(true);
        playerTowerCamera.SetActive(true);

        playerMovementScript.enabled = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, triggerRange);
    }
}