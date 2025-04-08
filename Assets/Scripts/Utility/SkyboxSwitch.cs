using UnityEngine;

public class SkyboxSwapper : MonoBehaviour
{
    public Material pastSkybox;
    public Material presentSkybox;
    public float swapCooldown = 2f; // Time in seconds between swaps

    private bool isPresent = true;
    private float lastSwapTime = -Mathf.Infinity;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && Time.time >= lastSwapTime + swapCooldown)
        {
            isPresent = !isPresent;
            RenderSettings.skybox = isPresent ? presentSkybox : pastSkybox;
            DynamicGI.UpdateEnvironment(); // Optional: updates lighting

            lastSwapTime = Time.time;
        }
    }
}