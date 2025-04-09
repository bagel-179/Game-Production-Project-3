using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class TimeShiftManager : MonoBehaviour
{
    public Collider playerCollider;
    public LayerMask pastLayer;
    public LayerMask presentLayer;
    public LayerMask playerLayer;
    public float effectDuration = 2f;
    [Range(-1, 1)] public float lensDistortionAmount = -0.5f;
    public float collisionCheckRadius = 0.5f;

    [Header("Audio Ambience")]
    public GameObject pastAudio;
    public GameObject presentAudio;

    private Camera playerCamera;
    private bool isPastActive = false;
    private bool isOnCooldown = false;
    private float originalFOV;
    private ChromaticAberration chromaticAberration;
    private LensDistortion lensDistortion;
    private Volume globalVolume;    

    void Start()
    {
        playerCamera = Camera.main;
        originalFOV = playerCamera.fieldOfView;
        SetActiveTimeline(false);

        globalVolume = FindObjectOfType<Volume>();
        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out chromaticAberration);
            globalVolume.profile.TryGet(out lensDistortion);
        }
    }

    public void TimeShift()
    {
        if (isOnCooldown || WouldCollideInOtherTimeline()) return;

        StartCoroutine(TimeShiftEffects());
    }

    private bool WouldCollideInOtherTimeline()
    {
        LayerMask targetLayerMask = isPastActive ? presentLayer : pastLayer;
        int targetLayer = isPastActive ? LayerMask.NameToLayer("Present") : LayerMask.NameToLayer("Past");

        Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            collisionCheckRadius,
            targetLayerMask
        );

        if (colliders.Length > 0)
        {
            Debug.Log("Can't timeshift - would collide with objects in the other timeline");
            return true;
        }

        return false;
    }

    private System.Collections.IEnumerator TimeShiftEffects()
    {
        isOnCooldown = true;

        isPastActive = !isPastActive;
        SetActiveTimeline(isPastActive);

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

        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = 1f;
        }

        if (lensDistortion != null)
        {
            lensDistortion.intensity.value = lensDistortionAmount;
        }

        float elapsed = 0f;
        while (elapsed < effectDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / effectDuration;

            if (chromaticAberration != null)
            {
                chromaticAberration.intensity.value = Mathf.Lerp(1f, 0f, progress);
            }

            if (lensDistortion != null)
            {
                lensDistortion.intensity.value = Mathf.Lerp(lensDistortionAmount, 0f, progress);
            }

            yield return null;
        }

        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = 0f;
        }
        if (lensDistortion != null)
        {
            lensDistortion.intensity.value = 0f;
        }

        yield return new WaitForSecondsRealtime(0.5f);
        isOnCooldown = false;
    }

    private void SetActiveTimeline(bool pastActive)
    {
        int allLayers = ~0;

        if (pastActive)
        {
            playerCamera.cullingMask = (allLayers & ~presentLayer) | pastLayer;
            Physics.IgnoreLayerCollision(LayerMaskToLayer(presentLayer), LayerMaskToLayer(playerLayer), true);
            Physics.IgnoreLayerCollision(LayerMaskToLayer(pastLayer), LayerMaskToLayer(playerLayer), false);

            presentAudio.SetActive(false);
            pastAudio.SetActive(true);
        }
        else
        {
            playerCamera.cullingMask = (allLayers & ~pastLayer) | presentLayer;
            Physics.IgnoreLayerCollision(LayerMaskToLayer(pastLayer), LayerMaskToLayer(playerLayer), true);
            Physics.IgnoreLayerCollision(LayerMaskToLayer(presentLayer), LayerMaskToLayer(playerLayer), false);

            presentAudio.SetActive(true);
            pastAudio.SetActive(false);
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