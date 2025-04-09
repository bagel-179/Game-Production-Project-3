using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DialogueTrigger : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Image portraitImage;

    [Header("Dialogue Content")]
    [TextArea(2, 5)]
    public string dialogueLine = "Default dialogue line here.";
    public Sprite portraitSprite;

    [Header("Settings")]
    public float dialogueDisplayTime = 4f;
    public float typeSpeed = 0.03f;

    private Coroutine typewriterCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
            }

            dialoguePanel.SetActive(true);

            if (portraitImage != null && portraitSprite != null)
                portraitImage.sprite = portraitSprite;

            typewriterCoroutine = StartCoroutine(ShowDialogue());
        }
    }

    private IEnumerator ShowDialogue()
    {
        dialogueText.text = "";
        foreach (char c in dialogueLine)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        // Wait a few seconds after the full line is typed out
        yield return new WaitForSeconds(dialogueDisplayTime);
        dialoguePanel.SetActive(false);
    }
}