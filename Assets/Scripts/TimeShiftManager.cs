using UnityEngine;

public class TimeShiftManager : MonoBehaviour
{
    public LayerMask pastLayer;
    public LayerMask presentLayer;
    public LayerMask playerLayer;

    private Camera playerCamera;
    private bool isPastActive = false;

    void Start()
    {
        playerCamera = Camera.main;
        SetActiveLayer(false);
    }

    public void TimeShift()
    {
        isPastActive = !isPastActive;
        SetActiveLayer(isPastActive);

        int inactiveLayer = isPastActive ? LayerMask.NameToLayer("Present") : LayerMask.NameToLayer("Past");
        int activeLayer = isPastActive ? LayerMask.NameToLayer("Past") : LayerMask.NameToLayer("Present");

        foreach (EnemyAI enemy in FindObjectsOfType<EnemyAI>())
        {
            if (enemy.gameObject.layer == inactiveLayer)
            {
                enemy.SetActiveState(false);
            }
            else if (enemy.gameObject.layer == activeLayer)
            {
                enemy.SetActiveState(true); 
            }
        }
    }

    private void SetActiveLayer(bool pastActive)
    {
        int allLayers = ~0;

        if (pastActive)
        {
            playerCamera.cullingMask = (allLayers & ~presentLayer) | pastLayer; 
            Physics.IgnoreLayerCollision(LayerMaskToLayer(presentLayer), LayerMaskToLayer(playerLayer), true);
            Physics.IgnoreLayerCollision(LayerMaskToLayer(pastLayer), LayerMaskToLayer(playerLayer), false);
        }
        else
        {
            playerCamera.cullingMask = (allLayers & ~pastLayer) | presentLayer;
            Physics.IgnoreLayerCollision(LayerMaskToLayer(pastLayer), LayerMaskToLayer(playerLayer), true);
            Physics.IgnoreLayerCollision(LayerMaskToLayer(presentLayer), LayerMaskToLayer(playerLayer), false);
        }
    }

    private int LayerMaskToLayer(LayerMask mask)
    {
        int layer = 0;
        int value = mask.value;
        while (value > 1)
        {
            value >>= 1;
            layer++;
        }
        return layer;
    }
}
