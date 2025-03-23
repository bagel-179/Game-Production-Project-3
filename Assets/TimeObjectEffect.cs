using UnityEngine;
using System.Collections;

public class TimeObjectEffect : MonoBehaviour
{
    [Header("Glitch Settings")]
    public float minGlitchTime = 0.2f; 
    public float maxGlitchTime = 1f;  
    public float glitchAmount = 1f;   
    public float resetTime = 3f;  

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
                    Vector3 originalLocalPosition = child.localPosition;

                    Vector3 glitchOffset = new Vector3(
                        Random.Range(-glitchAmount, glitchAmount),
                        Random.Range(-glitchAmount, glitchAmount),
                        Random.Range(-glitchAmount, glitchAmount)
                    );
                    child.localPosition += glitchOffset;

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