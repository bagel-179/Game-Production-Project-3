using UnityEngine;
using System.Collections;

public class BookTriggerPortal : MonoBehaviour
{
    public GameManager gm;
    public GameObject portalPrefab;
    public Transform portalSpawnPoint;
    public float fadeDuration = 2f;

    private bool portalSpawned = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!portalSpawned && other.CompareTag("Player"))
        {
            GameObject portal = Instantiate(portalPrefab, portalSpawnPoint.position, Quaternion.identity);
            StartCoroutine(FadeInPortal(portal));
            portalSpawned = true;
            gm.CompleteLevel();
        }
    }

    private IEnumerator FadeInPortal(GameObject portal)
    {
        Renderer renderer = portal.GetComponent<Renderer>();
        Material mat = renderer.material; 

        float brightness = 0f;
        while (brightness < 1f)
        {
            brightness += Time.deltaTime / fadeDuration;
            mat.SetFloat("Brightness", brightness);
            yield return null;
        }

        mat.SetFloat("Brightness", 1f);
    }
}
