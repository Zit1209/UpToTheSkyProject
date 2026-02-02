using UnityEngine;

/// <summary>
/// Manages loading saved game data or starting a new game
/// Attach this to a persistent GameObject (like GameManager)
/// </summary>
public class GameLoader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private PlayTimeScore playTimeScore;
    [SerializeField] private AudioSettingsManager audioSettings;

    [Header("Default Values")]
    [SerializeField] private Vector3 defaultPlayerPosition = Vector3.zero;
    [SerializeField] private float defaultVolume = 0.8f;

    /// <summary>
    /// Loads all saved data from PlayerPrefs
    /// Call this from a UI button or at game start
    /// </summary>
    public void LoadGame()
    {
        if (!PlayerPrefs.HasKey("PlayerPosX"))
        {
            Debug.Log("No save data found. Starting new game instead.");
            NewGame();
            return;
        }

        // Load player position
        if (player != null)
        {
            float x = PlayerPrefs.GetFloat("PlayerPosX", defaultPlayerPosition.x);
            float y = PlayerPrefs.GetFloat("PlayerPosY", defaultPlayerPosition.y);
            float z = PlayerPrefs.GetFloat("PlayerPosZ", defaultPlayerPosition.z);

            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
                player.position = new Vector3(x, y, z);
                controller.enabled = true;
            }
            else
            {
                player.position = new Vector3(x, y, z);
            }
        }

        // Load play time
        if (playTimeScore != null)
        {
            float savedTime = PlayerPrefs.GetFloat("PlayTime", 0f);
            playTimeScore.SetTime(savedTime);
            playTimeScore.StartTimer();
        }

        // Load and apply audio volume
        if (audioSettings != null)
        {
            audioSettings.LoadAndApplyVolume();
        }
        else
        {
            float savedVolume = PlayerPrefs.GetFloat("AudioVolume", defaultVolume);
            AudioListener.volume = savedVolume;
        }

        // Load dialogue progress
        LoadDialogueProgress();

        Debug.Log("Game loaded successfully!");
    }

    /// <summary>
    /// Starts a new game with default values
    /// Call this from a UI button
    /// </summary>
    public void NewGame()
    {
        // Reset player position
        if (player != null)
        {
            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
                player.position = defaultPlayerPosition;
                controller.enabled = true;
            }
            else
            {
                player.position = defaultPlayerPosition;
            }
        }

        // Reset play time
        if (playTimeScore != null)
        {
            playTimeScore.ResetTimer();
            playTimeScore.StartTimer();
        }

        // Reset audio volume to default
        AudioListener.volume = defaultVolume;
        if (audioSettings != null)
        {
            audioSettings.SetVolume(defaultVolume);
        }

        // Clear dialogue progress
        ClearDialogueProgress();

        Debug.Log("New game started!");
    }

    private void LoadDialogueProgress()
    {
        // Load dialogue count and state
        int dialogueCount = PlayerPrefs.GetInt("DialogueCount", 0);

        for (int i = 0; i < dialogueCount; i++)
        {
            int isRead = PlayerPrefs.GetInt($"Dialogue_{i}_Read", 0);
            // Apply loaded dialogue state to your dialogue system
        }
    }

    private void ClearDialogueProgress()
    {
        int dialogueCount = PlayerPrefs.GetInt("DialogueCount", 0);

        for (int i = 0; i < dialogueCount; i++)
        {
            PlayerPrefs.DeleteKey($"Dialogue_{i}_Read");
        }
    }
}