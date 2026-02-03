using UnityEngine;
using TMPro;

/// <summary>
/// Tracks and displays gameplay time
/// Attach this to a GameObject in the scene
/// </summary>
public class PlayTimeScore : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI timeText;

    [Header("Settings")]
    [SerializeField] private bool startImmediately = true;

    private float playTime = 0f;
    private bool isTimerRunning = false;

    private void Start()
    {
        if (startImmediately)
        {
            StartTimer();
        }
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            playTime += Time.deltaTime;
            UpdateTimeDisplay();
        }
    }

    private void UpdateTimeDisplay()
    {
        if (timeText != null)
        {
            int hours = Mathf.FloorToInt(playTime / 3600f);
            int minutes = Mathf.FloorToInt((playTime % 3600f) / 60f);
            int seconds = Mathf.FloorToInt(playTime % 60f);

            timeText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        }
    }

    public void StartTimer()
    {
        isTimerRunning = true;
    }

    public void StopTimer()
    {
        isTimerRunning = false;
    }

    public void ResetTimer()
    {
        playTime = 0f;
        UpdateTimeDisplay();
    }

    public void SetTime(float time)
    {
        playTime = time;
        UpdateTimeDisplay();
    }

    public float GetPlayTime()
    {
        return playTime;
    }
}