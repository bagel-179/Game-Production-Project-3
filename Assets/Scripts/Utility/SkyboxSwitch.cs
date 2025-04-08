using UnityEngine;

public class SkyboxSwitcher : MonoBehaviour
{
    public Material presentSkybox;
    public Material pastSkybox;

    private bool isPresent = true;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            isPresent = !isPresent;

            // Change the skybox
            RenderSettings.skybox = isPresent ? presentSkybox : pastSkybox;

            // Optional: force update the lighting
            DynamicGI.UpdateEnvironment();

            // Swap layer visibility or call your layer switch logic here too
            Debug.Log("Switched to: " + (isPresent ? "Present" : "Past"));
        }
    }
}
