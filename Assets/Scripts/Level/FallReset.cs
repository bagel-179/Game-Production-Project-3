using UnityEngine;
using System.Collections;

public class FallReset : MonoBehaviour
{
    public Transform spawnPoint;
    public AudioClip fallSound;
    //public ParticleSystem fallEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(HandleFallReset(other.transform.root));
        }
    }

    private IEnumerator HandleFallReset(Transform playerRoot)
    {
        Transform playerModel = playerRoot.Find("Capsule");
        playerModel.gameObject.SetActive(false);

        //Instantiate(fallEffect, playerRoot.position, Quaternion.identity);
        AudioSource.PlayClipAtPoint(fallSound, playerRoot.position);

        yield return new WaitForSeconds(2f);

        Rigidbody rb = playerRoot.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        playerRoot.position = spawnPoint.position;
        playerRoot.rotation = spawnPoint.rotation;

        playerModel.gameObject.SetActive(true);
    }
}