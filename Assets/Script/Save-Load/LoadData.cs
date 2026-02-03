using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages loading saved game data or starting a new game
/// UPDATED: Load skin + New Game chuyển sang SkinSelectionScene
/// </summary>
public class GameLoader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayTimeScore playTimeScore;
    [SerializeField] private AudioSettingsManager audioSettings;

    [Header("Default Values")]
    [SerializeField] private Vector3 defaultPlayerPosition = Vector3.zero;
    [SerializeField] private float defaultVolume = 0.8f;
    
    [Header("Scene Settings")]
    [SerializeField] private string skinSelectionSceneName = "SkinSelectionScene";
    [SerializeField] private string gameSceneName = "GameScene";

    /// <summary>
    /// Loads all saved data from PlayerPrefs
    /// Call this from a UI button
    /// </summary>
    public void LoadGame()
    {
        // Kiểm tra có save data không
        if (!PlayerPrefs.HasKey("HasSaveData"))
        {
            Debug.Log("⚠️ No save data found. Starting new game instead.");
            NewGame();
            return;
        }

        Debug.Log("📂 Loading saved game...");

        // ===== LOAD SKIN =====
        if (PlayerPrefs.HasKey("SavedSkin"))
        {
            string savedSkin = PlayerPrefs.GetString("SavedSkin", "v1.0");
            SkinManager.SaveSelectedSkin(savedSkin);
            Debug.Log($"🎨 Loaded skin: {savedSkin}");
        }
        // ====================

        // Load play time
        if (playTimeScore != null)
        {
            float savedTime = PlayerPrefs.GetFloat("PlayTime", 0f);
            playTimeScore.SetTime(savedTime);
        }

        // Load audio volume
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

        Debug.Log("✅ Game loaded successfully!");
        
        // Load vào GameScene với skin đã lưu
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// Starts a new game - Chuyển sang SkinSelectionScene
    /// </summary>
    public void NewGame()
    {
        Debug.Log("🎮 Starting new game...");
        
        // Reset play time
        if (playTimeScore != null)
        {
            playTimeScore.ResetTimer();
        }

        // Reset audio volume to default
        AudioListener.volume = defaultVolume;
        if (audioSettings != null)
        {
            audioSettings.SetVolume(defaultVolume);
        }

        // Clear dialogue progress
        ClearDialogueProgress();
        
        // ===== XÓA SAVE DATA CŨ =====
        PlayerPrefs.DeleteKey("HasSaveData");
        PlayerPrefs.DeleteKey("SavedSkin");
        PlayerPrefs.DeleteKey("PlayerPosX");
        PlayerPrefs.DeleteKey("PlayerPosY");
        PlayerPrefs.DeleteKey("PlayerPosZ");
        PlayerPrefs.DeleteKey("PlayTime");
        // ===========================

        Debug.Log("🔄 Cleared old save data");
        
        // ===== CHUYỂN SANG SCENE CHỌN SKIN =====
        Debug.Log($"➡️ Loading {skinSelectionSceneName}...");
        SceneManager.LoadScene(skinSelectionSceneName);
        // =======================================
    }

    private void LoadDialogueProgress()
    {
        int dialogueCount = PlayerPrefs.GetInt("DialogueCount", 0);

        for (int i = 0; i < dialogueCount; i++)
        {
            int isRead = PlayerPrefs.GetInt($"Dialogue_{i}_Read", 0);
            // Apply loaded dialogue state
        }
    }

    private void ClearDialogueProgress()
    {
        int dialogueCount = PlayerPrefs.GetInt("DialogueCount", 0);

        for (int i = 0; i < dialogueCount; i++)
        {
            PlayerPrefs.DeleteKey($"Dialogue_{i}_Read");
        }
        
        PlayerPrefs.DeleteKey("DialogueCount");
    }
    
    /// <summary>
    /// Kiểm tra có save data không (dùng để enable/disable Load button)
    /// </summary>
    public bool HasSaveData()
    {
        return PlayerPrefs.HasKey("HasSaveData");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}