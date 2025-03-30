using UnityEngine;

public class UIRotationAndScaler : MonoBehaviour
{
    public float maxDistance = 100f; 
    public float minScale = 7f;

    private Transform player;
    private Transform UI;

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        player = playerObject.transform;

        UI = transform.GetChild(0);
    }

    private void Update()
    {
        Vector3 directionToPlayer = transform.position - player.position; 
        directionToPlayer.y = 0;  
        Quaternion rotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 5f);

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        float scaleMultiplier = Mathf.Clamp01(distanceToPlayer / maxDistance); 
        float newScale = Mathf.Lerp(minScale, 10f, scaleMultiplier);  

        UI.localScale = new Vector3(newScale, newScale, newScale);
    }
}
