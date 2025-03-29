using System.Collections;
using UnityEngine;

public class CameraRaycastInteraction : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float raycastDistance = 30f; 
    public float raycastWidth = 0.5f;
    public LayerMask raycastLayerMask;

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

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (hit.collider.CompareTag("Platform") || hit.collider.CompareTag("Enemy"))
                {
                    var freezeableChild = hit.collider.GetComponent<IFreezeable>();
                    if (freezeableChild != null)
                    {
                        freezeableChild.SetFrozen(true);
                    }

                    var freezeableParent = hit.collider.transform.parent.GetComponent<IFreezeable>();
                    if (freezeableParent != null)
                    {
                        freezeableParent.SetFrozen(true);
                    }

                    StartCoroutine(UnfreezeObjectAfterTime(freezeableChild, freezeableParent, 5f));
                }
            }
        }
        else
        {
            var allVisualIndicators = FindObjectsOfType<VisualIndicator>();
            foreach (var indicator in allVisualIndicators)
            {
                indicator.EnableIndicator(false);
            }
        }
    }

        private IEnumerator UnfreezeObjectAfterTime(IFreezeable freezeableChild, IFreezeable freezeableParent, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (freezeableChild != null)
            freezeableChild.SetFrozen(false);

        if (freezeableParent != null)
            freezeableParent.SetFrozen(false);
    }

    private void OnDrawGizmos()
    {
        if (Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(ray.origin, ray.direction * raycastDistance); 
        Gizmos.DrawWireSphere(ray.origin + ray.direction * raycastDistance, raycastWidth); 
    }
}