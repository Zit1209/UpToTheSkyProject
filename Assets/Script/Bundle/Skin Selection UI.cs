using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// UI Controller cho MenuScene (chọn skin)
/// Attach vào Canvas
/// </summary>
public class SkinSelectionUI : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button btnSkinRed;
    public Button btnSkinBlue;
    public Button btnSkinYellow;
    public Button btnStartGame;
    
    [Header("Feedback UI")]
    public TMP_Text selectedText;
    
    [Header("Scene Settings")]
    public string gameSceneName = "GameScene";
    
    private string currentSelection = "v1.0";
    
    void Start()
    {
        // Setup button listeners
        if (btnSkinRed != null)
            btnSkinRed.onClick.AddListener(() => SelectSkin("v1.0"));
        
        if (btnSkinBlue != null)
            btnSkinBlue.onClick.AddListener(() => SelectSkin("v2.0"));
        
        if (btnSkinYellow != null)
            btnSkinYellow.onClick.AddListener(() => SelectSkin("v3.0"));
        
        if (btnStartGame != null)
            btnStartGame.onClick.AddListener(StartGame);
        
        // Load lựa chọn trước đó
        LoadLastSelection();
    }
    
    void SelectSkin(string version)
    {
        currentSelection = version;
        string skinName = SkinManager.GetSkinNameFromVersion(version);
        
        // Lưu ngay
        SkinManager.SaveSelectedSkin(version);
        
        // Update UI
        UpdateUI(skinName);
        
        // Highlight button
        HighlightButton(version);
        
        Debug.Log($"🎨 Chọn skin: {skinName} ({version})");
    }
    
    void UpdateUI(string skinName)
    {
        if (selectedText != null)
        {
            selectedText.text = $"Đã chọn: Skin {skinName}";
        }
    }
    
    void HighlightButton(string version)
    {
        // Reset all buttons to white
        ResetAllButtonColors();
        
        // Highlight selected button
        Button selectedBtn = version switch
        {
            "v1.0" => btnSkinRed,
            "v2.0" => btnSkinBlue,
            "v3.0" => btnSkinYellow,
            _ => btnSkinRed
        };
        
        if (selectedBtn != null)
        {
            // Change color to highlight
            ColorBlock colors = selectedBtn.colors;
            colors.normalColor = new Color(1f, 1f, 0.5f); // Light yellow
            selectedBtn.colors = colors;
        }
    }
    
    void ResetAllButtonColors()
    {
        Button[] buttons = { btnSkinRed, btnSkinBlue, btnSkinYellow };
        
        foreach (Button btn in buttons)
        {
            if (btn != null)
            {
                ColorBlock colors = btn.colors;
                colors.normalColor = Color.white;
                btn.colors = colors;
            }
        }
    }
    
    void LoadLastSelection()
    {
        if (SkinManager.HasSelectedSkin())
        {
            string savedSkin = SkinManager.LoadSelectedSkin();
            SelectSkin(savedSkin);
        }
        else
        {
            // Mặc định chọn Red
            SelectSkin("v1.0");
        }
    }
    
    void StartGame()
    {
        Debug.Log($"🎮 Bắt đầu game với skin: {currentSelection}");
        
        // Đảm bảo đã lưu
        SkinManager.SaveSelectedSkin(currentSelection);
        
        // Load GameScene
        SceneManager.LoadScene(gameSceneName);
    }
    
    void OnDestroy()
    {
        // Cleanup
        if (btnSkinRed != null) btnSkinRed.onClick.RemoveAllListeners();
        if (btnSkinBlue != null) btnSkinBlue.onClick.RemoveAllListeners();
        if (btnSkinYellow != null) btnSkinYellow.onClick.RemoveAllListeners();
        if (btnStartGame != null) btnStartGame.onClick.RemoveAllListeners();
    }
}