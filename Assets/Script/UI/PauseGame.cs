using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Quản lý tạm dừng game khi mở menu
/// UPDATED: Quản lý cursor - hiện khi mở menu, ẩn khi đóng menu
/// Attach script này vào Canvas Menu hoặc GameObject quản lý Menu
/// </summary>
public class PauseManager : MonoBehaviour
{
    [Header("Menu Canvas")]
    [SerializeField] private Canvas menuCanvas;
    
    [Header("Pause Key (Optional)")]
    [SerializeField] private bool enablePauseKey = true;
    
    [Header("References")]
    [SerializeField] private PlayTimeScore playTimeScore;
    
    [Header("Cursor Settings")]
    [SerializeField] private bool lockCursorWhenPlaying = true;
    
    private static bool isPaused = false;
    private static PauseManager instance;
    
    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Auto-find PlayTimeScore if not assigned
        if (playTimeScore == null)
        {
            playTimeScore = FindFirstObjectByType<PlayTimeScore>();
        }
    }
    
    void Start()
    {
        // Đảm bảo game không bị pause khi bắt đầu
        ResumeGame();
        
        // Ẩn menu khi bắt đầu
        if (menuCanvas != null)
        {
            menuCanvas.gameObject.SetActive(false);
        }
    }
    
    void Update()
    {
        // ESC để toggle menu (nếu enable)
        if (enablePauseKey)
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                TogglePause();
            }
        }
    }
    
    /// <summary>
    /// Toggle pause/resume - GỌI TỪ BUTTON hoặc ESC
    /// </summary>
    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }
    
    /// <summary>
    /// Tạm dừng game - GỌI TỪ BUTTON MỞ MENU
    /// </summary>
    public void PauseGame()
    {
        isPaused = true;
        
        // 1. Dừng thời gian game
        Time.timeScale = 0f;
        
        // 2. Dừng timer
        if (playTimeScore != null)
        {
            playTimeScore.StopTimer();
        }
        
        // 3. Hiện menu
        if (menuCanvas != null)
        {
            menuCanvas.gameObject.SetActive(true);
        }
        
        // 4. HIỆN CURSOR để click menu
        ShowCursor();
        
        Debug.Log("⏸️ Game paused - Cursor visible");
    }
    
    /// <summary>
    /// Tiếp tục game - GỌI TỪ BUTTON ĐÓNG MENU
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        
        // 1. Tiếp tục thời gian game
        Time.timeScale = 1f;
        
        // 2. Tiếp tục timer
        if (playTimeScore != null)
        {
            playTimeScore.StartTimer();
        }
        
        // 3. Ẩn menu
        if (menuCanvas != null)
        {
            menuCanvas.gameObject.SetActive(false);
        }
        
        // 4. ẨN CURSOR khi chơi game
        if (lockCursorWhenPlaying)
        {
            HideCursor();
        }
        
        Debug.Log("▶️ Game resumed - Cursor hidden");
    }
    
    /// <summary>
    /// Hiện cursor (khi mở menu)
    /// </summary>
    private void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("🖱️ Cursor shown");
    }
    
    /// <summary>
    /// Ẩn cursor (khi đóng menu)
    /// </summary>
    private void HideCursor()
    {
        // Check if CameraFollow wants cursor locked
        CameraFollow cameraFollow = Camera.main?.GetComponent<CameraFollow>();
        
        if (cameraFollow != null && cameraFollow.lockCursorOnStart)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Debug.Log("🖱️ Cursor hidden and locked");
        }
        else if (lockCursorWhenPlaying)
        {
            // Fallback: just lock it
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Debug.Log("🖱️ Cursor hidden and locked (fallback)");
        }
    }
    
    /// <summary>
    /// Kiểm tra game có đang pause không
    /// </summary>
    public static bool IsPaused()
    {
        return isPaused;
    }
    
    /// <summary>
    /// Get instance
    /// </summary>
    public static PauseManager Instance
    {
        get { return instance; }
    }
}