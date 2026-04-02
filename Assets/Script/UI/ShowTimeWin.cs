using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Shows saved time after credit timeline finishes, then loads Main Menu
/// Attach this to the same GameObject as PlayableDirector (Timeline)
/// </summary>
[RequireComponent(typeof(PlayableDirector))]
public class ShowTimeAfterCredit : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private GameObject timePanel;
    
    [Header("Display Format")]
    [SerializeField] private string prefix = "Your Time: ";
    [SerializeField] private bool showMilliseconds = false;

    [Header("Scene Settings")]
    [SerializeField] private string menuSceneName = "MainMenu";
    [SerializeField] private float delayBeforeMenu = 35f; // Giây chờ sau khi hiện time rồi mới chuyển
    
    private PlayableDirector director;
    private bool hasShownTime = false;

    private void Awake()
    {
        director = GetComponent<PlayableDirector>();
        
        // Hide time initially
        if (timeText != null)
        {
            timeText.gameObject.SetActive(false);
        }
        
        if (timePanel != null)
        {
            timePanel.SetActive(false);
        }
    }

    private void Update()
    {
        // Check if timeline has finished
        if (director != null && !hasShownTime)
        {
            if (director.state != PlayState.Playing && director.time >= director.duration)
            {
                ShowTime();
                hasShownTime = true;
            }
        }
    }

    private void ShowTime()
    {
        Debug.Log("🎬 Credit finished, showing time...");
        
        // Show panel
        if (timePanel != null)
        {
            timePanel.SetActive(true);
        }
        
        // Show and set time text
        if (timeText != null)
        {
            timeText.gameObject.SetActive(true);
            
            if (PlayerPrefs.HasKey("PlayTime"))
            {
                float savedTime = PlayerPrefs.GetFloat("PlayTime", 0f);
                timeText.text = prefix + FormatTime(savedTime);
                Debug.Log($"⏱️ Displayed time: {savedTime}s");
            }
            else
            {
                timeText.text = prefix + "00:00:00";
                Debug.LogWarning("⚠️ No saved time found!");
            }
        }

        // Chờ rồi chuyển về Main Menu
        Invoke(nameof(LoadMainMenu), delayBeforeMenu);
    }

    private void LoadMainMenu()
    {
        Debug.Log($"➡️ Loading {menuSceneName}...");
        SceneManager.LoadScene(menuSceneName);
    }

    private string FormatTime(float timeInSeconds)
    {
        int hours = Mathf.FloorToInt(timeInSeconds / 3600f);
        int minutes = Mathf.FloorToInt((timeInSeconds % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        
        if (showMilliseconds)
        {
            int milliseconds = Mathf.FloorToInt((timeInSeconds * 100f) % 100f);
            return string.Format("{0:00}:{1:00}:{2:00}.{3:00}", hours, minutes, seconds, milliseconds);
        }
        else
        {
            return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        }
    }
}