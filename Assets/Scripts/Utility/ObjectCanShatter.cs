using UnityEngine;

public class ObjectCanShatter : MonoBehaviour
{
    public bool canShatter = false;

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (canShatter)
            {
                Shatter shatterScript = GetComponent<Shatter>();
                if (shatterScript != null)
                {
                    shatterScript.EnableShattering();
                }
            }
        }
    }
}
