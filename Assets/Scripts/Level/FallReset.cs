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
        if (other.CompareTag("Player"))
        {
            StartCoroutine(HandleFallReset(other.transform.root));
        }
    }

    private IEnumerator HandleFallReset(Transform playerRoot)
    {
        isResetting = true;

        Transform playerModel = playerRoot.Find("Capsule");
        playerModel.gameObject.SetActive(false);

        //Instantiate(fallEffect, playerRoot.position, Quaternion.identity);
        AudioSource.PlayClipAtPoint(fallSound, playerRoot.position);

        var movement = playerRoot.GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;

        yield return new WaitForSeconds(2f);

        Rigidbody rb = playerRoot.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.MovePosition(spawnPoint.position);
            rb.MoveRotation(spawnPoint.rotation);
        }
        else
        {
            playerRoot.position = spawnPoint.position;
            playerRoot.rotation = spawnPoint.rotation;
        }

        if (playerModel != null)
        {
            playerModel.gameObject.SetActive(true);
        }

        if (movement != null) movement.enabled = true;

        isResetting = false;
    }
}