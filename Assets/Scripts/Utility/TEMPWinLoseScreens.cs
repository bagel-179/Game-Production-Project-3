using UnityEngine;
using System.Collections;

public class TEMPWinLoseScreens : MonoBehaviour
{
    [Header("References are from the canvas within the pause menu")]
    public GameObject loseText;
    public GameObject winText;
    public GameObject pauseMenu;

    public bool isWinTrigger = false;

    private void Start()
    {
        loseText.SetActive(false);
        winText.SetActive(false);
        pauseMenu.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(Menu());            
        }
    }

    private IEnumerator Menu()
    {        
        pauseMenu.SetActive(true);

        if (isWinTrigger)
        {
            winText.SetActive(true);
        }
        else
        {
            loseText.SetActive(true);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        yield return new WaitForSecondsRealtime(0.3f);
        Time.timeScale = 0;
    }
}