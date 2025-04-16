using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Timer Settings")]
    public int startingTimeInSeconds = 300;

    [Header("UI References")]
    public TextMeshProUGUI timerText;

    private float currentTime;
    private bool isRunning = true;

    void Start()
    {
        currentTime = startingTimeInSeconds;
        UpdateTimerDisplay();
    }

    void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;
        currentTime = Mathf.Max(currentTime, 0);
        UpdateTimerDisplay();

        if (currentTime <= 0)
        {
            isRunning = false;
            GameOver();
        }
    }

    void UpdateTimerDisplay()
    {
        TimeSpan time = TimeSpan.FromSeconds(currentTime);
        timerText.text = time.ToString(@"mm\:ss");
    }

    public void AddTime(float seconds)
    {
        currentTime += seconds;
        UpdateTimerDisplay();
    }

    void GameOver()
    {
        Debug.Log("Game Over");
    }
}