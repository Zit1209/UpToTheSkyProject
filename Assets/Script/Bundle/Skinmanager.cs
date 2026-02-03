using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Quản lý skin player - VERSION LINH HOẠT
/// Cho phép config tên asset trong Inspector
/// </summary>
public class SkinManager : MonoBehaviour
{
    private const string SKIN_KEY = "SelectedSkin";
    
    // ===== CONFIG TRONG INSPECTOR =====
    [Header("Skin Configurations")]
    [Tooltip("Danh sách cấu hình cho từng skin version")]
    public List<SkinConfig> skinConfigs = new List<SkinConfig>
    {
        new SkinConfig { version = "v1.0", displayName = "Red", assetName = "Player_Red" },
        new SkinConfig { version = "v2.0", displayName = "Blue", assetName = "Player_Blue" },
        new SkinConfig { version = "v3.0", displayName = "Yellow", assetName = "Player_Yellow" }
    };
    
    private static SkinManager instance;
    
    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // ===== PUBLIC STATIC METHODS =====
    
    /// <summary>
    /// Lưu skin đã chọn
    /// </summary>
    public static void SaveSelectedSkin(string skinVersion)
    {
        PlayerPrefs.SetString(SKIN_KEY, skinVersion);
        PlayerPrefs.Save();
        Debug.Log($"💾 Đã lưu skin: {skinVersion}");
    }
    
    /// <summary>
    /// Load skin đã chọn (default: v1.0)
    /// </summary>
    public static string LoadSelectedSkin()
    {
        string skin = PlayerPrefs.GetString(SKIN_KEY, "v1.0");
        Debug.Log($"📂 Load skin: {skin}");
        return skin;
    }
    
    /// <summary>
    /// Kiểm tra đã chọn skin chưa
    /// </summary>
    public static bool HasSelectedSkin()
    {
        return PlayerPrefs.HasKey(SKIN_KEY);
    }
    
    /// <summary>
    /// Reset về mặc định
    /// </summary>
    public static void ResetSkin()
    {
        PlayerPrefs.DeleteKey(SKIN_KEY);
        Debug.Log("🔄 Reset skin");
    }
    
    /// <summary>
    /// Lấy tên skin hiển thị từ version
    /// </summary>
    public static string GetSkinNameFromVersion(string version)
    {
        if (instance != null)
        {
            SkinConfig config = instance.skinConfigs.Find(c => c.version == version);
            if (config != null)
            {
                return config.displayName;
            }
        }
        
        // Fallback
        return version switch
        {
            "v1.0" => "Red",
            "v2.0" => "Blue",
            "v3.0" => "Yellow",
            _ => "Red"
        };
    }
    
    /// <summary>
    /// Lấy asset name từ version - LINH HOẠT
    /// </summary>
    public static string GetAssetNameFromVersion(string version)
    {
        if (instance != null)
        {
            SkinConfig config = instance.skinConfigs.Find(c => c.version == version);
            if (config != null)
            {
                Debug.Log($"📦 Asset name for {version}: {config.assetName}");
                return config.assetName;
            }
        }
        
        Debug.LogWarning($"⚠️ Không tìm thấy config cho version: {version}. Dùng fallback.");
        
        // Fallback nếu không có instance
        return version switch
        {
            "v1.0" => "Player_Red",
            "v2.0" => "Player_Blue",
            "v3.0" => "Player_Yellow",
            _ => "Player_Red"
        };
    }
    
    /// <summary>
    /// Lấy tất cả versions available
    /// </summary>
    public static List<string> GetAllVersions()
    {
        List<string> versions = new List<string>();
        
        if (instance != null)
        {
            foreach (var config in instance.skinConfigs)
            {
                versions.Add(config.version);
            }
        }
        else
        {
            // Fallback
            versions.AddRange(new[] { "v1.0", "v2.0", "v3.0" });
        }
        
        return versions;
    }
    
    /// <summary>
    /// Lấy config đầy đủ
    /// </summary>
    public static SkinConfig GetSkinConfig(string version)
    {
        if (instance != null)
        {
            return instance.skinConfigs.Find(c => c.version == version);
        }
        return null;
    }
}

/// <summary>
/// Cấu hình cho 1 skin
/// </summary>
[System.Serializable]
public class SkinConfig
{
    [Tooltip("Version ID (ví dụ: v1.0, v2.0)")]
    public string version;
    
    [Tooltip("Tên hiển thị (ví dụ: Red, Blue)")]
    public string displayName;
    
    [Tooltip("Tên asset trong bundle (ví dụ: Player_Red, MyCharacter_Skin1)")]
    public string assetName;
    
    [Header("Optional")]
    [Tooltip("Icon cho UI (optional)")]
    public Sprite icon;
    
    [Tooltip("Màu đại diện (optional)")]
    public Color themeColor = Color.white;
}