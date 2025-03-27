using UnityEngine;

public class Menu : MonoBehaviour
{
    public GameObject player; 
    private PlayerMovement playerMovementScript;
    private ThirdPersonCamera thirdPersonCameraScript; 

    [Header("UI References")]
    public GameObject playButton;
    public GameObject quitButton;

    private void Start()
    {
        playerMovementScript = player.GetComponent<PlayerMovement>();
        thirdPersonCameraScript = player.GetComponent<ThirdPersonCamera>();
    }
    public void PlayGame()
    {
        playerMovementScript.enabled = true;
        thirdPersonCameraScript.enabled = true;

        playButton.SetActive(false);
        quitButton.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
