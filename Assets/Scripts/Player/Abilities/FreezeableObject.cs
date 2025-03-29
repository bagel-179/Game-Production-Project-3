using UnityEngine;

public class FreezeableObject : MonoBehaviour, IFreezeable
{
    private bool isFrozen = false;

    public void SetFrozen(bool frozen)
    {
        isFrozen = frozen;

        if (isFrozen)
        {
            Debug.Log($"{gameObject.name} is frozen.");
        }
        else
        {
            Debug.Log($"{gameObject.name} is unfrozen.");
        }
    }
}