using UnityEngine;
using System.Collections;

public class FlutePlaying : MonoBehaviour
{
    public GameObject arrowKeyUI;

    [Header("Audio")]
    public AudioClip upSound;
    public AudioClip downSound;
    public AudioClip leftSound;
    public AudioClip rightSound;
    public float fadeOutTime = 0.5f;

    private AudioSource audioSource;
    private bool isPlayerInside = false;
    private KeyCode currentKey = KeyCode.None;
    private Coroutine fadeCoroutine = null;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.loop = true;

        arrowKeyUI.SetActive(false);
    }

    private void Update()
    {
        if (!isPlayerInside) return;

        if (Input.GetKeyDown(KeyCode.UpArrow)) PlayLoop(KeyCode.UpArrow, upSound);
        if (Input.GetKeyDown(KeyCode.DownArrow)) PlayLoop(KeyCode.DownArrow, downSound);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) PlayLoop(KeyCode.LeftArrow, leftSound);
        if (Input.GetKeyDown(KeyCode.RightArrow)) PlayLoop(KeyCode.RightArrow, rightSound);

        if (currentKey != KeyCode.None && Input.GetKeyUp(currentKey))
        {
            if (!IsAnyArrowKeyHeld())
            {
                StartFadeOut();
            }
        }
    }

    private void PlayLoop(KeyCode key, AudioClip clip)
    {
        if (currentKey == key) return;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.volume = 0f;
        audioSource.Play();
        StartCoroutine(FadeIn());

        currentKey = key;
    }

    private bool IsAnyArrowKeyHeld()
    {
        return Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow);
    }

    private void StartFadeOut()
    {
        if (fadeCoroutine != null)
        StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeOutAndStop());
    }

    private IEnumerator FadeOutAndStop()
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0f)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeOutTime;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = 1f;
        currentKey = KeyCode.None;
        fadeCoroutine = null;
    }

    private IEnumerator FadeIn(float fadeTime = 0.1f)
    {
        float targetVolume = 1f;
        float t = 0f;

        while (t < fadeTime)
        {
            audioSource.volume = Mathf.Lerp(0f, targetVolume, t / fadeTime);
            t += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = targetVolume;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            arrowKeyUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            arrowKeyUI.SetActive(false);

            StartFadeOut();
        }
    }
}
