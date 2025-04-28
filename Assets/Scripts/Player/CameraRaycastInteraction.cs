using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRaycastInteraction : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float raycastDistance = 30f;
    public float raycastWidth = 0.5f;
    public LayerMask raycastLayerMask;

    [Header("SphereCast Settings")]
    public float sphereRadius = 5f;
    public LayerMask sphereCastLayerMask;

    [SerializeField] private AudioClip freezeSound;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    private void Update()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if (Physics.SphereCast(ray, raycastWidth, out hit, raycastDistance, raycastLayerMask))
        {
            VisualIndicator visualIndicator = hit.collider.GetComponent<VisualIndicator>();

            if (visualIndicator != null)
            {
                visualIndicator.EnableIndicator(true);
            }

            if (Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("Freeze"))
            {
                TryFreezeObject(hit.collider);
                audioSource.clip = freezeSound;
                audioSource.Play();
            }
        }
        else
        {
            DisableAllIndicators();
        }

        if (Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("Freeze"))
        {
            Debug.Log("Attempting to freeze nearby objects...");
            FreezeNearbyObjects();
        }
    }

    private void TryFreezeObject(Collider col)
    {
        if (col.CompareTag("Platform") || col.CompareTag("Enemy"))
        {
            audioSource.clip = freezeSound;
            audioSource.Play();

            IFreezeable freezeableChild = col.GetComponent<IFreezeable>();
            IFreezeable freezeableParent = col.transform.parent?.GetComponent<IFreezeable>();

            if (freezeableChild != null)
            {
                freezeableChild.SetFrozen(true);
                Debug.Log($"Frozen child: {col.name}");
            }

            if (freezeableParent != null)
            {
                freezeableParent.SetFrozen(true);
                Debug.Log($"Frozen parent: {col.transform.parent.name}");
            }

            StartCoroutine(UnfreezeObjectAfterTime(freezeableChild, freezeableParent, 5f));
        }
    }

    private void FreezeNearbyObjects()
    {
        List<IFreezeable> freezeableObjects = new List<IFreezeable>();

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, sphereRadius, sphereCastLayerMask);

        Debug.Log($"Detected {hitColliders.Length} objects in spherecast.");

        foreach (Collider col in hitColliders)
        {
            if (col.CompareTag("Platform") || col.CompareTag("Enemy"))
            {
                audioSource.clip = freezeSound;
                audioSource.Play();

                IFreezeable freezeableChild = col.GetComponent<IFreezeable>();
                IFreezeable freezeableParent = col.transform.parent?.GetComponent<IFreezeable>();

                if (freezeableChild != null && !freezeableObjects.Contains(freezeableChild))
                {
                    freezeableObjects.Add(freezeableChild);
                    freezeableChild.SetFrozen(true);
                    Debug.Log($"Frozen child (sphere): {col.name}");
                }

                if (freezeableParent != null && !freezeableObjects.Contains(freezeableParent))
                {
                    freezeableObjects.Add(freezeableParent);
                    freezeableParent.SetFrozen(true);
                    Debug.Log($"Frozen parent (sphere): {col.transform.parent.name}");
                }
            }
        }

        if (freezeableObjects.Count > 0)
        {
            StartCoroutine(UnfreezeObjectsAfterTime(freezeableObjects, 5f));
        }
    }

    private void DisableAllIndicators()
    {
        var allVisualIndicators = FindObjectsOfType<VisualIndicator>();
        foreach (var indicator in allVisualIndicators)
        {
            indicator.EnableIndicator(false);
        }
    }

    private IEnumerator UnfreezeObjectAfterTime(IFreezeable freezeableChild, IFreezeable freezeableParent, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (freezeableChild != null)
        {
            freezeableChild.SetFrozen(false);
            Debug.Log($"Unfrozen child: {freezeableChild}");
        }

        if (freezeableParent != null)
        {
            freezeableParent.SetFrozen(false);
            Debug.Log($"Unfrozen parent: {freezeableParent}");
        }
    }

    private IEnumerator UnfreezeObjectsAfterTime(List<IFreezeable> freezeableObjects, float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (IFreezeable obj in freezeableObjects)
        {
            if (obj != null)
            {
                obj.SetFrozen(false);
                Debug.Log($"Unfrozen (sphere): {obj}");
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(ray.origin, ray.direction * raycastDistance);
        Gizmos.DrawWireSphere(ray.origin + ray.direction * raycastDistance, raycastWidth);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, sphereRadius);
    }
}