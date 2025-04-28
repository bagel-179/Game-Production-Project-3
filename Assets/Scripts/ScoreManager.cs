using UnityEngine;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int TotalScore { get; private set; }

    public GameObject goodEnding;
    public GameObject badEnding;

    void Awake()
    {       
        Debug.Log("Total Score: " + TotalScore);
        CheckEnding();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        //StartCoroutine(DelayedLoad());
    }

    //private IEnumerator DelayedLoad()
    //{
        
    //}

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
        if (TotalScore >= 50)
        {
            if (badEnding != null)
            {
                Destroy(badEnding);
            }

        }
        else
        {
            if (goodEnding != null)
            {
                Destroy(goodEnding);

            }            
            
        }
    }
}