using UnityEngine;

public class ObjectCanShatter : MonoBehaviour
{
    public bool canShatter = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (canShatter)
            {
                EnableShatterScript();
            }
        }
    }

    private void EnableShatterScript()
    {
        Shatter shatterScript = GetComponentInChildren<Shatter>();
        if (shatterScript != null)
        {
            shatterScript.EnableShattering();
        }
    }
}
