using UnityEngine;
using System.Collections;

public class FallReset : MonoBehaviour
{
    public Transform spawnPoint;
    public AudioClip fallSound;
    //public ParticleSystem fallEffect;

    private bool isResetting = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isResetting)
        {
            StartCoroutine(HandleFallReset(other.transform.root));
        }
    }

    private IEnumerator HandleFallReset(Transform playerRoot)
    {
        isResetting = true;

        Transform playerModel = playerRoot.Find("Capsule");
        PlayerMovement movement = playerRoot.GetComponent<PlayerMovement>();
        SplineGrinder grinder = playerRoot.GetComponent<SplineGrinder>();
        Rigidbody rb = playerRoot.GetComponent<Rigidbody>();

        bool originalUseGravity = true;
        originalUseGravity = rb.useGravity;
        rb.useGravity = false;

        playerModel.gameObject.SetActive(false);

        AudioSource.PlayClipAtPoint(fallSound, playerRoot.position);
        //Instantiate(fallEffect, playerRoot.position, Quaternion.identity);

        // Force exit any grinding state
        if (grinder != null && grinder.IsGrinding())
        {
            grinder.ForceEndGrind();
        }

        movement.enabled = false;
        movement.SetMovementLock(false);

        yield return new WaitForSeconds(2f);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = false;
        rb.MovePosition(spawnPoint.position);
        rb.MoveRotation(spawnPoint.rotation);
        rb.useGravity = originalUseGravity; 

        if (playerModel != null) playerModel.gameObject.SetActive(true);
        if (movement != null) movement.enabled = true;

        isResetting = false;
    }
}