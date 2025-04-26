using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int TotalScore { get; private set; }

    public GameObject goodEnding;
    public GameObject badEnding;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddScore(int amount)
    {
        TotalScore += amount;
        Debug.Log("Total Score: " + TotalScore);
    }

    public void ResetScore()
    {
        TotalScore = 0;
    }

    public void CheckEnding()
    {
        if (goodEnding != null && badEnding != null)
        {
            bool good = TotalScore >= 200;
            goodEnding.SetActive(good);
            badEnding.SetActive(!good);
        }
    }
}