using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main Menu UI Controller
/// Hiển thị buttons Load Game và New Game
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button btnLoadGame;
    public Button btnNewGame;
    public Button btnQuitGame;
    
    [Header("Optional UI")]
    public TMP_Text loadButtonText;
    public GameObject noSaveDataWarning;
    
    [Header("References")]
    public GameLoader gameLoader;
    
    void Start()
    {
        // Auto-find GameLoader nếu chưa gán
        if (gameLoader == null)
        {
            gameLoader = FindFirstObjectByType<GameLoader>();
        }
        
        // Setup buttons
        if (btnLoadGame != null)
        {
            btnLoadGame.onClick.AddListener(OnLoadGameClick);
        }
        
        if (btnNewGame != null)
        {
            btnNewGame.onClick.AddListener(OnNewGameClick);
        }
        
        if (btnQuitGame != null)
        {
            btnQuitGame.onClick.AddListener(OnQuitGameClick);
        }
        
        // Kiểm tra có save data không
        UpdateLoadButtonState();
    }
    
    void OnLoadGameClick()
    {
        if (gameLoader != null)
        {
            if (gameLoader.HasSaveData())
            {
                Debug.Log("📂 Loading game...");
                gameLoader.LoadGame();
            }
            else
            {
                Debug.LogWarning("⚠️ No save data found!");
                ShowNoSaveWarning();
            }
        }
    }
    
    void OnNewGameClick()
    {
        if (gameLoader != null)
        {
            Debug.Log("🎮 Starting new game...");
            gameLoader.NewGame();
        }
    }
    
    void OnQuitGameClick()
    {
        Debug.Log("👋 Quitting game...");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    /// <summary>
    /// Update trạng thái Load button
    /// </summary>
    void UpdateLoadButtonState()
    {
        if (gameLoader == null || btnLoadGame == null)
            return;
        
        bool hasSave = gameLoader.HasSaveData();
        
        // Enable/Disable button
        btnLoadGame.interactable = hasSave;
        
        // Update text nếu có
        if (loadButtonText != null)
        {
            if (hasSave)
            {
                loadButtonText.text = "LOAD GAME";
                loadButtonText.color = Color.white;
            }
            else
            {
                loadButtonText.text = "LOAD GAME (No Save)";
                loadButtonText.color = Color.gray;
            }
        }
        
        // Ẩn/hiện warning
        if (noSaveDataWarning != null)
        {
            noSaveDataWarning.SetActive(!hasSave);
        }
    }
    
    /// <summary>
    /// Hiện cảnh báo không có save data
    /// </summary>
    void ShowNoSaveWarning()
    {
        if (noSaveDataWarning != null)
        {
            noSaveDataWarning.SetActive(true);
            
            // Tự động ẩn sau 3 giây
            Invoke(nameof(HideNoSaveWarning), 3f);
        }
    }
    
    void HideNoSaveWarning()
    {
        if (noSaveDataWarning != null)
        {
            noSaveDataWarning.SetActive(false);
        }
    }
    
    void OnDestroy()
    {
        if (btnLoadGame != null) btnLoadGame.onClick.RemoveAllListeners();
        if (btnNewGame != null) btnNewGame.onClick.RemoveAllListeners();
        if (btnQuitGame != null) btnQuitGame.onClick.RemoveAllListeners();
    }
}