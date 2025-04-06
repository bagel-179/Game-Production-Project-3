using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [Header("Object References")]
    public GameObject player;     
    public GameObject freeLook;
    public GameObject towerView;
    public GameObject menuBackground;
    
    [Header("UI References")]
    public GameObject playButton;
    public GameObject quitButton;
    public GameObject pauseMenu;

    private PlayerMovement playerMovementScript;
    private ThirdPersonCamera thirdPersonCameraScript;
    private Rigidbody menuRb;
    private Animator pauseMenuAnimator;

    private bool isPaused = false;
    private bool isAnimating = false;

    private void Start()
    {
        playerMovementScript = player.GetComponent<PlayerMovement>();
        thirdPersonCameraScript = player.GetComponent<ThirdPersonCamera>();

        menuRb = menuBackground.GetComponentInChildren<Rigidbody>();
        pauseMenuAnimator = GetComponentInChildren<Animator>();

        pauseMenu.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isAnimating)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        isAnimating = true;

        Cursor.visible = isPaused;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;

        if (isPaused)
        {
            pauseMenu.SetActive(true);
            pauseMenuAnimator.speed = 1f;

            StartCoroutine(SetTimeScaleAfterAnimation(0f));
        }
        else
        {
            pauseMenuAnimator.speed = -1f;

            StartCoroutine(SetTimeScaleAfterAnimation(1f, true));
        }
    }

    private IEnumerator SetTimeScaleAfterAnimation(float timeScale, bool disableMenu = false)
    {
        yield return new WaitForSecondsRealtime(pauseMenuAnimator.GetCurrentAnimatorStateInfo(0).length);

        Time.timeScale = timeScale;
        isAnimating = false;

        if (disableMenu)
        {
            pauseMenuAnimator.speed = -1f;

            pauseMenu.SetActive(false);
        }
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

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
