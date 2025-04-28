using UnityEngine;

public class CheckForTalisman : MonoBehaviour
{
    public GameObject Warriors;
    public GameObject talismanDialgoueBox;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Warriors.SetActive(false);
            talismanDialgoueBox.SetActive(false);
        }
    }
}