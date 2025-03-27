using UnityEngine;

public class FreezeableObject : MonoBehaviour, IFreezeable
{
    private bool isFrozen = false;

    public void SetFrozen(bool frozen)
    {
        isFrozen = frozen;

        // Handle the freezing behavior, e.g., stop movement, animations, etc.
        if (isFrozen)
        {
            // Logic to freeze the object (e.g., disable movement, stop animations)
            Debug.Log($"{gameObject.name} is frozen.");
        }
        else
        {
            // Logic to unfreeze the object (e.g., re-enable movement)
            Debug.Log($"{gameObject.name} is unfrozen.");
        }
    }

    private void Update()
    {
        if (!isFrozen)
        {
            // Normal behavior when the object is not frozen
        }
    }
}