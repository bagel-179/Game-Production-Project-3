using UnityEngine;

public class BookTriggerPortal : MonoBehaviour
{
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
        }
    }

    private System.Collections.IEnumerator FadeInPortal(GameObject portal)
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
