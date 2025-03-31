using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class Menu : MonoBehaviour
{
    [Header("Object References")]
    public GameObject player; 
    private PlayerMovement playerMovementScript;
    private ThirdPersonCamera thirdPersonCameraScript;
    public GameObject freeLook;
    public GameObject towerView;
    public GameObject menuBackground;
    private Rigidbody menuRb;

    [Header("UI References")]
    public GameObject playButton;
    public GameObject quitButton;

    private void Start()
    {
        playerMovementScript = player.GetComponent<PlayerMovement>();
        thirdPersonCameraScript = player.GetComponent<ThirdPersonCamera>();

        menuRb = menuBackground.GetComponentInChildren<Rigidbody>();
    }
    public void PlayGame()
    {
        playerMovementScript.enabled = true;
        thirdPersonCameraScript.enabled = true;

        freeLook.SetActive(true);
        towerView.SetActive(true);

        playButton.SetActive(false);
        quitButton.SetActive(false);

        menuRb.useGravity = true;
        Destroy(menuBackground, 5f);
    }


    public void QuitGame()
    {
        Application.Quit();
    }
}
