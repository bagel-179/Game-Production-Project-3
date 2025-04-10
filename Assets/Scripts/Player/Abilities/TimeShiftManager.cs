using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class TimeShiftManager : MonoBehaviour
{
    [Header("Player References")]
    public GameObject player;
    public Collider playerCollider;
    public Camera playerCamera;

    [Header("Timeline Layers")]
    public LayerMask pastLayer;
    public LayerMask presentLayer;
    public LayerMask playerLayer;
    public float collisionCheckRadius = 0.5f;

    [Header("Timeline FX")]
    public GameObject PrestineFX;
    public GameObject EruptionFX;
    public Material PrestineSkybox;
    public Material EruptionSkybox;
        
    [Header("Shift Effects")]
    public float effectDuration = 2f;
    [Range(-1, 1)] public float lensDistortionAmount = -0.5f;    
    [Range(0, 10)] public float bloomIntensity = 5f;
    [Range(0, 1)] public float bloomThreshold = 0f;

    private bool isPastActive = false;
    private bool isOnCooldown = false;
    private ChromaticAberration chromaticAberration;
    private LensDistortion lensDistortion;
    private Bloom bloom;
    private Volume globalVolume;

    void Start()
    {
        SetActiveTimeline(false);

        playerCamera = Camera.main;

        globalVolume = FindObjectOfType<Volume>();

        globalVolume.profile.TryGet(out chromaticAberration);
        globalVolume.profile.TryGet(out lensDistortion);
        globalVolume.profile.TryGet(out bloom);
    }

    public void TimeShift()
    {
        if (isOnCooldown || WouldCollideInOtherTimeline()) return;

        StartCoroutine(TimeShiftEffects());
    }

    private bool WouldCollideInOtherTimeline()
    {
        if (playerCollider == null) return false;

        LayerMask targetLayerMask = isPastActive ? presentLayer : pastLayer;
        int targetLayer = isPastActive ? LayerMask.NameToLayer("Present") : LayerMask.NameToLayer("Past");

        Physics.IgnoreLayerCollision(LayerMaskToLayer(targetLayerMask), LayerMaskToLayer(playerLayer), false);

        bool wouldCollide = Physics.CheckSphere(
            playerCollider.transform.position,
            collisionCheckRadius,
            targetLayerMask
        );

        Physics.IgnoreLayerCollision(LayerMaskToLayer(targetLayerMask), LayerMaskToLayer(playerLayer), true);

        if (wouldCollide)
        {
            Debug.Log("Can't timeshift - collision detected");
            return true;
        }

        return false;
    }

    private IEnumerator TimeShiftEffects()
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

        RenderSettings.skybox = isPastActive ? EruptionSkybox : PrestineSkybox;
        DynamicGI.UpdateEnvironment();


        //HANDLES EFFECTS!!!

        float originalChromaIntensity = chromaticAberration != null ? chromaticAberration.intensity.value : 0f;
        float originalLensDistortion = lensDistortion != null ? lensDistortion.intensity.value : 0f;
        float originalBloomIntensity = bloom != null ? bloom.intensity.value : 2f;
        float originalBloomThreshold = bloom != null ? bloom.threshold.value : 0.8f;

        bloom.intensity.value = bloomIntensity;
        bloom.threshold.value = bloomThreshold;

        float elapsed = 0f;
        while (elapsed < effectDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / effectDuration;

            chromaticAberration.intensity.value = Mathf.Lerp(1f, 0f, progress);
            lensDistortion.intensity.value = Mathf.Lerp(lensDistortionAmount, 0f, progress);
            bloom.intensity.value = Mathf.Lerp(bloomIntensity, originalBloomIntensity, progress);
            bloom.threshold.value = Mathf.Lerp(bloomThreshold, originalBloomThreshold, progress);

            yield return null;
        }

        chromaticAberration.intensity.value = originalChromaIntensity;
        lensDistortion.intensity.value = originalLensDistortion;
        bloom.intensity.value = originalBloomIntensity;
        bloom.threshold.value = originalBloomThreshold;

        yield return new WaitForSecondsRealtime(0.5f);
        isOnCooldown = false;
    }

    private void SetActiveTimeline(bool pastActive)
    {
        if (playerCamera == null) return;

        int allLayers = ~0;

        if (pastActive)
        {
            playerCamera.cullingMask = (allLayers & ~presentLayer) | pastLayer;
            Physics.IgnoreLayerCollision(LayerMaskToLayer(presentLayer), LayerMaskToLayer(playerLayer), true);
            Physics.IgnoreLayerCollision(LayerMaskToLayer(pastLayer), LayerMaskToLayer(playerLayer), false);

            PrestineFX.SetActive(false);
            EruptionFX.SetActive(true);
        }
        else
        {
            playerCamera.cullingMask = (allLayers & ~pastLayer) | presentLayer;
            Physics.IgnoreLayerCollision(LayerMaskToLayer(pastLayer), LayerMaskToLayer(playerLayer), true);
            Physics.IgnoreLayerCollision(LayerMaskToLayer(presentLayer), LayerMaskToLayer(playerLayer), false);

            PrestineFX.SetActive(true);
            EruptionFX.SetActive(false);
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