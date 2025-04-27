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

        Debug.Log("Total Score: " + TotalScore);
        CheckEnding();
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
        if (TotalScore >= 200)
        {
            Destroy(badEnding);
        }
        else
        {
            Destroy(goodEnding);
        }
    }
}