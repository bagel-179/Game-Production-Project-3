using Unity.Cinemachine;
using System.Collections;
using UnityEngine;
using Unity.VisualScripting;

public class IntroTrigger : MonoBehaviour
{
    [Header("Camera Settings")]
    public GameObject introCamera; 
    public GameObject playerCamera;

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
    public float positionThreshold = 0.5f;

    private void Start()
    {
        playerMovementScript = player.GetComponent<PlayerMovement>();
        introCameraTransform = introCamera.transform;
        playerCameraTransform = playerCamera.transform;
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

        introCamera.SetActive(true);
        playerCamera.SetActive(false);

        float elapsedTime = 0f;
        Vector3 startPosition = playerCameraTransform.position;
        Quaternion startRotation = playerCameraTransform.rotation;

        while (Vector3.Distance(playerCameraTransform.position, introCameraTransform.position) > positionThreshold)
        {
            elapsedTime += Time.deltaTime * transitionSpeed;
            float t = Mathf.Clamp01(elapsedTime); 

            playerCameraTransform.position = Vector3.Lerp(startPosition, introCameraTransform.position, t);
            playerCameraTransform.rotation = Quaternion.Slerp(startRotation, introCameraTransform.rotation, t);

            yield return null;
        }

        playerCameraTransform.position = introCameraTransform.position;
        playerCameraTransform.rotation = introCameraTransform.rotation;

        introUI.SetActive(true);

        yield return new WaitForSeconds(4f);
        EndIntro();
    }

    private void EndIntro()
    {
        introUI.SetActive(false);
        introCamera.SetActive(false);
        playerCamera.SetActive(true);
        playerMovementScript.enabled = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, triggerRange);
    }
}
