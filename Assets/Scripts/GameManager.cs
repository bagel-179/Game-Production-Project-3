using UnityEngine;
using System.Collections;
using System;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Timer Settings")]
    public int startingTimeInSeconds = 240;

    [Header("UI References")]
    public TextMeshProUGUI timerText;

    private float currentTime;
    private bool isRunning = true;
    private bool hasFlashed = false;

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

        if (currentTime <= 0 && !hasFlashed)
        {
            hasFlashed = true;
            isRunning = false;
            StartCoroutine(FlashTimerText());
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

    IEnumerator FlashTimerText()
    {
        while (true)
        {
            timerText.enabled = !timerText.enabled;
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void CompleteLevel()
    {
        if (!isRunning) return;

        isRunning = false;

        int bonusPoints = Mathf.FloorToInt(currentTime);
        ScoreManager.Instance.AddScore(bonusPoints);
        Debug.Log("Level Completed! Bonus: " + bonusPoints);
    }
}