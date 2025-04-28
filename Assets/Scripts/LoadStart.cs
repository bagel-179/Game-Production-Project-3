using UnityEngine;
using UnityEngine.SceneManagement;


public class LoadStart : MonoBehaviour
{
    public bool isBadEnding = false;

    public void EndGame()
    {
        if (isBadEnding)
        {
            LoadBadEnding();
        }
        else
        {
            LoadGoodEnding();
        }
    }

    private void LoadBadEnding()
    {
        SceneManager.LoadScene("ClockTowerBlockout");
    }

    private void LoadGoodEnding()
    {
        SceneManager.LoadScene("ClockTowerEnding");
    }
}