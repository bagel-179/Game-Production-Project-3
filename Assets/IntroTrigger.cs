using Unity.Cinemachine;
using System.Collections;
using UnityEngine;
using Unity.VisualScripting;

public class IntroTrigger : MonoBehaviour
{
    [Header("Camera Settings")]
    public GameObject introCamera; // The secondary camera for the intro
    public GameObject playerCamera;

    [Header("Trigger Settings")]
    public Transform player;
    public float triggerRange = 25f; // Distance at which the intro triggers
    private bool introPlayed = false;

    [Header("UI Settings")]
    public GameObject introUI;

    [Header("Camera Transition Settings")]
    public float transitionSpeed = 2f; // Adjust for smoothness
    private Transform introCameraTransform;
    private Transform playerCameraTransform;
    public float positionThreshold = 0.5f;

    private void Start()
    {
        introCameraTransform = introCamera.transform;
        playerCameraTransform = playerCamera.transform;
    }

    private void Update()
    {
        if (!introPlayed && Vector3.Distance(transform.position, player.position) <= triggerRange)
        {
            StartCoroutine(StartIntro());
        }
    }

    private IEnumerator StartIntro()
    {
        introPlayed = true;

        // Enable intro camera & disable player camera
        introCamera.SetActive(true);
        playerCamera.SetActive(false);

        float elapsedTime = 0f;
        Vector3 startPosition = playerCameraTransform.position;
        Quaternion startRotation = playerCameraTransform.rotation;

        while (Vector3.Distance(playerCameraTransform.position, introCameraTransform.position) > positionThreshold)
        {
            elapsedTime += Time.deltaTime * transitionSpeed;
            float t = Mathf.Clamp01(elapsedTime); // Smoothly interpolate towards the target

            playerCameraTransform.position = Vector3.Lerp(startPosition, introCameraTransform.position, t);
            playerCameraTransform.rotation = Quaternion.Slerp(startRotation, introCameraTransform.rotation, t);

            yield return null;
        }

        // Ensure final position & rotation are exactly at the target
        playerCameraTransform.position = introCameraTransform.position;
        playerCameraTransform.rotation = introCameraTransform.rotation;

        // Only enable the UI after the camera has reached its target position
        introUI.SetActive(true);

        // Wait for 3 seconds, then disable UI & switch back to gameplay
        yield return new WaitForSeconds(4f);
        EndIntro();
    }

    private void EndIntro()
    {
        introUI.SetActive(false);
        introCamera.SetActive(false);
        playerCamera.SetActive(true);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, triggerRange);
    }
}
