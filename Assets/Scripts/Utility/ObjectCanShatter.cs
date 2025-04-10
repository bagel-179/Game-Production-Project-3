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
                shatterScript.EnableShattering();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (canShatter)
            {
                Shatter shatterScript = GetComponent<Shatter>();
                shatterScript.EnableShattering();
            }
        }
    }
}
