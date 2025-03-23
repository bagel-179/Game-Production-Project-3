using UnityEngine;
using System.Collections;

public class TimeObjectEffect : MonoBehaviour
{
    [Header("Glitch Settings")]
    public float minGlitchTime = 0.2f; // Minimum time between glitches
    public float maxGlitchTime = 1f;   // Maximum time between glitches
    public float glitchAmount = 1f;    // Maximum distance moved per glitch
    public float resetTime = 3f;       // Time before resetting to (0,0,0)

    private void Start()
    {
        StartCoroutine(GlitchChildren());
    }

    private IEnumerator GlitchChildren()
    {
        while (true)
        {
            float waitTime = Random.Range(minGlitchTime, maxGlitchTime);
            yield return new WaitForSeconds(waitTime);

            foreach (Transform child in transform)
            {
                if (child != null)
                {
                    // Store original position relative to parent
                    Vector3 originalLocalPosition = child.localPosition;

                    // Move in a random direction within the allowed range
                    Vector3 glitchOffset = new Vector3(
                        Random.Range(-glitchAmount, glitchAmount),
                        Random.Range(-glitchAmount, glitchAmount),
                        Random.Range(-glitchAmount, glitchAmount)
                    );

                    // Apply the glitch movement
                    child.localPosition += glitchOffset;

                    // Wait for the reset time, then return to original position
                    StartCoroutine(ResetPosition(child, originalLocalPosition));
                }
            }
        }
    }

    private IEnumerator ResetPosition(Transform child, Vector3 originalLocalPosition)
    {
        yield return new WaitForSeconds(resetTime);
        if (child != null)
        {
            child.localPosition = originalLocalPosition;
        }
    }
}