using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [Header("Object References")]
    public GameObject player;     
    public GameObject freeLook;
    public GameObject towerView;
    
    [Header("UI References")]
    public GameObject playButton;
    public GameObject quitButton;
    public GameObject pauseMenu;
    public GameObject creditsButton;
    public GameObject creditsMenu;
    public GameObject TitleImage;

    private PlayerMovement playerMovementScript;
    private ThirdPersonCamera thirdPersonCameraScript;
    private Animator pauseMenuAnimator;

    private bool isPaused = false;
    private bool isAnimating = false;
    [SerializeField] public bool gameStarted = false;

    private void Start()
    {
        playerMovementScript = player.GetComponent<PlayerMovement>();
        thirdPersonCameraScript = player.GetComponent<ThirdPersonCamera>();

        //pauseMenuAnimator = GetComponentInChildren<Animator>();

        pauseMenu.SetActive(false);
        if (creditsMenu != null)
        {
            creditsMenu.SetActive(false);
        }

        if (gameStarted)
        {
            playerMovementScript.enabled = true;
            thirdPersonCameraScript.enabled = true;

            freeLook.SetActive(true);
            towerView.SetActive(true);

            playButton.SetActive(false);
            quitButton.SetActive(false);

            if (creditsMenu != null)
            {
                creditsMenu.SetActive(false);
            }

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void Update()
    {
        if (gameStarted && Input.GetKeyDown(KeyCode.Escape) && !isAnimating)
        {
            TogglePause();
        }        
    }

    public void TogglePause()
    {
        if (!gameStarted) return;

        isPaused = !isPaused;
        //isAnimating = true;

        Cursor.visible = isPaused;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;

        if (isPaused)
        {
            pauseMenu.SetActive(true);
           // pauseMenuAnimator.speed = 1f;

            //StartCoroutine(SetTimeScaleAfterAnimation(0f));
        }
        else
        {
            pauseMenu.SetActive(false);
            //  pauseMenuAnimator.speed = -1f;

            //StartCoroutine(SetTimeScaleAfterAnimation(1f, true));
        }
    }

    //private IEnumerator SetTimeScaleAfterAnimation(float timeScale, bool disableMenu = false)
    //{
    //  //  yield return new WaitForSecondsRealtime(pauseMenuAnimator.GetCurrentAnimatorStateInfo(0).length);

    //    Time.timeScale = timeScale;
    //    isAnimating = false;

    //    if (disableMenu)
    //    {
    //        pauseMenuAnimator.speed = -1f;

    //        pauseMenu.SetActive(false);
    //    }
    //}

    public void PlayGame()
    {
        gameStarted = true;

        playerMovementScript.enabled = true;
        thirdPersonCameraScript.enabled = true;

        freeLook.SetActive(true);
        towerView.SetActive(true);

        playButton.SetActive(false);
        quitButton.SetActive(false);
        if (creditsButton != null)
        {
            creditsButton.SetActive(false);
        }

        if (TitleImage != null)
        {
            TitleImage.SetActive(false);
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
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

    public void CreditMenu()
    {
        if (creditsMenu != null)
        {
            creditsMenu.SetActive(true);
        }
        
    }

    public void CloseCreditMenu()
    {
        if (creditsMenu != null)
        {
            creditsMenu.SetActive(false);
        }
        
    }

    public void ReturnToStart()
    {
        SceneManager.LoadScene("ClockTowerBlockout");
    }
}
